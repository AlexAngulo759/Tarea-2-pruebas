using Proyecto_Grafos.Services.Validation;

namespace Proyecto_Grafos.Core.Interfaces
{
    public interface IValidationService
    {
        ValidationResult CanAddRoot(string personName);
        ValidationResult CanAddSuccessor(string parentName, string successorName);
        ValidationResult CanAddPredecessor(string childName, string predecessorName);
        ValidationResult ValidatePersonName(string name);
        ValidationResult CanAddRelationship(string parentName, string childName);
    }
}