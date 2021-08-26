using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Coomes.Equipper;

namespace ActivityDownloader 
{
    public class WriteAllRidesCSV : IActivityWriter
    {
        public string Name => "csv";

        public async Task Execute(IEnumerable<Activity> activities, string outfile)
        {
            var rides = activities
                .Where(a => a.Type == "Ride")
                .OrderByDescending(a => a.StartDate);

            var header = $"ID, StartDate, GearID, MovingTime, Distance, AverageSpeed, DeviceWatts, TotalElevationGain";
            var serializedRides = rides
                .Select(r => $"{r.Id}, {r.StartDate}, {r.GearId}, {r.MovingTime}, {r.Distance}, {r.AverageSpeed}, {r.DeviceWatts}, {r.TotalElevationGain}");

            var lines = serializedRides.Prepend(header);

            await File.WriteAllLinesAsync(outfile, lines);
        }
    }
}