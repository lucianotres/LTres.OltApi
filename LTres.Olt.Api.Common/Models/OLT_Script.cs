using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTres.Olt.Api.Common.Models;

public class OLT_Script
{
    /// <summary>
    /// Identification
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Description of script
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The script itself
    /// </summary>
    public string Script { get; set; } = string.Empty;

    /// <summary>
    /// Tags to classsify this OLT Host
    /// </summary>
    public IEnumerable<string>? tags { get; set; }
}
