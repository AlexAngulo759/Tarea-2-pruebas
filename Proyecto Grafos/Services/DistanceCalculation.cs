using GMap.NET;
using System;

namespace Proyecto_Grafos
{
    public static class DistanceCalculation
    {
        private const double EarthRadius = 6371.0;

        public static double CalculateDistance(PointLatLng point1, PointLatLng point2)
        {
            double dLat = (point2.Lat - point1.Lat) * Math.PI / 180.0;
            double dLon = (point2.Lng - point1.Lng) * Math.PI / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(point1.Lat * Math.PI / 180.0) * Math.Cos(point2.Lat * Math.PI / 180.0) *
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return EarthRadius * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }
    }
}
