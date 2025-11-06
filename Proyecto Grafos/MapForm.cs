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
        double InicialLat = 9.859023203965602;
        double InicialLng = -83.91360662945013;

        public MapForm()
        {
            InitializeComponent();
        }
        private void MapForm_Load(object sender, EventArgs e)
        {
            gMapControl1.DragButton = MouseButtons.Left;
            gMapControl1.CanDragMap = true;
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(InicialLat, InicialLng);
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 9;
            gMapControl1.AutoScroll = true;
        }
    }
}
