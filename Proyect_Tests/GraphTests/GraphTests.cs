using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto_Grafos;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Services.Validation;
using System;
using System.Linq;

namespace Proyect_Tests.GraphTests
{
    [TestClass]
    public class GraphTests
    {
        private class AlwaysValidValidator : IValidationService
        {
            public ValidationResult CanAddRoot(string personName) => ValidationResult.Valid();
            public ValidationResult CanAddSuccessor(string parentName, string successorName) => ValidationResult.Valid();
            public ValidationResult CanAddPredecessor(string childName, string predecessorName) => ValidationResult.Valid();
            public ValidationResult ValidatePersonName(string name) => ValidationResult.Valid();
            public ValidationResult CanAddRelationship(string parentName, string childName) => ValidationResult.Valid();
        }

        [TestMethod]
        public void FamilyGraph_AddPerson_PopulatesPeopleListAndGetPerson()
        {
            var family = new FamilyGraph();
            family.AddPerson("Alice", 10.0, 20.0);

            var people = family.GetPeople();
            Assert.IsTrue(people.Contains("Alice"), "GetPeople debe contener el nombre añadido.");

            var p = family.GetPerson("Alice");
            Assert.IsNotNull(p, "GetPerson debe devolver la instancia añadida.");
            Assert.AreEqual(10.0, p.Latitude, 1e-9);
            Assert.AreEqual(20.0, p.Longitude, 1e-9);
        }

        [TestMethod]
        public void FamilyGraph_AddRelationship_ConnectsParentAndChild()
        {
            var family = new FamilyGraph();
            family.AddPerson("Parent", 0, 0);
            family.AddPerson("Child", 1, 1);

            family.AddRelationship("Parent", "Child");

            var children = family.GetChildren("Parent");
            Assert.IsTrue(children.Contains("Child"), "GetChildren(parent) debe contener al hijo.");

            var parents = family.GetParents("Child");
            Assert.IsTrue(parents.Contains("Parent"), "GetParents(child) debe contener al padre.");
        }

        [TestMethod]
        public void FamilyGraph_UpdatePersonName_UpdatesNodesAndEdges()
        {
            var family = new FamilyGraph();
            family.AddPerson("P1", 0, 0);
            family.AddPerson("C1", 1, 1);
            family.AddRelationship("P1", "C1");

            bool ok = family.UpdatePersonName("P1", "P1_New");
            Assert.IsTrue(ok, "UpdatePersonName debe retornar true para un renombrado válido.");

            var people = family.GetPeople();
            Assert.IsFalse(people.Contains("P1"), "Lista de personas no debe contener el nombre antiguo.");
            Assert.IsTrue(people.Contains("P1_New"), "Lista de personas debe contener el nuevo nombre.");

            var parentsOfChild = family.GetParents("C1");
            Assert.IsTrue(parentsOfChild.Contains("P1_New"), "Los padres del hijo deben actualizarse al nuevo nombre.");
        }

        [TestMethod]
        public void GraphPathfinder_Dijkstra_CompleteGraph_ReturnsDistancesConsistentWithHaversine()
        {
            var family = new FamilyGraph();
            family.AddPerson("A", 0.0, 0.0);
            family.AddPerson("B", 0.0, 1.0);
            family.AddPerson("C", 1.0, 0.0);

            var service = new GraphService(family, new AlwaysValidValidator());

            var (distances, previous) = GraphPathfinder.Dijkstra(service, "A", treatAsUndirected: true, useCompleteGraph: true);

            Assert.IsTrue(distances.ContainsKey("B"), "Distances debe contener la llave 'B'.");
            Assert.IsTrue(distances.ContainsKey("C"), "Distances debe contener la llave 'C'.");

            double expectedAB = DistanceCalculation.CalculateDistance(
                new GMap.NET.PointLatLng(0.0, 0.0), new GMap.NET.PointLatLng(0.0, 1.0));
            double actualAB = distances.Get("B");
            Assert.AreEqual(expectedAB, actualAB, 1e-6, "La distancia A->B debe coincidir con Haversine.");

            double expectedAC = DistanceCalculation.CalculateDistance(
                new GMap.NET.PointLatLng(0.0, 0.0), new GMap.NET.PointLatLng(1.0, 0.0));
            double actualAC = distances.Get("C");
            Assert.AreEqual(expectedAC, actualAC, 1e-6, "La distancia A->C debe coincidir con Haversine.");
        }

        [TestMethod]
        public void FamilyGraph_AddRelationship_ThrowsWhenPersonMissing()
        {
            var family = new FamilyGraph();
            family.AddPerson("OnlyParent", 0, 0);
            Assert.Throws<InvalidOperationException>(() => family.AddRelationship("OnlyParent", "MissingChild"));

            var family2 = new FamilyGraph();
            family2.AddPerson("OnlyChild", 1, 1);
            Assert.Throws<InvalidOperationException>(() => family2.AddRelationship("MissingParent", "OnlyChild"));
        }
    }
}