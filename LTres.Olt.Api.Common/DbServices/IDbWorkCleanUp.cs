namespace LTres.Olt.Api.Common.DbServices;

public interface IDbWorkCleanUp
{
    Task<long> CleanUpExecute();
}
