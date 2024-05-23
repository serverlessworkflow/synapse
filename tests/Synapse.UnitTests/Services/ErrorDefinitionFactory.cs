namespace Synapse.UnitTests.Services;

internal static class ErrorDefinitionFactory
{

    internal static ErrorDefinition Create() => new()
    {
        Type = new("https://unit.tests.com"),
        Status = "400",
        Title = "Fake Error Title",
        Detail = "Fake Error Details",
        Instance = "/do/todo-1/failing"
    };

}