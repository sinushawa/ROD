
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ROD_engine_DX11
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var tutorial = new ROD_Main();

            tutorial.Run();
            tutorial.Dispose();
        }
    }
}
