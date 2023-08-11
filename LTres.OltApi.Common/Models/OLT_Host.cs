using System;
using System.Collections.Generic;

namespace LTres.OltApi.Common.Models;

/// <summary>
/// Model of an OLT host configuration
/// </summary>
public class OLT_Host
{
    /// <summary>
    /// Identification
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the host
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// IP address or dns domain to create an endpoint to OLT's host
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Snmp community to read of
    /// </summary>
    /// <value></value>
    public string? SnmpCommunity { get; set; }

    /// <summary>
    /// What interface should use to comunicate. See the compatibility table.
    /// </summary>
    public int Interface { get; set; }
    
    /// <summary>
    /// Tags to classsify this OLT Host
    /// </summary>
    public IEnumerable<string>? tags { get; set; }
}