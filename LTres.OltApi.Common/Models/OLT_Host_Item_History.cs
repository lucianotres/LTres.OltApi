namespace LTres.OltApi.Common.Models;

public class OLT_Host_Item_History
{
    /// <summary>
    /// Identification of history item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identification of OLT Host Item related
    /// </summary>
    public Guid IdItem { get; set; }

    /// <summary>
    /// Collected at
    /// </summary>
    public DateTime At { get; set; }

    public int? ValueInt { get; set; }

    public uint? ValueUInt { get; set; }

    public string? ValueStr { get; set; }


    public static OLT_Host_Item_History From(WorkProbeResponse workProbeResponse) => 
        new OLT_Host_Item_History()
        {
            IdItem = workProbeResponse.Id,
            At = workProbeResponse.ProbedAt,
            ValueInt = workProbeResponse.ValueInt,
            ValueUInt = workProbeResponse.ValueUInt,
            ValueStr = workProbeResponse.ValueStr
        };
}