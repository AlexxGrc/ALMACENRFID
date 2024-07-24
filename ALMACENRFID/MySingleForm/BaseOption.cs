
using ALMACENRFID;
using ALMACENRFID.MyFormTemplet;
using ALMACENRFID.Helper;
using ALMACENRFID.Forms;
using ALMACENRFID.Win32;
using ALMACENRFID.MyFormTemplet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ALMACENRFID.MySingleForm
{
    public partial class BaseOption : _360Form
    {
        protected String ConnID = "";

        #region Reader capability

        protected int minDB = 0;                                             // Minimum output power
        protected int maxDB = 36;                                            // Maximum output power
        protected int antCount = 2;                                          // Number of antennas
        protected List<Int32> bandList = new List<Int32>();                  // Frequency band list
        protected List<Int32> RFIDProtocolList = new List<Int32>();          // RFID protocol list

        #endregion

        public BaseOption()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Query reader properties
        /// </summary>
        protected void InitReaderProerty()
        {
            string strReaderProperty = Program.RFID_OPTION.GetReaderProperty(ConnID);
            string[] str_array = strReaderProperty.Split('|');
            if (str_array.Length == 5)
            {
                try
                {
                    minDB = Int32.Parse(str_array[0]);
                    maxDB = Int32.Parse(str_array[1]);
                    antCount = Int32.Parse(str_array[2]);
                    string[] str_bandList = str_array[3].Split(',');
                    string[] str_RFIDProtocolList = str_array[4].Split(',');
                    for (int i = 0; i < str_bandList.Length; i++)
                    {
                        bandList.Add(Int32.Parse(str_bandList[i]));
                    }
                    for (int i = 0; i < str_RFIDProtocolList.Length; i++)
                    {
                        RFIDProtocolList.Add(Int32.Parse(str_RFIDProtocolList[i]));
                    }
                }
                catch { }
            }
        }

        private void BaseOption_Load(object sender, EventArgs e)
        {
            this.ShowIcon = false;
            base.IsResize = false;
            this.SysButton = ESysButton.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 800; this.Height = 460;
            // this.TopMost = true;
        }

        private void BaseOption_Shown(object sender, EventArgs e)
        {
            
        }
    }
}
