
using System.Net;
using System.Text.RegularExpressions;
using Lextm.SharpSnmpLib.Security;
using LTres.OltApi.Common;
using LTres.OltApi.Communication;
using LTres.OltApi.Core.Tools;
using Microsoft.Extensions.Logging;
using org.matheval.Functions;

namespace LTres.OltApi.CLI;

public class MenuCommunication : Menu
{
    private ILoggerFactory logger;
    private bool askedTelnetInfo = false;
    private IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.None, 0);
    private string telnetUser = "";
    private string telnetPassword = "";

    public MenuCommunication()
    {
        logger = new LoggerFactory();

        Description = "-- Test communication with OLT terminal ---";
        Options.Add(new MenuOption('0', "Simple telnet connection test", SimpleTelnetConnectionTest));
        Options.Add(new MenuOption('1', "List all ONUs RX from pon", ListPonOnuRx));
        Options.Add(new MenuOption('2', "List all ONUs info", ListPonOnuInfo));
        Options.Add(new MenuOption('3', "List all ONUs unconfigured", ListUnconfiguredOnu));
        Options.Add(new MenuOption('4', "Get RX from specific ONU", GetOnuRx));
        Options.Add(new MenuOption('5', "Show ONU detail", GetOnuDetail));
        Options.Add(new MenuOption('6', "Remove ONU", RemoveOnu));
        Options.Add(new MenuOption('7', "Authorize ONU", AuthorizeOnu));
        Options.Add(new MenuOption('r', "to return"));
    }


    private void AskTelnetInfo()
    {
        if (askedTelnetInfo)
            return;

        askedTelnetInfo = true;

        var environmentVarIP = Environment.GetEnvironmentVariable("LTRES_HOSTIP");
        if (!string.IsNullOrEmpty(environmentVarIP) && IPAddress.TryParse(environmentVarIP, out IPAddress? environmentIPAddress))
            ipEndPoint = new IPEndPoint(environmentIPAddress, 23);

        var environmentTelnetUser = Environment.GetEnvironmentVariable("LTRES_TELNET_USR");
        if (!string.IsNullOrWhiteSpace(environmentTelnetUser))
            telnetUser = environmentTelnetUser;

        var environmentTelnetPassword = Environment.GetEnvironmentVariable("LTRES_TELNET_PSS");
        if (!string.IsNullOrWhiteSpace(environmentTelnetPassword))
            telnetPassword = environmentTelnetPassword;

        Console.Write($"Define host IP Address [{ipEndPoint.Address}]: ");
        var strIP = Console.ReadLine();

        if (!string.IsNullOrEmpty(strIP) && IPAddress.TryParse(strIP, out IPAddress? address))
            ipEndPoint = new IPEndPoint(address, 23);

        Console.Write($"Define telnet user [{telnetUser}]: ");
        var strTelnetUser = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(strTelnetUser))
            telnetUser = strTelnetUser;

        Console.Write($"Define telnet password [{telnetPassword}]: ");
        var strTelnetPassword = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(strTelnetPassword))
            telnetPassword = strTelnetPassword;
    }


    private async Task<ClientZteCLI> GetCommunicationChannel()
    {
        AskTelnetInfo();

        var channel = new ClientZteCLI(new TelnetZTEChannel(), logger.CreateLogger<ClientZteCLI>())
        {
            HostEndPoint = ipEndPoint,
            Username = telnetUser,
            Password = telnetPassword
        };

        var logged = await channel.Connect();
        if (!logged)
        {
            var error = channel.LastError.error;
            channel.Dispose();
            throw new Exception(error);
        }

        return channel;
    }

    private async Task<bool> SimpleTelnetConnectionTest()
    {
        using var channel = await GetCommunicationChannel();

        await channel.GoToBeginning();
        return false;
    }

    private (int olt, int slot, int port, int id) AskOnuID()
    {
        Console.Write($"Inform the id for ONU <1/1/1:1>: ");
        var userInput = Console.ReadLine() ?? "";

        var expressionId = new Regex(@"^([0-9]{1,3})\/([0-9]{1,3})\/([0-9]{1,3}):{0,1}([0-9]{0,3})");
        var match = expressionId.Match(userInput.Trim());

        if (match.Success)
        {
            var id = match.Groups.Count > 4 && !string.IsNullOrWhiteSpace(match.Groups[4].Value) ? int.Parse(match.Groups[4].Value) : 0;
            return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), id);
        }
        else
            return (0, 0, 0, 0);
    }

    private void CommonConsoleOutputLines(IEnumerable<string>? read, ClientZteCLI channel)
    {
        if (read == null)
            Console.WriteLine(channel.LastError.error);
        else
        {
            foreach (var l in read)
                Console.WriteLine(l);
        }
    }

    private async Task<bool> ListPonOnuRx()
    {
        using var channel = await GetCommunicationChannel();

        var ask = AskOnuID();
        if (ask.port > 0)
            CommonConsoleOutputLines(await channel.ShowPowerOnuRx(ask.olt, ask.slot, ask.port), channel);

        return false;
    }

    private async Task<bool> GetOnuRx()
    {
        using var channel = await GetCommunicationChannel();

        var ask = AskOnuID();
        if (ask.id > 0)
        {
            var dbmRx = await channel.GetPowerOnuRx(ask.olt, ask.slot, ask.port, ask.id);
            Console.WriteLine($"rx -->> {dbmRx} dbm");
        }

        return false;
    }

    private async Task<bool> GetOnuDetail()
    {
        using var channel = await GetCommunicationChannel();

        var ask = AskOnuID();
        if (ask.id > 0)
        {
            Console.WriteLine("---detail");
            CommonConsoleOutputLines(await channel.ShowGponOnuDetail(ask.olt, ask.slot, ask.port, ask.id), channel);

            Console.WriteLine("---remote pon");
            CommonConsoleOutputLines(await channel.ShowGponOnuRemoteInterfacePon(ask.olt, ask.slot, ask.port, ask.id), channel);

            Console.WriteLine("---remote eth");
            CommonConsoleOutputLines(await channel.ShowGponOnuRemoteInterfaceEth(ask.olt, ask.slot, ask.port, ask.id), channel);

            Console.WriteLine("---remote version");
            CommonConsoleOutputLines(await channel.ShowGponOnuRemoteVersion(ask.olt, ask.slot, ask.port, ask.id), channel);

            Console.WriteLine("---mac addresses");
            CommonConsoleOutputLines(await channel.ShowMacOnuInfo(ask.olt, ask.slot, ask.port, ask.id), channel);
        }

        return false;
    }

    private async Task<bool> ListPonOnuInfo()
    {
        using var channel = await GetCommunicationChannel();

        var ask = AskOnuID();
        if (ask.port > 0)
        {
            var read = await channel.ShowGponOnuBaseInfo(ask.olt, ask.slot, ask.port);
            if (read == null)
                Console.WriteLine(channel.LastError.error);
            else
            {
                foreach (var l in read)
                    Console.WriteLine($"id {l.id,3} | {l.tp,10} | {l.mode,4} | {l.auth,14} | {l.state}");
            }
        }

        return false;
    }

    private async Task<bool> ListUnconfiguredOnu()
    {
        using var channel = await GetCommunicationChannel();

        var read = await channel.ShowGponOnuUncfg();
        if (read == null)
            Console.WriteLine(channel.LastError.error);
        else
        {
            foreach (var l in read)
                Console.WriteLine($"gpon-olt_{l.olt}/{l.slot}/{l.port} | {l.id,-3} | {l.sn,14} | {l.state}");
        }

        return false;
    }

    private async Task<bool> RemoveOnu()
    {
        using var channel = await GetCommunicationChannel();

        Console.WriteLine("WARNING! The command will remove the ONU's configuration and itself from OLT!!!");
        var ask = AskOnuID();
        if (ask.id > 0)
        {
            var result = await channel.RemoveONU(ask.olt, ask.slot, ask.port, ask.id, true);
            Console.WriteLine($"{result.ok} -->> {result.msg}");
        }

        return false;
    }

    private async Task<bool> AuthorizeOnu()
    {
        using var channel = await GetCommunicationChannel();

        var ask = AskOnuID();
        if (ask.port > 0)
        {
            int tried = 0;
            string? onuType;
            do
            {
                Console.Write("Please inform a onu type to register: ");
                onuType = Console.ReadLine()?.Trim();
            } while (string.IsNullOrEmpty(onuType) && (tried++ <= 3));

            if (string.IsNullOrEmpty(onuType))
                return false;

            tried = 0;
            string? onuSN;
            do
            {
                Console.Write("Please inform a onu SN to register: ");
                onuSN = Console.ReadLine()?.Trim();
            } while (string.IsNullOrEmpty(onuSN) && (tried++ <= 3));

            if (string.IsNullOrEmpty(onuSN))
                return false;

            var firstUnusedIndex = await channel.GetFirstUnusedOnuIndex(ask.olt, ask.slot, ask.port);
            if (!firstUnusedIndex.HasValue)
            {
                Console.WriteLine(" Failed to get an unused index!");
                return false;
            }

            var result = await channel.AuthorizeONU(ask.olt, ask.slot, ask.port, firstUnusedIndex.Value, onuType, onuSN, true);
            Console.WriteLine($"{result.ok} -->> {result.msg}");
        }

        return false;
    }

}
