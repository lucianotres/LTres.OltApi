using System.Net;
using System.Text.Json.Serialization;
using LTres.OltApi.Common.Converters;

namespace LTres.OltApi.Common.Models;

public class WorkProbeInfo
{
    public required Guid Id { get; set; }

    public DateTime LastProbed { get; set; }

    [JsonConverter(typeof(IPEndPointConverter))]
    public required IPEndPoint Host { get; set; }

    public string? SnmpCommunity { get; set; }

    public int? SnmpVersion { get; set; }

    public bool SnmpBulk { get; set; } = true;

    public int? GetTimeout { get; set; }

    public DateTime? RequestedIn { get; set; }

    public bool DoHistory { get; set; } = false;

    public string Action { get; set; } = string.Empty;

    public string? ItemKey { get; set; }

    public string? Calc { get; set; }

    public bool? AsHex { get; set; }
}
