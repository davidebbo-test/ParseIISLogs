using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParseIISLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("e.g. ParseIISLogs.exe 2015 08");
                return;
            }

            string year = args[0];
            string month = args[1];

            var users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var usersInRegion = new Dictionary<string, HashSet<string>>();
            int requestCount = 0;
            int armRequestCount = 0;

            var allLogs = Enumerable.Empty<string>();

            // Find all the subtrees that match this year/month
            foreach (var topLevelDir in Directory.EnumerateDirectories("."))
            {
                string targetSubDir = Path.Combine(topLevelDir, year, month);
                if (Directory.Exists(targetSubDir))
                {
                    var logs = Directory.EnumerateFiles(targetSubDir, "*.log", SearchOption.AllDirectories);
                    allLogs = allLogs.Concat(logs);
                }
            }

            // e.g. .\EXPLORER-BAY\2015\07\03\02
            foreach (string file in allLogs.Where(s => { var segments = s.Split('\\'); return segments[2] == year && segments[3] == month; }))
            {
                foreach (string line in File.ReadAllLines(file))
                {
                    if (!line.Contains('@')) continue;

                    var parts = line.Split(' ');

                    requestCount++;
                    if (parts[3] == "POST" && parts[4] =="/api/operations")
                        armRequestCount++;

                    string user = parts[7];
                    if (user.Length < 2) continue;
                    users.Add(user);

                    // e.g. EXPLORER-HK1__24C4 -> HK1
                    string region = parts[2].Substring(9, 3);
                    HashSet<string> regionHashSet;
                    if (!usersInRegion.TryGetValue(region, out regionHashSet))
                    {
                        regionHashSet = usersInRegion[region] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                    regionHashSet.Add(user);
                }
            }

            Console.WriteLine($"Distinct users: {users.Count}");

            foreach (var region in usersInRegion.Keys)
            {
                Console.WriteLine($"{region}: {usersInRegion[region].Count}");
            }

            Console.WriteLine($"Total requests: {requestCount}");
            Console.WriteLine($"Total ARM requests: {armRequestCount}");


            //Console.WriteLine("***************************");

            //foreach (string user in users.OrderBy(u => u))
            //{
            //    Console.WriteLine(user);
            //}

            Console.WriteLine("***************************");

            var domains = from u in users
                          group u by u.Split('@')[1] into g
                          select new { Name = g.Key, Count = g.Count() };
            foreach (var domain in domains.Where(d => d.Count > 5).OrderByDescending(d => d.Count))
            {
                Console.WriteLine($"{domain.Name}: {domain.Count}");
            }
        }
    }
}
