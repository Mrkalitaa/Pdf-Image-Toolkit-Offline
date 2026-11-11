using PdfImageToolkit.Core.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace PdfImageToolkit.Tests;

public class RangeParserTests
{
    [Fact]
    public void Parse_StandardRangeAndSingle_ReturnsSortedPages()
    {
        var result = PageRangeParser.Parse("1-3,5", 10);
        Assert.Equal(new List<int> { 1, 2, 3, 5 }, result);
    }

    [Fact]
    public void Parse_ReverseRange_NormalizesGracefully()
    {
        var result = PageRangeParser.Parse("5-2", 10);
        Assert.Equal(new List<int> { 2, 3, 4, 5 }, result);
    }

    [Fact]
    public void Parse_ZeroPage_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PageRangeParser.Parse("0", 10));
    }

    [Fact]
    public void Parse_PageExceedingTotal_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PageRangeParser.Parse("99", 5));
    }

    [Fact]
    public void Parse_DuplicatePages_DeduplicatesCorrectly()
    {
        var result = PageRangeParser.Parse("1-3, 2, 4", 10);
        Assert.Equal(new List<int> { 1, 2, 3, 4 }, result);
    }

    [Fact]
    public void Parse_EmptyOrWhitespace_ReturnsAllPages()
    {
        var result = PageRangeParser.Parse("   ", 4);
        Assert.Equal(new List<int> { 1, 2, 3, 4 }, result);
    }

    [Fact]
    public void Parse_InvalidCharacters_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PageRangeParser.Parse("1-3,abc", 10));
    }
}
