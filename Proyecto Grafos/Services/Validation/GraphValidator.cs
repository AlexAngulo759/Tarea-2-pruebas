using Proyecto_Grafos.Core.Interfaces;

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
            var allPeople = _familyGraph.GetPeople();
            int rootCount = 0;
            for (int i = 0; i < allPeople.Count; i++)
            {
                var name = allPeople.Get(i);
                if (_familyGraph.GetParents(name).Count == 0)
                    rootCount++;
            }

            if (rootCount >= 1)
                return ValidationResult.Invalid("Ya existe un familiar inicial. Solo se permite un nodo raíz.");

            if (_familyGraph.GetPerson(personName) != null)
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{personName}'.");

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddSuccessor(string parentName, string successorName)
        {
            if (_familyGraph.GetPerson(successorName) != null)
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{successorName}'.");

            var parent = _familyGraph.GetPerson(parentName);
            if (parent == null)
                return ValidationResult.Invalid($"No se encontró la persona '{parentName}'.");

            return ValidationResult.Valid();
        }

        public ValidationResult CanAddPredecessor(string childName, string predecessorName)
        {
            if (_familyGraph.GetPerson(predecessorName) != null)
                return ValidationResult.Invalid($"Ya existe una persona con el nombre '{predecessorName}'.");

            var child = _familyGraph.GetPerson(childName);
            if (child == null)
                return ValidationResult.Invalid($"No se encontró la persona '{childName}'.");

            var parents = _familyGraph.GetParents(childName);
            if (parents.Count >= 2)
                return ValidationResult.Invalid($"El nodo '{childName}' ya tiene 2 predecesores/padres.");

            return ValidationResult.Valid();
        }

        public ValidationResult ValidatePersonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ValidationResult.Invalid("El nombre no puede estar vacío.");
            if (name.Length > 50)
                return ValidationResult.Invalid("El nombre no puede tener más de 50 caracteres.");
            return ValidationResult.Valid();
        }

        public ValidationResult CanAddRelationship(string parentName, string childName)
        {
            if (_familyGraph.GetPerson(parentName) == null)
                return ValidationResult.Invalid($"No se encontró el padre '{parentName}'.");
            if (_familyGraph.GetPerson(childName) == null)
                return ValidationResult.Invalid($"No se encontró el hijo '{childName}'.");

            var children = _familyGraph.GetChildren(parentName);
            for (int i = 0; i < children.Count; i++)
            {
                if (children.Get(i) == childName)
                    return ValidationResult.Invalid($"Ya existe una relación entre '{parentName}' y '{childName}'.");
            }
            return ValidationResult.Valid();
        }
    }
}