using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using LTres.OltApi.Common;
using Microsoft.Extensions.Logging;
using org.matheval.Functions;

namespace LTres.OltApi.Core.Tools;

public class ClientZteCLI : IDisposable
{
    private ICommunicationChannel channel;
    private ILogger logger;

    public ClientZteCLI(ICommunicationChannel channel, ILogger<ClientZteCLI> logger)
    {
        Disposed = false;

        this.channel = channel;
        this.logger = logger;
    }

    public bool Disposed { get; private set; } = false;

    public void Dispose()
    {
        if (!Disposed)
        {
            Disposed = true;
            channel.Dispose();
        }
    }


    public IPEndPoint HostEndPoint { get => channel.HostEndPoint; set => channel.HostEndPoint = value; }
    public string? Username { get => channel.Username; set => channel.Username = value; }
    public string? Password { get => channel.Password; set => channel.Password = value; }

    public Regex ExpressionToCheckElevatedMode { get; set; } = new Regex(@"^[\w-]+#");
    public Regex ExpressionToCheckConfigurationMode { get; set; } = new Regex(@"^[\w-]+\(config\)#");

    public (int code, string error) LastError { get => channel.LastReadErrorCount == 0 ? (0, string.Empty) : channel.LastReadErrors.Last(); }

    public async Task<bool> Connect()
    {
        if (Disposed)
            throw new ObjectDisposedException(nameof(channel));

        return await channel.Connect();
    }

    private async Task<IEnumerable<string>?> ReadLinesFromChannelWithMore(bool trimEmptyLines = false)
    {
        var finalLines = new List<string>();
        var readLines = await channel.ReadLinesFromChannel().ConfigureAwait(false);
        finalLines.AddRange(readLines);

        do
        {
            if (channel.LastReadErrorCount > 0)
                return null;

            //every read of lines with ending "--More--"
            //will give <space> key to get more lines
            //removing what as asked by communication channel
            //'til have no more at end

            if (readLines.Any(j => j.Contains("--More--", StringComparison.Ordinal)))
            {
                await channel.WriteByte(32).ConfigureAwait(false);
                readLines = await channel.ReadLinesFromChannel().ConfigureAwait(false);

                if (channel.LastReadBackspacesCount > 0)
                    channel.LastReadBackspacesCount -= finalLines.Last().Length;

                finalLines.RemoveAt(finalLines.Count - 1);
                finalLines.AddRange(readLines.Select(s =>
                {
                    if (channel.LastReadBackspacesCount > 0)
                    {
                        var h = channel.LastReadBackspacesCount;
                        channel.LastReadBackspacesCount = 0;
                        return new string(s.Skip(h).ToArray());
                    }
                    else
                        return s;
                }));
            }
            else
                break;

        } while (true);

        if (finalLines.Count == 0)
            return finalLines;
        else if (trimEmptyLines)
        {
            var firstNotEmpty = finalLines.FindIndex(s => !string.IsNullOrWhiteSpace(s));
            if (firstNotEmpty < 0)
                firstNotEmpty = 0;

            var lastNotEmpty = finalLines
                .Skip(firstNotEmpty)
                .Select((value, index) => new { value, index })
                .SkipLast(1)
                .Last(w => !string.IsNullOrWhiteSpace(w.value))
                .index;

            if (lastNotEmpty > 0)
                return finalLines.Skip(firstNotEmpty).Take(lastNotEmpty + 1).ToList();
            else
                return finalLines.Skip(firstNotEmpty).Take(finalLines.Count - firstNotEmpty - 1).ToList();
        }
        else
            return finalLines.Take(finalLines.Count - 1).ToList();
    }

    public async Task GoToBeginning()
    {
        //write ! go to start
        await channel.WriteCommand("!");
        await channel.ReadLinesFromChannel();
    }

    /// <summary>
    /// Go to elevated mode (if's not yet)
    /// </summary>
    public async Task<bool> ElevatedMode()
    {
        //write ! to get new line
        await channel.WriteCommand("!");

        //returs if already at elevated
        var readLines = await channel.ReadLinesFromChannel();
        if (readLines.Any() && ExpressionToCheckElevatedMode.IsMatch(readLines.Last()))
            return true;

        //go to elevated
        await channel.WriteCommand("enable");
        readLines = await channel.ReadLinesFromChannel();
        return readLines.Any() && ExpressionToCheckElevatedMode.IsMatch(readLines.Last());
    }

    /// <summary>
    /// Go to configuration mode
    /// </summary>
    public async Task<bool> ConfigurationMode(bool elevateBefore = true)
    {
        if (elevateBefore)
            await ElevatedMode();

        //write ! to get new line
        await channel.WriteCommand("!");

        //returs if already at configuration mode
        var readLines = await channel.ReadLinesFromChannel();
        if (readLines.Any() && ExpressionToCheckConfigurationMode.IsMatch(readLines.Last()))
            return true;

        //go to elevated
        await channel.WriteCommand("configure terminal");
        readLines = await channel.ReadLinesFromChannel();
        return readLines.Any() && ExpressionToCheckConfigurationMode.IsMatch(readLines.Last());
    }

    /// <summary>
    /// Exit configuration mode
    /// </summary>
    public async Task<bool> ExitConfigurationMode(bool elevateBefore = true)
    {
        //write ! to get new line
        await channel.WriteCommand("!");

        //check if it's at configuration mode
        var readLines = await channel.ReadLinesFromChannel();
        if (readLines.Any() && ExpressionToCheckConfigurationMode.IsMatch(readLines.Last()))
        {
            //go to elevated
            await channel.WriteCommand("exit");
            readLines = await channel.ReadLinesFromChannel();

            return readLines.Any() && !ExpressionToCheckConfigurationMode.IsMatch(readLines.Last());
        }
        else
            return true;
    }

    /// <summary>
    /// Run command to save configurations into flash
    /// </summary>
    public async Task<bool> WriteConfig()
    {
        if (!await ExitConfigurationMode())
            return false;

        await channel.WriteCommand("write");

        var startedAt = DateTime.Now;
        do
        {
            var readLines = await channel.ReadLinesFromChannel();
            if (channel.LastReadErrorCount > 0)
                return false;

            if (readLines.Any(w => w.Contains("[OK]", StringComparison.InvariantCultureIgnoreCase)))
                return true;
        }
        while (DateTime.Now.Subtract(startedAt).TotalSeconds < 60);

        return false;
    }

    /// <summary>
    /// Get a list of onu rx information
    /// </summary>
    public async Task<IEnumerable<string>?> ShowPowerOnuRx(int olt, int slot, int port, int? id = null)
    {
        await GoToBeginning();

        if (id == null)
            await channel.WriteCommand($"show pon power onu-rx gpon-olt_{olt}/{slot}/{port}");
        else
            await channel.WriteCommand($"show pon power onu-rx gpon-onu_{olt}/{slot}/{port}:{id}");

        return await ReadLinesFromChannelWithMore(true);
    }

    public async Task<double?> GetPowerOnuRx(int olt, int slot, int port, int id)
    {
        var readLines = await ShowPowerOnuRx(olt, slot, port, id);
        if (readLines == null)
            return null;

        var expressionValueDbm = new Regex(@"^\s*gpon-onu_([0-9]{1,3})\/([0-9]{1,3})\/([0-9]{1,3}):([0-9]{1,3})\s*(-{0,1}[0-9\.]{1,12})");

        foreach (var l in readLines)
        {
            var match = expressionValueDbm.Match(l);
            if (match.Success)
                return Double.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
        }
        return null;
    }

    /// <summary>
    /// Show onu detail info
    /// </summary>
    public async Task<IEnumerable<string>?> ShowGponOnuDetail(int olt, int slot, int port, int id)
    {
        await GoToBeginning();

        await channel.WriteCommand($"show gpon onu detail-info gpon-onu_{olt}/{slot}/{port}:{id}");

        return await ReadLinesFromChannelWithMore(true);
    }

    /// <summary>
    /// Show remote onu interface pon information
    /// </summary>
    public async Task<IEnumerable<string>?> ShowGponOnuRemoteInterfacePon(int olt, int slot, int port, int id)
    {
        await GoToBeginning();

        await channel.WriteCommand($"show gpon remote-onu interface pon gpon-onu_{olt}/{slot}/{port}:{id}");

        return await ReadLinesFromChannelWithMore(true);
    }

    /// <summary>
    /// Show remote onu interface eth information
    /// </summary>
    public async Task<IEnumerable<string>?> ShowGponOnuRemoteInterfaceEth(int olt, int slot, int port, int id)
    {
        await GoToBeginning();

        await channel.WriteCommand($"show gpon remote-onu interface eth gpon-onu_{olt}/{slot}/{port}:{id}");

        return await ReadLinesFromChannelWithMore(true);
    }

    /// <summary>
    /// Show remote onu version
    /// </summary>
    public async Task<IEnumerable<string>?> ShowGponOnuRemoteVersion(int olt, int slot, int port, int id)
    {
        await GoToBeginning();

        await channel.WriteCommand($"show remote-unit information gpon-olt_{olt}/{slot}/{port} {id}");

        return await ReadLinesFromChannelWithMore(true);
    }

    /// <summary>
    /// Show mac over that onu
    /// </summary>
    public async Task<IEnumerable<string>?> ShowMacOnuInfo(int olt, int slot, int port, int id)
    {
        await GoToBeginning();

        await channel.WriteCommand($"show mac gpon onu gpon-onu_{olt}/{slot}/{port}:{id}");

        return await ReadLinesFromChannelWithMore(true);
    }

    /// <summary>
    /// Search for unconfigured ONUs
    /// </summary>
    /// <param name="olt">Interface olt number, zero to look at all</param>
    /// <param name="slot">Interface slot number, zero to look at all</param>
    /// <param name="port">Interface port number, zero to look at all</param>
    /// <returns></returns>
    public async Task<IEnumerable<(int olt, int slot, int port, int id, string sn, string state)>?> ShowGponOnuUncfg(int olt = 0, int slot = 0, int port = 0)
    {
        await GoToBeginning();

        //get configured onu list of gpon interface
        if (olt <= 0 || slot <= 0 || port <= 0)
            await channel.WriteCommand($"show gpon onu uncfg");
        else
            await channel.WriteCommand($"show gpon onu uncfg gpon-olt_{olt}/{slot}/{port}");

        var readLines = await ReadLinesFromChannelWithMore();
        if (readLines == null)
            return null;

        //read values with regex
        var returnList = new List<(int olt, int slot, int port, int id, string sn, string state)>();
        var expressionLineInfo = new Regex(@"^\s*gpon-onu_([0-9]{1,3})\/([0-9]{1,3})\/([0-9]{1,3}):([0-9]{1,3}) +([\w-]+) +([\w-]+)");
        foreach (var n in readLines)
        {
            var m = expressionLineInfo.Match(n);
            if (m.Success)
                returnList.Add((
                    int.Parse(m.Groups[1].Value),
                    int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[3].Value),
                    int.Parse(m.Groups[4].Value),
                    m.Groups[5].Value,
                    m.Groups[6].Value));
        }

        return returnList;
    }

    /// <summary>
    /// Get list of all configured ONUs at GPON interface. Return null if an error occur, check lasterrors
    /// </summary>
    /// <param name="olt">Interface olt number</param>
    /// <param name="slot">Interface slot number</param>
    /// <param name="port">Interface port number</param>
    public async Task<IEnumerable<(int id, string tp, string mode, string auth, string state)>?> ShowGponOnuBaseInfo(int olt, int slot, int port)
    {
        await GoToBeginning();

        await channel.WriteCommand($"show gpon onu baseinfo gpon-olt_{olt}/{slot}/{port}");

        var readLines = await ReadLinesFromChannelWithMore();
        if (readLines == null)
            return null;

        //read values with regex
        var returnList = new List<(int id, string tp, string mode, string auth, string state)>();
        var expressionForOnuInfo = new Regex(@"^\s*gpon-onu_[0-9]{1,3}\/[0-9]{1,3}\/[0-9]{1,3}:([0-9]{1,3}) +([\w-]+) +([\w-]+) +SN:([\w-:]+) +([\w-]+)");
        foreach (var line in readLines)
        {
            var onuInfo = expressionForOnuInfo.Match(line);
            if (onuInfo.Success)
                returnList.Add((
                    int.Parse(onuInfo.Groups[1].Value),
                    onuInfo.Groups[2].Value,
                    onuInfo.Groups[3].Value,
                    onuInfo.Groups[4].Value,
                    onuInfo.Groups[5].Value));
        }

        return returnList;
    }

    /// <summary>
    /// Get first unused index at gpon interface. Return null if an error occur, check lasterrors
    /// </summary>
    /// <param name="olt">Interface olt number</param>
    /// <param name="slot">Interface slot number</param>
    /// <param name="port">Interface port number</param>
    public async Task<int?> GetFirstUnusedOnuIndex(int olt, int slot, int port)
    {
        var showGponOnuBaseInfoResult = await ShowGponOnuBaseInfo(olt, slot, port);
        
        if (showGponOnuBaseInfoResult != null)
        {
            var ordenedOnuIndexes = showGponOnuBaseInfoResult.Select(i => i.id).OrderBy(i => i).ToList();

            int index = 1;
            foreach(var i in ordenedOnuIndexes)
            {
                if (index < i)
                    break;
                index++;
            }

            return index;
        }
        else
            return null;
    }

    /// <summary>
    /// Enter in an interface configuration
    /// </summary>
    /// <param name="iface">Interface name</param>
    /// <param name="gotoConfigBefore">Enter in configuration mode before</param>
    public async Task<bool> ConfigInterface(string iface, bool gotoConfigBefore = false)
    {
        if (gotoConfigBefore)
            await ConfigurationMode();

        await GoToBeginning();

        //go to interface
        await channel.WriteCommand($"interface {iface}");
        var readLines = await channel.ReadLinesFromChannel();
        return readLines.Any() && Regex.IsMatch(readLines.Last(), @"^[\w-]+\(config-if\)#");
    }

    /// <summary>
    /// Authorize ONU by serial number. Return true or false with error message.
    /// </summary>
    /// <param name="olt">Interface olt number</param>
    /// <param name="slot">Interface slot number</param>
    /// <param name="port">Interface port number</param>
    /// <param name="id">ID for ONU at this interface</param>
    /// <param name="onuType">ONU Type</param>
    /// <param name="serialNumber">Serial Number to register</param>
    /// <param name="gotoConfigBefore">Enter in configuration mode before</param>
    public async Task<(bool ok, string msg)> AuthorizeONU(int olt, int slot, int port, int id, string onuType, string serialNumber, bool gotoConfigBefore = false)
    {
        //go to interface configuration
        var ok = await ConfigInterface($"gpon-olt_{olt}/{slot}/{port}", gotoConfigBefore);
        if (ok)
        {
            var expressionSuccess = new Regex(@"\[[Ss]uccessful\]");

            //register ONU by SN
            await channel.WriteCommand($"onu {id} type {onuType} sn {serialNumber}");
            var r = await channel.ReadLinesFromChannel();
            
            //Success?
            if (r.Any(w => expressionSuccess.IsMatch(w)))
                return (true, "OK");
            else
                return (false, channel.LastReadErrorCount == 0 ? string.Empty : channel.LastReadErrors.Last().error);
        }
        else
            return (ok, $"Failed to enter in interface gpon-olt_{olt}/{slot}/{port}!");
    }

    /// <summary>
    /// Remove an onu from interface gpon
    /// </summary>
    /// <param name="olt">Interface olt number</param>
    /// <param name="slot">Interface slot number</param>
    /// <param name="port">Interface port number</param>
    /// <param name="id">ID for ONU at this interface</param>
    /// <param name="gotoConfigBefore">Enter in configuration mode before</param>
    public async Task<(bool ok, string msg)> RemoveONU(int olt, int slot, int port, int id, bool gotoConfigBefore = false)
    {
        //go to interface configuration
        var ok = await ConfigInterface($"gpon-olt_{olt}/{slot}/{port}", gotoConfigBefore);
        if (ok)
        {
            var expressionSuccess = new Regex(@"\[[Ss]uccessful\]");

            //remove by id
            await channel.WriteCommand($"no onu {id}");
            var r = await channel.ReadLinesFromChannel();

            //Success?
            if (r.Any(w => expressionSuccess.IsMatch(w)))
                return (true, "OK");
            else
                return (false, channel.LastReadErrorCount == 0 ? string.Empty : channel.LastReadErrors.Last().error);
        }
        else
            return (ok, $"Failed to remove onu {id} at gpon-olt_{olt}/{slot}/{port}!");
    }


}
