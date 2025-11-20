using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Presenters;
using Proyecto_Grafos.Views;
using Proyecto_Grafos.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        private bool readOnlyMode = false;

        private bool forceVisualizationOnLoad = false;
        private List<Person> pendingPeople = null;

        private readonly System.Collections.Generic.Dictionary<string, Bitmap> photoCache = new System.Collections.Generic.Dictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);

        public double SelectedLatitude { get; private set; }
        public double SelectedLongitude { get; private set; }

        public MapForm()
        {
            InitializeComponent();
        }

        public void LoadMembersFromGraph(List<Person> people)
        {
            if (markersOverlay == null)
            {
                pendingPeople = people == null ? null : new List<Person>(people);
                return;
            }

            markersOverlay.Markers.Clear();
            if (people == null) return;

            foreach (var p in people)
            {
                if (p == null) continue;
                if (Math.Abs(p.Latitude) < double.Epsilon && Math.Abs(p.Longitude) < double.Epsilon)
                    continue;

                var pos = new PointLatLng(p.Latitude, p.Longitude);
                var bmp = GetCircularPhoto(p.PhotoPath);

                if (bmp != null)
                {
                    var photoMarker = new Proyecto_Grafos.Markers.PhotoMarker(pos, bmp, p.Name)
                    {
                        ToolTipText = $"{p.Name}\nLat: {p.Latitude:G}\nLng: {p.Longitude:G}",
                        Tag = p.Name
                    };
                    markersOverlay.Markers.Add(photoMarker);
                }
                else
                {
                    var blueMarker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(pos, GMap.NET.WindowsForms.Markers.GMarkerGoogleType.blue)
                    {
                        ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.Always,
                        ToolTipText = $"{p.Name}\nLat: {p.Latitude:G}\nLng: {p.Longitude:G}",
                        Tag = p.Name
                    };

                    blueMarker.Offset = new Point(-blueMarker.Size.Width / 2, -blueMarker.Size.Height);

                    markersOverlay.Markers.Add(blueMarker);
                }
            }

            var familyMembers = people
                .Where(p => p != null && !(Math.Abs(p.Latitude) < double.Epsilon && Math.Abs(p.Longitude) < double.Epsilon))
                .Select(p => new FamilyMember(p.Name, p.Latitude, p.Longitude))
                .ToList();

            presenter?.LoadMembers(familyMembers);
            RefreshMap();
        }

        public void AddOrUpdateMarker(string name, double lat, double lng)
        {
            if (markersOverlay == null) return;

            var existing = markersOverlay.Markers.FirstOrDefault(x => (x.Tag as string) == name);
            if (existing != null)
            {
                existing.Position = new PointLatLng(lat, lng);
                existing.ToolTipText = $"{name}\nLat: {lat:G}\nLng: {lng:G}";
            }
            else
            {
                if (Math.Abs(lat) < double.Epsilon && Math.Abs(lng) < double.Epsilon)
                    return;

                var placeholderMarker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.blue)
                {
                    ToolTipMode = MarkerTooltipMode.Always,
                    ToolTipText = $"{name}\nLat: {lat:G}\nLng: {lng:G}",
                    Tag = name
                };

                placeholderMarker.Offset = new Point(-placeholderMarker.Size.Width / 2, -placeholderMarker.Size.Height);

                markersOverlay.Markers.Add(placeholderMarker);
            }
            RefreshMap();
        }

        public void SetReadOnlyVisualization(bool readOnly)
        {
            readOnlyMode = readOnly;
            try
            {
                Acceptbtn.Visible = !readOnly && modeSelection;
            }
            catch { }
        }

        public void SetModeVisualization()
        {
            forceVisualizationOnLoad = true;
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

            gMapControl1.Overlays.Add(routesOverlay);
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Overlays.Add(tempOverlay);

            presenter = new MapPresenter(this);

            gMapControl1.OnMarkerClick -= FamilyMarker_Click;
            gMapControl1.OnMarkerClick += FamilyMarker_Click;

            this.KeyDown -= MapForm_KeyDown;
            this.KeyDown += MapForm_KeyDown;
            this.Resize -= MapForm_Resize;
            this.Resize += MapForm_Resize;

            marker = null;

            if (pendingPeople != null)
            {
                var toLoad = pendingPeople;
                pendingPeople = null;
                LoadMembersFromGraph(toLoad);
            }

            if (forceVisualizationOnLoad)
            {
                FamilyVisualization();
                try { presenter.UpdateStatistics(); } catch { }
            }
            else
            {
                LocationSelection();
            }
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
            m.Offset = new Point(-m.Size.Width / 2, -m.Size.Height);

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

                    var labelMarker = new GMarkerGoogle(new PointLatLng(midLat, midLng), transparentBitmap)
                    {
                        ToolTipMode = MarkerTooltipMode.Always,
                        ToolTipText = $"{distance:F2} km"
                    };

                    labelMarker.ToolTip.Fill = new SolidBrush(Color.FromArgb(200, Color.White));
                    labelMarker.ToolTip.Foreground = Brushes.Black;
                    labelMarker.ToolTip.Stroke = Pens.Black;

                    routesOverlay.Markers.Add(labelMarker);
                }
                catch { continue; }
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
            ChangeModebtn.Visible = false;
            label1.Visible = false;
            Descriptiontext.Visible = false;
            dataGridView1.Visible = false;
        }

        private void FamilyVisualization()
        {
            modeSelection = false;
            this.Text = "Visualización Familiares";
            Acceptbtn.Visible = false;
            dataGridView1.Visible = true;

            label1.Visible = true;
            Descriptiontext.Visible = true;

            ChangeModebtn.Visible = true;
            ChangeModebtn.Text = "Volver al Árbol";
            ChangeModebtn.AutoSize = true;
            ChangeModebtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ChangeModebtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            LayoutReturnButton();
        }

        private void Acceptbtn_Click(object sender, EventArgs e)
        {
            tempOverlay.Markers.Clear();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ChangeModebtn_Click(object sender, EventArgs e)
        {
            if (!modeSelection)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
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
                marker = null;
                RefreshMap();
            }
        }

        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!modeSelection || readOnlyMode) return;

            var point = gMapControl1.FromLocalToLatLng(e.X, e.Y);
            presenter?.HandleMapDoubleClick(point.Lat, point.Lng);

            if (marker == null)
            {
                marker = new GMarkerGoogle(point, GMarkerGoogleType.red)
                {
                    ToolTipMode = MarkerTooltipMode.Always
                };
            }

            marker.Position = point;
            marker.ToolTipText = $"Ubicación:\nLat: {point.Lat:G}\nLng: {point.Lng:G}";

            tempOverlay.Markers.Clear();
            tempOverlay.Markers.Add(marker);

            SelectedLatitude = point.Lat;
            SelectedLongitude = point.Lng;

            RefreshMap();
        }

        private void FamilyMarker_Click(GMapMarker clickedMarker, MouseEventArgs e)
        {
            if (clickedMarker == marker || clickedMarker.Tag == null) return;

            string name = clickedMarker.Tag.ToString();

            presenter?.SelectMemberByName(name, centerMap: false);
            presenter?.CalculateAndShowRoutes(name);

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
            presenter?.SelectMemberByName(name);
        }

        private void LayoutReturnButton()
        {
            const int margin = 8;

            ChangeModebtn.AutoSize = true;
            ChangeModebtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ChangeModebtn.Left = Longitudtext.Left;
            ChangeModebtn.Top = Longitudtext.Bottom + margin;

            int rightEdge = Longitudtext.Right;
            if (ChangeModebtn.Right > rightEdge)
            {
                ChangeModebtn.Left = Math.Max(Longitudtext.Left, rightEdge - ChangeModebtn.Width);
            }
        }

        private void MapForm_Resize(object sender, EventArgs e)
        {
            if (ChangeModebtn.Visible && !modeSelection)
            {
                LayoutReturnButton();
            }
        }

        private Bitmap GetCircularPhoto(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return null;

                if (photoCache.TryGetValue(path, out var cached))
                    return cached;

                using (var original = Image.FromFile(path))
                {
                    var size = 64;
                    var bmp = new Bitmap(size, size);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
                        {
                            gp.AddEllipse(0, 0, size, size);
                            g.SetClip(gp);
                            g.DrawImage(original, new Rectangle(0, 0, size, size));
                        }
                    }
                    photoCache[path] = bmp;
                    return bmp;
                }
            }
            catch
            {
                return null;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            foreach (var kv in photoCache)
            {
                kv.Value.Dispose();
            }
            photoCache.Clear();
        }
    }
}
