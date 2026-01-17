namespace DevQualX.Infrastructure.Tests.Adapters;

/// <summary>
/// Placeholder for infrastructure adapter tests.
/// Infrastructure tests should test adapters to third-party services.
/// </summary>
public class ExampleAdapterShould
{
    [Test]
    public async Task Pass_placeholder_test()
    {
        // Placeholder until we have infrastructure adapters to test
        var result = GetPlaceholderValue();
        await Assert.That(result).IsTrue();
    }

    private static bool GetPlaceholderValue() => true;
}
