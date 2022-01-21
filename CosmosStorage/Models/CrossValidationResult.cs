using System.Collections.Generic;
using System.Linq;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class CrossValidationResult
    {
        public int Total { get; set; }
        public int Correct { get; set; }
        public string AlgorithmName { get; set; }

        public CrossValidationResult()
        {
        }

        public CrossValidationResult(Domain.CrossValidationResult domainModel)
        {
            Total = domainModel.Total;
            Correct = domainModel.Correct;
            AlgorithmName = domainModel.AlgorithmName;
        }

        public Domain.CrossValidationResult ToDomainModel()
        {
            return new Domain.CrossValidationResult()
            {
                Total = Total,
                Correct = Correct,
                AlgorithmName = AlgorithmName
            };
        }
    } 

    internal static class CrossValidationCollectionExtensions
    {
        public static CrossValidationResult[] ToDataModels(this IEnumerable<Domain.CrossValidationResult> domainModels)
        {
            return domainModels
                .Select(domain => new CrossValidationResult(domain))
                .ToArray();
        }

        public static List<Domain.CrossValidationResult> ToDomainModels(this IEnumerable<CrossValidationResult> dataModels)
        {
            return dataModels
                .Select(data => data.ToDomainModel())
                .ToList();
        }
    }
}