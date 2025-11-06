using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Models;
using Proyecto_Grafos.Validate;

namespace Proyecto_Grafos
{
    public partial class Form1 : Form
    {
        private GraphService _graphService;
        private LayoutService _layoutService;
        private DrawingService _drawingService;
        private InteractionService _interactionService;
        private List<VisualNode> _visualNodes;

        private Panel _treePanel;

        public Form1()
        {
            InitializeServices();
            InitializeCustomComponents();
        }

        private void InitializeServices()
        {
            _graphService = new GraphService();
            _layoutService = new LayoutService();
            _drawingService = new DrawingService();
            _interactionService = new InteractionService();
            _visualNodes = new List<VisualNode>();
        }

        private void InitializeCustomComponents()
        {
            this.SuspendLayout();

            _treePanel = new Panel();
            _treePanel.Dock = DockStyle.Fill;
            _treePanel.BackColor = Color.White;
            _treePanel.Paint += TreePanel_Paint;
            _treePanel.MouseDown += TreePanel_MouseDown;

            this.Controls.Add(_treePanel);

            this.Text = "Árbol Genealógico - Click derecho para agregar";
            this.Size = new Size(1000, 600);
            this.ResumeLayout();
        }

        private void TreePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var clickedNode = FindNodeAtPosition(e.Location);
                string nodeName = clickedNode?.Name ?? string.Empty;
                bool isOverNode = clickedNode != null;

                var contextMenu = new ContextMenuStrip();

                if (!isOverNode)
                {
                    var addRootItem = new ToolStripMenuItem("Agregar Familiar Inicial");
                    addRootItem.Click += (s, ev) => ShowInputForm(InteractionService.NodeAction.AddRoot, "");
                    contextMenu.Items.Add(addRootItem);
                }
                else
                {
                    var addSuccessorItem = new ToolStripMenuItem("Agregar Sucesor");
                    addSuccessorItem.Click += (s, ev) => ShowInputForm(InteractionService.NodeAction.AddSuccessor, nodeName);
                    contextMenu.Items.Add(addSuccessorItem);

                    var addPredecessorItem = new ToolStripMenuItem("Agregar Predecesor");
                    addPredecessorItem.Click += (s, ev) => ShowInputForm(InteractionService.NodeAction.AddPredecessor, nodeName);
                    contextMenu.Items.Add(addPredecessorItem);

                    var addSiblingItem = new ToolStripMenuItem("Agregar Hermano");
                    addSiblingItem.Click += (s, ev) => ShowInputForm(InteractionService.NodeAction.AddSibling, nodeName);
                    contextMenu.Items.Add(addSiblingItem);
                }

                contextMenu.Show(_treePanel, e.Location);
            }
        }

        private void ShowInputForm(InteractionService.NodeAction action, string nodeName)
        {
            _interactionService.CurrentAction = action;
            _interactionService.SelectedNode = nodeName;

            string formTitle = GetFormTitle();

            using (var inputForm = new SimpleInputForm(formTitle))
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    ProcessFormResult(inputForm);
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

        private void ProcessFormResult(SimpleInputForm form)
        {
            bool success = false;
            string message = "";
            ValidationResult validationResult = null;

            switch (_interactionService.CurrentAction)
            {
                case InteractionService.NodeAction.AddRoot:
                    validationResult = _graphService.ValidateAddRoot(form.PersonName);
                    if (validationResult.IsValid)
                    {
                        success = _graphService.AddPerson(form.PersonName, form.Latitude, form.Longitude);
                        message = success ? "Familiar inicial agregado" : "Error al agregar familiar inicial";
                    }
                    else
                    {
                        message = validationResult.Message;
                    }
                    break;

                case InteractionService.NodeAction.AddSuccessor:
                    validationResult = _graphService.ValidateAddSuccessor(_interactionService.SelectedNode, form.PersonName);
                    if (validationResult.IsValid)
                    {
                        success = _graphService.AddPerson(form.PersonName, form.Latitude, form.Longitude);
                        if (success)
                        {
                            success = _graphService.AddRelationship(_interactionService.SelectedNode, form.PersonName);
                        }
                        message = success ? "Sucesor agregado" : "Error al agregar sucesor";
                    }
                    else
                    {
                        message = validationResult.Message;
                    }
                    break;

                case InteractionService.NodeAction.AddPredecessor:
                    validationResult = _graphService.ValidateAddPredecessor(_interactionService.SelectedNode, form.PersonName);
                    if (validationResult.IsValid)
                    {
                        success = _graphService.AddPerson(form.PersonName, form.Latitude, form.Longitude);
                        if (success)
                        {
                            success = _graphService.AddRelationship(form.PersonName, _interactionService.SelectedNode);
                        }
                        message = success ? "Predecesor agregado" : "Error al agregar predecesor";
                    }
                    else
                    {
                        message = validationResult.Message;
                    }
                    break;

                case InteractionService.NodeAction.AddSibling:
                    validationResult = _graphService.ValidateAddSibling(_interactionService.SelectedNode, form.PersonName);
                    if (validationResult.IsValid)
                    {
                        success = _graphService.AddPerson(form.PersonName, form.Latitude, form.Longitude);
                        if (success)
                        {
                            success = _graphService.AddSibling(_interactionService.SelectedNode, form.PersonName);
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

        private VisualNode FindNodeAtPosition(Point position)
        {
            foreach (var node in _visualNodes)
            {
                var bounds = node.GetBounds();
                if (bounds.Contains(position))
                    return node;
            }
            return null;
        }

        private void UpdateVisualTree()
        {
            var allPeople = _graphService.GetAllPeople();
            _visualNodes = _layoutService.CalculateLayout(allPeople, _graphService);
            _treePanel.Invalidate();
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            _drawingService.DrawTree(e.Graphics, _visualNodes, _graphService);
        }
    }
}