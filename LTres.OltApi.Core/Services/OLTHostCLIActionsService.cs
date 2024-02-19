using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using LTres.OltApi.Common;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Services;

public class OLTHostCLIActionsService : IOLTHostCLIActionsService
{
    const string CLIType_ZTE_Telnet = "zte-telnet";

    private readonly IDbOLTHost _db;
    private readonly ILogger _log;
    private readonly IServiceProvider _serviceProvider;
    
    public OLTHostCLIActionsService(ILogger<OLTHostCLIActionsService> logger, IDbOLTHost dbOLTHost, IServiceProvider serviceProvider)
    {
        _log = logger;
        _db = dbOLTHost;
        _serviceProvider = serviceProvider;
    }

    private async Task<ClientZteCLI> CreateCommunicationChannel(Guid oltId)
    {
        var oltQueryResult = await _db.ListOLTHosts(1, 0, null, oltId);
        if (!oltQueryResult.Any())
            throw new KeyNotFoundException("OLT not found!");

        var oltInfo = oltQueryResult.First();
        if (oltInfo.CLI == null)
            throw new KeyNotFoundException("CLI configuration for OLT not found!");

        //need to figure out a way to generalize this
        if (oltInfo.CLI.Type == CLIType_ZTE_Telnet)
        {
            var client = _serviceProvider.GetService<ClientZteCLI>();
            if (client == null)
                throw new Exception($"Cannot create a {oltInfo.CLI.Type} communication!");
            
            client.HostEndPoint = IPEndPoint.Parse(oltInfo.Host);
            client.HostEndPoint.Port = oltInfo.CLI.Port.GetValueOrDefault(23);

            client.Username = oltInfo.CLI.Username;
            client.Password = oltInfo.CLI.Password;

            if (!await client.Connect())
                throw new Exception($"Cannot connect to {oltInfo.CLI.Type}!");

            return client;
        }
        else
            throw new Exception($"OLT CLI type {oltInfo.CLI.Type} not implemented yet!");
    }

    public async Task<IEnumerable<string>> GetONUInfo(Guid oltId, int olt, int slot, int port, int id)
    {
        using var cli = await CreateCommunicationChannel(oltId);

        var result = await cli.ShowGponOnuDetail(olt, slot, port, id);
        if (result == null)
            return new string[] { cli.LastError.error };
        else
            return result;
    }

    public async Task<IEnumerable<string>> GetONUInterfaces(Guid oltId, int olt, int slot, int port, int id)
    {
        using var cli = await CreateCommunicationChannel(oltId);

        var finalResult = new List<string>();

        var result = await cli.ShowGponOnuRemoteInterfacePon(olt, slot, port, id);
        if (result == null)
            finalResult.Add(cli.LastError.error);
        else
            finalResult.AddRange(result);

        finalResult.Add("");

        result = await cli.ShowGponOnuRemoteInterfaceEth(olt, slot, port, id);
        if (result == null)
            finalResult.Add(cli.LastError.error);
        else
            finalResult.AddRange(result);

        return finalResult;
    }

    public async Task<IEnumerable<string>> GetONUVersion(Guid oltId, int olt, int slot, int port, int id)
    {
        using var cli = await CreateCommunicationChannel(oltId);

        var result = await cli.ShowGponOnuRemoteVersion(olt, slot, port, id);
        if (result == null)
            return new string[] { cli.LastError.error };
        else
            return result;
    }

    public async Task<IEnumerable<string>> GetONUmac(Guid oltId, int olt, int slot, int port, int id)
    {
        using var cli = await CreateCommunicationChannel(oltId);

        var result = await cli.ShowMacOnuInfo(olt, slot, port, id);
        if (result == null)
            return new string[] { cli.LastError.error };
        else
            return result;
    }

    public async Task<int?> GetFirstUnusedOnuIndex(Guid oltId, int olt, int slot, int port)
    {
        using var cli = await CreateCommunicationChannel(oltId);

        return await cli.GetFirstUnusedOnuIndex(olt, slot, port);
    }
}
