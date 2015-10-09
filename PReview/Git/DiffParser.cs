using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PReview.Git;

namespace PReview
{
    public class DiffParser
    {
        private readonly string _diffFilePath;
        private readonly string _solutionDir;

        public DiffParser(string solutionDir)
        {
            _solutionDir = solutionDir;
            _diffFilePath = Path.Combine(_solutionDir, "diff.txt");
        }

        public async Task<Dictionary<string, UnifiedDiff>> ParseAsync()
        {
            var unifiedDiffs = new List<UnifiedDiff>();
            var lines = new List<string>();

            var unifiedDiffParser = new UnifiedDiffParser(0);

            using (var reader = File.OpenText(_diffFilePath))
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

            return unifiedDiffs.ToDictionary(diff => _solutionDir.ToLower() + "\\" + diff.NewFile.Replace("/", "\\").ToLower());
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