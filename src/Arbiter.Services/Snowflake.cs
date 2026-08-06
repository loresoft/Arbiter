using System.Runtime.CompilerServices;

namespace Arbiter.Services;

/// <summary>
/// <para>High-performance, thread-safe Snowflake ID generator.</para>
/// <para>
/// A 63-bit positive <see cref="long"/> laid out (MSB to LSB) as:
/// <c>[unused sign bit][timestamp][instance][sequence]</c>. Because the timestamp occupies the most
/// significant bits, ids sort chronologically, which makes them well suited to clustered database keys.
/// </para>
/// <para>
/// The timestamp, instance, and sequence bit widths are configurable and must sum to 63 or fewer.
/// Generation is lock-free: state is packed into a single 64-bit word and advanced with a
/// compare-and-swap loop, so there is no mutex on the hot path and any number of threads may call
/// <see cref="NextId"/> concurrently.
/// </para>
/// <para>
/// Uniqueness is only guaranteed per <see cref="InstanceId"/>. Deployments with more than a handful of
/// nodes should assign each node an explicit, coordinated instance id rather than relying on the
/// randomly chosen default.
/// </para>
/// <para>
/// Ids are time-ordered and their layout is public, so they are predictable by design. Do not use them
/// where an unguessable identifier is required.
/// </para>
/// </summary>
/// <example>
/// <code>
/// long id = Snowflake.Default.NextId();
/// DateTime created = Snowflake.Default.GetTimestamp(id);
/// </code>
/// </example>
public sealed class Snowflake
{
    /// <summary>
    /// Default epoch: 2024-01-01 00:00:00 UTC. With the default 41 timestamp bits this gives
    /// roughly 69 years of range, into 2093.
    /// </summary>
    public static readonly DateTime DefaultEpoch = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// A shared generator using the default configuration and a randomly assigned instance id.
    /// </summary>
    /// <remarks>
    /// Safe for concurrent use. Because the instance id is random rather than coordinated, prefer a
    /// dedicated instance constructed with an explicit instance id when running multiple nodes.
    /// </remarks>
    public static Snowflake Default { get; } = new();

    private readonly long _epochTicks;
    private readonly int _sequenceBits;
    private readonly int _timestampShift;       // instanceBits + sequenceBits, precomputed
    private readonly long _maxSequence;         // doubles as the sequence mask
    private readonly long _maxTimestamp;
    private readonly long _instanceShifted;     // instanceId << sequenceBits, precomputed
    private readonly long _maxClockDriftMs;

    // Packed state: (lastTimestamp << sequenceBits) | sequence. Advanced via CAS only.
    private long _state;

    /// <summary>
    /// Gets the instance (machine/worker) id baked into every generated value.
    /// </summary>
    /// <value>A value between 0 and 2^instanceBits - 1.</value>
    public long InstanceId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Snowflake"/> class.
    /// </summary>
    /// <param name="instanceId">
    /// Worker id, 0 .. (2^instanceBits - 1). When negative (the default), a random id is chosen for the
    /// lifetime of this instance. Prefer an explicit, coordinated id when running more than a few nodes.
    /// </param>
    /// <param name="epoch">Custom epoch. Defaults to <see cref="DefaultEpoch"/>. Coerced to UTC.</param>
    /// <param name="timestampBits">Bits for the millisecond timestamp (default 41 ≈ 69 years).</param>
    /// <param name="instanceBits">Bits for the instance id (default 10 = 1024 workers).</param>
    /// <param name="sequenceBits">Bits for the per-ms sequence (default 12 = 4096 ids/ms).</param>
    /// <param name="maxClockDriftMs">
    /// How long to wait out a backward clock jump before giving up. If the clock regresses by
    /// more than this, <see cref="NextId"/> throws instead of blocking. 0 fails fast.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A bit width or <paramref name="maxClockDriftMs"/> is negative, <paramref name="timestampBits"/> is
    /// not positive, or <paramref name="instanceId"/> exceeds the width of <paramref name="instanceBits"/>.
    /// </exception>
    /// <exception cref="ArgumentException">The combined bit widths exceed 63.</exception>
    public Snowflake(
        long instanceId = -1,
        DateTime? epoch = null,
        int timestampBits = 41,
        int instanceBits = 10,
        int sequenceBits = 12,
        int maxClockDriftMs = 100)
    {
        if (timestampBits <= 0)
            throw new ArgumentOutOfRangeException(nameof(timestampBits), "Must be greater than zero.");
        if (instanceBits < 0)
            throw new ArgumentOutOfRangeException(nameof(instanceBits), "Must be non-negative.");
        if (sequenceBits < 0)
            throw new ArgumentOutOfRangeException(nameof(sequenceBits), "Must be non-negative.");
        if (maxClockDriftMs < 0)
            throw new ArgumentOutOfRangeException(nameof(maxClockDriftMs), "Must be non-negative.");

        int total = timestampBits + instanceBits + sequenceBits;
        if (total > 63)
            throw new ArgumentException($"timestampBits + instanceBits + sequenceBits must be 63 or fewer (got {total}) to keep the id a positive long.", nameof(timestampBits));

        var e = epoch ?? DefaultEpoch;
        if (e.Kind != DateTimeKind.Utc)
            e = e.ToUniversalTime();

        long maxInstance = (1L << instanceBits) - 1;

        // Negative means "pick an instance id". A random draw distributes evenly across the available
        // slots, unlike process ids which cluster at low values and are heavily reused.
        if (instanceId < 0)
            instanceId = Random.Shared.NextInt64(maxInstance + 1);

        if (instanceId > maxInstance)
            throw new ArgumentOutOfRangeException(nameof(instanceId), $"Instance id must be between 0 and {maxInstance}.");

        _epochTicks = e.Ticks;
        _sequenceBits = sequenceBits;
        _timestampShift = instanceBits + sequenceBits;
        _maxSequence = (1L << sequenceBits) - 1;
        _maxTimestamp = (1L << timestampBits) - 1;
        _instanceShifted = instanceId << sequenceBits;
        _maxClockDriftMs = maxClockDriftMs;

        InstanceId = instanceId;
    }

    /// <summary>
    /// Generates the next id. Ids from a single generator are strictly increasing.
    /// </summary>
    /// <returns>A positive 63-bit id combining the timestamp, <see cref="InstanceId"/>, and sequence.</returns>
    /// <remarks>
    /// Thread-safe and lock-free. If the sequence for the current millisecond is exhausted, or the system
    /// clock has moved backwards within the configured tolerance, the call spins until the clock advances.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// The timestamp has outgrown its bit width, or the clock regressed beyond the drift tolerance.
    /// </exception>
    public long NextId()
    {
        // The CAS loop is the hot path. It reads the current state, computes the next state, and attempts to swap it in.
        while (true)
        {
            long state = Volatile.Read(ref _state);
            long lastTs = state >> _sequenceBits;
            long seq = state & _maxSequence;
            long now = CurrentTimestamp();

            long newTs, newSeq;
            if (now > lastTs)
            {
                newTs = now;
                newSeq = 0;
            }
            else if (now == lastTs)
            {
                newSeq = (seq + 1) & _maxSequence;
                if (newSeq == 0)
                {
                    // Sequence exhausted this ms — block until the clock advances.
                    WaitPastTimestamp(lastTs);
                    continue;
                }
                newTs = lastTs;
            }
            else
            {
                // Clock regression: never emit a smaller timestamp than already issued.
                WaitPastTimestamp(lastTs);
                continue;
            }

            if (newTs > _maxTimestamp)
                throw new InvalidOperationException("Timestamp has exceeded the configured bit width; the generator is exhausted.");

            long newState = (newTs << _sequenceBits) | newSeq;

            // Attempt to swap in the new state. If another thread beat us to it, retry.
            if (Interlocked.CompareExchange(ref _state, newState, state) == state)
                return (newTs << _timestampShift) | _instanceShifted | newSeq;

            // Lost the CAS race — another thread advanced the state; retry immediately.
        }
    }

    /// <summary>Extracts the creation timestamp from an id produced by a generator with this configuration.</summary>
    /// <param name="id">The id to read the timestamp from.</param>
    /// <returns>The UTC timestamp, to millisecond precision, encoded in <paramref name="id"/>.</returns>
    /// <remarks>
    /// The bit widths and epoch of this generator are used to interpret the value. Reading an id created
    /// with a different configuration yields meaningless results.
    /// </remarks>
    public DateTime GetTimestamp(long id)
    {
        long ts = id >> _timestampShift;
        long ticks = _epochTicks + (ts * TimeSpan.TicksPerMillisecond);

        return new DateTime(ticks, DateTimeKind.Utc);
    }

    /// <summary>Milliseconds elapsed since the configured epoch.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long CurrentTimestamp() => (DateTime.UtcNow.Ticks - _epochTicks) / TimeSpan.TicksPerMillisecond;

    /// <summary>
    /// Blocks until the clock passes <paramref name="lastTs"/>. Re-checks drift every iteration,
    /// so a clock that keeps sliding backwards mid-wait still trips the tolerance.
    /// </summary>
    /// <param name="lastTs">The most recently issued timestamp, in milliseconds since the epoch.</param>
    /// <exception cref="InvalidOperationException">The clock regressed beyond the drift tolerance.</exception>
    private void WaitPastTimestamp(long lastTs)
    {
        var spin = new SpinWait();
        long now;

        // Wait until the clock advances past the last timestamp. If the clock regresses too far, throw.
        while ((now = CurrentTimestamp()) <= lastTs)
        {
            long drift = lastTs - now;
            if (drift > _maxClockDriftMs)
            {
                throw new InvalidOperationException(
                    $"System clock moved backwards by {drift} ms, exceeding the configured " +
                    $"tolerance of {_maxClockDriftMs} ms. Refusing to wait or to reissue ids " +
                    "for a timestamp already used.");
            }

            spin.SpinOnce();
        }
    }
}
