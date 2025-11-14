using Proyecto_Grafos.Services;
using System;

namespace Proyecto_Grafos.Validate
{
    public class GraphValidator
    {
        private readonly GraphService _graphService;

        public GraphValidator(GraphService graphService)
        {
            _graphService = graphService;
        }

        public ValidationResult CanAddRoot(string personName)
        {
            var allPeople = _graphService.GetAllPeople();

            int rootCount = 0;
            for (int i = 0; i < allPeople.Count; i++)
            {
                var person = _graphService.GetPersonData(allPeople.Get(i));
                if (person != null && string.IsNullOrEmpty(person.ChildOf))
                {
                    rootCount++;
                }
            }

            if (rootCount >= 1)
            {
                return ValidationResult.Invalid("Ya existe un familiar inicial. Solo se permite un nodo raíz.");
            }

            if (_graphService.GetPersonData(personName) != null)
            {
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{personName}'.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddPredecessor(string childName, string predecessorName)
        {
            if (_graphService.GetPersonData(predecessorName) != null)
            {
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{predecessorName}'.");
            }

            var child = _graphService.GetPersonData(childName);
            if (child == null)
            {
                return ValidationResult.Invalid($"No se encontró la persona '{childName}'.");
            }

            var allPeople = _graphService.GetAllPeople();
            int predecessorCount = 0;

            for (int i = 0; i < allPeople.Count; i++)
            {
                var person = _graphService.GetPersonData(allPeople.Get(i));
                if (person != null && person.FatherOf == childName)
                {
                    predecessorCount++;
                }
            }

            if (predecessorCount >= 2)
            {
                return ValidationResult.Invalid($"El nodo '{childName}' ya tiene 2 predecesores/padres. No se pueden agregar más.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddSuccessor(string parentName, string successorName)
        {
            if (_graphService.GetPersonData(successorName) != null)
            {
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{successorName}'.");
            }

            var parent = _graphService.GetPersonData(parentName);
            if (parent == null)
            {
                return ValidationResult.Invalid($"No se encontró la persona '{parentName}'.");
            }

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddRelationship(string parentName, string childName)
        {
            var children = _graphService.GetChildren(parentName);
            for (int i = 0; i < children.Count; i++)
            {
                if (children.Get(i) == childName)
                {
                    return ValidationResult.Invalid($"Ya existe una relación entre '{parentName}' y '{childName}'.");
                }
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
    }
}