using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfImageToolkit.Core.Helpers;

public static class PageRangeParser
{
    /// <summary>
    /// Parses a page range string such as "1-3, 5, 8-10" into 1-based page indices.
    /// Handles edge cases like "5-2" (normalized to 2-5), whitespace, duplicates, and out-of-range bounds.
    /// </summary>
    public static List<int> Parse(string rangeExpression, int totalPages)
    {
        if (totalPages <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalPages), "Total pages must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(rangeExpression))
        {
            // Empty expression means all pages
            return Enumerable.Range(1, totalPages).ToList();
        }

        var resultSet = new HashSet<int>();
        var tokens = rangeExpression.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens)
        {
            if (token.Contains('-'))
            {
                var dashIndex = token.IndexOf('-');
                var startStr = token[..dashIndex].Trim();
                var endStr = token[(dashIndex + 1)..].Trim();

                if (!int.TryParse(startStr, out int start) || !int.TryParse(endStr, out int end))
                {
                    throw new FormatException($"Invalid range token '{token}'. Numbers expected.");
                }

                if (start <= 0 || end <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(rangeExpression), $"Page numbers must be 1 or greater. Found '{token}'.");
                }

                // Handle reverse range like "5-2" by normalizing
                int min = Math.Min(start, end);
                int max = Math.Max(start, end);

                if (max > totalPages)
                {
                    throw new ArgumentOutOfRangeException(nameof(rangeExpression), $"Page number {max} exceeds total document pages ({totalPages}).");
                }

                for (int page = min; page <= max; page++)
                {
                    resultSet.Add(page);
                }
            }
            else
            {
                if (!int.TryParse(token, out int singlePage))
                {
                    throw new FormatException($"Invalid page number '{token}'.");
                }

                if (singlePage <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(rangeExpression), $"Page numbers must be 1 or greater. Found '{singlePage}'.");
                }

                if (singlePage > totalPages)
                {
                    throw new ArgumentOutOfRangeException(nameof(rangeExpression), $"Page number {singlePage} exceeds total document pages ({totalPages}).");
                }

                resultSet.Add(singlePage);
            }
        }

        return resultSet.OrderBy(p => p).ToList();
    }
}
