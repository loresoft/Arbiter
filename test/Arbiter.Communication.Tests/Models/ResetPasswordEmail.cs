namespace Arbiter.Communication.Tests.Models;

public class ResetPasswordEmail
{
    public string Link { get; set; } = null!;

    public string? ProductName { get; set; }

    public string? CompanyName { get; set; }

    public int ExpireHours { get; set; } = 24;

}
