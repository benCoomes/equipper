using System;
using System.Linq;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.StravaApi;
using System.Collections.Generic;

namespace ActivityDownloader
{
    class Program
    {
        static List<IActivityWriter> _operations = new List<IActivityWriter>()
        {
            new WriteActivitesWithMissingGear(),
            new WriteAllRides(),
            new WriteAllRidesCSV(),
            new WriteOutliers()
        };

        static async Task Main(string[] args)
        {
            var command = args.Length > 0 ? args[0] : null;
            var accessToken = args.Length > 1 ? args[1] : null;
            var outfile = args.Length > 2 ? args[2] : null;

            var supportedOps = _operations.Select(o => o.Name.ToLower()).Distinct();
            if(!supportedOps.Contains(command?.ToLower())
                || string.IsNullOrWhiteSpace(outfile)
                || string.IsNullOrWhiteSpace(accessToken))
            {
                Console.WriteLine("Usage: dotnet run <command> <access token> <outfile>");
                Console.WriteLine($"Valid commands are: {string.Join(", ", supportedOps)}");
                return;
            }

            Console.WriteLine("Starting activity download...");
            
            IActivityData activityData = new ActivityClient(null);
            var activities = await activityData.GetActivities(accessToken, limit: 200);
            
            Console.WriteLine($"Retrieved {activities.Count()} activities.");

            var op = _operations.First(o => string.Equals(o.Name, command, StringComparison.InvariantCultureIgnoreCase));
            await op.Execute(activities, outfile);

            Console.WriteLine("Done.");
        }
    }
}
