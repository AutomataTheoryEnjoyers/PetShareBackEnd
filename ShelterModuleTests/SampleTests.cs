using FluentAssertions;
using Xunit;

namespace ShelterModuleTests;

public class SampleTests
{
    [Fact]
    public void ShouldBeZero()
    {
        0.Should().Be(1);
    }
}
