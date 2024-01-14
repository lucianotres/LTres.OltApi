using System.Reflection;
using System.Text.Json.Serialization;
using LTres.OltApi.Common;
using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Communication;
using LTres.OltApi.Core;
using LTres.OltApi.Core.Services;
using LTres.OltApi.Core.Tools;
using LTres.OltApi.Mongo;

var builder = WebApplication.CreateBuilder(args);

//configure connection settings for database
builder.Services.Configure<MongoConfig>(o => builder.Configuration.Bind("MongoConfig", o));

MongoModelsConfiguration.RegisterClassMap();

//database handlers
builder.Services
    .AddScoped<IDbOLTHost, MongoDbOLTHost>()
    .AddScoped<IDbOLTHostItem, MongoDbOLTHostItem>();

//service handlers
builder.Services
    .AddScoped<IOLTHostService, OLTHostService>()
    .AddScoped<IOLTHostItemService, OLTHostItemService>()
    .AddScoped<IOLTHostCLIActionsService, OLTHostCLIActionsService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
