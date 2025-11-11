using System.Windows.Forms;

namespace Proyecto_Grafos.UI.Controls
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }
}