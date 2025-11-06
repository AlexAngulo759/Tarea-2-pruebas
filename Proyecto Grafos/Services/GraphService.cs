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

                _familyTree.AddRelationship(parent, child);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddSibling(string existingSibling, string newSibling)
        {
            try
            {
                var siblingValidation = _validator.CanAddSibling(existingSibling, newSibling);
                if (!siblingValidation.IsValid)
                    return false;

                var parents = _familyTree.GetParents(existingSibling);
                if (parents.Count == 0)
                    return false;

                _familyTree.AddPerson(newSibling);

                for (int i = 0; i < parents.Count; i++)
                {
                    string parent = parents.Get(i);
                    _familyTree.AddRelationship(parent, newSibling, setRelationships: false);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AddSibling: {ex.Message}");
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

        public ValidationResult ValidateAddSibling(string siblingName, string newSiblingName)
        {
            return _validator.CanAddSibling(siblingName, newSiblingName);
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

        public string GetParent(string personName)
        {
            return _familyTree.GetParent(personName);
        }
        public Models.LinkedList<string> GetParents(string personName)
        {
            return _familyTree.GetParents(personName);
        }

        public Models.LinkedList<string> GetSiblings(string personName)
        {
            var siblings = new Models.LinkedList<string>();
            var parent = GetParent(personName);

            if (!string.IsNullOrEmpty(parent))
            {
                var children = GetChildren(parent);
                for (int i = 0; i < children.Count; i++)
                {
                    string child = children.Get(i);
                    if (child != personName)
                    {
                        siblings.Add(child);
                    }
                }
            }

            return siblings;
        }
    }
}