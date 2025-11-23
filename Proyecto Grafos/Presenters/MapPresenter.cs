using GMap.NET;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Proyecto_Grafos.Presenters
{
    public class MapPresenter : IDisposable
    {
        private readonly IMapView _view;
        private readonly GraphService _graphService;
        private readonly List<Person> _people = new List<Person>();
        private readonly System.Collections.Generic.Dictionary<string, Bitmap> _photoCache = new System.Collections.Generic.Dictionary<string, Bitmap>();
        private bool _isSelectionMode;

        public MapPresenter(IMapView view, GraphService graphService)
        {
            _view = view;
            _graphService = graphService;

            _view.ViewLoaded += OnViewLoaded;
            _view.AcceptButtonClicked += OnAcceptButtonClicked;
            _view.ReturnButtonClicked += OnReturnButtonClicked;
            _view.MapDoubleClicked += OnMapDoubleClicked;
            _view.MarkerClicked += OnMarkerClicked;
            _view.GridCellClicked += OnGridCellClicked;
            _view.EscapeKeyPressed += OnEscapeKeyPressed; 
            _view.MapRightClicked += OnMapRightClicked;
        }

        public void SetMode(bool isSelectionMode)
        {
            _isSelectionMode = isSelectionMode;
            _view.SetUIMode(_isSelectionMode, isReadOnly: !_isSelectionMode);

            if (_isSelectionMode)
            {
                _view.ClearTemporaryMarker();
                _view.DrawRoutes(null);
                _view.Description = string.Empty;
                _view.Latitude = 0;
                _view.Longitude = 0;
            }
            else
            {
                UpdateStatistics();
            }
        }

        private void OnMapRightClicked(object sender, MouseEventArgs e)
        {
            _view.ClearTemporaryMarker();
            _view.Latitude = 0;
            _view.Longitude = 0;
        }

        private void OnViewLoaded(object sender, EventArgs e) => LoadDataAndRefreshView();
        private void OnAcceptButtonClicked(object sender, EventArgs e) => _view.CloseView(true);
        private void OnReturnButtonClicked(object sender, EventArgs e) => _view.CloseView(false);

        private void OnMapDoubleClicked(object sender, PointLatLng point)
        {
            string tooltip = $"Lat: {point.Lat}\nLon: {point.Lng}";
            _view.AddTemporaryMarker(point.Lat, point.Lng, tooltip);
            _view.Latitude = point.Lat;
            _view.Longitude = point.Lng;
        }

        private void OnMarkerClicked(object sender, string markerId)
        {
            var person = _people.FirstOrDefault(p => p.Name == markerId);
            if (person != null)
            {
                UpdateSelectionDetails(person);
                _view.SelectGridRow(person.Name);
                ShowDistancesUsingDijkstra(person.Name);
                UpdateStatistics();
            }
        }

        private void OnGridCellClicked(object sender, string personName)
        {
            var person = _people.FirstOrDefault(p => p.Name == personName);
            if (person != null)
            {
                UpdateSelectionDetails(person);
                _view.CenterMap(person.Latitude, person.Longitude);
                _view.DrawRoutes(null); 
            }
        }

        private void OnEscapeKeyPressed(object sender, EventArgs e)
        {
            _view.DrawRoutes(null); 
        }

        private void UpdateSelectionDetails(Person person)
        {
            _view.Description = person.Name;
            _view.Latitude = person.Latitude;
            _view.Longitude = person.Longitude;
        }

        public void LoadDataAndRefreshView()
        {
            _view.ClearAllMarkers();
            _people.Clear();
            _photoCache.Clear();

            var personNames = _graphService.GetPeople();
            foreach (var name in personNames)
            {
                var person = _graphService.GetPerson(name);
                if (person != null)
                {
                    _people.Add(person);
                    string tooltip = $"{person.Name}\nLat: {person.Latitude:F5}\nLon: {person.Longitude:F5}";

                    if (!string.IsNullOrEmpty(person.PhotoPath) && File.Exists(person.PhotoPath))
                    {
                        var photo = GetCircularPhoto(person.PhotoPath);
                        _photoCache[person.Name] = photo;
                        _view.AddPhotoMarker(person.Name, person.Latitude, person.Longitude, tooltip, photo);
                    }
                    else
                    {
                        _view.AddStandardMarker(person.Name, person.Latitude, person.Longitude, tooltip);
                    }
                }
            }

            _view.RefreshGrid(_people.Select(p => new { Nombre = p.Name }).ToList());
            UpdateStatistics();
            _view.RefreshMap();
        }
        private Bitmap GetCircularPhoto(string path)
        {
            try
            {
                using (var original = new Bitmap(path))
                {
                    var circular = new Bitmap(original.Width, original.Height);
                    using (var g = Graphics.FromImage(circular))
                    {
                        g.Clear(Color.Transparent);
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var brush = new TextureBrush(original))
                        {
                            using (var path_1 = new GraphicsPath())
                            {
                                path_1.AddEllipse(0, 0, original.Width, original.Height);
                                g.FillPath(brush, path_1);
                            }
                        }
                    }
                    return circular;
                }
            }
            catch { return null; }
        }

        public void UpdateStatistics()
        {
            if (_people.Count < 2)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Total de Personas: {_people.Count}");
                _view.UpdateStatistics(sb.ToString());
                return;
            }
            var membersForStats = _people.Select(p => new FamilyMember(p.Name, p.Latitude, p.Longitude)).ToList();

            var (f1, f2, maxD) = CalculateStatistics.FindFurthestPair(membersForStats);
            var (c1, c2, minD) = CalculateStatistics.FindClosestPair(membersForStats);
            var avgD = CalculateStatistics.CalculateAverageDistance(membersForStats);

            string stats =
$@"ESTADÍSTICAS FAMILIARES
-------------------------------------
Miembros totales: {_people.Count}

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

            _view.UpdateStatistics(stats);
        }

        public void AddOrUpdateMarker(string name, double lat, double lng)
        {
            var person = _people.FirstOrDefault(p => p.Name == name);
            if (person != null)
            {
                person.Latitude = lat;
                person.Longitude = lng;
            }
            LoadDataAndRefreshView();
        }

        public void RemoveMarker(string name)
        {
            var person = _people.FirstOrDefault(p => p.Name == name);
            if (person != null)
            {
                _people.Remove(person);
            }
            LoadDataAndRefreshView();
        }

        private void ShowDistancesUsingDijkstra(string originName)
        {
            if (string.IsNullOrEmpty(originName)) return;

            var (distances, previous) = GraphPathfinder.Dijkstra(_graphService, originName, treatAsUndirected: true, useCompleteGraph: true);

            var originPerson = _graphService.GetPerson(originName);
            if (originPerson == null) return;

            var routes = new List<List<PointLatLng>>();
            foreach (var destName in distances.Keys())
            {
                if (string.Equals(destName, originName, StringComparison.OrdinalIgnoreCase)) continue;
                var destPerson = _graphService.GetPerson(destName);
                if (destPerson == null) continue;

                routes.Add(new List<PointLatLng> {
                    new PointLatLng(originPerson.Latitude, originPerson.Longitude),
                    new PointLatLng(destPerson.Latitude, destPerson.Longitude)
                });
            }

            _view.DrawRoutes(routes); 
        }

        public void Dispose()
        {
            foreach (var photo in _photoCache.Values)
            {
                photo?.Dispose();
            }
            _photoCache.Clear();
        }
    }
}
