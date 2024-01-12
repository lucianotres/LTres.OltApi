using System.Net;
using LTres.OltApi.Common;
using Microsoft.Extensions.Logging;

namespace LTres.OltApi.Core.Tools;

public class CommunicationChannel : IDisposable
{
    private ICommunicationChannel _cc;
    private ILogger _log;

    public CommunicationChannel(ICommunicationChannel channel, ILogger logger)
    {
        Address = IPAddress.None;
        Port = 0;
        Disposed = false;

        _cc = channel;
        _log = logger;
    }

    public bool Disposed { get; private set; } = false;

    public void Dispose()
    {
        if (!Disposed)
        {
            Disposed = true;
            _cc.Dispose();
        }
    }


    public IPAddress Address { get; set; }
    public int Port { get; set; }

    public async Task<bool> Connect() => await _cc.Connect(new IPEndPoint(Address, Port));
    

    public async Task GoToBeginning()
    {
        //write ! go to start
        await _cc. WriteCommand("!");
        await _cc.ReadLinesFromChannel();
    }

}
