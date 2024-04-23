using FEL_ONE.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FEL_ONE
{
    static class Program
    {
        [STAThread]
        [Obsolete]
        static void Main()
        {
            SystemForm SBOSystemForm = new SystemForm();
            Application.Run();
        }
    }
}
