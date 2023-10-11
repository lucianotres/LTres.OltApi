namespace LTres.OltApi.Common.Models;

public class OLT_Host_Item
{
    /// <summary>
    /// Identification of item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identification of OLT Host related
    /// </summary>
    public Guid IdOltHost { get; set; }

    /// <summary>
    /// Item action of probing
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Item key to perform an action
    /// </summary>
    public string? ItemKey { get; set; }

    /// <summary>
    /// Latest probed datetime
    /// </summary>
    /// <value></value>
    public DateTime? LastProbed { get; set; }

    /// <summary>
    /// Probe interval (seconds)
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// Mantain values history for (minutes)
    /// </summary>
    public int? MantainFor { get; set; }


    public bool ProbedSuccess { get; set; }

    public int? ProbedValueInt { get; set; }

    public string? ProbedValueStr { get; set; }

}