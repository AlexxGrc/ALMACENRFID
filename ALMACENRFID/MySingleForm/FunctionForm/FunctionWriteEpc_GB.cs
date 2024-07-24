
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
    public partial class FunctionWriteEpc_GB : BaseOption
    {
        IAsynchronousMessage contextForm = null;
        Int32 AntNum = 0;                         // 所选的天线编号
        String addAntParam = string.Empty;        //新增9-24天线配置参数

        String[] EPCdata = new String[] { "", "", "" };  // EPC,TID,UserData   ,当前选择的标签数据  

        public FunctionWriteEpc_GB()
        {
            InitializeComponent();
        }
        public FunctionWriteEpc_GB(string readerID, String[] epcData, Int32 antNum, String AddAntParam, IAsynchronousMessage contextForm)
            : this()
        {
            this.ConnID = readerID;
            this.EPCdata = epcData;
            this.contextForm = contextForm;
            this.AntNum = antNum;
            this.addAntParam = AddAntParam;
        }

        private void FunctionWriteEpc_Load(object sender, EventArgs e)
        {
            this.Width = 500;
            this.Height = 300;
            Init();
        }
        
        public void Init() 
        {
            this.tb_SelectEPC.Text = EPCdata[0];
            this.tb_SelectTID.Text = EPCdata[1];
            InitReaderProerty();
        }
        // 获取写入EPC的长度
        private void tb_WriteEPCData_KeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                tb_WriteEPCLength.Text = (tb_WriteEPCData.Text.Length % 4 == 0 ? tb_WriteEPCData.Text.Length / 4 : tb_WriteEPCData.Text.Length / 4 + 1).ToString();
            }
        }
        // 
        private void tb_WriteEPCData_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        #region 通用方法
        private void WriteEPC() 
        {
            var antno = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            string pwd = tb_AccessPwd.Text.Trim();
            string selectEPC = tb_SelectEPC.Text.Trim();
            string selectTID = tb_SelectTID.Text.Trim();
            string writeData = tb_WriteEPCData.Text.Trim();
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
                    result = RFIDReader._TagGB.WriteEPC_MatchEPC(ConnID, antno, writeData, selectEPC, 0);
                }
                else
                {
                    result = RFIDReader._TagGB.WriteEPC_MatchTID(ConnID, antno, writeData, selectTID, 0);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(tb_SelectTID.Text.Trim()))
                {
                    result = RFIDReader._TagGB.WriteEPC_MatchEPC(ConnID, antno, writeData, selectEPC, 0, pwd);
                }
                else
                {
                    result = RFIDReader._TagGB.WriteEPC_MatchTID(ConnID, antno, writeData, selectTID, 0, pwd);
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
            WriteEPC();
        }

    }
}
