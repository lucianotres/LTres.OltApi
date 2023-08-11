using LTres.OltApi.Common.DbServices;
using LTres.OltApi.Core.Services;
using LTres.OltApi.Mongo;

var builder = WebApplication.CreateBuilder(args);

//configure connection settings for database
builder.Services.Configure<MongoConfig>(o => builder.Configuration.Bind("MongoConfig", o));

MongoModelsConfiguration.RegisterClassMap();

builder.Services.AddScoped<IDbOLTHost, MongoDbOLTHost>();
builder.Services.AddScoped<OLTHostService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
