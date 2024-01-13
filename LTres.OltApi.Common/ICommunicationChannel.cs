using System.Net;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace LTres.OltApi.Common;

public interface ICommunicationChannel : IDisposable
{
    IPEndPoint HostEndPoint { get; set; }
    string? Username { get; set; }
    string? Password { get; set; }

    int ToReadAvailable { get; }
    int LastReadBackspacesCount { get; set; }
    int LastReadErrorCount { get; }

    /// <summary>
    /// Write a single byte to channel
    /// </summary>
    Task WriteByte(byte b);

    /// <summary>
    /// Write a byte buffer to channel
    /// </summary>
    /// <param name="buffer">Byte buffer</param>
    /// <param name="count">How many bytes from buffer, or null to all</param>
    Task WriteBuffer(byte[] buffer, int? count = null);

    /// <summary>
    /// Write a command to channel
    /// </summary>
    /// <param name="command">Command to send</param>
    /// <param name="newLine">Write a new line after</param>
    Task WriteCommand(string command, bool newLine = true, bool doLogging = true);

    IEnumerable<(int code, string error)> LastReadErrors { get; }

    Task<int> ReadBuffer(byte[] buffer, int offset, int count);

    Task<int> ReadBuffer(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    Task<IEnumerable<string>> ReadLinesFromChannel(Regex? regexDetectWaitLine = null, CancellationTokenSource? cancellationTokenSource = null);

    Task<bool> Connect();

    Task Disconnect();
}
