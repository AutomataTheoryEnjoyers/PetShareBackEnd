using FluentAssertions;
using Xunit;

namespace ShelterModuleTests;

[Trait("Category", "Unit")]
public class SampleTests
{
    [Fact]
    public void ShouldBeZero()
    {
        0.Should().Be(0);
    }
}
