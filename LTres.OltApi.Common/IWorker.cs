using LTres.OltApi.Common.Models;

namespace LTres.OltApi.Common;

public interface IWorker
{
    void Start();

    void Stop();
}