using System.Reflection;
using System.Text.Json.Serialization;
using LTres.Olt.Api.Common;
using LTres.Olt.Api.Common.Plugin;
using LTres.Olt.Api.Communication;
using LTres.Olt.Api.Core;
using LTres.Olt.Api.Core.Services;
using LTres.Olt.Api.Core.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPluginManager(builder.Configuration);

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

await app.Services.PluginManagerBeforeStart();

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

await app.StartAsync();
await app.Services.PluginManagerAfterStart();
Console.WriteLine("Started");

await app.Services.PluginManagerBeforeStop();
await app.WaitForShutdownAsync();
await app.Services.PluginManagerAfterStop();
Console.WriteLine("Stopped");