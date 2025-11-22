using System;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Core.Models;

namespace Proyecto_Grafos.Core.Interfaces
{
    public interface IFamilyGraph
    {
        void AddPerson(string name, double latitude = 0.0, double longitude = 0.0,
                       string cedula = "", DateTime? fechaNacimiento = null,
                       bool estaVivo = true, DateTime? fechaFallecimiento = null,
                       string photoPath = "");

        void AddRelationship(string parent, string child);

        Person GetPerson(string name);
        LinkedList<string> GetChildren(string person);
        LinkedList<string> GetParents(string person);
        LinkedList<string> GetPeople();
        bool UpdatePersonName(string oldName, string newName);
    }
}