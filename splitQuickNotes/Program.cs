using System.Text;
using yyLib;

namespace splitQuickNotes
{
    class Program
    {
        static void Main (string [] args)
        {
            try
            {
                if (args.Length == 0 || args.Length >= 2)
                {
                    Console.WriteLine ("Usage: splitQuickNotes.exe <input file>");
                    return;
                }

                string xInputFilePath = args [0],
                       xInputFileExtension = Path.GetExtension (xInputFilePath),
                       xInputFileContents = File.ReadAllText (xInputFilePath, Encoding.UTF8);

                bool xIsMarkdown = xInputFileExtension.Equals (".md", StringComparison.OrdinalIgnoreCase);

                string xOutputDirectoryName = $"splitQuickNotes-{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}",
                       xOutputDirectoryPath = yyPath.GetAbsolutePath (yySpecialDirectories.Desktop, xOutputDirectoryName);

                Directory.CreateDirectory (xOutputDirectoryPath);

                foreach (var xEntry in QuickNotesParser.Parse (xInputFileContents))
                {
                    string xTitlePart = string.IsNullOrEmpty (xEntry.Title) == false ?
                               $" {yyPath.ReplaceAllInvalidFileNameChars (xEntry.Title)}" : string.Empty,
                           xOutputFileName = $"{xEntry.Utc:yyyyMMdd'T'HHmmss'Z'}{xTitlePart}{xInputFileExtension}",
                           xOutputFilePath = yyPath.GetAbsolutePath (xOutputDirectoryPath, xOutputFileName);

                    StringBuilder xBuilder = new ();

                    if (xIsMarkdown)
                    {
                        xBuilder.AppendLine ("<!--");

                        if (xEntry.Guid.HasValue)
                            xBuilder.AppendLine ($"GUID: {yyFormatter.GuidToString (xEntry.Guid.Value)}");

                        xBuilder.AppendLine ($"UTC: {yyFormatter.ToRoundtripString (xEntry.Utc)}");

                        if (string.IsNullOrEmpty (xEntry.Title) == false)
                            xBuilder.AppendLine ($"Title: {xEntry.Title}");

                        xBuilder.AppendLine ("-->");
                    }

                    else
                    {
                        if (xEntry.Guid.HasValue)
                            xBuilder.AppendLine ($"GUID: {yyFormatter.GuidToString (xEntry.Guid.Value)}");

                        xBuilder.AppendLine ($"UTC: {yyFormatter.ToRoundtripString (xEntry.Utc)}");

                        if (string.IsNullOrEmpty (xEntry.Title) == false)
                            xBuilder.AppendLine ($"Title: {xEntry.Title}");
                    }

                    xBuilder.AppendLine ();
                    xBuilder.AppendLine (xEntry.Content);

                    File.WriteAllText (xOutputFilePath, xBuilder.ToString (), Encoding.UTF8);
                    Console.WriteLine ($"Created: {xOutputFileName}");
                }
            }

            catch (Exception xException)
            {
                yyLogger.Default.TryWriteException (xException);
                Console.WriteLine (xException.ToString ());
            }

            finally
            {
                Console.Write ("Press any key to exit: ");
                Console.ReadKey (true);
                Console.WriteLine ();
            }
        }
    }
}
