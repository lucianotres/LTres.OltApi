using System;
using System.Collections.Generic;

namespace LTres.Olt.Api.Common.Models;

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
    public string? SnmpCommunity { get; set; }

    /// <summary>
    /// Snmp should use bulk get?
    /// </summary>
    public bool? SnmpBulk { get; set; }

    /// <summary>
    /// Snmp version: 1, 2, 3 (not implemented yet)
    /// </summary>
    public int? SnmpVersion { get; set; }

    public int? GetTimeout { get; set; }

    /// <summary>
    /// What interface should use to comunicate. See the compatibility table.
    /// </summary>
    public int Interface { get; set; }

    /// <summary>
    /// Ignore OLT Hosts when setted to false
    /// </summary>
    public bool? Active { get; set; }


    
    /// <summary>
    /// Tags to classsify this OLT Host
    /// </summary>
    public IEnumerable<string>? tags { get; set; }


    /// <summary>
    /// Onu info items references
    /// </summary>
    public OLT_Host_OnuRef? OnuRef { get; set; }

    /// <summary>
    /// Configuration to CLI access
    /// </summary> <summary>
    public OLT_Host_CLIconfig? CLI { get; set; }
}
