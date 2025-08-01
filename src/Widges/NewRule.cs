using Spectre.Console;
using Spectre.Console.Rendering;

namespace SolarScope.Widges;

// <summary>
/// A renderable horizontal rule with improved Unicode character width handling.
/// </summary>
public sealed class NewRule : Renderable, IHasJustification, IHasBoxBorder
{
    /// <summary>
    /// Gets or sets the rule title markup text.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the rule style.
    /// </summary>
    public Style? Style { get; set; }

    /// <summary>
    /// Gets or sets the rule's title justification.
    /// </summary>
    public Justify? Justification { get; set; }

    /// <inheritdoc/>
    public BoxBorder Border { get; set; } = BoxBorder.Square;

    internal int TitlePadding { get; set; } = 2;
    internal int TitleSpacing { get; set; } = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewRule"/> class.
    /// </summary>
    public NewRule()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewRule"/> class.
    /// </summary>
    /// <param name="title">The rule title markup text.</param>
    public NewRule(string title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var extraLength = 2 * TitlePadding + 2 * TitleSpacing;

        if (Title == null || maxWidth <= extraLength)
        {
            return GetLineWithoutTitle(options, maxWidth);
        }

        // Get the title and make sure it fits.
        var title = GetTitleSegments(options, Title, maxWidth - extraLength);
        var titleCellCount = GetAccurateCellCount(title);

        if (titleCellCount > maxWidth - extraLength)
        {
            // Truncate the title - fallback to original Segment.CellCount for compatibility
            title = TruncateWithEllipsis(title, maxWidth - extraLength);
            if (!title.Any())
            {
                // We couldn't fit the title at all.
                return GetLineWithoutTitle(options, maxWidth);
            }
            titleCellCount = GetAccurateCellCount(title);
        }

        var (left, right) = GetLineSegments(options, maxWidth, title, titleCellCount);

        var segments = new List<Segment>();
        segments.Add(left);
        segments.AddRange(title);
        segments.Add(right);
        segments.Add(Segment.LineBreak);

        return segments;
    }

    private IEnumerable<Segment> GetLineWithoutTitle(RenderOptions options, int maxWidth)
    {
        var border = Border.GetSafeBorder(safe: !options.Unicode);
        var text = RepeatString(border.GetPart(BoxBorderPart.Top), maxWidth);

        return new[]
        {
            new Segment(text, Style ?? Style.Plain),
            Segment.LineBreak,
        };
    }

    private IEnumerable<Segment> GetTitleSegments(RenderOptions options, string title, int width)
    {
        title = NormalizeNewLines(title).Replace("\n", " ").Trim();
        var markup = new Markup(title, Style);
        return ((IRenderable)markup).Render(options, width);
    }

    /// <summary>
    /// Gets a more accurate cell count that properly handles Unicode characters including emojis
    /// </summary>
    private int GetAccurateCellCount(IEnumerable<Segment> segments)
    {
        int totalWidth = 0;
        foreach (var segment in segments)
        {
            totalWidth += GetStringDisplayWidth(segment.Text);
        }
        return totalWidth;
    }

    /// <summary>
    /// Calculate the display width of a string, accounting for wide Unicode characters
    /// </summary>
    private int GetStringDisplayWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int width = 0;
        var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(text);

        while (enumerator.MoveNext())
        {
            var element = enumerator.GetTextElement();

            // Handle common wide characters and emojis
            if (IsWideCharacter(element))
            {
                width += 2; // Wide characters typically take 2 cells
            }
            else if (IsZeroWidthCharacter(element))
            {
                width += 0; // Zero-width characters
            }
            else
            {
                width += 1; // Normal characters
            }
        }

        return width;
    }

    /// <summary>
    /// Determines if a text element is a wide character (takes 2 cells)
    /// </summary>
    private bool IsWideCharacter(string element)
    {
        if (string.IsNullOrEmpty(element))
            return false;

        var codePoint = char.ConvertToUtf32(element, 0);

        // Emoji ranges and other wide characters
        return codePoint >= 0x1F300 && codePoint <= 0x1F9FF || // Miscellaneous Symbols and Pictographs, Emoticons, etc.
               codePoint >= 0x2600 && codePoint <= 0x26FF ||   // Miscellaneous Symbols
               codePoint >= 0x2700 && codePoint <= 0x27BF ||   // Dingbats
               codePoint >= 0x1F600 && codePoint <= 0x1F64F || // Emoticons
               codePoint >= 0x1F680 && codePoint <= 0x1F6FF || // Transport and Map Symbols
               codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF || // Regional Indicator Symbols
               codePoint >= 0x3040 && codePoint <= 0x309F ||   // Hiragana
               codePoint >= 0x30A0 && codePoint <= 0x30FF ||   // Katakana
               codePoint >= 0x4E00 && codePoint <= 0x9FFF ||   // CJK Unified Ideographs
               codePoint >= 0x3400 && codePoint <= 0x4DBF ||   // CJK Extension A
               codePoint >= 0xAC00 && codePoint <= 0xD7AF ||   // Hangul Syllables
               codePoint >= 0xFF01 && codePoint <= 0xFF60 ||   // Fullwidth Forms
               codePoint >= 0xFFE0 && codePoint <= 0xFFE6;     // Fullwidth Forms
    }

    /// <summary>
    /// Determines if a text element is a zero-width character
    /// </summary>
    private bool IsZeroWidthCharacter(string element)
    {
        if (string.IsNullOrEmpty(element))
            return false;

        var codePoint = char.ConvertToUtf32(element, 0);

        // Zero-width characters
        return codePoint == 0x200B || // Zero Width Space
               codePoint == 0x200C || // Zero Width Non-Joiner
               codePoint == 0x200D || // Zero Width Joiner
               codePoint == 0xFEFF || // Zero Width No-Break Space
               codePoint >= 0x1160 && codePoint <= 0x11FF; // Hangul Jamo
    }

    private (Segment Left, Segment Right) GetLineSegments(RenderOptions options, int width, IEnumerable<Segment> title, int titleLength)
    {
        var border = Border.GetSafeBorder(safe: !options.Unicode);
        var borderPart = border.GetPart(BoxBorderPart.Top);

        var alignment = Justification ?? Justify.Center;
        if (alignment == Justify.Left)
        {
            var leftPadding = RepeatString(borderPart, TitlePadding) + new string(' ', TitleSpacing);
            var left = new Segment(leftPadding, Style ?? Style.Plain);

            var rightLength = Math.Max(0, width - titleLength - GetStringDisplayWidth(leftPadding) - TitleSpacing);
            var right = new Segment(new string(' ', TitleSpacing) + RepeatString(borderPart, rightLength), Style ?? Style.Plain);

            return (left, right);
        }
        else if (alignment == Justify.Center)
        {
            var availableSpace = width - titleLength - 2 * TitleSpacing;
            var leftLength = Math.Max(0, availableSpace / 2);
            var left = new Segment(RepeatString(borderPart, leftLength) + new string(' ', TitleSpacing), Style ?? Style.Plain);

            var rightLength = Math.Max(0, width - titleLength - GetStringDisplayWidth(left.Text) - TitleSpacing);
            var right = new Segment(new string(' ', TitleSpacing) + RepeatString(borderPart, rightLength), Style ?? Style.Plain);

            return (left, right);
        }
        else if (alignment == Justify.Right)
        {
            var rightPadding = new string(' ', TitleSpacing) + RepeatString(borderPart, TitlePadding);
            var right = new Segment(rightPadding, Style ?? Style.Plain);

            var leftLength = Math.Max(0, width - titleLength - GetStringDisplayWidth(rightPadding) - TitleSpacing);
            var left = new Segment(RepeatString(borderPart, leftLength) + new string(' ', TitleSpacing), Style ?? Style.Plain);

            return (left, right);
        }

        throw new NotSupportedException("Unsupported alignment.");
    }

    /// <summary>
    /// Helper method to repeat a string character a specified number of times
    /// </summary>
    private string RepeatString(string text, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(text))
            return string.Empty;

        return new string(text[0], count);
    }

    /// <summary>
    /// Helper method to normalize newlines in text
    /// </summary>
    private string NormalizeNewLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// Truncate segments with ellipsis - fallback implementation
    /// </summary>
    private IEnumerable<Segment> TruncateWithEllipsis(IEnumerable<Segment> segments, int maxWidth)
    {
        var result = new List<Segment>();
        var currentWidth = 0;
        var ellipsisWidth = 3; // "..."

        foreach (var segment in segments)
        {
            var segmentWidth = GetStringDisplayWidth(segment.Text);

            if (currentWidth + segmentWidth <= maxWidth - ellipsisWidth)
            {
                result.Add(segment);
                currentWidth += segmentWidth;
            }
            else
            {
                // Add partial segment if possible
                var remainingWidth = maxWidth - ellipsisWidth - currentWidth;
                if (remainingWidth > 0)
                {
                    var truncatedText = TruncateString(segment.Text, remainingWidth);
                    if (!string.IsNullOrEmpty(truncatedText))
                    {
                        result.Add(new Segment(truncatedText, segment.Style));
                    }
                }
                result.Add(new Segment("...", segment.Style));
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Truncate a string to fit within specified width
    /// </summary>
    private string TruncateString(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
            return string.Empty;

        var width = 0;
        var result = new System.Text.StringBuilder();
        var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(text);

        while (enumerator.MoveNext() && width < maxWidth)
        {
            var element = enumerator.GetTextElement();
            var elementWidth = IsWideCharacter(element) ? 2 : 1;

            if (width + elementWidth <= maxWidth)
            {
                result.Append(element);
                width += elementWidth;
            }
            else
            {
                break;
            }
        }

        return result.ToString();
    }
}
