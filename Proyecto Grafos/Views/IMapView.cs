using GMap.NET;
using System.Collections.Generic;

namespace Proyecto_Grafos.Views
{
    public interface IMapView
    {
        string Description { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }

        void AddMarker(string name, double lat, double lng);
        void RemoveMarker(string name);
        void UpdateStatistics(string text);
        void RefreshMap();
        void RefreshGrid(object dataSource);
        void ShowMessage(string message, string caption = "Mensaje");
        void DrawRoutes(List<List<PointLatLng>> routes);
        void CenterMap(double lat, double lng);
    }
}
