namespace LTres.Olt.Api.Common.Models;

public class OLT_Host_CLIconfig
{
    public string Type { get; set; } = "none";
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
