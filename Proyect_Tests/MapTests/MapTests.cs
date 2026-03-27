using GMap.NET;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto_Grafos;
using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Presenters;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Services.Validation;
using Proyecto_Grafos.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Proyect_Tests.MapTests
{
    [TestClass]
    public class MapTests
    {
        private class MockMapView : IMapView
        {
            public bool AddPhotoMarkerCalled { get; set; }
            public bool AddStandardMarkerCalled { get; set; }
            public bool CenterMapCalled { get; set; }
            public bool DrawRoutesCalled { get; set; }
            public bool SelectGridRowCalled { get; set; }
            public bool ClearTemporaryMarkerCalled { get; set; }
            public bool UpdateStatisticsCalled { get; set; }
            public string? StatisticsText { get; set; }
            public List<List<PointLatLng>>? DrawnRoutes { get; set; }
            public string? SelectedGridRowName { get; set; }

            public string? Description { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public bool RefreshGridCalled { get; set; }
            public bool RefreshMapCalled { get; set; }
            public object? LastRefreshGridDataSource { get; set; }
            public bool TemporaryMarkerAdded { get; set; }
            public event EventHandler? ViewLoaded;
            public event EventHandler? AcceptButtonClicked;
            public event EventHandler? ReturnButtonClicked;
            public event EventHandler<PointLatLng>? MapDoubleClicked;
            public event EventHandler<string>? MarkerClicked;
            public event EventHandler<string>? GridCellClicked;
            public event EventHandler? EscapeKeyPressed;
            public event MouseEventHandler? MapRightClicked;
            public event EventHandler<string>? MarkerRightClicked;
            public void AddPhotoMarker(string name, double lat, double lng, string tooltip, Bitmap photo) => AddPhotoMarkerCalled = true;
            public void AddStandardMarker(string name, double lat, double lng, string tooltip) => AddStandardMarkerCalled = true;
            public void AddTemporaryMarker(double lat, double lng, string tooltip) => TemporaryMarkerAdded = true;
            public void CenterMap(double lat, double lng) => CenterMapCalled = true;
            public void ClearAllMarkers() { }
            public void ClearTemporaryMarker() => ClearTemporaryMarkerCalled = true;
            public void CloseView(bool dialogResult) { }
            public void BeginUpdate() { }
            public void EndUpdate() { }
            public void DrawRoutes(List<List<PointLatLng>>? routes)
            {
                DrawRoutesCalled = true;
                DrawnRoutes = routes;
            }
            public void RefreshGrid(object dataSource) { RefreshGridCalled = true; LastRefreshGridDataSource = dataSource; }
            public void RefreshMap() { RefreshMapCalled = true; }
            public void SelectGridRow(string personName)
            {
                SelectGridRowCalled = true;
                SelectedGridRowName = personName;
            }
            public void SetUIMode(bool isSelectionMode, bool isReadOnly) { }
            public void ShowMessage(string message, string caption = "Mensaje") { }
            public void UpdateSelectionInfo(string text) { }
            public void UpdateStatistics(string text)
            {
                UpdateStatisticsCalled = true;
                StatisticsText = text;
            }
            public void AddOrUpdateMarker(string name, double lat, double lng) { }
            public void RemoveMarker(string name) { }
            public void SimulateViewLoaded() => ViewLoaded?.Invoke(this, EventArgs.Empty);
            public void SimulateMarkerClick(string markerId) => MarkerClicked?.Invoke(this, markerId);
            public void SimulateGridCellClick(string personName) => GridCellClicked?.Invoke(this, personName);
            public void SimulateEscapeKeyPress() => EscapeKeyPressed?.Invoke(this, EventArgs.Empty);
            public void SimulateMapRightClick() => MapRightClicked?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));
            public void SimulateMapDoubleClick(PointLatLng point) => MapDoubleClicked?.Invoke(this, point);
            public void DrawLabeledSegments(List<RouteSegment> segments)
            {

            }
        }
        private class MockFamilyGraph : IFamilyGraph
        {
            private readonly System.Collections.Generic.Dictionary<string, Person> _people = new System.Collections.Generic.Dictionary<string, Person>();
            private readonly System.Collections.Generic.Dictionary<string, List<string>> _children =
    new System.Collections.Generic.Dictionary<string, List<string>>();

            public void AddPerson(string name, double latitude = 0, double longitude = 0, string cedula = "", DateTime? fechaNacimiento = null, bool estaVivo = true, DateTime? fechaFallecimiento = null, string photoPath = "")
            {
                var person = new Person(name, latitude, longitude)
                {
                    Cedula = cedula,
                    FechaNacimiento = fechaNacimiento ?? DateTime.MinValue,
                    EstaVivo = estaVivo,
                    FechaFallecimiento = fechaFallecimiento,
                    PhotoPath = photoPath
                };
                _people[name] = person;
            }

            public void AddRelationship(string parent, string child)
            {
                if (!_people.ContainsKey(parent) || !_people.ContainsKey(child)) return;
                if (!_children.ContainsKey(parent)) _children[parent] = new List<string>();
                if (!_children[parent].Contains(child)) _children[parent].Add(child);
            }

            public Person? GetPerson(string name) => _people.TryGetValue(name, out var p) ? p : null;

            public Proyecto_Grafos.Models.LinkedList<string> GetChildren(string person) => new Proyecto_Grafos.Models.LinkedList<string>();

            public Proyecto_Grafos.Models.LinkedList<string> GetParents(string person) => new Proyecto_Grafos.Models.LinkedList<string>();

            public Proyecto_Grafos.Models.LinkedList<string> GetPeople()
            {
                var list = new Proyecto_Grafos.Models.LinkedList<string>();
                foreach (var name in _people.Keys)
                    list.Add(name);
                return list;
            }

            public bool UpdatePersonName(string oldName, string newName)
            {
                if (!_people.ContainsKey(oldName) || string.IsNullOrEmpty(newName)) return false;
                var p = _people[oldName];
                _people.Remove(oldName);
                p.Name = newName;
                _people[newName] = p;
                return true;
            }
        }

        private class MockValidationService : IValidationService
        {
            public ValidationResult CanAddRoot(string personName) => ValidationResult.Valid();
            public ValidationResult CanAddSuccessor(string parentName, string successorName) => ValidationResult.Valid();
            public ValidationResult CanAddPredecessor(string childName, string predecessorName) => ValidationResult.Valid();
            public ValidationResult ValidatePersonName(string name) => ValidationResult.Valid();
            public ValidationResult CanAddRelationship(string parentName, string childName) => ValidationResult.Valid();
        }
        private class MockGraphService : GraphService
        {
            public MockGraphService(MockFamilyGraph family) : base(family, new MockValidationService())
            {
            }
        }
        private MockGraphService CreateMockServiceWithPeople()
        {
            var family = new MockFamilyGraph();
            family.AddPerson("Juan", 9.93, -84.07, "1", new DateTime(1990, 1, 1));
            family.AddPerson("Ana", 9.85, -83.91, "2", new DateTime(1992, 2, 2));
            return new MockGraphService(family);
        }

        [TestMethod]
        public void UpdateStatistics_CalculatesAndDisplaysCorrectDistances()
        {
            var view = new MockMapView();
            var service = CreateMockServiceWithPeople();
            var presenter = new MapPresenter(view, service);
            presenter.LoadDataAndRefreshView();
            presenter.UpdateStatistics();
            Assert.IsTrue(view.UpdateStatisticsCalled, "UpdateStatistics no fue llamado en la vista.");
            Assert.IsNotNull(view.StatisticsText, "El texto de estadísticas no debería ser nulo.");
            StringAssert.Contains(view.StatisticsText, "PAR MÁS LEJANO:", "El texto de estadísticas no contiene la sección del par más lejano.");
            StringAssert.Contains(view.StatisticsText, "PAR MÁS CERCANO:", "El texto de estadísticas no contiene la sección del par más cercano.");
            StringAssert.Contains(view.StatisticsText, "DISTANCIA PROMEDIO:", "El texto de estadísticas no contiene la sección de la distancia promedio.");
        }

        [TestMethod]
        public void MapDoubleClick_AddsTemporaryMarkerAndSetsCoordinates()
        {
            var view = new MockMapView();
            var service = CreateMockServiceWithPeople();
            var presenter = new MapPresenter(view, service);
            view.SimulateMapDoubleClick(new PointLatLng(11.11, -85.85));
            Assert.IsTrue(view.TemporaryMarkerAdded, "AddTemporaryMarker no fue invocado al hacer doble click en el mapa.");
            Assert.AreEqual(11.11, view.Latitude, 1e-6, "La latitud no fue asignada correctamente.");
            Assert.AreEqual(-85.85, view.Longitude, 1e-6, "La longitud no fue asignada correctamente.");
        }

        [TestMethod]
        public void MarkerClick_SelectsGridRow_DrawsRoutes_AndUpdatesSelection()
        {
            var view = new MockMapView();
            var service = CreateMockServiceWithPeople();
            var presenter = new MapPresenter(view, service);
            presenter.LoadDataAndRefreshView();
            view.SimulateMarkerClick("Juan");
            Assert.IsTrue(view.SelectGridRowCalled, "SelectGridRow no fue invocado al hacer click en marcador.");
            Assert.IsTrue(view.DrawRoutesCalled, "DrawRoutes no fue invocado para mostrar rutas.");
            Assert.AreEqual("Juan", view.Description, "La descripción no se actualizó con el nombre seleccionado.");
            var p = service.GetPerson("Juan");
            Assert.IsNotNull(p);
            Assert.AreEqual(p.Latitude, view.Latitude, 1e-6, "La latitud de selección no coincide.");
            Assert.IsTrue(view.UpdateStatisticsCalled, "UpdateStatistics no fue invocado tras seleccionar un marcador.");
        }

        [TestMethod]
        public void AddOrUpdateMarker_TriggersRefreshGridAndMap()
        {
            var view = new MockMapView();
            var service = CreateMockServiceWithPeople();
            var presenter = new MapPresenter(view, service);
            presenter.AddOrUpdateMarker("Juan", 12.34, -56.78);
            Assert.IsTrue(view.RefreshGridCalled, "RefreshGrid debería ser llamado después de AddOrUpdateMarker");
            Assert.IsTrue(view.RefreshMapCalled, "RefreshMap debería ser llamado después de AddOrUpdateMarker");
        }

        [TestMethod]
        public void DistanceCalculation_CalculatesDistanceWithinExpectedRange()
        {
            var p1 = new PointLatLng(0.0, 0.0);
            var p2 = new PointLatLng(0.0, 1.0);

            double dist = DistanceCalculation.CalculateDistance(p1, p2);
            Assert.AreEqual(111.1949, dist, 0.3, $"DistanceCalculation returned {dist}, se espera aproximadamente 111.19 km.");
        }
    }
}