using System;
using System.Collections.Generic;
using System.Linq;

using LibGit2Sharp;

namespace GitHours
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("USAGE EXAMPLES:");
                
                Console.WriteLine(@"githours.exe C:\git\repo1 2019-05-01");
                Console.WriteLine(@"githours.exe C:\git\repo1 C:\git\repo2 2019-05-01");

                return;
            }

            CountTotalHours(args.Take(args.Count() - 1), args.TakeLast(1).ToArray()[0]);

            Console.ReadLine();
        }

        static void CountTotalHours(IEnumerable<string> repos, string limit)
        {
            double totalHours = 0;
            Console.WriteLine($"Repo: ");
            foreach (var rs in repos)
            {
                var repo = new Repository(rs);
                var list = new List<DateTime>();
                var processed = new HashSet<string>();

                Console.WriteLine($" References: ");
                foreach (var rf in repo.Refs)
                {
                    Console.WriteLine($"  {rs} | {rf}");
                }

                Console.WriteLine($" Branches: ");
                foreach (var br in repo.Branches) //Add repo.Commits
                {
                    Console.WriteLine("  {0}", br.FriendlyName);
                    var commitarray = br.Commits.ToArray();
                    var lc = DateTime.MinValue;
                    for (int i = br.Commits.Count() - 1; i >= 0; i--)
                    {
                        var commit = commitarray[i];
                        var totalHoursBk = totalHours;

                        var interesting = (DateTime.Parse(limit) - lc.Date).TotalDays < 0;
                        if (lc > DateTime.MinValue)
                        {
                            if (processed.Contains(commit.Sha))
                                //already processed commit
                                continue;

                            var span = commit.Committer.When.UtcDateTime - lc;
                            if (span.TotalHours < 1)
                            {
                                //add time span
                                if (interesting)
                                    totalHours += span.TotalHours;
                            }
                            else if ((span.TotalHours >= 1) && (span.TotalHours < 2))
                            {
                                //protection for quick commits
                            }
                            else if (span.TotalHours >= 1)
                            {
                                //more than 2h between commits, probably idle >4h
                                if (interesting)
                                    totalHours += 1;
                            }
                        }

                        lc = commit.Committer.When.UtcDateTime;
                        if (interesting && totalHoursBk != totalHours)
                        {
                            Console.WriteLine($"  {commit.Sha.Substring(0, 6)} | {(int)totalHours}h | {commit.Committer.When.ToLocalTime()} | {commit.MessageShort}");
                            processed.Add(commit.Sha);
                        }
                    }
                    
                }
            }
        }
    }
}
