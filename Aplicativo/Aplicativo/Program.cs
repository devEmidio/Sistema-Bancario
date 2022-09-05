using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aplicativo
{
    static class Program
    {
        //Form
        public static Form1 loginForm;

        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Instancia classe
            loginForm = new Form1();
            Application.Run(loginForm);
        }
    }
}
