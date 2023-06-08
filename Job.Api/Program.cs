using Job.Api.JobExamples.SimpleLoop;
using Akka.Jobs;
using Akka.Jobs.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Job library registration
builder.Services.AddScoped<IJob<ForEachJobInput, ForEachJobResult>, ForEachJob>();
builder.Services.AddJobContext();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();