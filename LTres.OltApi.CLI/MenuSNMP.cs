using System.Net;
using LTres.OltApi.Common.Models;
using LTres.OltApi.Snmp;

namespace LTres.OltApi.CLI;

public class MenuSNMP : Menu
{
    private bool askedHostInfo;
    private IPEndPoint ipEndPoint;
    private string snmpCommunity = "public";

    public MenuSNMP()
    {
        askedHostInfo = false;
        ipEndPoint = new IPEndPoint(IPAddress.Loopback, 161);

        Description = "-- SNMP worker tests ---";
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

    private async Task<bool> SnmpGetSysName()
    {
        AskHostInfo();

        var workSnmpGetAction = new WorkSnmpGetAction();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            Action = "snmpget",
            ItemKey = "1.3.6.1.2.1.1.5.0"
        };

        var workResult = await workSnmpGetAction.Execute(probeInfo);

        Console.WriteLine($"sysName | {(workResult.Success ? "ok" : workResult.FailMessage)} -> {workResult.ValueStr}");
        return false;
    }

    private async Task<bool> SnmpGetSysUpTime()
    {
        AskHostInfo();

        var workSnmpGetAction = new WorkSnmpGetAction();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            Action = "snmpget",
            ItemKey = "1.3.6.1.2.1.1.3.0"
        };

        var workResult = await workSnmpGetAction.Execute(probeInfo);

        Console.WriteLine($"sysUpTime | {(workResult.Success ? "ok" : workResult.FailMessage)} -> {TimeSpan.FromMilliseconds((double)workResult.ValueUInt * 10)}");
        return false;
    }

    private async Task<bool> SnmpWalkIfDescr()
    {
        AskHostInfo();

        var workSnmpGetAction = new WorkSnmpWalkAction();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            SnmpVersion = 2,
            Action = "snmpwalk",
            ItemKey = "1.3.6.1.2.1.2.2.1.2"
        };

        var datetimeStarted = DateTime.Now;

        var workResult = await workSnmpGetAction.Execute(probeInfo);
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

        var workSnmpGetAction = new WorkSnmpWalkAction();
        var probeInfo = new WorkProbeInfo()
        {
            Id = Guid.NewGuid(),
            Host = ipEndPoint,
            SnmpCommunity = snmpCommunity,
            SnmpVersion = 2,
            Action = "snmpwalk",
            ItemKey = "1.3.6.1.4.1.3902.1012.3.28.1.1.2"
        };

        var datetimeStarted = DateTime.Now;

        var workResult = await workSnmpGetAction.Execute(probeInfo);
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
