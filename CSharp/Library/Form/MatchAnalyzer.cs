using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    internal class MatchAnalyzer
    {
        internal static void PrintMatches(IEnumerable<TermMatch> matches, int offset = 0)
        {
            foreach (var match in matches)
            {
                string message = "";
                message = message.PadRight(match.Start + offset, ' ');
                message = message.PadRight(match.End + offset, '_');
                Console.WriteLine("{0} {1}", message, match.Value);
            }
        }

        internal static bool IsIgnorable(string input, int start, int end)
        {
            return Language.NonWord(input.Substring(start, end - start));
        }

        internal static bool IsSpecial(object value)
        {
            return value is SpecialValues && (SpecialValues)value == SpecialValues.Field;
        }

        // Collapse together subsequent matches for same value
        internal static IEnumerable<TermMatch> Coalesce(IEnumerable<TermMatch> matches, string input)
        {
            var sorted = (from match in matches orderby match.Start ascending, match.End ascending select match).ToList();
            while (sorted.Count() > 0)
            {
                var current = sorted.First();
                sorted.Remove(current);
                bool emit = true;
                foreach (var next in sorted.ToList())
                {
                    if (next.Covers(current))
                    {
                        // Current is completely covered by a subsequent match
                        emit = false;
                        break;
                    }
                    else if (current.End < next.Start)
                    {
                        var gap = next.Start - current.End;
                        if (gap > 1 && !Language.NonWord(input.Substring(current.End, gap)))
                        {
                            // Unmatched word means we can't merge any more
                            emit = true;
                            break;
                        }
                        else if (current.Value == next.Value || IsSpecial(current.Value) || IsSpecial(next.Value))
                        {
                            // Comptabile, extend current match
                            current = new TermMatch(current.Start, next.End - current.Start, Math.Max(current.Confidence, next.Confidence),
                                        IsSpecial(current.Value) ? next.Value : current.Value);
                            sorted.Remove(next);
                        }
                    }
                }
                if (emit)
                {
                    sorted = (from match in sorted where !current.Covers(match) select match).ToList();
                    yield return current;
                }
            }
        }

        internal static IEnumerable<TermMatch> HighestConfidence(IEnumerable<TermMatch> matches)
        {
            var sorted = (from match in matches orderby match.Start ascending, match.End ascending, match.Confidence descending select match);
            TermMatch last = null;
            foreach (var match in sorted)
            {
                if (last == null || !last.Same(match))
                {
                    last = match;
                    yield return match;
                }
            }
        }

        // Full match if everything left is white space or punctuation
        internal static bool IsFullMatch(string input, IEnumerable<TermMatch> matches)
        {
            bool fullMatch = matches.Count() > 0;
            var sorted = from match in matches orderby match.Start ascending select match;
            var current = 0;
            var minConfidence = 1.0;
            foreach (var match in sorted)
            {
                if (match.Start > current)
                {
                    if (!IsIgnorable(input, current, match.Start))
                    {
                        fullMatch = false;
                        break;
                    }
                }
                if (match.Confidence < minConfidence)
                {
                    minConfidence = match.Confidence;
                }
                current = match.End;
            }
            if (fullMatch && current < input.Length)
            {
                fullMatch = IsIgnorable(input, current, input.Length);
            }
            return fullMatch && minConfidence == 1.0;
        }

        internal static IEnumerable<string> Unmatched(string input, IEnumerable<TermMatch> matches)
        {
            var unmatched = new List<string>();
            var sorted = from match in matches orderby match.Start ascending select match;
            var current = 0;
            foreach (var match in sorted)
            {
                if (match.Start > current)
                {
                    if (!IsIgnorable(input, current, match.Start))
                    {
                        yield return input.Substring(current, match.Start - current).Trim();
                    }
                }
                current = match.End;
            }
            if (input.Length > current)
            {
                yield return input.Substring(current).Trim();
            }
        }

        internal static double MinConfidence(IEnumerable<TermMatch> matches)
        {
            return matches.Count() == 0 ? 0.0 : (from match in matches select match.Confidence).Min();
        }

        internal static int Coverage(IEnumerable<TermMatch> matches)
        {
            // TODO: This does not handle partial overlaps
            return matches.Count() == 0 ? 0 : (from match in GroupedMatches(matches) select match.First().Length).Sum();
        }

        internal static int BestMatches(params IEnumerable<TermMatch>[] allMatches)
        {
            int bestMatch = 0;
            var confidences = (from matches in allMatches select MinConfidence(matches)).ToArray();
            int bestCoverage = 0;
            double bestConfidence = 0;
            for (var i = 0; i < allMatches.Length; ++i)
            {
                var confidence = confidences[i];
                var coverage = allMatches[i].Count();
                if (confidence > bestConfidence)
                {
                    bestConfidence = confidence;
                    bestCoverage = coverage;
                    bestMatch = i;
                }
                else if (confidence == bestConfidence && coverage > bestCoverage)
                {
                    bestCoverage = coverage;
                    bestMatch = i;
                }
            }
            return bestMatch;
        }

        internal static List<List<TermMatch>> GroupedMatches(IEnumerable<TermMatch> matches)
        {
            var groups = new List<List<TermMatch>>();
            var sorted = from match in matches orderby match.Start ascending, match.End descending select match;
            var current = sorted.FirstOrDefault();
            var currentGroup = new List<TermMatch>();
            foreach (var match in sorted)
            {
                if (match != current)
                {
                    if (current.Same(match))
                    {
                        // Ambiguous match
                        currentGroup.Add(match);
                    }
                    else if (!current.Overlaps(match))
                    // TODO: We are not really handling partial overlap.  To do so we need a lattice.
                    {
                        // New group
                        currentGroup.Add(current);
                        groups.Add(currentGroup);
                        current = match;
                        currentGroup = new List<TermMatch>();
                    }
                }
            }
            if (current != null)
            {
                currentGroup.Add(current);
                groups.Add(currentGroup);
            }
            return groups;
        }
    }
}
