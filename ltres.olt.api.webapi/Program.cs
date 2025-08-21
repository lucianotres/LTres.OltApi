using System.Reflection;
using System.Text.Json.Serialization;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.DbServices;
using LTres.Olt.Api.Communication;
using LTres.Olt.Api.Core;
using LTres.Olt.Api.Core.Services;
using LTres.Olt.Api.Core.Tools;
using LTres.Olt.Api.Mongo;
using LTres.Olt.Api.WebApi;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

//configure connection settings for database
builder.Services.Configure<MongoConfig>(o => builder.Configuration.Bind("MongoConfig", o));

MongoModelsConfiguration.RegisterClassMap();

//database handlers
builder.Services
    .AddScoped<IDbOLTHost, MongoDbOLTHost>()
    .AddScoped<IDbOLTScript, MongoDbOLTHost>()
    .AddScoped<IDbOLTHostItem, MongoDbOLTHostItem>();

//service handlers
builder.Services
    .AddMemoryCache()
    .AddScoped<IOLTHostService, OLTHostService>()
    .AddScoped<IOLTHostItemService, OLTHostItemService>()
    .AddScoped<IOLTScriptService, OLTScriptService>()
    .AddScoped<IOLTHostCLIActionsService, OLTHostCLIActionsService>()
    .AddTransient<IOLTHostCLIScriptService, OLTHostCLIScriptService>();

//OLT CLI channel
builder.Services
    .AddScoped<ICommunicationChannel, TelnetZTEChannel>()
    .AddScoped<ClientZteCLI>();

//webapi controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Set the comments path for the XmlComments file.
    try
    {
        string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    }
    catch { }
});

var app = builder.Build();

//do BD migrations before start
await MongoDbOltApiMigrations.Do(app.Services.GetRequiredService<IOptions<MongoConfig>>().Value);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
