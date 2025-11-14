using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Presenters;
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
        private GMarkerGoogle marker; 
        private GMapOverlay markersOverlay;
        private GMapOverlay routesOverlay;
        private GMapOverlay tempOverlay;
        private MapPresenter presenter;
        private int selectedRow = -1;

        private bool modeSelection = true;
        private double initialLat = 9.859023203965602;
        private double initialLng = -83.91360662945013;

        public MapForm()
        {
            InitializeComponent();
        }

        private void MapForm_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1080, 800);
            gMapControl1.DragButton = MouseButtons.Left;
            gMapControl1.CanDragMap = true;
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(initialLat, initialLng);
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 9;
            gMapControl1.AutoScroll = true;

            markersOverlay = new GMapOverlay("markers");
            routesOverlay = new GMapOverlay("routes");
            tempOverlay = new GMapOverlay("temp");

            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Overlays.Add(routesOverlay);
            gMapControl1.Overlays.Add(tempOverlay);

            marker = new GMarkerGoogle(new PointLatLng(initialLat, initialLng), GMarkerGoogleType.red_dot)
            {
                ToolTipMode = MarkerTooltipMode.Always,
                ToolTipText = $"Ubicación:\nLat: {initialLat:G}\nLng: {initialLng:G}"
            };

            gMapControl1.OnMarkerClick += FamilyMarker_Click;
            gMapControl1.MouseClick += gMapControl1_MouseClick;
            gMapControl1.MouseDoubleClick += gMapControl1_MouseDoubleClick;
            this.KeyDown += MapForm_KeyDown;

            presenter = new MapPresenter(this);

            LocationSelection();
        }

        public string Description
        {
            get => Descriptiontext.Text;
            set => Descriptiontext.Text = value;
        }

        public double Latitude
        {
            get => double.TryParse(Latitudtext.Text, out var v) ? v : 0;
            set => Latitudtext.Text = value.ToString("G"); 
        }

        public double Longitude
        {
            get => double.TryParse(Longitudtext.Text, out var v) ? v : 0;
            set => Longitudtext.Text = value.ToString("G"); 
        }

        public void AddMarker(string name, double lat, double lng)
        {
            var m = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.blue)
            {
                ToolTipMode = MarkerTooltipMode.Always,
                ToolTipText = $"{name}\nLat: {lat:G}\nLng: {lng:G}",
                Tag = name
            };
            markersOverlay.Markers.Add(m);
            RefreshMap();
        }

        public void RemoveMarker(string name)
        {
            var m = markersOverlay.Markers.FirstOrDefault(x => x.Tag as string == name);
            if (m != null) markersOverlay.Markers.Remove(m);
            RefreshMap();
        }

        public void RefreshMap() => gMapControl1.Refresh();

        public void RefreshGrid(object dataSource)
        {
            if (dataSource is List<FamilyMember> members)
            {
                dataGridView1.DataSource = members
                    .Select(m => new { Nombre = m.Name })
                    .ToList();
            }
        }

        public void UpdateStatistics(string text) => richTextBox1.Text = text;

        public void ShowMessage(string message, string caption = "Mensaje") => MessageBox.Show(message, caption);

        public void DrawRoutes(List<List<PointLatLng>> routes)
        {
            if (gMapControl1 == null || gMapControl1.IsDisposed) return;
            if (routesOverlay == null) return;

            routesOverlay.Clear();

            foreach (var routePoints in routes)
            {
                if (routePoints == null || routePoints.Count < 2) continue;
                var route = new GMapRoute(routePoints, "connection")
                {
                    Stroke = new Pen(Color.Red, 2)
                };
                routesOverlay.Routes.Add(route);

                try
                {
                    var start = routePoints[0];
                    var end = routePoints[1];
                    double distance = DistanceCalculation.CalculateDistance(start, end);

                    var midLat = (start.Lat + end.Lat) / 2.0;
                    var midLng = (start.Lng + end.Lng) / 2.0;

                    Bitmap transparentBitmap = new Bitmap(1, 1);
                    transparentBitmap.SetPixel(0, 0, Color.Transparent);

                    var labelMarker = new GMarkerGoogle(
                        new PointLatLng(midLat, midLng),
                        transparentBitmap)
                    {
                        ToolTipMode = MarkerTooltipMode.Always,
                        ToolTipText = $"{distance:F2} km"
                    };
                    labelMarker.ToolTip.Fill = new SolidBrush(Color.FromArgb(200, Color.White));
                    labelMarker.ToolTip.Foreground = Brushes.Black;
                    labelMarker.ToolTip.Stroke = Pens.Black;
                    routesOverlay.Markers.Add(labelMarker);
                }
                catch
                {
                    continue;
                }
            }

            RefreshMap();
        }
        public void CenterMap(double lat, double lng)
        {
            gMapControl1.Position = new PointLatLng(lat, lng);
        }

        private void LocationSelection()
        {
            modeSelection = true;
            this.Text = "Seleccionar Ubicación";
            Acceptbtn.Visible = true;
            ChangeModebtn.Visible = true;
            dataGridView1.Visible = false;
            Addbtn.Visible = false;
            Deletebtn.Visible = false;
            ChangeModebtn.Text = "Cambiar Modo";
        }

        private void FamilyVisualization()
        {
            modeSelection = false;
            this.Text = "Visualización Familiares";
            Acceptbtn.Visible = false;
            dataGridView1.Visible = true;
            Addbtn.Visible = true;
            Deletebtn.Visible = true;
            ChangeModebtn.Visible = true;
            ChangeModebtn.Text = "Cambiar Modo";
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            tempOverlay.Markers.Clear();
            presenter.AddMember();
        }

        private void Deletebtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string name = dataGridView1.CurrentRow.Cells["Nombre"].Value.ToString();
                presenter.DeleteMember(name);
            }
        }

        private void Acceptbtn_Click(object sender, EventArgs e)
        {
            tempOverlay.Markers.Clear();
            presenter.AddMember();
        }

        private void ChangeModebtn_Click(object sender, EventArgs e)
        {
            if (modeSelection) FamilyVisualization(); else LocationSelection();
        }

        private void MapForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                routesOverlay.Clear();
                RefreshMap();
                e.Handled = true;
            }
        }
        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tempOverlay.Markers.Clear();
                RefreshMap();
            }
        }

        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var point = gMapControl1.FromLocalToLatLng(e.X, e.Y);
            presenter.HandleMapDoubleClick(point.Lat, point.Lng);

            marker.Position = point;
            marker.ToolTipText = $"Ubicación:\nLat: {point.Lat:G}\nLng: {point.Lng:G}";
            if (!tempOverlay.Markers.Contains(marker))
                tempOverlay.Markers.Add(marker);

            routesOverlay.Clear();
            RefreshMap();
        }

        private void FamilyMarker_Click(GMapMarker clickedMarker, MouseEventArgs e)
        {
            if (clickedMarker == marker || clickedMarker.Tag == null) return;

            string name = clickedMarker.Tag.ToString();

            presenter.SelectMemberByName(name, centerMap: false);

            presenter.CalculateAndShowRoutes(name);

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Nombre"].Value?.ToString() == name)
                {
                    row.Selected = true;
                    dataGridView1.CurrentCell = row.Cells["Nombre"];
                    break;
                }
            }
        }


        private void SelectUbication(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string name = dataGridView1.Rows[e.RowIndex].Cells["Nombre"].Value.ToString();
            presenter.SelectMemberByName(name);
        }
    }
}
