
using System.Net;
using LTres.OltApi.Common;
using LTres.OltApi.Communication;
using org.matheval.Functions;

namespace LTres.OltApi.CLI;

public class MenuCommunication : Menu
{
    private bool askedTelnetInfo = false;
    private IPEndPoint ipEndPoint;
    private string telnetUser = "";
    private string telnetPassword = "";

    public MenuCommunication()
    {
        Description = "-- Test communication with OLT terminal ---";
        Options.Add(new MenuOption('0', "Simple telnet connection test", SimpleTelnetConnectionTest));
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


    private async Task<ICommunicationChannel> GetCommunicationChannel()
    {
        AskTelnetInfo();

        var channel = new TelnetZTEChannel();
        var logged = await channel.Connect(ipEndPoint, telnetUser, telnetPassword);
        if (!logged)
        {
            var error = channel.LastReadErrors.First().error;
            channel.Dispose();
            throw new Exception(error);
        }

        return channel;
    }

    private async Task<bool> SimpleTelnetConnectionTest()
    {
        using var channel = await GetCommunicationChannel();

        await channel.WriteCommand("show version-running");
        var read = await channel.ReadLinesFromChannel();

        foreach(var l in read)
            Console.WriteLine(l);

        return false;
    }
}
