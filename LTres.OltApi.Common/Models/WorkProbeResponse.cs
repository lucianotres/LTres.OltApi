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

    public IEnumerable<WorkProbeResponseVar>? Values { get; set; }

    public override string ToString() => 
        $"Id: {Id}, {(Success ? "succeed" : "not succeed")} at {ProbedAt}: {(Values != null ? Values.Count().ToString() : ValueInt.HasValue ? ValueInt.ToString() : ValueUInt.HasValue ? ValueUInt.ToString() : ValueStr)}";
}

public enum WorkProbeResponseType : uint
{
    Value = 0,
    Walk = 1
}

public class WorkProbeResponseVar
{
    public string Key { get; set; } = string.Empty;
    public int? ValueInt { get; set; }
    public uint? ValueUInt { get; set; }
    public string? ValueStr { get; set; }
}