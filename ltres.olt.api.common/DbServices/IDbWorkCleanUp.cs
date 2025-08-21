namespace LTres.Olt.Api.Common;

public interface IDbWorkCleanUp
{
    Task<long> CleanUpExecute();
}
