using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Services.Validation;
using Proyecto_Grafos.UI.Controls;

namespace Proyecto_Grafos.UI.Forms
{
    public partial class MainForm : Form // ✅ Cambiado de "Form1" a "MainForm"
    {
        private GraphService _graphService; // ✅ Quitar readonly temporalmente
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

        public MainForm() // ✅ Cambiado de "Form1" a "MainForm"
        {
            InitializeServices();
            InitializeCustomComponents();
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

        private void InitializeCustomComponents()
        {
            this.SuspendLayout();

            _treePanel = new DoubleBufferedPanel();
            _treePanel.Dock = DockStyle.Fill;
            _treePanel.BackColor = Color.White;
            _treePanel.Paint += TreePanel_Paint;
            _treePanel.MouseDown += TreePanel_MouseDown;
            _treePanel.MouseMove += TreePanel_MouseMove;
            _treePanel.MouseUp += TreePanel_MouseUp;
            _treePanel.MouseWheel += TreePanel_MouseWheel;

            this.Controls.Add(_treePanel);

            this.Text = "Árbol Genealógico - Click derecho para agregar | Arrastrar: Click izquierdo | Zoom: Rueda del mouse";
            this.Size = new Size(1000, 600);
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
                }
                else
                {
                    var addSuccessorItem = new ToolStripMenuItem("Agregar Sucesor");
                    addSuccessorItem.Click += (s, ev) => ShowPersonDetailForm(InteractionService.NodeAction.AddSuccessor, nodeName);
                    contextMenu.Items.Add(addSuccessorItem);

                    var addPredecessorItem = new ToolStripMenuItem("Agregar Predecesor");
                    addPredecessorItem.Click += (s, ev) => ShowPersonDetailForm(InteractionService.NodeAction.AddPredecessor, nodeName);
                    contextMenu.Items.Add(addPredecessorItem);

                    var addSiblingItem = new ToolStripMenuItem("Agregar Hermano");
                    addSiblingItem.Click += (s, ev) => ShowPersonDetailForm(InteractionService.NodeAction.AddSibling, nodeName);
                    contextMenu.Items.Add(addSiblingItem);

                    var viewDetailsItem = new ToolStripMenuItem("Ver Detalles");
                    viewDetailsItem.Click += (s, ev) => ShowPersonDetails(nodeName);
                    contextMenu.Items.Add(viewDetailsItem);
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
            {
                _zoomFactor = Math.Min(_zoomFactor + ZOOM_INCREMENT, MAX_ZOOM);
            }
            else
            {
                _zoomFactor = Math.Max(_zoomFactor - ZOOM_INCREMENT, MIN_ZOOM);
            }

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

        private void ShowPersonDetailForm(InteractionService.NodeAction action, string nodeName)
        {
            _interactionService.CurrentAction = action;
            _interactionService.SelectedNode = nodeName;

            string formTitle = GetFormTitle();

            using (var detailForm = new PersonDetailForm(""))
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
                case InteractionService.NodeAction.AddSibling:
                    return $"Agregar Hermano de {_interactionService.SelectedNode}";
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
                    {
                        message = validationResult.Message;
                    }
                    break;

                case InteractionService.NodeAction.AddSuccessor:
                    validationResult = _graphService.ValidateAddSuccessor(_interactionService.SelectedNode, personData.Name);
                    if (validationResult.IsValid)
                    {
                        success = AddPersonWithData(personData);
                        if (success)
                        {
                            success = _graphService.AddRelationship(_interactionService.SelectedNode, personData.Name);
                        }
                        message = success ? "Sucesor agregado" : "Error al agregar sucesor";
                    }
                    else
                    {
                        message = validationResult.Message;
                    }
                    break;

                case InteractionService.NodeAction.AddPredecessor:
                    validationResult = _graphService.ValidateAddPredecessor(_interactionService.SelectedNode, personData.Name);
                    if (validationResult.IsValid)
                    {
                        success = AddPersonWithData(personData);
                        if (success)
                        {
                            success = _graphService.AddRelationship(personData.Name, _interactionService.SelectedNode);
                        }
                        message = success ? "Predecesor agregado" : "Error al agregar predecesor";
                    }
                    else
                    {
                        message = validationResult.Message;
                    }
                    break;

                case InteractionService.NodeAction.AddSibling:
                    validationResult = _graphService.ValidateAddSibling(_interactionService.SelectedNode, personData.Name);
                    if (validationResult.IsValid)
                    {
                        success = AddPersonWithData(personData);
                        if (success)
                        {
                            success = _graphService.AddSibling(_interactionService.SelectedNode, personData.Name);
                        }
                        message = success ? "Hermano agregado" : "Error al agregar hermano";
                    }
                    else
                    {
                        message = validationResult.Message;
                    }
                    break;
            }

            if (success)
            {
                UpdateVisualTree();
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
            var person = _graphService.GetPersonData(nodeName);
            if (person != null)
            {
                using (var detailForm = new PersonDetailForm(person.Name))
                {
                    detailForm.Text = $"Detalles de {person.Name}"; 
                    detailForm.SetExistingData(person);

                    if (detailForm.ShowDialog() == DialogResult.OK)
                    {
                        ShowPersonInfo(person);
                    }
                }
            }
        }

        private void ShowPersonInfo(Person person)
        {
            string estado = person.EstaVivo ? "Vivo" : "Fallecido";
            string infoFallecimiento = person.EstaVivo ? "" : $"\nFecha de Fallecimiento: {person.FechaFallecimiento.Value.ToShortDateString()}";

            string info = $@"Nombre: {person.Name}
Cédula: {person.Cedula}
Fecha de Nacimiento: {person.FechaNacimiento.ToShortDateString()}
Edad: {person.Edad} años
Estado: {estado}{infoFallecimiento}
Coordenadas: ({person.Latitude}, {person.Longitude})
{(string.IsNullOrEmpty(person.PhotoPath) ? "Sin fotografía" : "Con fotografía")}";

            MessageBox.Show(info, "Información de Persona", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            var allPeople = _graphService.GetAllPeople();

            var peopleList = new List<string>();
            for (int i = 0; i < allPeople.Count; i++)
            {
                peopleList.Add(allPeople.Get(i));
            }

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