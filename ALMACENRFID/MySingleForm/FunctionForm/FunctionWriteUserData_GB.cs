
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

namespace ALMACENRFID.MySingleForm.TestForm.FunctionForm
{
    public partial class FunctionWriteUserData_GB : BaseOption
    {
        IAsynchronousMessage contextForm = null;
        Int32 AntNum = 0;                         // 所选的天线编号
        String addAntParam = string.Empty;        //新增9-24天线配置参数

        String[] EPCdata = new String[] { "", "", "" };  // EPC,TID,UserData   

        byte writeType = 0x30;                           // 
        Dictionary<Int32, byte> dic_WriteType = new Dictionary<int, byte>() 
        {
            {0,0x30},
            {1,0x31},
            {2,0x32},
            {3,0x33},
            {4,0x34},
            {5,0x35},
            {6,0x36},
            {7,0x37},
            {8,0x38},
            {9,0x39},
            {10,0x3A},
            {11,0x3B},
            {12,0x3C},
            {13,0x3D},
            {14,0x3E},
            {15,0x3F}
        };

        public FunctionWriteUserData_GB()
        {
            InitializeComponent();
        }

        public FunctionWriteUserData_GB(string readerID, String[] epcData, Int32 antNum, String AddAntParam, IAsynchronousMessage contextForm)
            : this()
        {
            this.ConnID = readerID;
            this.EPCdata = epcData;
            this.contextForm = contextForm;
            this.AntNum = antNum;
            this.addAntParam = AddAntParam;
            Init();
        }

        public void Init()
        {
            this.tb_SelectEPC.Text = EPCdata[0];
            this.tb_SelectTID.Text = EPCdata[1];
            this.cb_WriteUser_DataType.SelectedIndex = 0;
            InitReaderProerty();
        }

        private void FunctionWriteUserData_Load(object sender, EventArgs e)
        {
            this.Width = 500;
            this.Height = 300;
        }

        private void tb_WriteEPCData_KeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                tb_WriteEPCLength.Text = (tb_WriteUserData.Text.Length % 4 == 0 ? tb_WriteUserData.Text.Length / 4 : tb_WriteUserData.Text.Length / 4 + 1).ToString();
            }
        }

        #region 通用方法
        private void WriteUserData()
        {
            var antno = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            string pwd = tb_AccessPwd.Text.Trim();
            string selectEPC = tb_SelectEPC.Text.Trim();
            string selectTID = tb_SelectTID.Text.Trim();
            string writeData = this.tb_WriteUserData.Text.Trim();
            int typeIndex= cb_WriteUser_DataType.SelectedIndex;
            int result = -1;
            if (string.IsNullOrEmpty(writeData))
            {
                MessageBox.Show("Write Data is null!");
                return;
            }
            else if (string.IsNullOrEmpty(pwd))
            {
                if (string.IsNullOrEmpty(selectTID))
                {

                    result = RFIDReader._TagGB.WriteUserData_MatchEPC(ConnID, (eAntennaNo)AntNum, typeIndex, writeData, 0, selectEPC,0);
                }
                else
                {
                    result = RFIDReader._TagGB.WriteUserData_MatchTID(ConnID, (eAntennaNo)AntNum, typeIndex, writeData, 0, selectTID, 0);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(tb_SelectTID.Text.Trim()))
                {
                    result = RFIDReader._TagGB.WriteUserData_MatchEPC(ConnID, (eAntennaNo)AntNum, typeIndex, writeData, 0, selectEPC, 0);
                }
                else
                {
                    result = RFIDReader._TagGB.WriteUserData_MatchTID(ConnID, (eAntennaNo)AntNum, typeIndex, writeData, 0, selectTID, 0);
                }
            }
            if (result != 0)
            {
                ShowMessage(_RC.GetString("FunctionWrite_EPC_Error") + result);
            }
            else
            {
                ShowMessage(_RC.GetString("FunctionWrite_Success"));
            }
        }

        #endregion

        private void btn_Write_Click(object sender, EventArgs e)
        {
            WriteUserData();
        }
        private void cb_WriteUser_DataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                writeType = dic_WriteType[cb_WriteUser_DataType.SelectedIndex];
            }
            catch { }
        }
    }
}
