using Proyecto_Grafos.Models;
using Proyecto_Grafos.Views;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;

namespace Proyecto_Grafos.Presenters
{
    public class MapPresenter
    {
        private readonly IMapView view;
        private readonly List<FamilyMember> members = new List<FamilyMember>();

        public MapPresenter(IMapView view)
        {
            this.view = view;
        }

        public void AddMember()
        {
            if (string.IsNullOrWhiteSpace(view.Description))
            {
                view.ShowMessage("Ingrese un nombre válido.");
                return;
            }

            double lat = view.Latitude;
            double lng = view.Longitude;

            if (lat == 0 && lng == 0)
            {
                view.ShowMessage("Coordenadas inválidas. Haga doble clic en el mapa.");
                return;
            }

            var member = new FamilyMember(view.Description, lat, lng);
            members.Add(member);
            view.AddMarker(member.Name, member.Lat, member.Lng);
            view.RefreshGrid(members.ToList());
            UpdateStatistics();

            view.Description = "";
            view.Latitude = 0;
            view.Longitude = 0;
        }

        public void DeleteMember(string name)
        {
            var member = members.FirstOrDefault(m => m.Name == name);
            if (member == null) { view.ShowMessage("No encontrado."); return; }

            members.Remove(member);
            view.RemoveMarker(name);
            view.RefreshGrid(members.ToList());
            UpdateStatistics();
        }

        public void SelectMemberByName(string name, bool centerMap = true)
        {
            var member = members.FirstOrDefault(m => m.Name == name);
            if (member == null) return;

            view.Description = member.Name;
            view.Latitude = member.Lat;
            view.Longitude = member.Lng;

            if (centerMap)
                view.CenterMap(member.Lat, member.Lng);
        }
        public void CalculateAndShowRoutes(string originName)
        {
            var origin = members.FirstOrDefault(m => m.Name == originName);
            if (origin == null) return;

            var allRoutes = members
                .Where(m => m.Name != originName)
                .Select(m => new List<PointLatLng> {
            new PointLatLng(origin.Lat, origin.Lng),
            new PointLatLng(m.Lat, m.Lng)
                })
                .ToList();

            view.DrawRoutes(allRoutes);
        }


        public void HandleMapDoubleClick(double lat, double lng)
        {
            view.Latitude = lat;
            view.Longitude = lng;
        }

        public void UpdateStatistics()
        {
            if (members.Count < 2)
            {
                view.UpdateStatistics("Se necesitan al menos 2 miembros para estadísticas.");
                return;
            }

            var (f1, f2, maxD) = CalculateStatistics.FindFurthestPair(members);
            var (c1, c2, minD) = CalculateStatistics.FindClosestPair(members);
            var avgD = CalculateStatistics.CalculateAverageDistance(members);

            string stats =
$@"ESTADÍSTICAS FAMILIARES
-----------------------
Miembros totales: {members.Count}

PAR MÁS LEJANO:
├ {f1}
├ {f2}
└ Distancia: {maxD:F2} km

PAR MÁS CERCANO:
├ {c1}
├ {c2}
└ Distancia: {minD:F2} km

DISTANCIA PROMEDIO:
└ {avgD:F2} km";

            view.UpdateStatistics(stats);
        }
    }
}
