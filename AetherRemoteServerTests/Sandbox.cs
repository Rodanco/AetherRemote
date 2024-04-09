namespace AetherRemoteServerTests;

public class Sandbox
{
    [Test]
    public void SandboxTest()
    {
        List<string> list = new List<string>()
        {
            "a", "b", "c", "d"
        };

        var result = string.Join(", ", list);

        Assert.That(result, Is.EqualTo("a, b, c, d"));
    }
}
