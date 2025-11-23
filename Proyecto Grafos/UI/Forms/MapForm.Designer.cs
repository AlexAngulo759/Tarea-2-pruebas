namespace Proyecto_Grafos
{
    partial class MapForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Descriptiontext = new System.Windows.Forms.TextBox();
            this.Latitudtext = new System.Windows.Forms.TextBox();
            this.Longitudtext = new System.Windows.Forms.TextBox();
            this.Acceptbtn = new System.Windows.Forms.Button();
            this.ChangeModebtn = new System.Windows.Forms.Button();
            this.gMapControl1 = new GMap.NET.WindowsForms.GMapControl();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // gMapControl1
            // 
            this.gMapControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gMapControl1.Bearing = 0F;
            this.gMapControl1.CanDragMap = true;
            this.gMapControl1.EmptyTileColor = System.Drawing.Color.Navy;
            this.gMapControl1.GrayScaleMode = false;
            this.gMapControl1.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.gMapControl1.LevelsKeepInMemory = 5;
            this.gMapControl1.Location = new System.Drawing.Point(-1, 2);
            this.gMapControl1.MarkersEnabled = true;
            this.gMapControl1.MaxZoom = 18;
            this.gMapControl1.MinZoom = 2;
            this.gMapControl1.MouseWheelZoomEnabled = true;
            this.gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.gMapControl1.Name = "gMapControl1";
            this.gMapControl1.NegativeMode = false;
            this.gMapControl1.PolygonsEnabled = true;
            this.gMapControl1.RetryLoadTile = 0;
            this.gMapControl1.RoutesEnabled = true;
            this.gMapControl1.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.gMapControl1.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.gMapControl1.ShowTileGridLines = false;
            this.gMapControl1.Size = new System.Drawing.Size(750, 691);
            this.gMapControl1.TabIndex = 0;
            this.gMapControl1.Zoom = 5D;
            this.gMapControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GMapControl1_MouseClick);
            this.gMapControl1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnGMapControlMouseDoubleClick);
            // 
            // rightPanel
            // 
            this.rightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rightPanel.BackColor = System.Drawing.ColorTranslator.FromHtml("#404040");
            this.rightPanel.Location = new System.Drawing.Point(750, 0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(400, 691);
            this.rightPanel.TabIndex = 100;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(20, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Nombre";
            // 
            // Descriptiontext
            // 
            this.Descriptiontext.Location = new System.Drawing.Point(20, 54);
            this.Descriptiontext.Name = "Descriptiontext";
            this.Descriptiontext.Size = new System.Drawing.Size(280, 31);
            this.Descriptiontext.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(20, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "Latitud";
            // 
            // Latitudtext
            // 
            this.Latitudtext.Location = new System.Drawing.Point(20, 144);
            this.Latitudtext.Name = "Latitudtext";
            this.Latitudtext.Size = new System.Drawing.Size(280, 31);
            this.Latitudtext.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(20, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "Longitud";
            // 
            // Longitudtext
            // 
            this.Longitudtext.Location = new System.Drawing.Point(20, 241);
            this.Longitudtext.Name = "Longitudtext";
            this.Longitudtext.Size = new System.Drawing.Size(280, 31);
            this.Longitudtext.TabIndex = 8;
            // 
            // Acceptbtn
            // 
            this.Acceptbtn.Location = new System.Drawing.Point(104, 300);
            this.Acceptbtn.Name = "Acceptbtn";
            this.Acceptbtn.Size = new System.Drawing.Size(112, 44);
            this.Acceptbtn.TabIndex = 9;
            this.Acceptbtn.Text = "Aceptar";
            this.Acceptbtn.UseVisualStyleBackColor = true;
            this.Acceptbtn.Click += new System.EventHandler(this.OnAcceptButtonClick);
            // 
            // ChangeModebtn
            // 
            this.ChangeModebtn.Location = new System.Drawing.Point(60, 300);
            this.ChangeModebtn.Name = "ChangeModebtn";
            this.ChangeModebtn.Size = new System.Drawing.Size(200, 44);
            this.ChangeModebtn.TabIndex = 10;
            this.ChangeModebtn.Text = "Volver al Arbol";
            this.ChangeModebtn.UseVisualStyleBackColor = true;
            this.ChangeModebtn.Click += new System.EventHandler(this.OnChangeModeButtonClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(10, 354);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            this.splitContainer1.Panel2.Controls.Add(this.richTextBox1);
            this.splitContainer1.Size = new System.Drawing.Size(380, 327);
            this.splitContainer1.SplitterDistance = 163; 
            this.splitContainer1.TabIndex = 200;
            // 
            // dataGridView1
            // 
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 82;
            this.dataGridView1.RowTemplate.Height = 33;
            this.dataGridView1.TabIndex = 11;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.ReadOnly = true; 
            this.richTextBox1.TabIndex = 12;
            this.richTextBox1.Text = "";
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(64,64,64);
            this.richTextBox1.ForeColor = System.Drawing.Color.White;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // MapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 691);
            this.Controls.Add(this.gMapControl1);
            this.Controls.Add(this.rightPanel);
            this.MinimumSize = new System.Drawing.Size(1150, 750);
            this.Name = "MapForm";
            this.Text = "Mapa Interactivo";
            this.Load += new System.EventHandler(this.MapForm_Load);
            this.rightPanel.Controls.Add(this.label1);
            this.rightPanel.Controls.Add(this.Descriptiontext);
            this.rightPanel.Controls.Add(this.label2);
            this.rightPanel.Controls.Add(this.Latitudtext);
            this.rightPanel.Controls.Add(this.label3);
            this.rightPanel.Controls.Add(this.Longitudtext);
            this.rightPanel.Controls.Add(this.Acceptbtn);
            this.rightPanel.Controls.Add(this.ChangeModebtn);
            this.rightPanel.Controls.Add(this.splitContainer1);
            this.rightPanel.ResumeLayout(false);
            this.rightPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private GMap.NET.WindowsForms.GMapControl gMapControl1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Descriptiontext;
        private System.Windows.Forms.TextBox Latitudtext;
        private System.Windows.Forms.TextBox Longitudtext;
        private System.Windows.Forms.Button Acceptbtn;
        private System.Windows.Forms.Button ChangeModebtn;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}
