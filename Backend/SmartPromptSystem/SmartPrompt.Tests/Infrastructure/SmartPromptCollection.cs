using Xunit;

namespace SmartPrompt.Tests.Infrastructure;

[CollectionDefinition("IntegrationTests")]
public class SmartPromptCollection : ICollectionFixture<SmartPromptTestFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
