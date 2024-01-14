namespace LTres.OltApi.Common;

public interface IOLTHostCLIActionsService
{
    /// <summary>
    /// Get detailed information about a specific ONU
    /// </summary>
    /// <param name="oltId">ID of OLT</param>
    /// <param name="olt">OLT chassi number</param>
    /// <param name="slot">Slot</param>
    /// <param name="port">Port</param>
    /// <param name="id">ID of ONU</param>
    Task<IEnumerable<string>> GetONUInfo(Guid oltId, int olt, int slot, int port, int id);

    /// <summary>
    /// Get information about interfaces of specific ONU
    /// </summary>
    /// <param name="oltId">ID of OLT</param>
    /// <param name="olt">OLT chassi number</param>
    /// <param name="slot">Slot</param>
    /// <param name="port">Port</param>
    /// <param name="id">ID of ONU</param>
    Task<IEnumerable<string>> GetONUInterfaces(Guid oltId, int olt, int slot, int port, int id);

    /// <summary>
    /// Get the version of specific ONU firmware
    /// </summary>
    /// <param name="oltId">ID of OLT</param>
    /// <param name="olt">OLT chassi number</param>
    /// <param name="slot">Slot</param>
    /// <param name="port">Port</param>
    /// <param name="id">ID of ONU</param>
    Task<IEnumerable<string>> GetONUVersion(Guid oltId, int olt, int slot, int port, int id);

    /// <summary>
    /// Get mac address list of specific ONU firmware
    /// </summary>
    /// <param name="oltId">ID of OLT</param>
    /// <param name="olt">OLT chassi number</param>
    /// <param name="slot">Slot</param>
    /// <param name="port">Port</param>
    /// <param name="id">ID of ONU</param>
    Task<IEnumerable<string>> GetONUmac(Guid oltId, int olt, int slot, int port, int id);
}
