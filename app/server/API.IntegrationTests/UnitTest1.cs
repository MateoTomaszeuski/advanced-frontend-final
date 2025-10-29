namespace API.IntegrationTests;

public class UnitTest1
{
    [Test]
    public async Task Test1()
    {
        await Assert.That(1 + 1).IsEqualTo(2);
    }
}
