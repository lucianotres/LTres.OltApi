using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTres.OltApi.Common.Models;

public class OltApiConfiguration
{
    public int implementationVersion { get; set; } = 2;

    public bool usingMock { get; set; } = false;

}


public static class OltApiConfigurationExtensions
{
    public static OltApiConfiguration FillFromEnvironmentVars(this OltApiConfiguration configuration)
    {
        var implementationStr = Environment.GetEnvironmentVariable("LTRES_SNMP_IMPLEMENTATION");

        if (!string.IsNullOrWhiteSpace(implementationStr) && int.TryParse(implementationStr, out int i) && i > 0 && i <= 2)
            configuration.implementationVersion = i;

        if (bool.TryParse(Environment.GetEnvironmentVariable("LTRES_MOCKING"), out bool usingMock))
            configuration.usingMock = usingMock;

        return configuration;
    }
}