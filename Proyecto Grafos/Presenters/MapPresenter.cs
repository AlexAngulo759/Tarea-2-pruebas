using System;
using System.Data;
using System.Linq;
using Proyecto_Grafos.Views;
using Proyecto_Grafos.Models;

namespace Proyecto_Grafos.Presenters
{
    public class MapPresenter
    {
        private readonly IMapView view;
        private readonly DataTable dt;

        public MapPresenter(IMapView view, DataTable dt)
        {
            this.view = view;
            this.dt = dt;
        }
        public void AddMember()
        {
            if (string.IsNullOrWhiteSpace(view.Description))
            {
                view.ShowMessage("Debe ingresar un nombre válido.");
                return;
            }

            if (!double.TryParse(view.Latitude, out double lat) ||
                !double.TryParse(view.Longitude, out double lng))
            {
                view.ShowMessage("Las coordenadas no son válidas.");
                return;
            }

            string name = view.Description;
            dt.Rows.Add(name, lat, lng);

            view.AddMarker(name, lat, lng);
            UpdateStatistics();
        }
        public void DeleteMember(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dt.Rows.Count)
            {
                dt.Rows.RemoveAt(rowIndex);
                UpdateStatistics();
                view.RefreshMap();
            }
            else
            {
                view.ShowMessage("No se encontró el miembro a eliminar.");
            }
        }

        public void UpdateStatistics()
        {
            if (dt.Rows.Count < 2)
            {
                view.UpdateStatistics("Agrega al menos dos miembros para calcular estadísticas.");
                return;
            }

            var (m1, m2, maxD) = CalculateStatistics.FindFurthestPair(dt);
            var (c1, c2, minD) = CalculateStatistics.FindClosestPair(dt);
            var avg = CalculateStatistics.CalculateAverageDistance(dt);

            string stats = $" Estadísticas de Familia:\n\n" +
                           $"• Par más cercano: {c1} - {c2} ({minD:F2} km)\n" +
                           $"• Par más lejano: {m1} - {m2} ({maxD:F2} km)\n" +
                           $"• Distancia promedio: {avg:F2} km";

            view.UpdateStatistics(stats);
        }
        public void HandleMapDoubleClick(double lat, double lng)
        {
            view.Latitude = lat.ToString("F6");
            view.Longitude = lng.ToString("F6");
        }
    }
}
