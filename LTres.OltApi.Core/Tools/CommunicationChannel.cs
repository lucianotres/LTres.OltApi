using System.Dynamic;
using System.Net;
using LTres.OltApi.Common;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Tools;

public class CommunicationChannel : IDisposable
{
    private ICommunicationChannel channel;
    private ILogger logger;

    public CommunicationChannel(ICommunicationChannel channel, ILogger logger)
    {
        Address = new IPEndPoint(IPAddress.None, 0);
        Username = string.Empty;
        Password = string.Empty;
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


    public IPEndPoint Address { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public (int code, string error) LastError { get => channel.LastReadErrorCount == 0 ? (0, string.Empty) : channel.LastReadErrors.Last(); }

    public async Task<bool> Connect() => await channel.Connect(Address, Username, Password);


    private async Task<IEnumerable<string>?> ReadLinesFromChannelWithMore()
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
        else
            return finalLines.Take(finalLines.Count - 1).ToList();
    }

    public async Task GoToBeginning()
    {
        //write ! go to start
        await channel.WriteCommand("!");
        await channel.ReadLinesFromChannel();
    }

    public async Task<IEnumerable<string>?> GetPowerOnuRx(int chassi, int board, int pon, int? onu = null)
    {
        if (onu == null)
            await channel.WriteCommand($"show pon power onu-rx gpon-olt_{chassi}/{board}/{pon}");
        else
            await channel.WriteCommand($"show pon power onu-rx gpon-onu_{chassi}/{board}/{pon}:{onu}");

        return await ReadLinesFromChannelWithMore();
    }

}
