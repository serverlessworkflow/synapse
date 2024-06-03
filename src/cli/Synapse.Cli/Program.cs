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
    var serviceProvider = services.BuildServiceProvider();
    var scope = serviceProvider.CreateScope();
    var rootCommand = new RootCommand();
    foreach (var command in scope.ServiceProvider.GetServices<Command>()) rootCommand.AddCommand(command);
    return new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseExceptionHandler((ex, context) =>
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            var inner = ex.InnerException;
            while (inner != null)
            {
                AnsiConsole.MarkupLine($"[red]{inner.Message.EscapeMarkup()}[/]");
                inner = inner.InnerException;
            }
        })
        .Build();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var applicationOptions = new ApplicationOptions();
    configuration.Bind(applicationOptions);
    services.AddSingleton(configuration);
    services.Configure<ApplicationOptions>(configuration);
    services.AddLogging();
    services.AddServerlessWorkflowIO();
    services.AddSynapseHttpApiClient(http =>
    {
        if (string.IsNullOrWhiteSpace(applicationOptions.Api.Current)) return; //todo: warn
        http.BaseAddress = applicationOptions.Api.Configurations[applicationOptions.Api.Current].Server ?? null!; //todo: warn
    });
    services.AddCliCommands();
    services.AddSingleton<IOptionsManager, OptionsManager>();
}