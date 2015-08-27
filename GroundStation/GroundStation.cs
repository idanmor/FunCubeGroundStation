using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GroundStation
{
    static class GroundStation
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static public List<Telemetry> AllTelem = new List<Telemetry>();
        static public int IndexOfTelemetry = 0;
        
        [STAThread]
        
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TelemetryWindow());

        }

    }
}
