namespace LTres.OltApi.Common.Models;

public class WorkProbeResponse
{
    public required Guid Id { get; set; }

    public WorkProbeResponseType Type { get; set; } = WorkProbeResponseType.Value;

    public DateTime ProbedAt { get; set; }

    public bool Success { get; set; }

    public string? FailMessage { get; set; }

    public int? ValueInt { get; set; }

    public uint? ValueUInt { get; set; }

    public string? ValueStr { get; set; }

    public override string ToString() => $"Id: {Id}, {(Success ? "succeed" : "not succeed")} at {ProbedAt}";
}

public enum WorkProbeResponseType : uint
{
    Value = 0,
    Walk = 1
}