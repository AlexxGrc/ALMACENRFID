using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ALMACENRFID
{
    static class Program
    {
        public static RFIDReaderAPI.RFID_Option RFID_OPTION = new RFIDReaderAPI.RFID_Option();
        public static RFIDReaderAPI.Param_Option PARAM_SET = new RFIDReaderAPI.Param_Option();
        public static RFIDReaderAPI.Test_Option TEST_OPTION = new RFIDReaderAPI.Test_Option();
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Menu());
        }
    }
}
