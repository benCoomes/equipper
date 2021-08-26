using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Coomes.Equipper;

namespace ActivityDownloader 
{
    public class WriteAllRides : IActivityWriter
    {
        public string Name => "rides";

        public async Task Execute(IEnumerable<Activity> activities, string outfile)
        {
            var rides = activities
                .Where(a => a.Type == "Ride")
                .OrderByDescending(a => a.StartDate);

            var seriazliedRides = JsonSerializer.Serialize(rides, new JsonSerializerOptions() { WriteIndented = true });
            
            await File.WriteAllTextAsync(outfile, seriazliedRides);
        }
    }
}