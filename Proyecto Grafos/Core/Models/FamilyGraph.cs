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

        public bool UpdatePersonName(string oldName, string newName)
        {
            if (!_people.ContainsKey(oldName) || _people.ContainsKey(newName))
                return false;


            var person = _people.Get(oldName);
            var children = _adjacencyList.ContainsKey(oldName) ? _adjacencyList.Get(oldName) : new LinkedList<string>();
            var parents = _parentList.ContainsKey(oldName) ? _parentList.Get(oldName) : new LinkedList<string>();

            person.Name = newName;

            _people.Remove(oldName);
            _adjacencyList.Remove(oldName);
            _parentList.Remove(oldName);

            _people.Add(newName, person);
            _adjacencyList.Add(newName, children);
            _parentList.Add(newName, parents);
            for (int i = 0; i < children.Count; i++)
            {
                var childName = children.Get(i);
                if (_parentList.ContainsKey(childName))
                {
                    var childParents = _parentList.Get(childName);
                    for (int j = 0; j < childParents.Count; j++)
                    {
                        if (childParents.Get(j) == oldName)
                        {
                            childParents.RemoveAt(j);
                            childParents.Add(newName);
                            break;
                        }
                    }
                }
                
                var childPerson = _people.Get(childName);
                if (childPerson.ChildOf == oldName)
                    childPerson.ChildOf = newName;
            }

            for (int i = 0; i < parents.Count; i++)
            {
                var parentName = parents.Get(i);
                if (_adjacencyList.ContainsKey(parentName))
                {
                    var parentChildren = _adjacencyList.Get(parentName);
                    for (int j = 0; j < parentChildren.Count; j++)
                    {
                        if (parentChildren.Get(j) == oldName)
                        {
                            parentChildren.RemoveAt(j);
                            parentChildren.Add(newName);
                            break;
                        }
                    }
                }
                
                var parentPerson = _people.Get(parentName);
                if (parentPerson.FatherOf == oldName)
                    parentPerson.FatherOf = newName;
            }

            return true;
        }

        public void UpdatePerson(string name, double latitude, double longitude,
                                string cedula, DateTime fechaNacimiento,
                                bool estaVivo, DateTime? fechaFallecimiento,
                                string photoPath)
        {
            if (_people.ContainsKey(name))
            {
                var person = _people.Get(name);
                person.Latitude = latitude;
                person.Longitude = longitude;
                person.Cedula = cedula;
                person.FechaNacimiento = fechaNacimiento;
                person.EstaVivo = estaVivo;
                person.FechaFallecimiento = fechaFallecimiento;
                person.PhotoPath = photoPath;
            }
        }
    }
}