using Microsoft.Extensions.Configuration;
using Synapse.Cli.Configuration;
using Synapse.Cli.Services;

var parser = BuildCommandLineParser();
await parser.InvokeAsync(args);

static Parser BuildCommandLineParser()
{
    var configuration = new ConfigurationBuilder()
        .AddYamlFile(CliConstants.ConfigurationFileName, true, true)
        .Build();
    var services = new ServiceCollection();
    ConfigureServices(services, configuration);
    using var serviceProvider = services.BuildServiceProvider();
    var rootCommand = new RootCommand();
    foreach (var command in serviceProvider.GetServices<Command>()) rootCommand.AddCommand(command);
    return new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseExceptionHandler((ex, context) =>
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            var inner = ex.InnerException;
            while (inner != null)
            {
                AnsiConsole.MarkupLine($"[red]{inner.Message}[/]");
                inner = inner.InnerException;
            }
        })
        .Build();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddSingleton(configuration);
    services.Configure<ApplicationOptions>(configuration);
    services.AddLogging();
    services.AddServerlessWorkflowIO();
    services.AddSynapseHttpApiClient(http => http.BaseAddress = new Uri("http://localhost:42286")); //todo: config based
    services.AddCliCommands();
    services.AddSingleton<IOptionsManager, OptionsManager>();
}