using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Proyecto_Grafos.Presenters;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Proyecto_Grafos
{
    public partial class MapForm : Form, IMapView
    {
        private readonly MapPresenter _presenter;
        private readonly GMapOverlay _markersOverlay = new GMapOverlay("markers");
        private readonly GMapOverlay _routesOverlay = new GMapOverlay("routes");
        private readonly GMapOverlay _tempOverlay = new GMapOverlay("temp");
        private bool _isSelectionMode;

        public double SelectedLatitude { get; private set; }
        public double SelectedLongitude { get; private set; }

        public event EventHandler AcceptButtonClicked;
        public event EventHandler ReturnButtonClicked;
        public event EventHandler ViewLoaded;
        public event EventHandler<PointLatLng> MapDoubleClicked;
        public event EventHandler<string> MarkerClicked;
        public event EventHandler<string> GridCellClicked;
        public event EventHandler EscapeKeyPressed;
        public event MouseEventHandler MapRightClicked;
        public event EventHandler<string> MarkerRightClicked; 

        public MapForm(GraphService graphService, bool isSelectionMode, bool isReadOnly)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;
            _presenter = new MapPresenter(this, graphService);
            _presenter.SetMode(isSelectionMode); 
            SetUIMode(isSelectionMode, isReadOnly);
        }

        private void MapForm_Load(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(9.85, -83.91);
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 9;
            gMapControl1.DragButton = MouseButtons.Left;

            gMapControl1.Overlays.Add(_routesOverlay);
            gMapControl1.Overlays.Add(_markersOverlay);
            gMapControl1.Overlays.Add(_tempOverlay);

            gMapControl1.OnMarkerClick += OnFamilyMarkerClick;
            gMapControl1.MouseDoubleClick += OnGMapControlMouseDoubleClick;
            dataGridView1.CellMouseClick += OnSelectUbication;
            this.KeyDown += MapForm_KeyDown;

            if (rightPanel != null)
            {
                rightPanel.Resize += RightPanel_Resize;
                RightPanel_Resize(rightPanel, EventArgs.Empty); 
            }

            ViewLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void RightPanel_Resize(object sender, EventArgs e)
        {
            if (splitContainer1 == null || rightPanel == null) return;

            int top = splitContainer1.Top;
            int available = rightPanel.ClientSize.Height - top - 8;
            if (available < 150) return;

            splitContainer1.Height = available;
            splitContainer1.SplitterDistance = available / 2;
        }

        public string Description { get => Descriptiontext.Text; set => Descriptiontext.Text = value; }
        public double Latitude { get => double.TryParse(Latitudtext.Text, out var v) ? v : 0; set => Latitudtext.Text = value.ToString("F5"); }
        public double Longitude { get => double.TryParse(Longitudtext.Text, out var v) ? v : 0; set => Longitudtext.Text = value.ToString("F5"); }

        public void AddPhotoMarker(string name, double lat, double lng, string tooltip, Bitmap photo)
        {
            var marker = new Markers.PhotoMarker(new PointLatLng(lat, lng), photo, name)
            {
                Tag = name,
                ToolTipText = tooltip,
                ToolTipMode = MarkerTooltipMode.Always
            };
            _markersOverlay.Markers.Add(marker);
        }

        public void AddStandardMarker(string name, double lat, double lng, string tooltip)
        {
            var marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.blue)
            {
                Tag = name,
                ToolTipText = tooltip,
                ToolTipMode = MarkerTooltipMode.Always 
            };
            _markersOverlay.Markers.Add(marker);
        }

        public void AddTemporaryMarker(double lat, double lng)
        {
            AddTemporaryMarker(lat, lng, string.Empty);
        }

        public void AddTemporaryMarker(double lat, double lng, string tooltip)
        {
            _tempOverlay.Markers.Clear();
            var marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.red)
            {
                ToolTipText = tooltip,
                ToolTipMode = MarkerTooltipMode.Always
            };
            _tempOverlay.Markers.Add(marker);
            RefreshMap();
        }

        public void ClearTemporaryMarker()
        {
            _tempOverlay.Markers.Clear();
            RefreshMap();
        }

        public void ClearAllMarkers()
        {
            _markersOverlay.Markers.Clear();
            _routesOverlay.Routes.Clear();
            _routesOverlay.Markers.Clear(); 
            _tempOverlay.Markers.Clear();
        }

        public void DrawRoutes(List<List<PointLatLng>> routes)
        {
            _routesOverlay.Routes.Clear();
            _routesOverlay.Markers.Clear(); 

            if (routes == null)
            {
                RefreshMap();
                return;
            }

            foreach (var routePoints in routes)
            {
                if (routePoints.Count < 2) continue;

                var route = new GMapRoute(routePoints, "route") { Stroke = new Pen(Color.Red, 3) };
                _routesOverlay.Routes.Add(route);

                double distance = DistanceCalculation.CalculateDistance(routePoints[0], routePoints[1]);

                double midLat = (routePoints[0].Lat + routePoints[1].Lat) / 2;
                double midLng = (routePoints[0].Lng + routePoints[1].Lng) / 2;
                var midPoint = new PointLatLng(midLat, midLng);
                var distanceMarker = new GMarkerGoogle(midPoint, new Bitmap(1, 1))
                {
                    ToolTipText = $"{distance:F2} km",
                    ToolTipMode = MarkerTooltipMode.Always,
                    Offset = new Point(-25, -10) 
                };
                _routesOverlay.Markers.Add(distanceMarker);
            }
            RefreshMap();
        }

        public void DrawLabeledSegments(List<RouteSegment> segments)
        {
            _routesOverlay.Routes.Clear();
            _routesOverlay.Markers.Clear();

            if (segments == null)
            {
                RefreshMap();
                return;
            }

            foreach (var segment in segments)
            {
                var routePoints = new List<PointLatLng> { segment.From, segment.To };
                if (routePoints.Count < 2) continue;

                var route = new GMapRoute(routePoints, "segment") { Stroke = new Pen(Color.Blue, 3) };
                _routesOverlay.Routes.Add(route);
                double midLat = (segment.From.Lat + segment.To.Lat) / 2.0;
                double midLng = (segment.From.Lng + segment.To.Lng) / 2.0;
                var midPoint = new PointLatLng(midLat, midLng);

                var labelMarker = new GMarkerGoogle(midPoint, new Bitmap(1, 1))
                {
                    ToolTipText = segment.Label,
                    ToolTipMode = MarkerTooltipMode.Always,
                    Offset = new Point(-25, -10)
                };
                _routesOverlay.Markers.Add(labelMarker);
            }

            RefreshMap();
        }

        public void SetUIMode(bool isSelectionMode, bool isReadOnly)
        {
            _isSelectionMode = isSelectionMode;
            Acceptbtn.Visible = isSelectionMode && !isReadOnly;
            ChangeModebtn.Visible = !isSelectionMode;
            bool isVisualizationMode = !isSelectionMode;
            Descriptiontext.Visible = isVisualizationMode;
            dataGridView1.Visible = isVisualizationMode;
            richTextBox1.Visible = isVisualizationMode;
            Latitudtext.Visible = true;
            Longitudtext.Visible = true;
            label1.Visible = isVisualizationMode;
            label2.Visible = true;
            label3.Visible = true;

            if (isSelectionMode)
            {
                Text = "Seleccionar Ubicación - Doble click para seleccionar ubicación | Navegar: mantener click izquierdo y mover cursor";
            }
            else
            {
                Text = "Mapa Familiar | Click izquierdo: distancias directas | Click derecho: rutas y distancias con parentesco | " +
                       "Navegar: mantener click izquierdo y mover cursor | ESC: ocultar líneas";
            }
        }

        public void SelectGridRow(string personName)
        {
            dataGridView1.ClearSelection();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Nombre"].Value as string == personName)
                {
                    row.Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                    return;
                }
            }
        }

        public void AddOrUpdateMarker(string name, double lat, double lng) => _presenter.AddOrUpdateMarker(name, lat, lng);
        public void RemoveMarker(string name) => _presenter.RemoveMarker(name);

        public void RefreshMap() => gMapControl1.Refresh();
        public void RefreshGrid(object dataSource) => dataGridView1.DataSource = dataSource;
        public void UpdateStatistics(string text = "") => richTextBox1.Text = text;
        public void UpdateSelectionInfo(string text) => richTextBox1.Text = text;
        public void ShowMessage(string message, string caption = "Mensaje") => MessageBox.Show(message, caption);
        public void CenterMap(double lat, double lng) => gMapControl1.Position = new PointLatLng(lat, lng);
        public void CloseView(bool dialogResult)
        {
            DialogResult = dialogResult ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        private void OnAcceptButtonClick(object sender, EventArgs e) => AcceptButtonClicked?.Invoke(this, EventArgs.Empty);
        private void OnChangeModeButtonClick(object sender, EventArgs e) => ReturnButtonClicked?.Invoke(this, EventArgs.Empty);
        private void ResetPhotoMarkerBorders()
        {
            foreach (var pm in _markersOverlay.Markers.OfType<Proyecto_Grafos.Markers.PhotoMarker>())
                pm.SetBorderColor(Color.White);
        }
        private void OnFamilyMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (!(item.Tag is string id)) return;

            if (e.Button == MouseButtons.Right)
            {
                if (item is Proyecto_Grafos.Markers.PhotoMarker pm)
                {
                    ClearTemporaryMarker();            
                    ResetPhotoMarkerBorders();        
                    pm.SetBorderColor(Color.Red);      
                    RefreshMap();
                }
                else
                {
                    ResetPhotoMarkerBorders();        
                    ClearTemporaryMarker();
                    AddTemporaryMarker(item.Position.Lat, item.Position.Lng, string.Empty);
                }

                MarkerRightClicked?.Invoke(this, id);
            }
            else if (e.Button == MouseButtons.Left)
            {
                ResetPhotoMarkerBorders();
                ClearTemporaryMarker();
                MarkerClicked?.Invoke(this, id);
            }
        }
        private void MapForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ResetPhotoMarkerBorders();       
                EscapeKeyPressed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnGMapControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_isSelectionMode)
            {
                var point = gMapControl1.FromLocalToLatLng(e.X, e.Y);
                SelectedLatitude = point.Lat;
                SelectedLongitude = point.Lng;
                MapDoubleClicked?.Invoke(this, point);
            }
        }

        private void OnSelectUbication(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Rows[e.RowIndex].Cells["Nombre"].Value is string name)
            {
                ResetPhotoMarkerBorders();
                ClearTemporaryMarker();

                GridCellClicked?.Invoke(this, name);
                EscapeKeyPressed?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _presenter?.Dispose();
        }

        private void OnGMapControlMouseClick(object sender, MouseEventArgs e)
        {
            if (_isSelectionMode && e.Button == MouseButtons.Right)
            {
                MapRightClicked?.Invoke(sender, e);
            }
        }

        private void GMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            OnGMapControlMouseClick(sender, e);
        }
    }
}
