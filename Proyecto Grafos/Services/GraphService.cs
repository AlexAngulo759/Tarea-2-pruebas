using System;
using Proyecto_Grafos.Validate;

namespace Proyecto_Grafos.Services
{
    public class GraphService
    {
        private Models.Graph _familyTree;
        private GraphValidator _validator;

        public GraphService()
        {
            _familyTree = new Models.Graph();
            _validator = new GraphValidator(this);
        }

        public bool AddPerson(string name, double latitude = 0.0, double longitude = 0.0)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            var nameValidation = _validator.ValidatePersonName(name);
            if (!nameValidation.IsValid)
                return false;

            try
            {
                _familyTree.AddPerson(name, latitude, longitude);
                return true;
            }
            catch (Exception)
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

                _familyTree.AddPerson(parent);
                _familyTree.AddPerson(child);
                _familyTree.AddRelationship(parent, child);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ValidationResult ValidateAddRoot(string personName)
        {
            return _validator.CanAddRoot(personName);
        }

        public ValidationResult ValidateAddPredecessor(string childName, string predecessorName)
        {
            return _validator.CanAddPredecessor(childName, predecessorName);
        }

        public ValidationResult ValidateAddSuccessor(string parentName, string successorName)
        {
            return _validator.CanAddSuccessor(parentName, successorName);
        }

        public Models.LinkedList<string> GetChildren(string personName)
        {
            return _familyTree.GetChildren(personName);
        }

        public Models.LinkedList<string> GetAllPeople()
        {
            return _familyTree.GetPeople();
        }

        public Models.Person GetPersonData(string name)
        {
            return _familyTree.GetPerson(name);
        }
    }
}