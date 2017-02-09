using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PReview.Git;
using System.Net;
using System;

namespace PReview
{
    public class DiffParser
    {
        private readonly string _solutionDir;

        public DiffParser(string solutionDir)
        {
            _solutionDir = solutionDir;
        }

        public async Task<Dictionary<string, UnifiedDiff>> ParseAsync()
        {
            var unifiedDiffs = new List<UnifiedDiff>();
            var lines = new List<string>();

            var unifiedDiffParser = new UnifiedDiffParser(3);

            var reader = FindPatchReader(_solutionDir);
            if(reader != null)
            {
                using (reader)
                {
                    var n = 0;

                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.StartsWith("diff --git"))
                        {
                            var unifiedDiff = new UnifiedDiff(line);
                            if (n > 0)
                            {
                                unifiedDiffs[unifiedDiffs.Count - 1].HunkRanges.AddRange(unifiedDiffParser.Parse(lines));
                            }

                            n++;

                            unifiedDiffs.Add(unifiedDiff);

                            lines.Clear();

                            lines.Add(line);
                        }
                        else
                        {
                            lines.Add(line);
                        }
                    }
                }
            }

            return unifiedDiffs.ToDictionary(diff => _solutionDir.ToLower() + "\\" + diff.NewFile.Replace("/", "\\").ToLower());
        }

        private static TextReader FindPatchReader(string solutionDir)
        {
            var diffUrl = (string)AppDomain.CurrentDomain.GetData("PReview.diff.url");
            if (diffUrl != null)
            {
                var webClient = new WebClient();
                var stream = webClient.OpenRead(diffUrl);
                return new StreamReader(stream);
            }

            var patchFile = Path.Combine(solutionDir, "diff.txt");
            if (File.Exists(patchFile))
            {
                return File.OpenText(patchFile);
            }

            return null;
        }
    }

    public class UnifiedDiff
    {
        public UnifiedDiff(string firstLine)
        {
            HunkRanges = new List<HunkRangeInfo>();

            var split = firstLine.Split(' ');

            OriginalFile = split[2].Substring(2);
            NewFile = split[3].Substring(2);
        }

        public string OriginalFile { get; }

        public string NewFile { get; }

        public List<HunkRangeInfo> HunkRanges { get; }
    }
}