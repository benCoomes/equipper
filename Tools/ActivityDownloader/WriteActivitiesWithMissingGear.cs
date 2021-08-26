using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Coomes.Equipper;

namespace ActivityDownloader 
{
    public class WriteActivitesWithMissingGear : IActivityWriter
    {
        public string Name => "missing";

        public async Task Execute(IEnumerable<Activity> activities, string outfile)
        {
            var missingGear = activities
                .Where(a => string.IsNullOrWhiteSpace(a.GearId))
                .OrderByDescending(a => a.StartDate)
                .Select(a => new {
                    GearId = a.GearId,
                    ActivityId = a.Id,
                    Date = a.StartDate
                });

            var serializedActivities = JsonSerializer.Serialize(missingGear, new JsonSerializerOptions() { WriteIndented = true });

            await File.WriteAllTextAsync(outfile, serializedActivities);
        }
    }
}