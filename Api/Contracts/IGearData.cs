using System.Threading.Tasks;
using System.Collections.Generic;

namespace Coomes.Equipper.Contracts
{
    public interface IGearData
    {
        Task<Gear> GetGear(string accessToken, string gearId);
    }
}