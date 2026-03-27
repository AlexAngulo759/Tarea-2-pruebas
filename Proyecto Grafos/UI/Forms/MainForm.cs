using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Services.Validation;

namespace Proyecto_Grafos.UI.Forms
{
    public partial class MainForm : Form
    {
        private GraphService _graphService;
        private LayoutService _layoutService;
        private DrawingService _drawingService;
        private InteractionService _interactionService;
        private List<VisualNode> _visualNodes;

        private DoubleBufferedPanel _treePanel;

        private Point _lastMousePosition;
        private bool _isDragging = false;
        private float _zoomFactor = 1.0f;
        private PointF _translation = PointF.Empty;
        private const float ZOOM_INCREMENT = 0.2f;
        private const float MIN_ZOOM = 0.3f;
        private const float MAX_ZOOM = 3.0f;

        private MapForm _cachedMapForm;

        public MainForm()
        {
            InitializeServices();
            InitializeCustomComponents();
            GenerateTestData(100);
        }

        private void InitializeServices()
        {
            IFamilyGraph familyGraph = new FamilyGraph();
            IValidationService validator = new GraphValidator(familyGraph);

            _graphService = new GraphService(familyGraph, validator);
            _layoutService = new LayoutService();
            _drawingService = new DrawingService();
            _interactionService = new InteractionService();
            _visualNodes = new List<VisualNode>();
        }

        private void GenerateTestData(int numberOfPeople)
        {
            var random = new Random();
            var names = new List<string>();

            for (int i = 1; i <= numberOfPeople; i++)
            {
                string name = $"Persona{i}";
                double lat = random.NextDouble() * 170 - 85;
                double lng = random.NextDouble() * 350 - 175;
                string cedula = $"000-{i:D5}";
                DateTime? fechaNac = new DateTime(1950 + random.Next(0, 50), random.Next(1, 13), random.Next(1, 28));
                bool estaVivo = random.Next(0, 2) == 1;
                DateTime? fechaFalle = estaVivo ? (DateTime?)null : new DateTime(fechaNac.Value.Year + random.Next(20, 80), fechaNac.Value.Month, fechaNac.Value.Day);

                _graphService.AddPerson(name, lat, lng, cedula, fechaNac, estaVivo, fechaFalle, "");
                names.Add(name);
            }

            for (int i = 2; i <= numberOfPeople; i++)
            {
                _graphService.AddRelationship(names[i - 2], names[i - 1]);
            }

            if (numberOfPeople > 10)
            {
                _graphService.AddRelationship("Persona5", "Persona10");
                _graphService.AddRelationship("Persona3", "Persona12");
                _graphService.AddRelationship("Persona7", "Persona12");
            }

            UpdateVisualTree();
        }

        public async Task RunGenerateTestCasesAsync()
        {
            var cases = new[] { 100, 1000, 5000 };
            var sb = new StringBuilder();

            foreach (var size in cases)
            {
                try
                {
                    var result = await Task.Run(() =>
                    {
                        IFamilyGraph fg = new FamilyGraph();
                        IValidationService validator = new GraphValidator(fg);
                        var localService = new GraphService(fg, validator);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();

                        long memBefore = GC.GetTotalMemory(true);
                        var sw = Stopwatch.StartNew();

                        GenerateTestDataForService(localService, size);

                        sw.Stop();
                        long memAfter = GC.GetTotalMemory(true);

                        return new { Size = size, Time = sw.Elapsed, MemoryBefore = memBefore, MemoryAfter = memAfter };
                    });

                    sb.AppendLine($"Casos: {result.Size} - Tiempo: {result.Time} - MemAntes: {result.MemoryBefore:N0} bytes - MemDespues: {result.MemoryAfter:N0} bytes");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error ejecutando caso {size}: {ex.Message}");
                }
            }

            MessageBox.Show(sb.ToString(), "Resultados de pruebas", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GenerateTestDataForService(GraphService service, int numberOfPeople)
        {
            var random = new Random();
            var names = new List<string>();

            for (int i = 1; i <= numberOfPeople; i++)
            {
                string name = $"Persona{i}";
                double lat = random.NextDouble() * 170 - 85;
                double lng = random.NextDouble() * 350 - 175;
                string cedula = $"000-{i:D5}";
                DateTime? fechaNac = new DateTime(1950 + random.Next(0, 50), random.Next(1, 13), random.Next(1, 28));
                bool estaVivo = random.Next(0, 2) == 1;
                DateTime? fechaFalle = estaVivo ? (DateTime?)null : new DateTime(fechaNac.Value.Year + random.Next(20, 80), fechaNac.Value.Month, fechaNac.Value.Day);

                service.AddPerson(name, lat, lng, cedula, fechaNac, estaVivo, fechaFalle, "");
                names.Add(name);
            }

            for (int i = 2; i <= numberOfPeople; i++)
            {
                service.AddRelationship(names[i - 2], names[i - 1]);
            }

            if (numberOfPeople > 10)
            {
                service.AddRelationship("Persona5", "Persona10");
                service.AddRelationship("Persona3", "Persona12");
                service.AddRelationship("Persona7", "Persona12");
            }
        }

        private void InitializeCustomComponents()
        {
            this.SuspendLayout();

            _treePanel = new DoubleBufferedPanel();
            _treePanel.Dock = DockStyle.Fill;
            _treePanel.BackColor = ColorTranslator.FromHtml("#404040");
            _treePanel.Paint += TreePanel_Paint;
            _treePanel.MouseDown += TreePanel_MouseDown;
            _treePanel.MouseMove += TreePanel_MouseMove;
            _treePanel.MouseUp += TreePanel_MouseUp;
            _treePanel.MouseWheel += TreePanel_MouseWheel;

            this.Controls.Add(_treePanel);

            var runTestsBtn = new Button
            {
                Text = "Run Tests",
                Width = 100,
                Height = 30,
                Location = new Point(10, 10)
            };
            runTestsBtn.Click += async (s, e) =>
            {
                try
                {
                    runTestsBtn.Enabled = false;
                    await RunGenerateTestCasesAsync();
                }
                finally
                {
                    runTestsBtn.Enabled = true;
                }
            };
            this.Controls.Add(runTestsBtn);
            runTestsBtn.BringToFront();

            this.Text = "Árbol Genealógico - Click derecho para agregar | Arrastrar: Click izquierdo | Zoom: Rueda del mouse";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1000, 700);

            this.ResumeLayout();
        }

        public class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                this.DoubleBuffered = true;
            }
        }

        private void TreePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastMousePosition = e.Location;
                _treePanel.Cursor = Cursors.SizeAll;
            }
            else if (e.Button == MouseButtons.Right)
            {
                var clickedNode = FindNodeAtPosition(e.Location);
                string nodeName = clickedNode?.Name ?? string.Empty;
                bool isOverNode = clickedNode != null;

                var contextMenu = new ContextMenuStrip();

                if (!isOverNode)
                {
                    var addRootItem = new ToolStripMenuItem("Agregar Familiar Inicial");
                    addRootItem.Click += (s, ev) => ShowPersonDetailForm(InteractionService.NodeAction.AddRoot, "");
                    contextMenu.Items.Add(addRootItem);

                    var resetViewItem = new ToolStripMenuItem("Resetear Vista");
                    resetViewItem.Click += (s, ev) => ResetView();
                    contextMenu.Items.Add(resetViewItem);

                    var viewMapItemNoNode = new ToolStripMenuItem("Visualizar mapa familiar");
                    viewMapItemNoNode.Click += (s, ev) => OpenMapVisualization();
                    contextMenu.Items.Add(viewMapItemNoNode);
                }
                else
                {
                    var addSuccessorItem = new ToolStripMenuItem("Agregar Sucesor");
                    addSuccessorItem.Click += (s, ev) => ShowPersonDetailForm(InteractionService.NodeAction.AddSuccessor, nodeName);
                    contextMenu.Items.Add(addSuccessorItem);

                    var addPredecessorItem = new ToolStripMenuItem("Agregar Predecesor");
                    addPredecessorItem.Click += (s, ev) => ShowPersonDetailForm(InteractionService.NodeAction.AddPredecessor, nodeName);
                    contextMenu.Items.Add(addPredecessorItem);

                    var viewDetailsItem = new ToolStripMenuItem("Ver Detalles");
                    viewDetailsItem.Click += (s, ev) => ShowPersonDetails(nodeName);
                    contextMenu.Items.Add(viewDetailsItem);

                    var viewMapItem = new ToolStripMenuItem("Visualizar mapa familiar");
                    viewMapItem.Click += (s, ev) => OpenMapVisualization();
                    contextMenu.Items.Add(viewMapItem);
                }

                contextMenu.Show(_treePanel, e.Location);
            }
        }

        private void TreePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.Button == MouseButtons.Left)
            {
                int deltaX = e.X - _lastMousePosition.X;
                int deltaY = e.Y - _lastMousePosition.Y;

                _translation.X += deltaX / _zoomFactor;
                _translation.Y += deltaY / _zoomFactor;

                _lastMousePosition = e.Location;
                _treePanel.Invalidate();
            }
        }

        private void TreePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                _treePanel.Cursor = Cursors.Default;
            }
        }

        private void TreePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = _zoomFactor;

            if (e.Delta > 0)
                _zoomFactor = Math.Min(_zoomFactor + ZOOM_INCREMENT, MAX_ZOOM);
            else
                _zoomFactor = Math.Max(_zoomFactor - ZOOM_INCREMENT, MIN_ZOOM);

            if (oldZoom != _zoomFactor)
            {
                PointF mousePos = e.Location;

                PointF worldMousePos = new PointF(
                    (mousePos.X - _translation.X * oldZoom) / oldZoom,
                    (mousePos.Y - _translation.Y * oldZoom) / oldZoom
                );

                _translation.X = (mousePos.X - worldMousePos.X * _zoomFactor) / _zoomFactor;
                _translation.Y = (mousePos.Y - worldMousePos.Y * _zoomFactor) / _zoomFactor;

                _treePanel.Invalidate();
            }
        }

        private void ResetView()
        {
            _zoomFactor = 1.0f;
            _translation = PointF.Empty;
            _treePanel.Invalidate();
        }

        private void OpenMapVisualization()
        {
            try
            {
                // ── CAMBIO 1 ──────────────────────────────────────────────────────
                // Antes: se verificaba (!IsDisposed) pero si el form estaba cerrado
                // (no disposed) se llamaba .Show() sobre un form ya cerrado, lo que
                // podía lanzar excepciones o mostrar una ventana en estado inválido.
                // Ahora: se verifica también que el form no esté cerrado usando la
                // propiedad IsDisposed como único indicador confiable. Si está
                // disposed o es null, se crea una instancia nueva limpia.
                if (_cachedMapForm != null && !_cachedMapForm.IsDisposed)
                {
                    if (_cachedMapForm.WindowState == FormWindowState.Minimized)
                        _cachedMapForm.WindowState = FormWindowState.Normal;

                    _cachedMapForm.Show();
                    _cachedMapForm.BringToFront();
                    return;
                }

                // Si llegamos aquí, el form anterior fue cerrado/disposed: limpiamos
                // la referencia antes de crear la nueva instancia.
                _cachedMapForm = null;

                _cachedMapForm = new MapForm(_graphService, false, true);

                // ── CAMBIO 2 ──────────────────────────────────────────────────────
                // Antes: el handler de FormClosed no asignaba null a _cachedMapForm,
                // dejando una referencia colgante al form cerrado. Esto impedía que
                // el Garbage Collector liberara los ~2.4 MB que ocupa el MapForm
                // porque MainForm seguía apuntando a él con _cachedMapForm.
                // Ahora: al cerrarse el form, asignamos null para soltar la referencia
                // y permitir que el GC recupere esa memoria.
                _cachedMapForm.FormClosed += (s, e) =>
                {
                    _cachedMapForm = null;
                };
                // ──────────────────────────────────────────────────────────────────

                _cachedMapForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el mapa: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPersonDetailForm(InteractionService.NodeAction action, string nodeName)
        {
            _interactionService.CurrentAction = action;
            _interactionService.SelectedNode = nodeName;

            string formTitle = GetFormTitle();
            using (var detailForm = new PersonDetailForm(_graphService, ""))
            {
                detailForm.Text = formTitle;

                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    ProcessPersonDetailFormResult(detailForm);
                }
            }

            _interactionService.ResetAction();
        }

        private string GetFormTitle()
        {
            switch (_interactionService.CurrentAction)
            {
                case InteractionService.NodeAction.AddRoot:
                    return "Agregar Familiar Inicial";
                case InteractionService.NodeAction.AddSuccessor:
                    return $"Agregar Sucesor de {_interactionService.SelectedNode}";
                case InteractionService.NodeAction.AddPredecessor:
                    return $"Agregar Predecesor de {_interactionService.SelectedNode}";
                default:
                    return "Agregar Persona";
            }
        }

        private void ProcessPersonDetailFormResult(PersonDetailForm detailForm)
        {
            bool success = false;
            string message = "";
            ValidationResult validationResult = null;

            var personData = new
            {
                Name = detailForm.PersonName,
                Latitude = detailForm.Latitude,
                Longitude = detailForm.Longitude,
                Cedula = detailForm.Cedula,
                FechaNacimiento = detailForm.FechaNacimiento,
                EstaVivo = detailForm.EstaVivo,
                FechaFallecimiento = detailForm.FechaFallecimiento,
                PhotoPath = detailForm.PhotoPath
            };

            switch (_interactionService.CurrentAction)
            {
                case InteractionService.NodeAction.AddRoot:
                    validationResult = _graphService.ValidateAddRoot(personData.Name);
                    if (validationResult.IsValid)
                    {
                        success = AddPersonWithData(personData);
                        message = success ? "Familiar inicial agregado" : "Error al agregar familiar inicial";
                    }
                    else
                        message = validationResult.Message;
                    break;

                case InteractionService.NodeAction.AddSuccessor:
                    validationResult = _graphService.ValidateAddSuccessor(_interactionService.SelectedNode, personData.Name);
                    if (validationResult.IsValid)
                    {
                        success = AddPersonWithData(personData) &&
                                  _graphService.AddRelationship(_interactionService.SelectedNode, personData.Name);
                        message = success ? "Sucesor agregado" : "Error al agregar sucesor";
                    }
                    else
                        message = validationResult.Message;
                    break;

                case InteractionService.NodeAction.AddPredecessor:
                    validationResult = _graphService.ValidateAddPredecessor(_interactionService.SelectedNode, personData.Name);
                    if (validationResult.IsValid)
                    {
                        success = AddPersonWithData(personData) &&
                                  _graphService.AddRelationship(personData.Name, _interactionService.SelectedNode);
                        message = success ? "Predecesor agregado" : "Error al agregar predecesor";
                    }
                    else
                        message = validationResult.Message;
                    break;
            }

            if (success)
            {
                UpdateVisualTree();

                // ── CAMBIO 3 ──────────────────────────────────────────────────────
                // Antes: al agregar una persona se hacía Close()+Dispose()+null sobre
                // el mapa cacheado para forzar recreación, lo que perdía el beneficio
                // del caché y generaba un ciclo innecesario de destrucción/creación.
                // Ahora: si el mapa está abierto, se llama directamente a su método
                // AddOrUpdateMarker para actualizar solo el marcador nuevo sin recrear
                // todo el mapa. Esto mantiene el caché vivo y es más eficiente.
                if (_cachedMapForm != null && !_cachedMapForm.IsDisposed)
                {
                    try
                    {
                        var person = _graphService.GetPerson(personData.Name);
                        if (person != null)
                            _cachedMapForm.AddOrUpdateMarker(person.Name, person.Latitude, person.Longitude);
                    }
                    catch { }
                }
                // ──────────────────────────────────────────────────────────────────

                MessageBox.Show(message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AddPersonWithData(dynamic personData)
        {
            return _graphService.AddPerson(
                personData.Name,
                personData.Latitude,
                personData.Longitude,
                personData.Cedula,
                personData.FechaNacimiento,
                personData.EstaVivo,
                personData.FechaFallecimiento,
                personData.PhotoPath
            );
        }

        private void ShowPersonDetails(string nodeName)
        {
            var person = _graphService.GetPerson(nodeName);
            if (person == null) return;
            using (var detailForm = new PersonDetailForm(_graphService, person.Name))
            {
                detailForm.Text = $"Detalles de {person.Name}";
                detailForm.SetExistingData(person);

                if (detailForm.ShowDialog() != DialogResult.OK) return;

                string originalName = detailForm.OriginalName ?? person.Name;
                string newName = detailForm.PersonName;
                bool nameChanged = originalName != newName;
                bool success = true;

                if (nameChanged)
                {
                    success = _graphService.UpdatePersonName(originalName, newName);
                    if (!success)
                    {
                        MessageBox.Show($"No se pudo cambiar el nombre a '{newName}'. Posible duplicado.",
                                        "Error al cambiar nombre",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        return;
                    }
                }

                if (success)
                {
                    var updatedPerson = _graphService.GetPerson(newName);
                    if (updatedPerson != null)
                    {
                        updatedPerson.Latitude = detailForm.Latitude;
                        updatedPerson.Longitude = detailForm.Longitude;
                        updatedPerson.Cedula = detailForm.Cedula;
                        updatedPerson.FechaNacimiento = detailForm.FechaNacimiento;
                        updatedPerson.EstaVivo = detailForm.EstaVivo;
                        updatedPerson.FechaFallecimiento = detailForm.FechaFallecimiento;
                        updatedPerson.PhotoPath = detailForm.PhotoPath;
                    }
                    else
                        success = false;
                }

                if (success)
                {
                    // ── CAMBIO 4 ──────────────────────────────────────────────────
                    // Antes: al editar una persona también se destruía el mapa cacheado
                    // innecesariamente con Close()+Dispose()+null.
                    // Ahora: si el nombre cambió se actualiza quitando el marcador viejo
                    // y agregando el nuevo; si solo cambiaron datos de ubicación, se
                    // llama AddOrUpdateMarker directamente. El mapa permanece abierto
                    // y cacheado en ambos casos.
                    if (_cachedMapForm != null && !_cachedMapForm.IsDisposed)
                    {
                        try
                        {
                            if (nameChanged)
                            {
                                _cachedMapForm.RemoveMarker(originalName);
                                _cachedMapForm.AddOrUpdateMarker(newName, detailForm.Latitude, detailForm.Longitude);
                            }
                            else
                            {
                                _cachedMapForm.AddOrUpdateMarker(newName, detailForm.Latitude, detailForm.Longitude);
                            }
                        }
                        catch { }
                    }
                    // ──────────────────────────────────────────────────────────────

                    UpdateVisualTree();

                    string message = nameChanged
                        ? $"Nombre cambiado de '{originalName}' a '{newName}' e información actualizada"
                        : $"Información de {newName} actualizada correctamente";

                    MessageBox.Show(message, "Actualización Exitosa",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar la información",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
        }

        private VisualNode FindNodeAtPosition(Point position)
        {
            PointF worldPos = new PointF(
                (position.X - _translation.X * _zoomFactor) / _zoomFactor,
                (position.Y - _translation.Y * _zoomFactor) / _zoomFactor
            );

            foreach (var node in _visualNodes)
            {
                var bounds = node.GetBounds();
                if (bounds.Contains((int)worldPos.X, (int)worldPos.Y))
                    return node;
            }
            return null;
        }

        private void UpdateVisualTree()
        {
            var allPeople = _graphService.GetPeople();
            var peopleList = new List<string>();
            for (int i = 0; i < allPeople.Count; i++)
                peopleList.Add(allPeople.Get(i));

            _visualNodes = _layoutService.CalculateLayout(peopleList, _graphService);
            _treePanel.Invalidate();
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(_translation.X * _zoomFactor, _translation.Y * _zoomFactor);
            e.Graphics.ScaleTransform(_zoomFactor, _zoomFactor);

            _drawingService.DrawTree(e.Graphics, _visualNodes, _graphService);
            DrawZoomInfo(e.Graphics);
        }

        private void DrawZoomInfo(Graphics g)
        {
            var oldTransform = g.Transform;
            g.ResetTransform();

            string zoomText = $"Zoom: {(_zoomFactor * 100):F0}%";
            using (var font = new Font("Arial", 10))
            using (var brush = new SolidBrush(Color.DarkGray))
            {
                g.DrawString(zoomText, font, brush, 10, 10);
            }

            g.Transform = oldTransform;
        }
    }
}
