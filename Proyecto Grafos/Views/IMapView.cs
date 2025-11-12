namespace Proyecto_Grafos.Views
{
    public interface IMapView
    {
        string Description { get; }
        string Latitude { get; }
        string Longitude { get; }
        void AddMarker(string name, double lat, double lng);
        void UpdateStatistics(string text);
        void ShowMessage(string text, string caption = "Mensaje");
        void RefreshMap();
    }
}
