using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Services.Validation;
using Proyecto_Grafos.Services.Validation;

namespace Proyecto_Grafos
{
    public static class DependencyConfig
    {
        public static GraphService CreateGraphService()
        {
            IFamilyGraph familyGraph = new FamilyGraph();
            IValidationService validator = new GraphValidator(familyGraph);
            return new GraphService(familyGraph, validator);
        }

        public static GraphService CreateGraphService(IFamilyGraph familyGraph, IValidationService validator)
        {
            return new GraphService(familyGraph, validator);
        }
    }
}