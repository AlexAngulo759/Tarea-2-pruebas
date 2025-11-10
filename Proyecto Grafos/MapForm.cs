using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.MinimumSize = new System.Drawing.Size(1080, 800);

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

            marker = new GMarkerGoogle(new PointLatLng(InitialLat, InitialLng), GMarkerGoogleType.red_dot);
            markersOverlay.Markers.Add(marker);

            marker.ToolTipMode = MarkerTooltipMode.Always;
            marker.ToolTipText = string.Format("Ubicación: \n Latitud: {0} \n Longitud: {1}", InitialLat, InitialLng);

            gMapControl1.Overlays.Add(markersOverlay);

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

        private void CreateFamilyMarker(string name, double lat, double lng)
        {
            GMarkerGoogle familyMarker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.blue);

            familyMarker.ToolTipMode = MarkerTooltipMode.Always;
            familyMarker.ToolTipText = string.Format("{0}\nLat: {1}\nLng: {2}", name, lat, lng);

            markersOverlay.Markers.Add(familyMarker);
        }

        private void SelectUbication(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
                return;

            selectedrow = e.RowIndex;
            Descriptiontext.Text = dataGridView1.Rows[selectedrow].Cells[0].Value.ToString();
            Latitudtext.Text = dataGridView1.Rows[selectedrow].Cells[1].Value.ToString();
            Longitudtext.Text = dataGridView1.Rows[selectedrow].Cells[2].Value.ToString();

            marker.Position = new PointLatLng(Convert.ToDouble(Latitudtext.Text), Convert.ToDouble(Longitudtext.Text));
            gMapControl1.Position = marker.Position;
        }

        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            double lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
            double lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

            Latitudtext.Text = lat.ToString();
            Longitudtext.Text = lng.ToString();

            marker.Position = new PointLatLng(lat, lng);
            marker.ToolTipText = string.Format("Ubicación: \n Latitud: {0} \n Longitud: {1}", lat, lng);
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Descriptiontext.Text))
            {
                MessageBox.Show("Por favor ingrese un nombre para el familiar");
                return;
            }
            dt.Rows.Add(Descriptiontext.Text, Convert.ToDouble(Latitudtext.Text), Convert.ToDouble(Longitudtext.Text));

            CreateFamilyMarker(Descriptiontext.Text, Convert.ToDouble(Latitudtext.Text), Convert.ToDouble(Longitudtext.Text));

            Descriptiontext.Text = "";
            Latitudtext.Text = "";
            Longitudtext.Text = "";
        }

        private void Deletebtn_Click(object sender, EventArgs e)
        {
            if (selectedrow >= 0 && selectedrow < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows.RemoveAt(selectedrow);

                if (selectedrow + 1 < markersOverlay.Markers.Count)
                {
                    markersOverlay.Markers.RemoveAt(selectedrow + 1);
                }
            }
        }

        private void ChangeModebtn_Click(object sender, EventArgs e)
        {
            if (modeSelection)
            {
                FamilyVisualization();
            }
            else
            {
                LocationSelection();
            }
        }

        private void Acceptbtn_Click(object sender, EventArgs e)
        {
           
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}