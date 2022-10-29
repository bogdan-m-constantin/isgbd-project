
using Microsoft.Extensions.Configuration;
using MiniDBMS;
using MiniDBMS.Context;
using Raven.Client.Documents;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Security.Cryptography.X509Certificates;
using var log = new LoggerConfiguration()
    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(),"logs/minidms.log"), rollingInterval: RollingInterval.Day, levelSwitch: new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Verbose))
    .CreateLogger();

try
{
    string ravenDbUrl = "http://localhost:8080";

    var store = new DocumentStore()
    {
        Urls = new[] { ravenDbUrl },
        Conventions =
                {
                    MaxNumberOfRequestsPerSession = 32,
                    UseOptimisticConcurrency = true,
                },
        Database = "MiniDBMS",
        Certificate = default,
    }.Initialize();

    if (store == null)
        throw new Exception("Could not connect to RavenDB");

    var context = new SqlExecutionContext(store);
    context.LoadCatalog();
    log.Write(LogEventLevel.Information, "Loaded catalog");

    var controller = new Controller(log, context);
    

    while (true) controller.Loop();


}catch(Exception e)
{
    Console.WriteLine($"Exception: {e.Message}");
    log.Write(LogEventLevel.Fatal, e, "Unhandled exception");
}