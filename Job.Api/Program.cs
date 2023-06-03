using Job.Api.Controllers;
using Job.Core;
using Job.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Job library registration
builder.Services.AddScoped<IJob<TestJob>, ForEachJob>();
builder.Services.ConfigureJobServices();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();