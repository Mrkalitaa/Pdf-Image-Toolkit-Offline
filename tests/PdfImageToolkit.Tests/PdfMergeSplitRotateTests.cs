using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfImageToolkit.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PdfImageToolkit.Tests;

public class PdfMergeSplitRotateTests : IDisposable
{
    private readonly string _testDir;
    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;
    private readonly PdfService _pdfService;

    public PdfMergeSplitRotateTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "PdfMergeSplitRotateTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);

        _loggingService = new LoggingService();
        var settingsService = new SettingsService { DefaultOutputFolder = _testDir };
        _fileService = new FileService(settingsService);
        _pdfService = new PdfService(_fileService, _loggingService);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // Ignore
        }
    }

    private string CreateSamplePdf(string name, int pageCount)
    {
        var filePath = Path.Combine(_testDir, name);
        using var doc = new PdfDocument();
        for (int i = 0; i < pageCount; i++)
        {
            var page = doc.AddPage();
            using var gfx = XGraphics.FromPdfPage(page);
            gfx.DrawString($"Page {i + 1} of {pageCount}", new XFont("Arial", 16), XBrushes.Black, new XPoint(50, 100));
        }
        doc.Save(filePath);
        return filePath;
    }

    [Fact]
    public async Task Merge_TwoPdfs_ProducesCorrectTotalPageCount()
    {
        var pdf1 = CreateSamplePdf("doc1.pdf", 3);
        var pdf2 = CreateSamplePdf("doc2.pdf", 2);

        var result = await _pdfService.MergePdfsAsync([pdf1, pdf2], _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));

        using var mergedDoc = PdfReader.Open(result.OutputFilePath, PdfDocumentOpenMode.Import);
        Assert.Equal(5, mergedDoc.PageCount);
    }

    [Fact]
    public async Task Split_ByRange_ExtractsExpectedPages()
    {
        var pdf = CreateSamplePdf("doc_to_split.pdf", 5);

        var result = await _pdfService.SplitPdfAsync(pdf, "1,3-4", _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));

        using var splitDoc = PdfReader.Open(result.OutputFilePath, PdfDocumentOpenMode.Import);
        Assert.Equal(3, splitDoc.PageCount);
    }

    [Fact]
    public async Task SplitEveryPage_ProducesIndividualSinglePageDocuments()
    {
        var pdf = CreateSamplePdf("doc_every_page.pdf", 4);

        var result = await _pdfService.SplitEveryPageAsync(pdf, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePaths);
        Assert.Equal(4, result.OutputFilePaths.Count);

        foreach (var path in result.OutputFilePaths)
        {
            Assert.True(File.Exists(path));
            using var pageDoc = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            Assert.Equal(1, pageDoc.PageCount);
        }
    }

    [Fact]
    public async Task SplitEveryNPages_ProducesChunks()
    {
        var pdf = CreateSamplePdf("doc_chunks.pdf", 5);

        var result = await _pdfService.SplitEveryNPagesAsync(pdf, 2, _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePaths);
        Assert.Equal(3, result.OutputFilePaths.Count); // 2 + 2 + 1 = 5

        using var part1 = PdfReader.Open(result.OutputFilePaths[0], PdfDocumentOpenMode.Import);
        Assert.Equal(2, part1.PageCount);

        using var part3 = PdfReader.Open(result.OutputFilePaths[2], PdfDocumentOpenMode.Import);
        Assert.Equal(1, part3.PageCount);
    }

    [Fact]
    public async Task Rotate_SelectedPages_RotatesOnlyTargetPages()
    {
        var pdf = CreateSamplePdf("doc_rotate.pdf", 3);

        var result = await _pdfService.RotatePdfAsync(pdf, 90, "2", _testDir);

        Assert.True(result.Success);
        Assert.NotNull(result.OutputFilePath);
        Assert.True(File.Exists(result.OutputFilePath));

        using var rotatedDoc = PdfReader.Open(result.OutputFilePath, PdfDocumentOpenMode.Import);
        Assert.Equal(0, rotatedDoc.Pages[0].Rotate);
        Assert.Equal(90, rotatedDoc.Pages[1].Rotate);
        Assert.Equal(0, rotatedDoc.Pages[2].Rotate);
    }
}
