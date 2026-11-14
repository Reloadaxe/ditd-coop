using System;
using System.Windows.Forms;

namespace DitdLauncher
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Updater.CleanupOldLauncher();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
