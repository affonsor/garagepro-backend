using FluentAssertions;
using GaragePro.Core.ValueObjects;

namespace GaragePro.UnitTests.Domain;

public class DocumentTests
{
    [Fact]
    public void TryCreate_ShouldNormalizeValidCpf()
    {
        var result = Document.TryCreate("529.982.247-25", out var document);

        result.Should().BeTrue();
        document!.Value.Should().Be("52998224725");
        document.Type.Should().Be(DocumentType.Cpf);
    }

    [Fact]
    public void TryCreate_ShouldNormalizeValidCnpj()
    {
        var result = Document.TryCreate("04.252.011/0001-10", out var document);

        result.Should().BeTrue();
        document!.Value.Should().Be("04252011000110");
        document.Type.Should().Be(DocumentType.Cnpj);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-24")]
    [InlineData("00.000.000/0000-00")]
    [InlineData("04.252.011/0001-11")]
    public void TryCreate_ShouldRejectInvalidDocuments(string value)
    {
        var result = Document.TryCreate(value, out var document);

        result.Should().BeFalse();
        document.Should().BeNull();
    }
}
