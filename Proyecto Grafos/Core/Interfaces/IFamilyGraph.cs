using System;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Models;

namespace Proyecto_Grafos.Core.Interfaces
{
    public interface IFamilyGraph
    {
        void AddPerson(string name, double latitude = 0.0, double longitude = 0.0,
                     string cedula = "", DateTime? fechaNacimiento = null,
                     bool estaVivo = true, DateTime? fechaFallecimiento = null,
                     string photoPath = "");

        void AddRelationship(string parent, string child, bool setRelationships = true);
        Person GetPerson(string name);
        Person GetPersonData(string name); 
        LinkedList<string> GetChildren(string person);
        string GetParent(string childName);
        LinkedList<string> GetParents(string person);
        LinkedList<string> GetPeople();

        LinkedList<string> GetAllPeople(); 
    }
}