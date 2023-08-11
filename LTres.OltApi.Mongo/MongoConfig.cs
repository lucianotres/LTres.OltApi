using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LTres.OltApi.Mongo;

public class MongoConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
