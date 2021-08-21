using System;

namespace Coomes.Equipper.StravaApi.Models
{
    internal interface StravaModel<T> 
    {
        T ToDomainModel();
    }
}