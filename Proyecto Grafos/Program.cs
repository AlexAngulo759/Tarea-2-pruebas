using System;
using System.Windows.Forms;
using Proyecto_Grafos.UI.Forms;

namespace Proyecto_Grafos
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
