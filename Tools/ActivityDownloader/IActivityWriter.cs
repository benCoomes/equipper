using System.Collections.Generic;
using System.Threading.Tasks;
using Coomes.Equipper;

namespace ActivityDownloader
{

    public interface IActivityWriter 
    {
        string Name { get; }
        Task Execute(IEnumerable<Activity> activities, string outfile);
    }
}