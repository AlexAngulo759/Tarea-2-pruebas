using System;
using System.Drawing;
using System.Windows.Forms;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Services; 

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

        public string OriginalName { get; private set; }

        private OpenFileDialog openFileDialog;

        private TextBox txtName;
        private TextBox txtCedula;
        private DateTimePicker dtpNacimiento;
        private TextBox txtEdadActual;
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
        private readonly GraphService _graphService; 

        public PersonDetailForm(GraphService graphService, string defaultName = "", string formTitle = "Información de Persona")
        {
            InitializeComponent();
            _graphService = graphService; 

            if (!string.IsNullOrEmpty(formTitle))
                this.Text = formTitle;

            PersonName = defaultName;
            txtName.Text = defaultName;
            dtpNacimiento.Value = DateTime.Now.AddYears(-30);
            chkEstaVivo.Checked = true;
            ToggleFallecimientoControls(false);
            UpdateEdadActual();
        }

        private void InitializeComponent()
        {
            this.Text = "Información de Persona";
            this.Size = new Size(520, 770);                 
            this.MinimumSize = new Size(520, 770);           
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ColorTranslator.FromHtml("#404040");

            var lblTitle = new Label
            {
                Text = "Datos de la Persona",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                Width = 200,
                Height = 25
            };

            var lblName = new Label
            {
                Text = "Nombre completo:",
                Location = new Point(20, 50),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White
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
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White
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
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White
            };
            dtpNacimiento = new DateTimePicker
            {
                Location = new Point(150, 120),
                Width = 300,
                Font = new Font("Arial", 9),
                Format = DateTimePickerFormat.Short
            };
            dtpNacimiento.ValueChanged += (s, e) => UpdateEdadActual();

            var lblEdadActual = new Label
            {
                Text = "Edad actual:",
                Location = new Point(20, 155),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White
            };
            txtEdadActual = new TextBox
            {
                Location = new Point(150, 155),
                Width = 150,
                Font = new Font("Arial", 9),
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                Text = "0 años"
            };

            chkEstaVivo = new CheckBox
            {
                Text = "Persona viva",
                Location = new Point(150, 190),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Checked = true,
                ForeColor = Color.White,
            };

            var lblFallecimiento = new Label
            {
                Text = "Fecha Fallecimiento:",
                Location = new Point(20, 225),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White
            };
            dtpFallecimiento = new DateTimePicker
            {
                Location = new Point(150, 225),
                Width = 300,
                Font = new Font("Arial", 9),
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };
            dtpFallecimiento.ValueChanged += (s, e) => UpdateEdadActual();

            var lblCoordsTitle = new Label
            {
                Text = "Coordenadas de Residencia",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 265),
                Width = 220,
                Height = 20
            };

            var lblLatitude = new Label
            {
                Text = "Latitud:",
                Location = new Point(20, 295),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White,
            };
            txtLatitude = new TextBox
            {
                Location = new Point(150, 295),
                Width = 300,
                Text = "0.0",
                Font = new Font("Arial", 9)
            };
            
            txtLatitude.ReadOnly = true;
            txtLatitude.BackColor = Color.WhiteSmoke;

            var lblLongitude = new Label
            {
                Text = "Longitud:",
                Location = new Point(20, 330),
                Width = 120,
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.White,
            };
            txtLongitude = new TextBox
            {
                Location = new Point(150, 330),
                Width = 300,
                Text = "0.0",
                Font = new Font("Arial", 9)
            };
            txtLongitude.ReadOnly = true;
            txtLongitude.BackColor = Color.WhiteSmoke;

            btnSelectLocation = new Button
            {
                Text = "Seleccionar en mapa",
                Location = new Point(150, 360),
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
                ForeColor = Color.White,
                Location = new Point(20, 400),
                Width = 100,
                Height = 20
            };

            btnSelectPhoto = new Button
            {
                Text = "Seleccionar Foto",
                Location = new Point(150, 400),
                Width = 120,
                Height = 30,
                Font = new Font("Arial", 9),
                BackColor = Color.LightBlue
            };

            lblPhotoInfo = new Label
            {
                Text = "No se ha seleccionado foto",
                Location = new Point(280, 407),
                Width = 200,
                Font = new Font("Arial", 8),
                ForeColor = Color.Gray
            };

            picPhoto = new PictureBox
            {
                Location = new Point(150, 440),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };

            btnOK = new Button
            {
                Text = "Aceptar",
                Location = new Point(150, 680), 
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.LightGreen,
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(260, 680), 
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
            this.Controls.Add(lblEdadActual);
            this.Controls.Add(txtEdadActual);
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

            chkEstaVivo.CheckedChanged += (s, e) => 
            {
                ToggleFallecimientoControls(!chkEstaVivo.Checked);
                UpdateEdadActual();
            };
            btnSelectPhoto.Click += BtnSelectPhoto_Click;
            btnOK.Click += BtnOK_Click;

            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtCedula, "Número de cédula");
            toolTip.SetToolTip(txtLatitude, "Latitud (-90 a 90)");
            toolTip.SetToolTip(txtLongitude, "Longitud (-180 a 180)");
            toolTip.SetToolTip(btnSelectPhoto, "Seleccionar fotografía");
            toolTip.SetToolTip(txtEdadActual, "Edad calculada automáticamente basada en la fecha de nacimiento");
        }

        private void UpdateEdadActual()
        {
            EstaVivo = chkEstaVivo.Checked;
            FechaNacimiento = dtpNacimiento.Value;
            FechaFallecimiento = !EstaVivo ? dtpFallecimiento.Value : (DateTime?)null;

            int edad = CalcularEdadInterno(FechaNacimiento, FechaFallecimiento, EstaVivo);
            txtEdadActual.Text = $"{edad} años";
        }

        private int CalcularEdadInterno(DateTime nacimiento, DateTime? fallecimiento, bool vivo)
        {
            var referencia = vivo ? DateTime.Now : fallecimiento.Value;
            int edad = referencia.Year - nacimiento.Year;
            if (nacimiento.Date > referencia.AddYears(-edad))
                edad--;
            return edad;
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

            OriginalName = person.Name; 
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

            UpdateEdadActual();
        }
        private void BtnSelectLocation_Click(object sender, EventArgs e)
        {
            using (var map = new MapForm(_graphService, true, false))
            {
                if (double.TryParse(txtLatitude.Text, out double lat) &&
                    double.TryParse(txtLongitude.Text, out double lng))
                {
                }

                if (map.ShowDialog() == DialogResult.OK)
                {
                    txtLatitude.Text = map.SelectedLatitude.ToString("F6");
                    txtLongitude.Text = map.SelectedLongitude.ToString("F6");
                }
            }
        }

        private void TxtCedula_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TxtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '\'')
            {
                e.Handled = true;
            }
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            bool isModifying = !string.IsNullOrEmpty(OriginalName);
            string action = isModifying ? "modificar" : "crear";

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show($"El nombre es requerido. No se puede {action} la persona sin un nombre.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            if (txtName.Text.Trim().Length < 2)
            {
                MessageBox.Show($"El nombre debe tener al menos 2 caracteres. No se puede {action} la persona.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            foreach (char ch in txtName.Text.Trim())
            {
                if (!(char.IsLetter(ch) || ch == ' ' || ch == '-' || ch == '\''))
                {
                    MessageBox.Show($"El nombre solo puede contener letras, espacios, guiones y apóstrofes. No se puede {action} la persona.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtName.Focus();
                    return;
                }
            }

            if (!double.TryParse(txtLatitude.Text, out double lat))
            {
                MessageBox.Show($"Latitud inválida. Debe ser un número. No se puede {action} la persona sin una ubicación válida.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLatitude.Focus();
                return;
            }

            if (lat < -90 || lat > 90)
            {
                MessageBox.Show($"Latitud debe estar entre -90 y 90 grados. No se puede {action} la persona sin una ubicación válida.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLatitude.Focus();
                return;
            }

            if (!double.TryParse(txtLongitude.Text, out double lon))
            {
                MessageBox.Show($"Longitud inválida. Debe ser un número. No se puede {action} la persona sin una ubicación válida.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLongitude.Focus();
                return;
            }

            if (lon < -180 || lon > 180)
            {
                MessageBox.Show($"Longitud debe estar entre -180 y 180 grados. No se puede {action} la persona sin una ubicación válida.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLongitude.Focus();
                return;
            }

            if (lat == 0.0 && lon == 0.0)
            {
                MessageBox.Show($"Debe seleccionar una ubicación válida en el mapa. No se puede {action} la persona sin una ubicación.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSelectLocation.Focus();
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
    }
}
