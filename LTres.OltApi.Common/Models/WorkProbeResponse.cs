namespace LTres.OltApi.Common.Models;

public class WorkProbeResponse
{
    public required Guid Id { get; set; }

    public DateTime ProbedAt { get; set; }

    public bool Success { get; set; }

    public int? ValueInt { get; set; }

    public string? ValueStr { get; set; }

    public override string ToString() => $"Id: {Id}, {(Success ? "succeed" : "not succeed")} at {ProbedAt}";
}
