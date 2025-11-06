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

        public NodeAction CurrentAction { get; set; }
        public string SelectedNode { get; set; }

        public InteractionService()
        {
            CurrentAction = NodeAction.None;
            SelectedNode = string.Empty;
        }

        public void ResetAction()
        {
            CurrentAction = NodeAction.None;
            SelectedNode = string.Empty;
        }
    }
}