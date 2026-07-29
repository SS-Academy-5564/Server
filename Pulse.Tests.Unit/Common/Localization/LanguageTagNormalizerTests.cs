using FluentAssertions;
using Pulse.BL.Common.Localization;

namespace Pulse.Tests.Unit.Common.Localization;

public class LanguageTagNormalizerTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("en-US", "en")]
    [InlineData("uk_UA", "uk")]
    [InlineData(" EN-us ", "en")]
    public void NormalizePrimarySubtag_WhenTagIsValid_ReturnsPrimarySubtag(string input, string expected)
    {
        // Act
        string normalized = LanguageTagNormalizer.NormalizePrimarySubtag(input);

        // Assert
        normalized.Should().Be(expected);
    }

    [Theory]
    [InlineData("uk--UA")]
    [InlineData("uk__UA")]
    [InlineData("-uk")]
    [InlineData("uk-")]
    public void NormalizePrimarySubtag_WhenTagHasRepeatedOrEdgeSeparators_ReturnsEmpty(string input)
    {
        // Act
        string normalized = LanguageTagNormalizer.NormalizePrimarySubtag(input);

        // Assert
        normalized.Should().BeEmpty();
    }

    [Theory]
    [InlineData("u1-UA")]
    [InlineData("1")]
    [InlineData("a")]
    [InlineData("toolongtag")]
    [InlineData("en- US")]
    [InlineData("*")]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizePrimarySubtag_WhenTagIsInvalid_ReturnsEmpty(string input)
    {
        // Act
        string normalized = LanguageTagNormalizer.NormalizePrimarySubtag(input);

        // Assert
        normalized.Should().BeEmpty();
    }
}
