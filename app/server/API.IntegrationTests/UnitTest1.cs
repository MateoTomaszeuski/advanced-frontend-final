namespace API.IntegrationTests;

public class UnitTest1 {
    [Test]
    public async Task Test1() {
        var value = 1 + 1;
        await Assert.That(value).IsEqualTo(2);
    }
}