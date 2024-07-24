
using RFIDReaderAPI.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RFIDReaderAPI;
using ALMACENRFID;

namespace ALMACENRFID.MySingleForm.TestForm
{
    public partial class Tag_Option6B : BaseOption
    {
        IAsynchronousMessage contextForm = null;
        Int32 AntNum = 0;                            // 所选的天线编号
        String addAntParam = string.Empty;           //新增9-24天线配置参数

        String[] EPCdata = new String[] { "", "", "" };  // EPC,TID,UserData 

        public Tag_Option6B()
        {
            InitializeComponent();
        }

        public Tag_Option6B(string readerID, String[] epcData, Int32 antNum, String AddAntParam, IAsynchronousMessage contextForm)
            : this()
        {
            this.ConnID = readerID;
            this.EPCdata = epcData;
            this.contextForm = contextForm;
            this.AntNum = antNum;
            this.addAntParam = AddAntParam;
            this.Text = readerID.Trim() + _RC.GetString("Test_Tag_Option_Title2");
            this.tb_TID.Text = this.EPCdata[1];
            InitReaderProerty();
        }

        private void btn_0010_41_Click(object sender, EventArgs e)
        {
            try 
            {
                eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum+"");
                string tid = tb_TID.Text.Trim();
                int startIndex = Convert.ToInt32(tb_0010_41_00.Text.Trim());
                string writeData = tb_0010_41_01.Text.Trim();
                int rt = -1;
                if ((int)antNo > 255)
                {
                    MessageBox.Show("6B Label does not support 24 channels"); return;
                }
                rt = RFIDReader._Tag6B.Write6B(ConnID, antNo, tid, startIndex, writeData);
                ShowResult(rt);
            }
            catch { }
        }

        private static void ShowResult(int rt)
        {
            if (rt == 0)
            {
                MessageBox.Show("OK!");
            }
            else
            {
                MessageBox.Show("Failed!");
            }
        }

        private void btn_0010_42_Click(object sender, EventArgs e)
        {
            try
            {
                eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum+"");
                string tid = tb_TID.Text.Trim();
                int lockIndex = Convert.ToInt32(tb_0010_42_00.Text.Trim());
                string writeData = tb_0010_41_01.Text.Trim();
                int rt = -1;
                rt = RFIDReader._Tag6B.Lock6B(ConnID, antNo, tid, lockIndex);
                ShowResult(rt);

            }
            catch { }
        }

        private void btn_0010_43_Click(object sender, EventArgs e)
        {
            try
            {
                eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum+"");
                string tid = tb_TID.Text.Trim();
                int lockIndex = Convert.ToInt32(tb_0010_42_00.Text.Trim());
                string writeData = tb_0010_41_01.Text.Trim();
                
                string st =  RFIDReader._Tag6B.GetLock6B_State(ConnID, antNo, tid, lockIndex);
                if (st.StartsWith("-"))
                {
                    MessageBox.Show("Communication timeout!");
                }
                else
                {
                    MessageBox.Show(st);
                }
            }
            catch 
            {
                MessageBox.Show(_RC.GetString("Config_RFID_Parameter_Error"));
            }
        }

    }
}
