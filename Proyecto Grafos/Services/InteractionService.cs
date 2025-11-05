using System.Windows.Forms;

namespace Proyecto_Grafos.Services
{
    public class InteractionService
    {
        public enum NodeAction
        {
            None,
            AddRoot,
            AddSuccessor,
            AddPredecessor,
            AddSibling 
        }

        public NodeAction CurrentAction { get; private set; }
        public string SelectedNode { get; private set; }
        public bool ShouldShowForm { get; private set; }

        public InteractionService()
        {
            CurrentAction = NodeAction.None;
            SelectedNode = string.Empty;
            ShouldShowForm = false;
        }

        public void SetAction(NodeAction action, string nodeName = "")
        {
            CurrentAction = action;
            SelectedNode = nodeName;
            ShouldShowForm = true;
        }

        public void ResetAction()
        {
            CurrentAction = NodeAction.None;
            SelectedNode = string.Empty;
            ShouldShowForm = false;
        }

        public ContextMenuStrip CreateContextMenu(bool isOverNode, string nodeName)
        {
            var contextMenu = new ContextMenuStrip();

            if (!isOverNode)
            {
                var addRootItem = new ToolStripMenuItem("Agregar Familiar Inicial");
                addRootItem.Click += (s, e) => SetAction(NodeAction.AddRoot);
                contextMenu.Items.Add(addRootItem);
            }
            else
            {
                var addSuccessorItem = new ToolStripMenuItem("Agregar Sucesor");
                addSuccessorItem.Click += (s, e) => SetAction(NodeAction.AddSuccessor, nodeName);
                contextMenu.Items.Add(addSuccessorItem);

                var addPredecessorItem = new ToolStripMenuItem("Agregar Predecesor");
                addPredecessorItem.Click += (s, e) => SetAction(NodeAction.AddPredecessor, nodeName);
                contextMenu.Items.Add(addPredecessorItem);

                var addSiblingItem = new ToolStripMenuItem("Agregar Hermano");
                addSiblingItem.Click += (s, e) => SetAction(NodeAction.AddSibling, nodeName);
                contextMenu.Items.Add(addSiblingItem);
            }

            return contextMenu;
        }

        public bool ShouldProcessAction()
        {
            return ShouldShowForm;
        }
    }
}