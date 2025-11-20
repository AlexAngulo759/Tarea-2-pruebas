using System;
using System.Drawing;
using System.Windows.Forms;
using Proyecto_Grafos.Models;

namespace Proyecto_Grafos
{
    public partial class PersonDetailForm : Form
    {
        public string PersonName { get; private set; }
        public string Cedula { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public DateTime FechaNacimiento { get; private set; }
        public bool EstaVivo { get; private set; }
        public DateTime? FechaFallecimiento { get; private set; }
        public string PhotoPath { get; private set; }

        private OpenFileDialog openFileDialog;

        private TextBox txtName;
        private TextBox txtCedula;
        private DateTimePicker dtpNacimiento;
        private CheckBox chkEstaVivo;
        private DateTimePicker dtpFallecimiento;
        private TextBox txtLatitude;
        private TextBox txtLongitude;
        private Button btnSelectPhoto;

        private Button btnSelectLocation;  

        private PictureBox picPhoto;
        private Button btnOK;
        private Button btnCancel;
        private Label lblPhotoInfo;

        public PersonDetailForm(string defaultName = "", string formTitle = "Información de Persona")
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(formTitle))
                this.Text = formTitle;

            PersonName = defaultName;
            txtName.Text = defaultName;
            dtpNacimiento.Value = DateTime.Now.AddYears(-30);
            chkEstaVivo.Checked = true;
            ToggleFallecimientoControls(false);
        }

        private void InitializeComponent()
        {
            this.Text = "Información de Persona";
            this.Size = new Size(520, 760);                 
            this.MinimumSize = new Size(520, 760);           
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            var lblTitle = new Label
            {
                Text = "Datos de la Persona",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 15),
                Width = 200,
                Height = 25
            };

            var lblName = new Label
            {
                Text = "Nombre completo:",
                Location = new Point(20, 50),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            txtName = new TextBox
            {
                Location = new Point(150, 50),
                Width = 300,
                Font = new Font("Arial", 9)
            };
            txtName.KeyPress += TxtName_KeyPress;

            var lblCedula = new Label
            {
                Text = "Cédula:",
                Location = new Point(20, 85),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            txtCedula = new TextBox
            {
                Location = new Point(150, 85),
                Width = 300,
                Font = new Font("Arial", 9)
            };
            txtCedula.KeyPress += TxtCedula_KeyPress;

            var lblNacimiento = new Label
            {
                Text = "Fecha Nacimiento:",
                Location = new Point(20, 120),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            dtpNacimiento = new DateTimePicker
            {
                Location = new Point(150, 120),
                Width = 300,
                Font = new Font("Arial", 9),
                Format = DateTimePickerFormat.Short
            };

            chkEstaVivo = new CheckBox
            {
                Text = "Persona viva",
                Location = new Point(150, 155),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Checked = true
            };

            var lblFallecimiento = new Label
            {
                Text = "Fecha Fallecimiento:",
                Location = new Point(20, 190),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            dtpFallecimiento = new DateTimePicker
            {
                Location = new Point(150, 190),
                Width = 300,
                Font = new Font("Arial", 9),
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };

            var lblCoordsTitle = new Label
            {
                Text = "Coordenadas de Residencia",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location = new Point(20, 230),
                Width = 220,
                Height = 20
            };

            var lblLatitude = new Label
            {
                Text = "Latitud:",
                Location = new Point(20, 260),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            txtLatitude = new TextBox
            {
                Location = new Point(150, 260),
                Width = 300,
                Text = "0.0",
                Font = new Font("Arial", 9)
            };
            // Latitude/Longitude should not be typed manually; only via map selection
            txtLatitude.ReadOnly = true;
            txtLatitude.BackColor = Color.WhiteSmoke;

            var lblLongitude = new Label
            {
                Text = "Longitud:",
                Location = new Point(20, 295),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            txtLongitude = new TextBox
            {
                Location = new Point(150, 295),
                Width = 300,
                Text = "0.0",
                Font = new Font("Arial", 9)
            };
            txtLongitude.ReadOnly = true;
            txtLongitude.BackColor = Color.WhiteSmoke;

            btnSelectLocation = new Button
            {
                Text = "Seleccionar en mapa",
                Location = new Point(150, 325),
                Width = 150,
                Height = 30,
                Font = new Font("Arial", 9),
                BackColor = Color.LightYellow
            };
            btnSelectLocation.Click += BtnSelectLocation_Click;

            var lblPhotoTitle = new Label
            {
                Text = "Fotografía",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkOrange,
                Location = new Point(20, 360),
                Width = 100,
                Height = 20
            };

            btnSelectPhoto = new Button
            {
                Text = "Seleccionar Foto",
                Location = new Point(150, 360),
                Width = 120,
                Height = 30,
                Font = new Font("Arial", 9),
                BackColor = Color.LightBlue
            };

            lblPhotoInfo = new Label
            {
                Text = "No se ha seleccionado foto",
                Location = new Point(280, 367),
                Width = 200,
                Font = new Font("Arial", 8),
                ForeColor = Color.Gray
            };

            picPhoto = new PictureBox
            {
                Location = new Point(150, 400),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };

            btnOK = new Button
            {
                Text = "Aceptar",
                Location = new Point(150, 670), 
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.LightGreen,
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(260, 670), 
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 9),
                BackColor = Color.LightCoral,
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblCedula);
            this.Controls.Add(txtCedula);
            this.Controls.Add(lblNacimiento);
            this.Controls.Add(dtpNacimiento);
            this.Controls.Add(chkEstaVivo);
            this.Controls.Add(lblFallecimiento);
            this.Controls.Add(dtpFallecimiento);
            this.Controls.Add(lblCoordsTitle);
            this.Controls.Add(lblLatitude);
            this.Controls.Add(txtLatitude);
            this.Controls.Add(lblLongitude);
            this.Controls.Add(txtLongitude);
            this.Controls.Add(btnSelectLocation);
            this.Controls.Add(lblPhotoTitle);
            this.Controls.Add(btnSelectPhoto);
            this.Controls.Add(lblPhotoInfo);
            this.Controls.Add(picPhoto);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            chkEstaVivo.CheckedChanged += (s, e) => ToggleFallecimientoControls(!chkEstaVivo.Checked);
            btnSelectPhoto.Click += BtnSelectPhoto_Click;
            btnOK.Click += BtnOK_Click;

            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtCedula, "Número de cédula");
            toolTip.SetToolTip(txtLatitude, "Latitud (-90 a 90)");
            toolTip.SetToolTip(txtLongitude, "Longitud (-180 a 180)");
            toolTip.SetToolTip(btnSelectPhoto, "Seleccionar fotografía");
        }

        private void ToggleFallecimientoControls(bool enabled)
        {
            dtpFallecimiento.Enabled = enabled;
            if (!enabled)
            {
                dtpFallecimiento.Value = DateTime.Now;
            }
            else
            {
                dtpFallecimiento.Value = DateTime.Now.AddMonths(-1);
            }
        }

        private void BtnSelectPhoto_Click(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp|Todos los archivos|*.*",
                Title = "Seleccionar Fotografía",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PhotoPath = openFileDialog.FileName;

                    var image = Image.FromFile(PhotoPath);
                    picPhoto.Image = image;

                    lblPhotoInfo.Text = System.IO.Path.GetFileName(PhotoPath);
                    lblPhotoInfo.ForeColor = Color.Green;

                    var fileInfo = new System.IO.FileInfo(PhotoPath);
                    string sizeInfo = fileInfo.Length > 1024 * 1024
                        ? $"{(fileInfo.Length / (1024.0 * 1024.0)):F1} MB"
                        : $"{(fileInfo.Length / 1024.0):F0} KB";

                    lblPhotoInfo.Text += $" ({sizeInfo})";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PhotoPath = string.Empty;
                    picPhoto.Image = null;
                    lblPhotoInfo.Text = "Error al cargar";
                    lblPhotoInfo.ForeColor = Color.Red;
                }
            }
        }

        public void SetExistingData(Person person)
        {
            if (person == null) return;

            txtName.Text = person.Name;
            txtCedula.Text = person.Cedula;
            dtpNacimiento.Value = person.FechaNacimiento;
            chkEstaVivo.Checked = person.EstaVivo;

            if (person.FechaFallecimiento.HasValue)
            {
                dtpFallecimiento.Value = person.FechaFallecimiento.Value;
            }

            txtLatitude.Text = person.Latitude.ToString("F6");
            txtLongitude.Text = person.Longitude.ToString("F6");

            if (!string.IsNullOrEmpty(person.PhotoPath))
            {
                PhotoPath = person.PhotoPath;
                try
                {
                    picPhoto.Image = Image.FromFile(person.PhotoPath);
                    lblPhotoInfo.Text = System.IO.Path.GetFileName(person.PhotoPath);
                    lblPhotoInfo.ForeColor = Color.Green;
                }
                catch
                {
                    lblPhotoInfo.Text = "Imagen no encontrada";
                    lblPhotoInfo.ForeColor = Color.Red;
                }
            }
            else
            {
                lblPhotoInfo.Text = "Sin fotografía";
                lblPhotoInfo.ForeColor = Color.Gray;
                picPhoto.Image = null;
            }
        }
        private void BtnSelectLocation_Click(object sender, EventArgs e)
        {
            var map = new MapForm();
            if (double.TryParse(txtLatitude.Text, out double lat) &&
                double.TryParse(txtLongitude.Text, out double lng))
            {
                map.Latitude = lat;
                map.Longitude = lng;
            }

            if (map.ShowDialog() == DialogResult.OK)
            {
                txtLatitude.Text = map.SelectedLatitude.ToString("F6");
                txtLongitude.Text = map.SelectedLongitude.ToString("F6");
            }
        }

        private void TxtCedula_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow control keys (backspace), and digits only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TxtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, control keys, spaces, hyphen and apostrophe
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '\'')
            {
                e.Handled = true;
            }
        }
        public void SetLocation(double lat, double lng)
        {
            txtLatitude.Text = lat.ToString("F6");
            txtLongitude.Text = lng.ToString("F6");
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nombre es requerido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            if (txtName.Text.Trim().Length < 2)
            {
                MessageBox.Show("El nombre debe tener al menos 2 caracteres", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            // Validate name contains only allowed characters (letters, spaces, hyphen, apostrophe)
            foreach (char ch in txtName.Text.Trim())
            {
                if (!(char.IsLetter(ch) || ch == ' ' || ch == '-' || ch == '\''))
                {
                    MessageBox.Show("El nombre solo puede contener letras, espacios, guiones y apóstrofes", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtName.Focus();
                    return;
                }
            }

            if (!double.TryParse(txtLatitude.Text, out double lat))
            {
                MessageBox.Show("Latitud inválida. Debe ser un número.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLatitude.Focus();
                return;
            }

            if (lat < -90 || lat > 90)
            {
                MessageBox.Show("Latitud debe estar entre -90 y 90 grados", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLatitude.Focus();
                return;
            }

            if (!double.TryParse(txtLongitude.Text, out double lon))
            {
                MessageBox.Show("Longitud inválida. Debe ser un número.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLongitude.Focus();
                return;
            }

            if (lon < -180 || lon > 180)
            {
                MessageBox.Show("Longitud debe estar entre -180 y 180 grados", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLongitude.Focus();
                return;
            }

            if (dtpNacimiento.Value > DateTime.Now)
            {
                MessageBox.Show("La fecha de nacimiento no puede ser futura", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dtpNacimiento.Focus();
                return;
            }

            if (!chkEstaVivo.Checked && dtpFallecimiento.Value > DateTime.Now)
            {
                MessageBox.Show("La fecha de fallecimiento no puede ser futura", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dtpFallecimiento.Focus();
                return;
            }

            if (!chkEstaVivo.Checked && dtpFallecimiento.Value < dtpNacimiento.Value)
            {
                MessageBox.Show("La fecha de fallecimiento no puede ser anterior a la fecha de nacimiento", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dtpFallecimiento.Focus();
                return;
            }

            PersonName = txtName.Text.Trim();
            Cedula = txtCedula.Text.Trim();
            FechaNacimiento = dtpNacimiento.Value;
            EstaVivo = chkEstaVivo.Checked;
            Latitude = lat;
            Longitude = lon;

            if (!EstaVivo)
            {
                FechaFallecimiento = dtpFallecimiento.Value;
            }
            else
            {
                FechaFallecimiento = null;
            }

            // Cedula must be digits only (if provided)
            var ced = txtCedula.Text.Trim();
            if (!string.IsNullOrEmpty(ced))
            {
                foreach (char c in ced)
                {
                    if (!char.IsDigit(c))
                    {
                        MessageBox.Show("La cédula solo debe contener números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCedula.Focus();
                        return;
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public int CalcularEdad()
        {
            var fechaReferencia = EstaVivo ? DateTime.Now : FechaFallecimiento.Value;
            int edad = fechaReferencia.Year - FechaNacimiento.Year;

            if (FechaNacimiento.Date > fechaReferencia.AddYears(-edad))
                edad--;

            return edad;
        }

        public string ObtenerResumen()
        {
            string estado = EstaVivo ? "Vivo" : "Fallecido";
            string infoFallecimiento = EstaVivo ? "" : $", Falleció: {FechaFallecimiento.Value.ToShortDateString()}";

            return $"{PersonName} (Cédula: {Cedula})\n" +
                   $"Nacimiento: {FechaNacimiento.ToShortDateString()}, Edad: {CalcularEdad()} años\n" +
                   $"Estado: {estado}{infoFallecimiento}\n" +
                   $"Ubicación: {Latitude:F6}, {Longitude:F6}";
        }
    }
}
