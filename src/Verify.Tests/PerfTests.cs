[UsesVerify]
public class PerfTests
{
    [Fact]
    public async Task Test()
    {
        var target = new Target
        {
            Property = "Value"
        };
        for (int i = 0; i < 10000; i++)
        {
            await Verify(target)
                .IgnoreMember("Foo")
                .DisableRequireUniquePrefix();
        }
    }

    public class Target
    {
        public string? Property { get; set; }
    }
}