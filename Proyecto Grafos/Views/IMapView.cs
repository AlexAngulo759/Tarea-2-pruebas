namespace Proyecto_Grafos.Views
{
    public interface IMapView
    {
        string Description {get; set;}
        string Latitude {get; set;}
        string Longitude {get; set; }
        void AddMarker(string name, double lat, double lng);
        void UpdateStatistics(string text);
        void ShowMessage(string text, string caption = "Mensaje");
        void RefreshMap();
    }
}
