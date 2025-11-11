using System;
using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Models;

namespace Proyecto_Grafos.Core.Models
{
    public class FamilyGraph : IFamilyGraph
    {
        private Dictionary<string, Person> _people;
        private Dictionary<string, LinkedList<string>> _adjacencyList;
        private Dictionary<string, LinkedList<string>> _parentList;

        public FamilyGraph()
        {
            _people = new Dictionary<string, Person>();
            _adjacencyList = new Dictionary<string, LinkedList<string>>();
            _parentList = new Dictionary<string, LinkedList<string>>();
        }
        public Person GetPersonData(string name)
        {
            return GetPerson(name); 
        }
        public LinkedList<string> GetAllPeople()
        {
            return GetPeople(); 
        }

        public void AddPerson(string name, double latitude = 0.0, double longitude = 0.0,
                           string cedula = "", DateTime? fechaNacimiento = null,
                           bool estaVivo = true, DateTime? fechaFallecimiento = null,
                           string photoPath = "")
        {
            if (!_people.ContainsKey(name))
            {
                var person = new Person(name, latitude, longitude)
                {
                    Cedula = cedula,
                    FechaNacimiento = fechaNacimiento ?? DateTime.Now,
                    EstaVivo = estaVivo,
                    FechaFallecimiento = fechaFallecimiento,
                    PhotoPath = photoPath
                };
                _people.Add(name, person);
            }

            if (!_adjacencyList.ContainsKey(name))
                _adjacencyList.Add(name, new LinkedList<string>());

            if (!_parentList.ContainsKey(name))
                _parentList.Add(name, new LinkedList<string>());
        }

        public void AddRelationship(string parent, string child, bool setRelationships = true)
        {
            AddPerson(parent);
            AddPerson(child);

            if (!_adjacencyList[parent].Contains(child))
                _adjacencyList[parent].Add(child);

            if (!_parentList[child].Contains(parent))
                _parentList[child].Add(parent);

            if (setRelationships)
            {
                _people[parent].FatherOf = child;
                _people[child].ChildOf = parent;
            }
        }

        public Person GetPerson(string name)
        {
            return _people.ContainsKey(name) ? _people[name] : null;
        }

        public LinkedList<string> GetChildren(string person)
        {
            return _adjacencyList.ContainsKey(person) ? _adjacencyList[person] : new LinkedList<string>();
        }

        public string GetParent(string childName)
        {
            var person = GetPerson(childName);
            return person?.ChildOf ?? string.Empty;
        }

        public LinkedList<string> GetParents(string person)
        {
            return _parentList.ContainsKey(person) ? _parentList[person] : new LinkedList<string>();
        }

        public LinkedList<string> GetPeople()
        {
            return _people.Keys();
        }
    }
}