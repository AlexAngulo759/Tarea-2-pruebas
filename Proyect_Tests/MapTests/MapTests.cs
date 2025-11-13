using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto_Grafos;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Presenters;
using Proyecto_Grafos.Views;
using GMap.NET;
using System.Collections.Generic;

namespace Proyect_Tests.MapTests
{
    [TestClass]
    public class MapTests
    {
        [TestMethod]
        public void CalculateDistance_ReturnsExpectedDistance()
        {
            var pointA = new PointLatLng(9.9325, -84.0789);
            var pointB = new PointLatLng(9.8590, -83.9136);

            double result = DistanceCalculation.CalculateDistance(pointA, pointB);

            Assert.IsTrue(result > 19 && result < 20,
                $"Se esperaba distancia entre 19-20 km, se obtuvo {result:F2} km");
        }

        private class MockMapView : IMapView
        {
            public bool AddMarkerCalled { get; set; }
            public bool RemoveMarkerCalled { get; set; }
            public bool CenterCalled { get; set; }
            public bool DrawRoutesCalled { get; set; }

            public string Description { get; set; } = "Juan";
            public double Latitude { get; set; } = 9.85;
            public double Longitude { get; set; } = -83.91;

            public void AddMarker(string name, double lat, double lng) => AddMarkerCalled = true;
            public void RemoveMarker(string name) => RemoveMarkerCalled = true;
            public void RefreshMap() { }
            public void RefreshGrid(object dataSource) { }
            public void UpdateStatistics(string text) { }
            public void ShowMessage(string message, string caption = "Mensaje") { }
            public void DrawRoutes(List<List<PointLatLng>> routes) => DrawRoutesCalled = true;
            public void CenterMap(double lat, double lng) => CenterCalled = true;
        }

        [TestMethod]
        public void AddMember_ShouldCallAddMarker()
        {
            var view = new MockMapView();
            var presenter = new MapPresenter(view);

            presenter.AddMember();

            Assert.IsTrue(view.AddMarkerCalled, "AddMarker debería ser llamado cuando se agrega un miembro.");
        }

        [TestMethod]
        public void DeleteMember_ShouldCallRemoveMarker()
        {
            var view = new MockMapView();
            var presenter = new MapPresenter(view);
            presenter.AddMember();

            presenter.DeleteMember("Juan");

            Assert.IsTrue(view.RemoveMarkerCalled, "RemoveMarker debería ser llamado cuando se elimina un miembro.");
        }

        [TestMethod]
        public void SelectMemberByName_ShouldCallCenterMap()
        {
            var view = new MockMapView();
            var presenter = new MapPresenter(view);
            presenter.AddMember();

            presenter.SelectMemberByName("Juan");

            Assert.IsTrue(view.CenterCalled, "CenterMap debería ser llamado cuando se selecciona un miembro.");
        }

        [TestMethod]
        public void CalculateAndShowRoutes_ShouldCallDrawRoutes()
        {
            var view = new MockMapView();
            var presenter = new MapPresenter(view);
            presenter.AddMember();

            presenter.CalculateAndShowRoutes("Juan");

            Assert.IsTrue(view.DrawRoutesCalled, "DrawRoutes debería ser llamado cuando se calculan rutas.");
        }
    }
}