
using System.Net;
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
    private IPEndPoint ipEndPoint;
    private string telnetUser = "";
    private string telnetPassword = "";

    public MenuCommunication()
    {
        logger = new LoggerFactory();

        Description = "-- Test communication with OLT terminal ---";
        Options.Add(new MenuOption('0', "Simple telnet connection test", SimpleTelnetConnectionTest));
        Options.Add(new MenuOption('1', "List all ONUs RX from pon", ListPonOnuRx));
        Options.Add(new MenuOption('2', "Get RX from specific ONU", GetOnuRx));
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


    private async Task<CommunicationChannel> GetCommunicationChannel()
    {
        AskTelnetInfo();

        var channel = new CommunicationChannel(new TelnetZTEChannel(), logger.CreateLogger<CommunicationChannel>())
        {
            Address = ipEndPoint,
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

    private async Task<bool> ListPonOnuRx()
    {
        using var channel = await GetCommunicationChannel();

        Console.Write($"Complete pon ID gpon-olt_1/1/x: ");
        var strPon = Console.ReadLine();
        if (string.IsNullOrEmpty(strPon) || !int.TryParse(strPon, out int pon))
            return false;

        var read = await channel.GetPowerOnuRx(1, 1, pon);

        if (read == null)
            Console.WriteLine(channel.LastError.error);
        else
        {
            foreach (var l in read)
                Console.WriteLine(l);
        }

        return false;
    }

    private async Task<bool> GetOnuRx()
    {
        using var channel = await GetCommunicationChannel();

        Console.Write($"Complete gpon-onu_1/1/x: ");
        var strPon = Console.ReadLine();
        if (string.IsNullOrEmpty(strPon) || !int.TryParse(strPon, out int pon))
            return false;

        Console.Write($"And the onu id gpon-nu_1/1/{pon}:x: ");
        var strID = Console.ReadLine();
        if (string.IsNullOrEmpty(strID) || !int.TryParse(strID, out int id))
            return false;

        var read = await channel.GetPowerOnuRx(1, 1, pon, id);

        if (read == null)
            Console.WriteLine(channel.LastError.error);
        else
        {
            foreach (var l in read)
                Console.WriteLine(l);
        }

        return false;
    }
}
