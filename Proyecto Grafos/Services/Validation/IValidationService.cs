using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Services.Validation;

namespace Proyecto_Grafos.Services.Validation
{
    public interface IValidationService
    {
        ValidationResult CanAddRoot(string personName);
        ValidationResult CanAddSuccessor(string parentName, string successorName);
        ValidationResult CanAddSibling(string siblingName, string newSiblingName);
        ValidationResult CanAddPredecessor(string childName, string predecessorName);
        ValidationResult ValidatePersonName(string name);
        ValidationResult CanAddRelationship(string parentName, string childName);
    }
}