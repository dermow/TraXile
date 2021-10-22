using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraXile
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Mutex mutex = new Mutex(false, appGuid);
            if (!mutex.WaitOne(1000, false))
            {
                MessageBox.Show("Instance already running");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main() { Visible = false }); ;

            mutex.ReleaseMutex();
        }
#pragma warning disable IDE0044 // Modifizierer "readonly" hinzufügen
        private static string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
#pragma warning restore IDE0044 // Modifizierer "readonly" hinzufügen
    }
}
