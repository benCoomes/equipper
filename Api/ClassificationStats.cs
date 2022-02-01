using System;
using System.Collections.Generic;

namespace Coomes.Equipper
{
    public class ClassificationStats
    {
        public Guid Id { get; set; }
        public List<CrossValidationResult> CrossValidations { get; set; } = new List<CrossValidationResult>();
    }
}