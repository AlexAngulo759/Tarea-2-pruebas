using System;

namespace Proyecto_Grafos.Services
{
    public class GraphService
    {
        private Models.Graph _familyTree;

        public GraphService()
        {
            _familyTree = new Models.Graph();
        }

        public bool AddPerson(string name, double latitude = 0.0, double longitude = 0.0)
        {
            if (string.IsNullOrEmpty(name))
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