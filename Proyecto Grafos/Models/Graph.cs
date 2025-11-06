using System;

namespace Proyecto_Grafos.Models
{
    public class Graph
    {
        private Dictionary<string, Person> _people;
        private Dictionary<string, LinkedList<string>> _adjacencyList;
        private Dictionary<string, LinkedList<string>> _parentList; 
        public Graph()
        {
            _people = new Dictionary<string, Person>();
            _adjacencyList = new Dictionary<string, LinkedList<string>>();
            _parentList = new Dictionary<string, LinkedList<string>>();
        }

        public void AddPerson(string name, double latitude = 0.0, double longitude = 0.0) 
        {
            if (!_people.ContainsKey(name))
            {
                _people.Add(name, new Person(name, latitude, longitude));
            }

            if (!_adjacencyList.ContainsKey(name))
            {
                _adjacencyList.Add(name, new LinkedList<string>());
            }

            if (!_parentList.ContainsKey(name))
            {
                _parentList.Add(name, new LinkedList<string>());
            }
        }

        public void AddRelationship(string parent, string child, bool setRelationships = true)
        {
            AddPerson(parent);
            AddPerson(child);

            if (!_adjacencyList[parent].Contains(child))
            {
                _adjacencyList[parent].Add(child);
            }

            if (!_parentList[child].Contains(parent))
            {
                _parentList[child].Add(parent);
            }

            if (setRelationships)
            {
                _people[parent].FatherOf = child;
                _people[child].ChildOf = parent;
            }
        }

        public Person GetPerson(string name)
        {
            if (_people.ContainsKey(name))
            {
                return _people[name];
            }
            return null;
        }

        public LinkedList<string> GetChildren(string person)
        {
            if (_adjacencyList.ContainsKey(person))
            {
                return _adjacencyList[person];
            }
            return new LinkedList<string>();
        }
        public string GetParent(string childName)
        {
            var person = GetPerson(childName);
            return person?.ChildOf ?? string.Empty;
        }

        public LinkedList<string> GetParents(string person)
        {
            if (_parentList.ContainsKey(person))
            {
                return _parentList[person];
            }
            return new LinkedList<string>();
        }

        public LinkedList<string> GetPeople()
        {
            return _people.Keys();
        }
    }
}