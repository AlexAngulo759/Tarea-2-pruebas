using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Proyecto_Grafos
{
    public partial class MapForm : Form
    {
        GMarkerGoogle marker;
        GMapOverlay markersOverlay;
        GMapOverlay routesOverlay;
        GMapOverlay tempOverlay;
        DataTable dt;

        int selectedrow = 0;
        double InitialLat = 9.859023203965602;
        double InitialLng = -83.91360662945013;

        private bool modeSelection = true;

        public MapForm()
        {
            InitializeComponent();
        }

        private void MapForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1080, 800);
            this.KeyPreview = true;

            dt = new DataTable();
            dt.Columns.Add(new DataColumn("Nombre", typeof(string)));
            dt.Columns.Add(new DataColumn("Lat", typeof(double)));
            dt.Columns.Add(new DataColumn("Long", typeof(double)));
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[1].Visible = false;
            dataGridView1.Columns[2].Visible = false;

            gMapControl1.DragButton = MouseButtons.Left;
            gMapControl1.CanDragMap = true;
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(InitialLat, InitialLng);
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 9;
            gMapControl1.AutoScroll = true;

            markersOverlay = new GMapOverlay("markers");
            routesOverlay = new GMapOverlay("routes");
            tempOverlay = new GMapOverlay("temp");

            marker = new GMarkerGoogle(new PointLatLng(InitialLat, InitialLng), GMarkerGoogleType.red_dot);
            marker.ToolTipMode = MarkerTooltipMode.Always;
            marker.ToolTipText = $"Ubicación:\nLatitud: {InitialLat}\nLongitud: {InitialLng}";

            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Overlays.Add(routesOverlay);
            gMapControl1.Overlays.Add(tempOverlay);

            gMapControl1.OnMarkerClick += new MarkerClick(FamilyMarker_Click);
            gMapControl1.MouseClick += new MouseEventHandler(gMapControl1_MouseClick);
            this.KeyDown += new KeyEventHandler(MapForm_KeyDown);

            LocationSelection();
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

        private void UpdateStatistics()
        {
            if (dt.Rows.Count < 2)
            {
                richTextBox1.Text =
                    "ESTADÍSTICAS FAMILIARES\n" +
                    "-----------------------\n" +
                    "Se necesitan al menos 2 miembros\n" +
                    "para calcular estadísticas.";
                return;
            }

            var furthest = CalculateStatistics.FindFurthestPair(dt);
            var closest = CalculateStatistics.FindClosestPair(dt);
            double average = CalculateStatistics.CalculateAverageDistance(dt);

            richTextBox1.Text =
                $"ESTADÍSTICAS FAMILIARES\n" +
                $"-----------------------\n" +
                $"Miembros totales: {dt.Rows.Count}\n\n" +
                $"PAR MÁS LEJANO:\n" +
                $"├ {furthest.member1}\n" +
                $"├ {furthest.member2}\n" +
                $"└ Distancia: {furthest.distance:F2} km\n\n" +
                $"PAR MÁS CERCANO:\n" +
                $"├ {closest.member1}\n" +
                $"├ {closest.member2}\n" +
                $"└ Distancia: {closest.distance:F2} km\n\n" +
                $"DISTANCIA PROMEDIO:\n" +
                $"└ {average:F2} km";
        }

        private void CreateFamilyMarker(string name, double lat, double lng)
        {
            var familyMarker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.blue)
            {
                ToolTipMode = MarkerTooltipMode.Always,
                ToolTipText = $"{name}\nLat: {lat}\nLng: {lng}"
            };

            markersOverlay.Markers.Add(familyMarker);
        }

        private void FamilyMarker_Click(GMapMarker clickedMarker, MouseEventArgs e)
        {
            if (clickedMarker == marker)
                return;

            routesOverlay.Clear();

            PointLatLng selectedPosition = clickedMarker.Position;

            foreach (GMapMarker otherMarker in markersOverlay.Markers)
            {
                if (otherMarker != clickedMarker)
                {
                    PointLatLng otherPosition = otherMarker.Position;
                    var route = new GMapRoute(new List<PointLatLng> { selectedPosition, otherPosition }, "connection")
                    {
                        Stroke = new Pen(Color.Red, 2)
                    };
                    routesOverlay.Routes.Add(route);
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                double rowLat = Convert.ToDouble(dt.Rows[i]["Lat"]);
                double rowLng = Convert.ToDouble(dt.Rows[i]["Long"]);

                if (rowLat == selectedPosition.Lat && rowLng == selectedPosition.Lng)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;

                    selectedrow = i;
                    Descriptiontext.Text = dt.Rows[i]["Nombre"].ToString();
                    Latitudtext.Text = rowLat.ToString();
                    Longitudtext.Text = rowLng.ToString();
                    break;
                }
            }
        }

        private void SelectUbication(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
                return;

            selectedrow = e.RowIndex;
            Descriptiontext.Text = dataGridView1.Rows[selectedrow].Cells[0].Value.ToString();
            Latitudtext.Text = dataGridView1.Rows[selectedrow].Cells[1].Value.ToString();
            Longitudtext.Text = dataGridView1.Rows[selectedrow].Cells[2].Value.ToString();

            marker.Position = new PointLatLng(
                Convert.ToDouble(Latitudtext.Text),
                Convert.ToDouble(Longitudtext.Text));
            gMapControl1.Position = marker.Position;

            if (!tempOverlay.Markers.Contains(marker))
            {
                tempOverlay.Markers.Add(marker);
            }
        }
        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            double lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
            double lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

            Latitudtext.Text = lat.ToString();
            Longitudtext.Text = lng.ToString();

            marker.Position = new PointLatLng(lat, lng);
            marker.ToolTipText = $"Ubicación:\nLatitud: {lat}\nLongitud: {lng}";

            if (!tempOverlay.Markers.Contains(marker))
            {
                tempOverlay.Markers.Add(marker);
            }

            routesOverlay.Clear();
            gMapControl1.Refresh();
        }
        private void MapForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                routesOverlay.Clear();
                gMapControl1.Refresh();
                e.Handled = true;
            }
        }

        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tempOverlay.Markers.Clear();
                gMapControl1.Refresh();
            }
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Descriptiontext.Text))
            {
                MessageBox.Show("Por favor ingrese un nombre para el familiar");
                return;
            }

            if (!tempOverlay.Markers.Contains(marker))
            {
                MessageBox.Show("No hay marcador rojo en el mapa.\n\nHaga doble click en el mapa para colocar un marcador rojo primero.",
                               "Marcador no encontrado",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(Latitudtext.Text) || string.IsNullOrEmpty(Longitudtext.Text))
            {
                MessageBox.Show("No hay coordenadas para crear el familiar.\n\nHaga doble click en el mapa para establecer coordenadas.",
                               "Coordenadas faltantes",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            double lat = Convert.ToDouble(Latitudtext.Text);
            double lng = Convert.ToDouble(Longitudtext.Text);
            string name = Descriptiontext.Text;

            dt.Rows.Add(name, lat, lng);
            CreateFamilyMarker(name, lat, lng);

            Descriptiontext.Text = "";
            Latitudtext.Text = "";
            Longitudtext.Text = "";

            tempOverlay.Markers.Clear();
            UpdateStatistics();
            gMapControl1.Refresh();
        }

        private void Deletebtn_Click(object sender, EventArgs e)
        {
            if (selectedrow >= 0 && selectedrow < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows.RemoveAt(selectedrow);

                if (selectedrow < markersOverlay.Markers.Count)
                {
                    markersOverlay.Markers.RemoveAt(selectedrow);
                }
                routesOverlay.Clear();
                UpdateStatistics();
            }
        }

        private void ChangeModebtn_Click(object sender, EventArgs e)
        {
            if (modeSelection)
                FamilyVisualization();
            else
                LocationSelection();
        }

        private void Acceptbtn_Click(object sender, EventArgs e) { }
    }
}
