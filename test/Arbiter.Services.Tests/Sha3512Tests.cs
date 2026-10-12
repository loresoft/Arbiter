using System.Security.Cryptography;
using System.Text;

namespace Arbiter.Services.Tests;

public class Sha3512Tests
{
    [Test]
    [Arguments("", "a69f73cca23a9ac5c8b567dc185a756e97c982164fe25859e0d1dcc1475c80a615b2123af1f5f94c11e3e9402c3ac558f500199d95b6d3e301758586281dcd26")]
    [Arguments("abc", "b751850b1a57168a5693cd924b6b096e08f621827444f70d884f5d0240d2712e10e116e9192af3c91a7ec57647e3934057340b4cf408d5a56592f8274eec53f0")]
    public void HashData_KnownVectors_Match(string input, string expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var digest = new byte[Sha3512.HashSizeInBytes];

        Sha3512.HashData(bytes, digest);

        Convert.ToHexStringLower(digest).Should().Be(expected);
    }

    [Test]
    [Arguments(1)]
    [Arguments(71)]
    [Arguments(72)]
    [Arguments(73)]
    [Arguments(144)]
    [Arguments(1000)]
    public void HashData_MatchesPlatform(int length)
    {
        if (!SHA3_512.IsSupported)
            Skip.Test("SHA3-512 is not supported on this platform.");

        var bytes = new byte[length];
        Random.Shared.NextBytes(bytes);

        var managed = new byte[Sha3512.HashSizeInBytes];
        Sha3512.HashData(bytes, managed);

        var expected = SHA3_512.HashData(bytes);
        managed.Should().Equal(expected);
    }
}
