using System.Net;
using System.Text.Json.Serialization;
using LTres.OltApi.Common.Converters;

namespace LTres.OltApi.Common.Models;

public class WorkProbeInfo
{
    public required Guid Id { get; set; }

    public required DateTime LastProbed { get; set; }

    [JsonConverter(typeof(IPEndPointConverter))]
    public required IPEndPoint Host { get; set; }

    public bool WaitingResponse { get; set; } = false;

    public string Action { get; set; } = string.Empty;

    public string? ItemKey { get; set; }
}
