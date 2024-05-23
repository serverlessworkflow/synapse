namespace Synapse.UnitTests.Services;

internal static class EventDefinitionFactory
{

    internal static EventDefinition Create() => new()
    {
        With = 
        [
            new("source", "https://com.test.unit"),
            new("type", "com.test.unit.events.fake.v1test")
        ]
    };

}