namespace Synapse.UnitTests.Services;

internal static class EventFilterDefinitionFactory
{

    internal static EventFilterDefinition Create() => new()
    {
        With = [new("type", "com.test.unit.type.v1test")]
    };

}