using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Services.Validation;

namespace Proyecto_Grafos.Services.Validation
{
    public class GraphValidator : IValidationService
    {
        private readonly IFamilyGraph _familyGraph;

        public GraphValidator(IFamilyGraph familyGraph)
        {
            _familyGraph = familyGraph;
        }

        public ValidationResult CanAddRoot(string personName)
        {
            var allPeople = _familyGraph.GetAllPeople();

            int rootCount = 0;
            for (int i = 0; i < allPeople.Count; i++)
            {
                var person = _familyGraph.GetPersonData(allPeople.Get(i));
                if (person != null && string.IsNullOrEmpty(person.ChildOf))
                {
                    rootCount++;
                }
            }

            if (rootCount >= 1)
            {
                return ValidationResult.Invalid("Ya existe un familiar inicial. Solo se permite un nodo raíz.");
            }

            if (_familyGraph.GetPersonData(personName) != null)
            {
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{personName}'.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddSuccessor(string parentName, string successorName)
        {
            if (_familyGraph.GetPersonData(successorName) != null)
            {
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{successorName}'.");
            }

            var parent = _familyGraph.GetPersonData(parentName);
            if (parent == null)
            {
                return ValidationResult.Invalid($"No se encontró la persona '{parentName}'.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddSibling(string siblingName, string newSiblingName)
        {
            if (_familyGraph.GetPersonData(newSiblingName) != null)
            {
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{newSiblingName}'.");
            }

            var sibling = _familyGraph.GetPersonData(siblingName);
            if (sibling == null)
            {
                return ValidationResult.Invalid($"No se encontró la persona '{siblingName}'.");
            }

            if (string.IsNullOrEmpty(sibling.ChildOf))
            {
                return ValidationResult.Invalid($"No se pueden agregar hermanos a '{siblingName}' porque no tiene padres definidos.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddPredecessor(string childName, string predecessorName)
        {
            if (_familyGraph.GetPersonData(predecessorName) != null)
            {
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{predecessorName}'.");
            }

            var child = _familyGraph.GetPersonData(childName);
            if (child == null)
            {
                return ValidationResult.Invalid($"No se encontró la persona '{childName}'.");
            }

            var parents = _familyGraph.GetParents(childName);
            if (parents.Count >= 2)
            {
                return ValidationResult.Invalid($"El nodo '{childName}' ya tiene 2 predecesores/padres. No se pueden agregar más.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult ValidatePersonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Invalid("El nombre no puede estar vacío.");
            }

            if (name.Length > 50)
            {
                return ValidationResult.Invalid("El nombre no puede tener más de 50 caracteres.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddRelationship(string parentName, string childName)
        {
            var children = _familyGraph.GetChildren(parentName);
            for (int i = 0; i < children.Count; i++)
            {
                if (children.Get(i) == childName)
                {
                    return ValidationResult.Invalid($"Ya existe una relación entre '{parentName}' y '{childName}'.");
                }
            }

            return ValidationResult.Valid();
        }
    }
}