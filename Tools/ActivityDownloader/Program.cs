using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.StravaApi;

namespace ActivityDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var accessToken = args.Length > 0 ? args[0] : null;
            if(string.IsNullOrWhiteSpace(accessToken)) {
                Console.WriteLine("Please provide an access token as the single parameter.");
                return;
            }

            Console.WriteLine("Starting activity download...");

            IActivityData activityData = new ActivityClient();
            var activities = await activityData.GetActivities(accessToken);
            var serializedActivities = JsonSerializer.Serialize(activities);

            Console.WriteLine($"Retrieved {activities.Count()} activities: ");
            Console.WriteLine(serializedActivities);
        }
    }
}
