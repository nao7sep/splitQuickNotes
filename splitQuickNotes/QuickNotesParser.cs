using yyLib;

namespace splitQuickNotes
{
    public static class QuickNotesParser
    {
        private enum LineType
        {
            Guid,
            Utc,
            Title,
            Else
        }

        private class LineParseResult
        {
            public LineType Type { get; set; }

            public Guid? Guid { get; set; }

            public DateTime? Utc { get; set; }

            public string? Title { get; set; }
        }

        private static LineParseResult ParseLine (string line)
        {
            if (line.StartsWith ('[') && line.EndsWith (']'))
            {
                string xSubstring = line [1..^1];

                if (yyConvertor.TryStringToGuid (xSubstring, out Guid xGuid))
                    return new LineParseResult { Type = LineType.Guid, Guid = xGuid };

                else if (yyFormatter.TryParseRoundtripDateTimeString (xSubstring, out DateTime xUtc))
                    return new LineParseResult { Type = LineType.Utc, Utc = xUtc };

                // Length is not checked here because QuickNotes should have trimmed the title.
                // When the entry class instance is created, the title will be trimmed again just to be sure.
                else return new LineParseResult { Type = LineType.Title, Title = xSubstring };
            }

            return new LineParseResult { Type = LineType.Else };
        }

        private static string BuildAndOptimizeContent (IEnumerable <string> contentLines, string? newLine = null)
        {
            // Not a very efficient implementation, but it works.

            string xContent = string.Join ("\n", contentLines); // Will be extracted again.
            return xContent.Optimize (newLine: newLine)!;
        }

        public static IEnumerable <QuickNote> Parse (string str, string? newLine = null)
        {
            // Just the beginning and the end, not the inside part.
            var xLines = yyStringLines.TrimRedundantLines (str);

            Guid? xGuid = null;
            DateTime? xUtc = null;
            string? xTitle = null;
            List <string> xContentLines = [];

            for (int temp = 0; temp < xLines.Count; ) // Not auto-incremented.
            {
                string xCurrentLine = xLines [temp];
                LineParseResult xCurrentLineParseResult = ParseLine (xCurrentLine);

                if (xCurrentLineParseResult.Type == LineType.Guid || xCurrentLineParseResult.Type == LineType.Utc)
                {
                    QuickNote? xEntry = null;

                    if (xUtc != null && xContentLines.Count > 0)
                        xEntry = new (xGuid, xUtc.Value, xTitle, BuildAndOptimizeContent (xContentLines, newLine));

                    xGuid = null;
                    xUtc = null;
                    xTitle = null;
                    xContentLines.Clear ();

                    if (xEntry != null)
                        yield return xEntry;

                    if (xCurrentLineParseResult.Type == LineType.Guid)
                    {
                        xGuid = xCurrentLineParseResult.Guid;
                        temp ++;

                        string xNextLine = xLines [temp]; // May throw an exception and that's OK.
                        LineParseResult xNextLineParseResult = ParseLine (xNextLine);

                        if (xNextLineParseResult.Type != LineType.Utc)
                            throw new yyFormatException ("The GUID line must be followed by a UTC line.");

                        // The following code is a little redundant.
                        // Keep the 2 portions of code in sync.

                        xUtc = xNextLineParseResult.Utc;
                        temp ++;

                        string xNextNextLine = xLines [temp]; // May throw.
                        LineParseResult xNextNextLineParseResult = ParseLine (xNextNextLine);

                        if (xNextNextLineParseResult.Type == LineType.Title)
                        {
                            xTitle = xNextNextLineParseResult.Title;
                            temp ++;
                        }
                    }

                    else
                    {
                        xUtc = xCurrentLineParseResult.Utc;
                        temp ++;

                        string xNextLine = xLines [temp]; // May throw.
                        LineParseResult xNextLineParseResult = ParseLine (xNextLine);

                        if (xNextLineParseResult.Type == LineType.Title)
                        {
                            xTitle = xNextLineParseResult.Title;
                            temp ++;
                        }
                    }
                }

                else
                {
                    xContentLines.Add (xCurrentLine);
                    temp ++;
                }
            }

            // At EOF:
            if (xUtc != null && xContentLines.Count > 0)
                yield return new QuickNote (xGuid, xUtc.Value, xTitle, BuildAndOptimizeContent (xContentLines, newLine));

            yield break;
        }
    }
}
