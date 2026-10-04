using System.Runtime.CompilerServices;

namespace Arbiter.Services;

/// <summary>
/// <para>High-performance, thread-safe, non-blocking Snowflake-style ID generator.</para>
/// <para>
/// A 63-bit positive <see cref="long"/> laid out (MSB to LSB) as:
/// <c>[unused sign bit][timestamp][instance][counter]</c>. Because the timestamp occupies the most
/// significant bits, ids sort chronologically by millisecond, which makes them well suited to clustered database keys.
/// </para>
/// <para>
/// Internally the counter is sequential within each millisecond. Before it is embedded in the id, it is passed
/// through a bijective permutation keyed by the timestamp and a per-generator random secret. Every counter value
/// is therefore usable and unique, but ids within the same millisecond appear in a shuffled order and are hard
/// to enumerate from one another. Ids are ordered by millisecond only; ordering within a millisecond is not preserved.
/// </para>
/// <para>
/// Generation never blocks. If the system clock moves backwards, the last issued timestamp is reused and
/// the counter keeps advancing. If the counter is exhausted within a millisecond, the generator borrows the
/// next millisecond. In both cases the embedded timestamp may briefly run ahead of wall-clock time until
/// the clock catches up.
/// </para>
/// <para>
/// Uniqueness is guaranteed per <see cref="InstanceId"/>. When instance ids are chosen randomly (the default),
/// uniqueness across nodes is probabilistic; assign explicit, coordinated instance ids for strict guarantees.
/// </para>
/// <para>
/// The random component makes ids harder to guess but is not a security boundary. Do not use these ids
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

    #region Default Instance
    private static Snowflake? _default;

    /// <summary>
    /// A shared generator. Uses the instance set via <see cref="Configure(Snowflake)"/>, otherwise
    /// the default configuration with a randomly assigned instance id.
    /// </summary>
    /// <remarks>
    /// Safe for concurrent use. Because the default instance id is random rather than coordinated, call
    /// <see cref="Configure(Snowflake)"/> at startup with an explicit instance id when running multiple nodes.
    /// </remarks>
    public static Snowflake Default
    {
        get
        {
            var current = Volatile.Read(ref _default);
            if (current != null)
                return current;

            var created = new Snowflake();
            return Interlocked.CompareExchange(ref _default, created, comparand: null) ?? created;
        }
    }

    /// <summary>
    /// Sets the shared <see cref="Default"/> generator. Must be called at startup, before <see cref="Default"/> is first used.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Default"/> can only be set once. Reading <see cref="Default"/> before calling this method lazily creates
    /// a generator with a random instance id and locks it in, so any later call to <see cref="Configure(Snowflake)"/> fails.
    /// </para>
    /// <para>
    /// Calling this method again with the same instance that is already configured is a no-op, which makes it safe
    /// to call from idempotent startup code. This method is thread-safe.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Program.cs
    /// var snowflake = new Snowflake(instanceId: 3);
    /// Snowflake.Configure(snowflake);
    /// </code>
    /// </example>
    /// <param name="snowflake">The generator to use as <see cref="Default"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="snowflake"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Default"/> has already been set to a different instance, or has already been read and therefore
    /// initialized with the default configuration.
    /// </exception>
    public static void Configure(Snowflake snowflake)
    {
        ArgumentNullException.ThrowIfNull(snowflake);

        var existing = Interlocked.CompareExchange(ref _default, snowflake, comparand: null);
        if (existing != null && !ReferenceEquals(existing, snowflake))
            throw new InvalidOperationException("Snowflake.Default has already been set or used. Call Configure at startup before any ids are generated.");
    }

    /// <summary>
    /// Sets the shared <see cref="Default"/> generator to a new <see cref="Snowflake"/> using the specified
    /// <paramref name="instanceId"/> and the default configuration. Must be called at startup, before
    /// <see cref="Default"/> is first used.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Equivalent to calling <see cref="Configure(Snowflake)"/> with <c>new Snowflake(instanceId)</c>.
    /// </para>
    /// <para>
    /// Because a new instance is created on every call, calling this method more than once always fails, even
    /// with the same <paramref name="instanceId"/>. Use <see cref="Configure(Snowflake)"/> with a shared instance
    /// when startup code may run more than once.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Program.cs
    /// Snowflake.Configure(instanceId: 3);
    /// </code>
    /// </example>
    /// <param name="instanceId">
    /// Worker id, 0 .. 63 with the default 6 instance bits. Assign a unique, coordinated id to each node.
    /// A negative value selects a random instance id.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="instanceId"/> exceeds the width of the default instance bits.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Default"/> has already been set, or has already been read and therefore initialized with the
    /// default configuration.
    /// </exception>
    public static void Configure(long instanceId)
    {
        var snowflake = new Snowflake(instanceId);
        Configure(snowflake);
    }

    #endregion

    private readonly long _epochTicks;
    private readonly int _counterBits;
    private readonly int _timestampShift;       // instanceBits + counterBits, precomputed
    private readonly long _maxCounter;          // doubles as the counter mask
    private readonly int _mixShift;             // xorshift distance used by the counter permutation
    private readonly ulong _secret;             // per-generator key for the counter permutation
    private readonly long _maxTimestamp;
    private readonly long _instanceShifted;     // instanceId << counterBits, precomputed

    // Packed state: (lastTimestamp << counterBits) | counter. Advanced via CAS only.
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
    /// <param name="instanceBits">Bits for the instance id (default 6 = 64 workers).</param>
    /// <param name="counterBits">
    /// Bits for the per-ms counter (default 16). 2^counterBits ids are available per millisecond before the
    /// generator borrows the next millisecond.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A bit width is out of range, or <paramref name="instanceId"/> exceeds the width of <paramref name="instanceBits"/>.
    /// </exception>
    /// <exception cref="ArgumentException">The combined bit widths exceed 63.</exception>
    public Snowflake(
        long instanceId = -1,
        DateTime? epoch = null,
        int timestampBits = 41,
        int instanceBits = 6,
        int counterBits = 16)
    {
        if (timestampBits <= 0)
            throw new ArgumentOutOfRangeException(nameof(timestampBits), "Must be greater than zero.");
        if (instanceBits < 0)
            throw new ArgumentOutOfRangeException(nameof(instanceBits), "Must be non-negative.");
        if (counterBits <= 0)
            throw new ArgumentOutOfRangeException(nameof(counterBits), "Must be greater than zero.");

        int total = timestampBits + instanceBits + counterBits;
        if (total > 63)
            throw new ArgumentException($"timestampBits + instanceBits + counterBits must be 63 or fewer (got {total}) to keep the id a positive long.", nameof(timestampBits));

        var e = epoch ?? DefaultEpoch;
        if (e.Kind != DateTimeKind.Utc)
            e = e.ToUniversalTime();

        long maxInstance = (1L << instanceBits) - 1;

        // Negative means "pick an instance id". A random draw distributes evenly across the available slots.
        if (instanceId < 0)
            instanceId = Random.Shared.NextInt64(maxInstance + 1);

        if (instanceId > maxInstance)
            throw new ArgumentOutOfRangeException(nameof(instanceId), $"Instance id must be between 0 and {maxInstance}.");

        _epochTicks = e.Ticks;
        _counterBits = counterBits;
        _timestampShift = instanceBits + counterBits;
        _maxCounter = (1L << counterBits) - 1;
        _mixShift = (counterBits + 1) / 2;
        _secret = (ulong)Random.Shared.NextInt64() ^ ((ulong)Random.Shared.NextInt64() << 1);
        _maxTimestamp = (1L << timestampBits) - 1;
        _instanceShifted = instanceId << counterBits;

        InstanceId = instanceId;
    }

    /// <summary>
    /// Generates the next id. Ids from a single generator never have a smaller timestamp than previously issued ids;
    /// ids sharing a millisecond are unique but in a shuffled order.
    /// </summary>
    /// <returns>A positive 63-bit id combining the timestamp, <see cref="InstanceId"/>, and counter.</returns>
    /// <remarks>
    /// Thread-safe, lock-free, and non-blocking. Clock regressions reuse the last issued timestamp and
    /// counter exhaustion borrows the next millisecond rather than waiting for the clock.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The timestamp has outgrown its bit width.</exception>
    public long NextId()
    {
        // The CAS loop is the hot path. It reads the current state, computes the next state, and attempts to swap it in.
        while (true)
        {
            long state = Volatile.Read(ref _state);
            long lastTs = state >> _counterBits;
            long counter = state & _maxCounter;
            long now = CurrentTimestamp();

            long newTs;
            long newCounter;
            if (now > lastTs)
            {
                newTs = now;
                newCounter = 0;
            }
            else
            {
                // Same millisecond or clock regression: never emit a smaller timestamp than already issued.
                newTs = lastTs;
                newCounter = counter + 1;

                if (newCounter > _maxCounter)
                {
                    // Counter exhausted: borrow the next millisecond instead of blocking.
                    newTs = lastTs + 1;
                    newCounter = 0;
                }
            }

            if (newTs > _maxTimestamp)
                throw new InvalidOperationException("Timestamp has exceeded the configured bit width; the generator is exhausted.");

            long newState = (newTs << _counterBits) | newCounter;

            // Attempt to swap in the new state. If another thread beat us to it, retry.
            if (Interlocked.CompareExchange(ref _state, newState, state) == state)
                return (newTs << _timestampShift) | _instanceShifted | Permute(newCounter, newTs);

            // Lost the CAS race — another thread advanced the state; retry immediately.
        }
    }

    /// <summary>Extracts the creation timestamp from an id produced by a generator with this configuration.</summary>
    /// <param name="id">The id to read the timestamp from.</param>
    /// <returns>The UTC timestamp, to millisecond precision, encoded in <paramref name="id"/>.</returns>
    /// <remarks>
    /// The bit widths and epoch of this generator are used to interpret the value. Reading an id created
    /// with a different configuration yields meaningless results. Under clock regression or heavy bursts the
    /// value may be slightly ahead of the actual creation time.
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
    /// Bijective permutation of <paramref name="counter"/> over [0, 2^counterBits), keyed by the timestamp and
    /// the per-generator secret. Each step (xor, odd multiply, add, xorshift) is invertible modulo 2^counterBits,
    /// so distinct counters within a millisecond always map to distinct values. Obfuscation only, not cryptographic.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long Permute(long counter, long timestamp)
    {
        ulong key = Mix((ulong)timestamp ^ _secret);
        ulong mask = (ulong)_maxCounter;
        ulong mul1 = key | 1;
        ulong mul2 = (key >> 32) | 1;

        ulong x = (ulong)counter;

        x = (((x ^ key) * mul1) + (key >> 16)) & mask;
        x ^= x >> _mixShift;
        x = (((x ^ (key >> 8)) * mul2) + (key >> 24)) & mask;
        x ^= x >> _mixShift;

        return (long)x;
    }

    /// <summary>SplitMix64 finalizer, used to derive a well-distributed per-millisecond key.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix(ulong z)
    {
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
