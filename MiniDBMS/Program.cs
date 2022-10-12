
using Microsoft.Extensions.Configuration;
using MiniDBMS;
using MiniDBMS.Context;
using Raven.Client.Documents;
using Serilog;
using Serilog.Core;
using System.Security.Cryptography.X509Certificates;

    var builder = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json", true, true);

    var config = builder.Build();

    string ravenDbUrl = config.GetSection("ravendb").Value;

    var store = new DocumentStore()
    {
        Urls = new[] { ravenDbUrl },
        Conventions =
                {
                    MaxNumberOfRequestsPerSession = 32,
                    UseOptimisticConcurrency = true,
                },
        Database = "mini-dbms",
        Certificate = default,
    }.Initialize();

if (store == null)
    throw new Exception("Could not connect to RavenDB");

using var log = new LoggerConfiguration()
    .WriteTo.File("logs/minidms.log",rollingInterval: RollingInterval.Day, levelSwitch : new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Verbose))
    .CreateLogger();

var controller = new Controller(log, new SqlExecutionContext(store));
// use something smarter
while (true) controller.Loop();
