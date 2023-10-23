namespace LTres.OltApi.Common;

public interface IDbWorkCleanUp
{
    Task<long> CleanUpExecute();
}
