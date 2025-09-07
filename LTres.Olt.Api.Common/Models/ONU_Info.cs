namespace LTres.Olt.Api.Common.Models;

public class ONU_Info
{
    public string key { get; set; } = string.Empty;
    public string? sn { get; set; }
    public string? name { get; set; }
    public string? desc { get; set; }
    public int? state { get; set; }
    public int? rx { get; set; }
    public int? distance { get; set; }
}
