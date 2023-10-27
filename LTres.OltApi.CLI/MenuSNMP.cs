using System.Net;
using LTres.OltApi.Common;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Snmp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.CLI;

public class MenuSNMP : Menu
{
    private bool askedHostInfo;
    private IPEndPoint ipEndPoint;
    private string snmpCommunity = "public";
    private ILoggerFactory logger;

    public MenuSNMP()
    {
        askedHostInfo = false;
        ipEndPoint = new IPEndPoint(IPAddress.Loopback, 161);

        logger = new LoggerFactory();

        Description = "-- SNMP worker tests ---";
        Options.Add(new MenuOption('0', "Change implementation", ChangeCurrentlyImplementation));
        Options.Add(new MenuOption('1', "Try to get sysName", SnmpGetSysName));
        Options.Add(new MenuOption('2', "Try to get sysUpTime", SnmpGetSysUpTime));
        Options.Add(new MenuOption('3', "Try to walk ifDescr", SnmpWalkIfDescr));
        Options.Add(new MenuOption('4', "Try to walk ZTE ONUs", SnmpWalkZteOnu));
        Options.Add(new MenuOption('r', "to return"));
    }

    private void AskHostInfo()
    {
        if (askedHostInfo)
            return;

        askedHostInfo = true;
        var environmentVarIP = Environment.GetEnvironmentVariable("LTRES_HOSTIP");
        if (!string.IsNullOrEmpty(environmentVarIP) && IPAddress.TryParse(environmentVarIP, out IPAddress? environmentIPAddress))
            ipEndPoint = new IPEndPoint(environmentIPAddress, 161);

        var environmentSnmpCommunity = Environment.GetEnvironmentVariable("LTRES_SNMPCOMMUNITY");
        if (!string.IsNullOrWhiteSpace(environmentSnmpCommunity))
            snmpCommunity = environmentSnmpCommunity;

        Console.Write($"Define host IP Address [{ipEndPoint.Address}]: ");
        var strIP = Console.ReadLine();

        if (!string.IsNullOrEmpty(strIP) && IPAddress.TryParse(strIP, out IPAddress? address))
            ipEndPoint = new IPEndPoint(address, 161);

        Console.Write($"Define snmp community [{snmpCommunity}]: ");
        var strSnmpCommunity = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(strSnmpCommunity))
            snmpCommunity = strSnmpCommunity;
    }

    private int implementationToUse = 2;

    private Task<bool> ChangeCurrentlyImplementation()
    {
        Console.Write($"Currently implementation: {implementationToUse}, to change enter a positive integer value: ");
        var strNewValue = Console.ReadLine();

        if (!string.IsNullOrEmpty(strNewValue) && int.TryParse(strNewValue, out int newImplementation) && newImplementation > 0)
        {
            if (newImplementation <= 2)
            {
                Console.WriteLine($"Changed to {newImplementation}");
                implementationToUse = newImplementation;
            }
            else
                Console.WriteLine("Not valid implementation option!");
        }

        return Task.FromResult(false);
    }

    private IWorkerActionSnmpGet GetSnmpGetImplementation() =>
        implementationToUse == 2 ? new WorkSnmpGetAction2() : new WorkSnmpGetAction();

    private IWorkerActionSnmpWalk GetSnmpWalkImplementation() =>
        implementationToUse == 2 ?
            new WorkSnmpWalkAction2(logger.CreateLogger<WorkSnmpWalkAction>()) :
            new WorkSnmpWalkAction(logger.CreateLogger<WorkSnmpWalkAction>());


    private async Task<bool> SnmpGetSysName()
    {
        AskHostInfo();

        var workSnmpGetAction = GetSnmpGetImplementation();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            SnmpVersion = 2,
            Action = "snmpget",
            ItemKey = "1.3.6.1.2.1.1.5.0"
        };

        var workResult = await workSnmpGetAction.Execute(probeInfo, CancellationToken.None);

        Console.WriteLine($"sysName | {(workResult.Success ? "ok" : workResult.FailMessage)} -> {workResult.ValueStr}");
        return false;
    }

    private async Task<bool> SnmpGetSysUpTime()
    {
        AskHostInfo();

        var workSnmpGetAction = GetSnmpGetImplementation();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            Action = "snmpget",
            ItemKey = "1.3.6.1.2.1.1.3.0"
        };

        var workResult = await workSnmpGetAction.Execute(probeInfo, CancellationToken.None);

        Console.WriteLine($"sysUpTime | {(workResult.Success ? "ok" : workResult.FailMessage)} -> {TimeSpan.FromMilliseconds((double)workResult.ValueUInt.GetValueOrDefault() * 10)}");
        return false;
    }

    private async Task<bool> SnmpWalkIfDescr()
    {
        AskHostInfo();

        var workSnmpGetAction = GetSnmpWalkImplementation();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            SnmpVersion = 1,
            SnmpBulk = false,
            Action = "snmpwalk",
            ItemKey = "1.3.6.1.2.1.2.2.1.2"
        };

        var datetimeStarted = DateTime.Now;

        var workResult = await workSnmpGetAction.Execute(probeInfo, CancellationToken.None);
        if (workResult.Success)
        {
            var timespanGotResponse = DateTime.Now.Subtract(datetimeStarted);

            if (workResult.Values != null)
            {
                foreach (var v in workResult.Values)
                    Console.WriteLine($"{v.Key}: {v.ValueStr}");
            }

            Console.WriteLine($"ifDescr [{(workResult.Values?.Count()).GetValueOrDefault()}] in {timespanGotResponse}");
        }

        return false;
    }

    private async Task<bool> SnmpWalkZteOnu()
    {
        AskHostInfo();

        var workSnmpGetAction = GetSnmpWalkImplementation();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            SnmpVersion = 2,
            SnmpBulk = true,
            Action = "snmpwalk",
            ItemKey = "1.3.6.1.4.1.3902.1012.3.28.1.1.2"
        };

        var datetimeStarted = DateTime.Now;

        var workResult = await workSnmpGetAction.Execute(probeInfo, CancellationToken.None);
        if (workResult.Success)
        {
            var timespanGotResponse = DateTime.Now.Subtract(datetimeStarted);

            if (workResult.Values != null)
            {
                foreach (var v in workResult.Values)
                    Console.WriteLine($"{v.Key}: {v.ValueStr}");
            }

            Console.WriteLine($"ZTE Onu Names [{(workResult.Values?.Count()).GetValueOrDefault()}] in {timespanGotResponse}");
        }

        return false;
    }
}
