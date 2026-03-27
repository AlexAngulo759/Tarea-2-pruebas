using System;
using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Services.Validation;

namespace Proyecto_Grafos.Services
{
    public class GraphService
    {
        private readonly IFamilyGraph _familyTree;
        private readonly IValidationService _validator;

        public GraphService(IFamilyGraph familyTree, IValidationService validator)
        {
            _familyTree = familyTree ?? throw new ArgumentNullException(nameof(familyTree));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public bool AddPerson(string name, double latitude = 0.0, double longitude = 0.0,
                              string cedula = "", DateTime? fechaNacimiento = null,
                              bool estaVivo = true, DateTime? fechaFallecimiento = null,
                              string photoPath = "")
        {
            if (string.IsNullOrEmpty(name))
                return false;

            var nameValidation = _validator.ValidatePersonName(name);
            if (!nameValidation.IsValid)
                return false;

            try
            {
                _familyTree.AddPerson(name, latitude, longitude, cedula, fechaNacimiento,
                                      estaVivo, fechaFallecimiento, photoPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddRelationship(string parent, string child)
        {
            try
            {
                var relationshipValidation = _validator.CanAddRelationship(parent, child);
                if (!relationshipValidation.IsValid)
                    return false;

                _familyTree.AddRelationship(parent, child);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ValidationResult ValidateAddRoot(string personName) => _validator.CanAddRoot(personName);
        public ValidationResult ValidateAddPredecessor(string childName, string predecessorName) => _validator.CanAddPredecessor(childName, predecessorName);
        public ValidationResult ValidateAddSuccessor(string parentName, string successorName) => _validator.CanAddSuccessor(parentName, successorName);

        public LinkedList<string> GetChildren(string personName) => _familyTree.GetChildren(personName);
        public LinkedList<string> GetPeople() => _familyTree.GetPeople();
        public Person GetPerson(string name) => _familyTree.GetPerson(name);
        public LinkedList<string> GetParents(string personName) => _familyTree.GetParents(personName);

        public bool UpdatePersonName(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
                return false;
            if (oldName == newName)
                return true;

            var existingPerson = _familyTree.GetPerson(newName);
            if (existingPerson != null)
                return false;

            var nameValidation = _validator.ValidatePersonName(newName);
            if (!nameValidation.IsValid)
                return false;

            try
            {
                var person = _familyTree.GetPerson(oldName);
                if (person == null)
                    return false;

                return _familyTree.UpdatePersonName(oldName, newName);
            }
            catch
            {
                return false;
            }
        }
    }
}