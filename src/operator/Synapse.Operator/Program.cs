if (args.Length != 0 && args.Contains("--debug") && !Debugger.IsAttached) Debugger.Launch();

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", true, true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
        config.AddEnvironmentVariables("SYNAPSE_OPERATOR");
        config.AddCommandLine(args);
        config.AddKeyPerFile("/run/secrets/synapse", true, true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<OperatorOptions>(context.Configuration);
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        });
        services.AddSingleton<IUserAccessor, ApplicationUserAccessor>();
        services.AddSynapse(context.Configuration);
        services.AddSingleton<IDockerClient>(provider =>
        {
            var configuration = new DockerClientConfiguration();
            return configuration.CreateClient();
        });
        services.AddSingleton<IContainerPlatform, DockerContainerPlatform>();
        services.AddScoped<NativeRuntime>();
        services.AddScoped<ContainerRuntime>();
        services.AddScoped<IWorkflowRuntime>(provider => 
        {
            var options = provider.GetRequiredService<IOptionsMonitor<OperatorOptions>>().CurrentValue;
            return options.Runner.Runtime.Mode switch
            {
                OperatorRuntimeMode.Native => provider.GetRequiredService<NativeRuntime>(),
                OperatorRuntimeMode.Containerized => provider.GetRequiredService<ContainerRuntime>(),
                _ => throw new NotSupportedException($"The specified operator runtime mode '{options.Runner.Runtime.Mode}' is not supported")
            };
        });

        services.AddScoped<OperatorController>();
        services.AddScoped<IOperatorController>(provider => provider.GetRequiredService<OperatorController>());

        services.AddScoped<WorkflowController>();
        services.AddScoped<IResourceController<Workflow>>(provider => provider.GetRequiredService<WorkflowController>());

        services.AddScoped<WorkflowInstanceController>();
        services.AddScoped<IResourceController<WorkflowInstance>>(provider => provider.GetRequiredService<WorkflowInstanceController>());

        services.AddHostedService<Application>();
    });

using var app = builder.Build();

await app.RunAsync();
