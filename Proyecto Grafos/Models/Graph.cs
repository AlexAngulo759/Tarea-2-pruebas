using System;

namespace Proyecto_Grafos.Models
{
    public class Graph
    {
        private Dictionary<string, Person> _people;
        private Dictionary<string, LinkedList<string>> _adjacencyList;

        public Graph()
        {
            _people = new Dictionary<string, Person>();
            _adjacencyList = new Dictionary<string, LinkedList<string>>();
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
        }

        public void AddRelationship(string parent, string child, bool setRelationships = true)
        {
            AddPerson(parent);
            AddPerson(child);

            _adjacencyList[parent].Add(child);

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

        public LinkedList<string> GetPeople()
        {
            return _people.Keys();
        }
    }
}