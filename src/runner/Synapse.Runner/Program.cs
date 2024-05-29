if (args.Length != 0 && args.Contains("--debug") && !Debugger.IsAttached) Debugger.Launch();

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", true, true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
        config.AddEnvironmentVariables("SYNAPSE");
        config.AddCommandLine(args);
        config.AddKeyPerFile("/run/secrets/synapse", true, true);
    })
    .ConfigureServices((context, services) =>
    {
        var options = new RunnerOptions();
        context.Configuration.Bind(options);
        services.Configure<RunnerOptions>(context.Configuration);
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddJsonSerializer();
        services.AddYamlDotNetSerializer();
        services.AddSynapseHttpApiClient(http =>
        {
            http.BaseAddress = options.Api.BaseAddress;
        });
        services.AddHostedService<WorkflowExecutorInitializer>();
    });

using var app = builder.Build();

await app.RunAsync();