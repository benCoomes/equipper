using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.StravaApi;
using System.Collections.Generic;
using Coomes.Equipper;
using System.IO;

namespace ActivityDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var accessToken = args.Length > 0 ? args[0] : null;
            var outfile = args.Length > 1 ? args[1] : null;
            if(string.IsNullOrWhiteSpace(accessToken)
                || string.IsNullOrWhiteSpace(outfile))
            {
                Console.WriteLine("Usage: dotnet run <access token> <outfile>");
                return;
            }

            Console.WriteLine("Starting activity download...");

            IActivityData activityData = new ActivityClient();
            var activities = await activityData.GetActivities(accessToken, limit: 200);
            var serializedActivities = JsonSerializer.Serialize(activities);

            Console.WriteLine($"Retrieved {activities.Count()} activities.");

            await WriteActivitiesWithMissingGear(activities, outfile);
        }

        private static async Task WriteToConsole(IEnumerable<Activity> activities, string outfile) 
        {
            var serializedActivities = JsonSerializer.Serialize(activities);
            await File.WriteAllTextAsync(outfile, serializedActivities);
        }

        private static async Task WriteActivitiesWithMissingGear(IEnumerable<Activity> activities, string outfile) 
        {
            var missingGear = activities
                .Where(a => string.IsNullOrWhiteSpace(a.GearId))
                .OrderByDescending(a => a.StartDate)
                .Select(a => new {
                    GearId = a.GearId,
                    ActivityId = a.Id,
                    Date = a.StartDate
                });

            var serializedActivities = JsonSerializer.Serialize(missingGear);

            await File.WriteAllTextAsync(outfile, serializedActivities);
        }
    }
}
