using GMap.NET;

namespace Proyecto_Grafos.Views
{
    public class RouteSegment
    {
        public PointLatLng From { get; }
        public PointLatLng To { get; }
        public string Label { get; }

        public RouteSegment(PointLatLng from, PointLatLng to, string label)
        {
            From = from;
            To = to;
            Label = label ?? string.Empty;
        }
    }
}