using System;
using System.Windows.Forms;

namespace Proyecto_Grafos
{
    public partial class SimpleInputForm : Form
    {
        public string PersonName { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        private TextBox txtName;
        private TextBox txtLatitude;
        private TextBox txtLongitude;

        public SimpleInputForm(string title)
        {
            InitializeForm(title);
        }

        private void InitializeForm(string title)
        {
            this.SuspendLayout();

            this.Text = title;
            this.Size = new System.Drawing.Size(300, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblName = new Label() { Text = "Nombre:", Location = new System.Drawing.Point(20, 20), Width = 80 };
            txtName = new TextBox() { Location = new System.Drawing.Point(100, 20), Width = 150 };

            Label lblLatitude = new Label() { Text = "Latitud:", Location = new System.Drawing.Point(20, 50), Width = 80 };
            txtLatitude = new TextBox() { Location = new System.Drawing.Point(100, 50), Width = 150, Text = "0.0" };

            Label lblLongitude = new Label() { Text = "Longitud:", Location = new System.Drawing.Point(20, 80), Width = 80 };
            txtLongitude = new TextBox() { Location = new System.Drawing.Point(100, 80), Width = 150, Text = "0.0" };

            Button btnConfirm = new Button() { Text = "Agregar", Location = new System.Drawing.Point(100, 120), Width = 80 };
            Button btnCancel = new Button() { Text = "Cancelar", Location = new System.Drawing.Point(190, 120), Width = 80 };

            this.Controls.AddRange(new Control[] { lblName, txtName, lblLatitude, txtLatitude, lblLongitude, txtLongitude, btnConfirm, btnCancel });

            btnConfirm.Click += BtnConfirm_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.AcceptButton = btnConfirm;

            this.ResumeLayout();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("El nombre es requerido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PersonName = txtName.Text.Trim();

            if (!double.TryParse(txtLatitude.Text, out double lat))
                lat = 0.0;
            Latitude = lat;

            if (!double.TryParse(txtLongitude.Text, out double lon))
                lon = 0.0;
            Longitude = lon;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}