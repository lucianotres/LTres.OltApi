using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using LTres.OltApi.Common;

namespace LTres.OltApi.Communication;

public class TelnetZTEChannel : ICommunicationChannel
{
    private TcpClient? telnetClient;
    private Stream? telnetStream;
    private bool telnetLoggedIn = false;
    private readonly List<(int code, string error)> lastReadErrors = new();
    private readonly Regex regexErrorDetect = new(@"^%Error ([0-9]{1,11})\: (.*)$");

    public int DelayToStartRead { get; set; } = 300;
    public Regex ExpressionForToWaitCommand { get; set; } = new Regex(@"^[\w-]+(\(config\)#|#)", RegexOptions.Multiline);


    public async Task<bool> Connect(IPEndPoint ipEndPoint, string? username = null, string? password = null)
    {
        await Disconnect();

        //starts new one
        try
        {
            telnetClient = new TcpClient
            {
                ReceiveTimeout = 700
            };

            await telnetClient.ConnectAsync(ipEndPoint);

            telnetStream = telnetClient.GetStream();
        }
        catch
        {
            telnetClient?.Dispose();
            throw;
        }

        //if connected, start shell
        if (telnetClient != null && telnetClient.Connected)
        {
            //read welcome message
            var rLines = await ReadLinesTelnet(3, false, new Regex("[Uu]sername"));

            //asking for username?
            if (rLines.Any(s => s.Contains("username", StringComparison.InvariantCultureIgnoreCase)))
            {
                await WriteCommand(username ?? "", true, false);
                rLines = await ReadLinesTelnet(3, false, new Regex("[Pp]assword"));

                //now asking for password?
                if (rLines.Any(s => s.Contains("password", StringComparison.InvariantCultureIgnoreCase)))
                {
                    await WriteCommand(password ?? "", true, false);
                    await ReadLinesTelnet(3, false);
                }
            }

            telnetLoggedIn = LastReadErrorCount == 0;
        }
        else
            telnetLoggedIn = false;

        return telnetLoggedIn;
    }

    public async Task Disconnect() => await Task.Run(Dispose).ConfigureAwait(false);

    public void Dispose()
    {
        telnetLoggedIn = false;

        if (telnetClient != null)
        {
            try { telnetClient.Close(); }
            catch { }
            try { telnetClient.Dispose(); }
            catch { }
        }

        if (telnetStream != null)
        {
            try { telnetStream.Dispose(); }
            catch { }
        }
    }

    public int ToReadAvailable { get => telnetClient == null ? 0 : telnetClient.Available; }

    public async Task<int> ReadBuffer(byte[] buffer, int offset, int count) =>
        await ReadBuffer(buffer, offset, count, CancellationToken.None).ConfigureAwait(false);

    public async Task<int> ReadBuffer(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (telnetClient == null || telnetStream == null)
            throw new Exception("Not connected yet to read!");

        return await telnetStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IEnumerable<string>> ReadLinesTelnet(int seconds = 5,
                                                            bool startDelay = true,
                                                            Regex? regexDetectWaitLine = null,
                                                            CancellationTokenSource? cancellationTokenSource = null)
    {
        lastReadErrors.Clear();

        if (startDelay)
            await Task.Delay(DelayToStartRead);

        //read all response
        var buffer = new byte[1024];
        var stringBuilder = new StringBuilder();
        int tried = 0;
        do
        {
            //if no data yet..
            if (ToReadAvailable == 0)
            {
                //.. await a delay to read
                await Task.Delay(200);
                //if nothing.. wait for x times
                if (ToReadAvailable == 0)
                {
                    tried++;
                    if (tried >= 5)
                        break;
                    else
                        continue;
                }
            }

            //do the read after all
            tried = 0;
            int readCount = 0;
            try
            {
                readCount = await ReadBuffer(
                    buffer,
                    0,
                    buffer.Length,
                    cancellationTokenSource == null ? CancellationToken.None : cancellationTokenSource.Token);
            }
            catch { }

            //nothing? should try again?
            if (readCount == 0)
            {
                tried++;
                if (tried >= seconds)
                    break;
            }
            else
                stringBuilder.Append(Encoding.ASCII.GetString(buffer, 0, readCount));

            //reach a line where the OLT wait for new command?
            if ((regexDetectWaitLine ?? ExpressionForToWaitCommand).IsMatch(stringBuilder.ToString()))
                break;
        }
        while (cancellationTokenSource == null || !cancellationTokenSource.IsCancellationRequested);

        //count backspaces and remove it
        LastReadBackspacesCount = stringBuilder.ToString().Where(w => w == '\b').Count();
        stringBuilder.Replace("\b", "");

        //break lines to return as list
        stringBuilder.Replace("\r", "");
        var ret = stringBuilder.ToString().Split('\n').ToList();

        //verify for errors
        foreach (var l in ret)
        {
            var matchError = regexErrorDetect.Match(l);
            if (matchError.Success)
                lastReadErrors.Add((int.Parse(matchError.Groups[1].Value), matchError.Groups[2].Value));
        }

        //if ((OnReadResponse != null) && _telnet_logged)
        //    OnReadResponse(this, new MessageEventArgs<IEnumerable<string>>(ret));

        return ret;
    }

    public async Task<IEnumerable<string>> ReadLinesFromChannel() => await ReadLinesTelnet().ConfigureAwait(false);

    public int LastReadBackspacesCount { get; set; }

    public int LastReadErrorCount { get => lastReadErrors.Count; }

    public IEnumerable<(int code, string error)> LastReadErrors => lastReadErrors;

    public async Task WriteBuffer(byte[] buffer, int? count = null)
    {
        if (telnetClient == null || telnetStream == null)
            throw new Exception("Not connected yet to write!");

        await telnetStream.WriteAsync(buffer, 0, count ?? buffer.Length).ConfigureAwait(false);
    }

    public async Task WriteByte(byte b) => await WriteBuffer(new byte[] { b }).ConfigureAwait(false);

    public async Task WriteCommand(string command, bool newLine = true, bool doLogging = true)
    {
        if (newLine)
            command += '\n';

        //if (doLog && (OnWriteCommand != null))
        //    OnWriteCommand(this, new MessageEventArgs<string>(value));

        var vBytes = Encoding.ASCII.GetBytes(command);
        await WriteBuffer(vBytes);
    }


}
