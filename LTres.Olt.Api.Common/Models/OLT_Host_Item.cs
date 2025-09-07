namespace LTres.Olt.Api.Common.Models;

public class OLT_Host_Item
{
    /// <summary>
    /// Identification of item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identification of OLT Host related
    /// </summary>
    public Guid? IdOltHost { get; set; }

    /// <summary>
    /// If it was created by another item
    /// </summary>
    public Guid? IdRelated { get; set; }

    /// <summary>
    /// Item action of probing
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Item key to perform an action
    /// </summary>
    public string? ItemKey { get; set; }

    /// <summary>
    /// Latest succeed probed datetime
    /// </summary>
    /// <value></value>
    public DateTime? LastProbed { get; set; }
    
    /// <summary>
    /// Do next probe after
    /// </summary>
    public DateTime? NextProbe { get; set; }

    /// <summary>
    /// Probe interval (seconds)
    /// </summary>
    public int? Interval { get; set; }

    /// <summary>
    /// Maintain values related for (minutes)
    /// </summary>
    public int? MaintainFor { get; set; }

    /// <summary>
    /// Maintain values history for (minutes)
    /// </summary>
    public int? HistoryFor { get; set; }

    /// <summary>
    /// This item is active
    /// </summary>
    public bool Active { get; set; } = true;


    public bool ProbedSuccess { get; set; }
    public string? ProbeFailedMessage { get; set; }

    public int? ProbedValueInt { get; set; }

    public uint? ProbedValueUInt { get; set; }

    public string? ProbedValueStr { get; set; }


    /// <summary>
    /// This item is a template
    /// </summary>
    public bool? Template { get; set; }

    /// <summary>
    /// If it's "template", this indicates from who (parent)
    /// </summary>
    public Guid? From { get; set; }

    
    /// <summary>
    /// Calc expression to numeric values
    /// </summary>
    public string? Calc { get; set; }

    /// <summary>
    /// Should read as hexstring instead of string
    /// </summary>
    public bool? AsHex { get; set; }


    public string? Description { get; set; }
}


public static class OLT_Host_ItemExtensions
{
    public static readonly string[] ValidActions = new []{ "ping", "snmpget", "snmpwalk" };

    public const int MinInterval = 1;
    public const int MaxInterval = 86400; //1 day 

    public const int MinHistoryFor = 1;
    public const int MaxHistoryFor = 2628000; //5 years

    public const int MinMaintainFor = 0; //0 is for not maintain at failed read 
    public const int MaxMaintainFor = 2628000; //5 years
}
