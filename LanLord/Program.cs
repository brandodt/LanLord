using System.Windows.Forms;

namespace LanLord
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [System.STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new ArpForm());
        }
    }
}
