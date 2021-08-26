using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Coomes.Equipper;

namespace ActivityDownloader 
{
    public class WriteOutliers : IActivityWriter
    {
        public string Name => "outliers";

        public async Task Execute(IEnumerable<Activity> activities, string outfile)
        {
            var ridesWithGear = activities
                .Where(a => !string.IsNullOrWhiteSpace(a.GearId) && a.Type == "Ride");

            var gearStats = ridesWithGear
                .GroupBy(a => a.GearId)
                .Select(g => new {
                    GearId = g.Key,
                    AverageSpeed = g.Average(a => a.AverageSpeed),
                    StdDev = StandardDeviation(g.Select(a => a.AverageSpeed))
                })
                .ToDictionary(g => g.GearId, g => g);

            var outliers = ridesWithGear
                .Select(a => {
                    var gear = gearStats[a.GearId];
                    var diff = Math.Abs(a.AverageSpeed - gear.AverageSpeed);
                    return new {
                        ActivityID = a.Id,
                        GearId = gear.GearId,
                        GearAverageSpeed = gear.AverageSpeed,
                        ActivityAverageSpeed = a.AverageSpeed,
                        Difference = diff,
                        StandardDeviation = gear.StdDev,
                        IsOutlier = diff > (gear.StdDev) // rides are remarkably consistent!
                    };
                })
                .Where(o => o.IsOutlier);
            
            var serializedResults = JsonSerializer.Serialize(outliers, new JsonSerializerOptions() { WriteIndented = true });

            await File.WriteAllTextAsync(outfile, serializedResults);   
        }

        private static double StandardDeviation(IEnumerable<double> values) {
            var mean = values.Average();
            var sumSquaredDiffs = values
                .Select(v => Math.Pow((v - mean), 2))
                .Sum();
            var variance = values.Sum() / (values.Count() - 1); // assume sample
            var stdDev = Math.Sqrt(variance);
            return stdDev;
        }
    }
}