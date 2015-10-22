using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseIISLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            var users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in Directory.GetFiles(".", "*.log", SearchOption.AllDirectories))
            {
                foreach (string line in File.ReadAllLines(file))
                {
                    if (line[0] == '#') continue;

                    var parts = line.Split(' ');
                    string user = parts[7];
                    if (user.Length < 2) continue;
                    users.Add(user);
                }
            }

            Console.WriteLine($"Distinct users: {users.Count}");
            foreach (string user in users.OrderBy(u => u))
            {
                Console.WriteLine(user);
            }
        }
    }
}
