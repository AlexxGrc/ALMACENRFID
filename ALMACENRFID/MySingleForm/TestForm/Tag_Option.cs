
using RFIDReaderAPI.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RFIDReaderAPI;
using ALMACENRFID.Helper;
using ALMACENRFID;

namespace ALMACENRFID.MySingleForm.TestForm
{
    public partial class Tag_Option : BaseOption
    {
        IAsynchronousMessage contextForm = null;
        Int32 AntNum = 0;                           // 所选的天线编号
        String addAntParam = string.Empty;          //新增9-24天线配置参数

        String[] EPCdata = new String[] { "", "", "" };  // EPC, TID, UserData
        public Tag_Option()
        {
            InitializeComponent();
            Init();
        }

        public Tag_Option(string readerID, String[] epcData, Int32 antNum, String AddAntParam, IAsynchronousMessage contextForm)
            : this()
        {
            this.ConnID = readerID;
            this.EPCdata = epcData;
            this.contextForm = contextForm;
            this.AntNum = antNum;
            this.addAntParam = AddAntParam;
            this.Text = readerID.Trim() + _RC.GetString("Test_Tag_Option_Title1");
            this.tb_EPC.Text = this.EPCdata[0];
            this.tb_TID.Text = this.EPCdata[1];
            this.tb_UserData.Text = this.EPCdata[2];
            Init();
        }

        public void Init()
        {
            InitReaderProerty();
            InitWrite();
            InitLock();
        }


        #region 标签写操作集合

        public void InitWrite()
        {
            cb_0010_11_DataType_EPC_Item.Visible = true;
            cb_0010_11_DataType_EPC_Item.SelectedIndex = 0;
            cb_0010_11_DataType_Pwd_Item.Visible = false;
            cb_0010_11_DataType_Pwd_Item.SelectedIndex = 0;
            cb_0010_11_DataType.SelectedIndex = 1;
            cb_0010_11_MatchType.SelectedIndex = 0;
           
        }

        private void cb_0010_11_MatchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_11_MatchType.SelectedIndex == 1)
            {
                tb_0010_11_MatchStart.Text = "0032";
            }
            else
            {
                tb_0010_11_MatchStart.Text = "0000";
            }

            if (cb_0010_11_MatchType.SelectedIndex == 0)
            {
                lb_0010_11_MatchStart.Visible = false;
                tb_0010_11_MatchStart.Visible = false;
            }
            else
            {
                lb_0010_11_MatchStart.Visible = true;
                tb_0010_11_MatchStart.Visible = true;
            }
        }

       

        private void btn_0010_11_Click(object sender, EventArgs e)
        {
            SynchroEPCdata();
            eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            string writeData = tb_0010_11_WriteData.Text.Trim(); 
            string startdata = tb_0010_11_WriteDataStart.Text.Trim();
            string pwd = tb_0010_11_AccessPwd.Text.Trim();
            int macthStartIndex = Convert.ToInt32(tb_0010_11_MatchStart.Text.Trim());
            string rt = "-1";

            switch (cb_0010_11_DataType.SelectedIndex) 
            {
                case 0: // 写 reserve
                    if (string.IsNullOrEmpty(pwd))
                    {
                        MessageBox.Show("When writing a reserved area, the password cannot be empty!");
                        return;
                    }

                    #region 写保留区 (密码区)
                    switch (cb_0010_11_MatchType.SelectedIndex)
                    {
                        case 0: 
                            if (cb_0010_11_DataType_Pwd_Item.SelectedIndex == 0) 
                            {
                                rt = RFIDReader._Tag6C.WriteDestroyPassWord(ConnID, (eAntennaNo)AntNum, writeData, accessPassword: pwd);
                            }
                            else  
                            {
                                rt = RFIDReader._Tag6C.WriteAccessPassWord(ConnID, (eAntennaNo)AntNum, writeData , accessPassword: pwd);
                            }
                            break;
                        case 1: // MacthEPC
                            if (cb_0010_11_DataType_Pwd_Item.SelectedIndex == 0) 
                            {
                                rt = RFIDReader._Tag6C.WriteDestroyPassWord(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.EPC, matchCode: EPCdata[0], matchWordStartIndex: macthStartIndex, accessPassword: pwd);
                            }
                            else 
                            {
                                rt = RFIDReader._Tag6C.WriteAccessPassWord(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.EPC, matchCode: EPCdata[0], matchWordStartIndex: macthStartIndex, accessPassword: pwd);
                            }
                            break;
                        case 2: // Macth TID
                            if (cb_0010_11_DataType_Pwd_Item.SelectedIndex == 0) 
                            {
                                rt = RFIDReader._Tag6C.WriteDestroyPassWord(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.TID, matchCode: EPCdata[1], matchWordStartIndex: macthStartIndex, accessPassword: pwd);
                            }
                            else 
                            {
                                rt = RFIDReader._Tag6C.WriteAccessPassWord(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.TID, matchCode: EPCdata[1], matchWordStartIndex: macthStartIndex, accessPassword: pwd);
                            }
                            break;
                    }
                    break; 
                    #endregion

                case 1: // 写 EPC

                    #region 写 EPC
                    switch (cb_0010_11_MatchType.SelectedIndex)
                    {
                        case 0: 
                            
                              rt = RFIDReader._Tag6C.WriteEPC(ConnID, (eAntennaNo)AntNum, writeData, startdata);
                            break;
                        case 1: // Macth EPC
                            if (string.IsNullOrEmpty(pwd)) 
                            {
                                rt = RFIDReader._Tag6C.WriteEPC(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.EPC, matchCode: EPCdata[0], matchWordStartIndex: macthStartIndex);
                            }
                            else
                            {
                                rt = RFIDReader._Tag6C.WriteEPC(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.EPC, matchCode: EPCdata[0], matchWordStartIndex: macthStartIndex, accessPassword: pwd);
                            }
                            break;
                        case 2: // Macth TID
                            if (string.IsNullOrEmpty(pwd)) 
                            {
                                rt = RFIDReader._Tag6C.WriteEPC(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.TID, matchCode: EPCdata[1], matchWordStartIndex: macthStartIndex);
                            }
                            else
                            {
                                rt = RFIDReader._Tag6C.WriteEPC(ConnID, (eAntennaNo)AntNum, writeData, matchType: eMatchCode.TID, matchCode: EPCdata[1], matchWordStartIndex: macthStartIndex, accessPassword: pwd);
                            }
                            break;
                    } 
	                #endregion

                    break;
                case 2: //  写 user data

                    #region 写 user data
                    switch (cb_0010_11_MatchType.SelectedIndex)
                    {
                        case 0: // Mismatch
                            if (string.IsNullOrEmpty(pwd))
                            {
                                rt = RFIDReader._Tag6C.WriteUserData(ConnID, (eAntennaNo)AntNum, writeData, Convert.ToInt32(startdata));
                            }
                            else
                            {
                                rt = RFIDReader._Tag6C.WriteUserData(ConnID, (eAntennaNo)AntNum, writeData, Convert.ToInt32(startdata),accessPassword:pwd);
                            }
                            break;
                        case 1: // Match EPC
                            if (string.IsNullOrEmpty(pwd))
                            {
                                rt = RFIDReader._Tag6C.WriteUserData(ConnID, (eAntennaNo)AntNum, writeData, Convert.ToInt32(startdata),matchType:eMatchCode.EPC,matchCode: EPCdata[0],matchWordStartIndex: macthStartIndex);
                            }
                            else
                            {
                                rt = RFIDReader._Tag6C.WriteUserData(ConnID, (eAntennaNo)AntNum, writeData, Convert.ToInt32(startdata), matchType: eMatchCode.EPC, matchCode: EPCdata[0], matchWordStartIndex: macthStartIndex,accessPassword:pwd);
                            }
                            break;
                        case 2: // Match TID
                            if (string.IsNullOrEmpty(pwd))
                            {
                                rt = RFIDReader._Tag6C.WriteUserData(ConnID, (eAntennaNo)AntNum, writeData, Convert.ToInt32(startdata), matchType: eMatchCode.TID, matchCode: EPCdata[1], matchWordStartIndex: macthStartIndex);
                            }
                            else
                            {
                                rt = RFIDReader._Tag6C.WriteUserData(ConnID, (eAntennaNo)AntNum, writeData, Convert.ToInt32(startdata), matchType: eMatchCode.TID, matchCode: EPCdata[1], matchWordStartIndex: macthStartIndex, accessPassword: pwd);
                            }
                            break;
                    } 
	                #endregion

                    break;
            }
            
            MessageBox.Show(rt);
            

        }

        private void cb_0010_11_DataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_11_DataType.SelectedIndex == 0)
            {
                cb_0010_11_DataType_EPC_Item.Visible = false;
                cb_0010_11_DataType_Pwd_Item.Visible = true;
                cb_0010_11_DataType_Pwd_Item_SelectedIndexChanged(null, null);
            }
            else if (cb_0010_11_DataType.SelectedIndex == 1)
            {
                cb_0010_11_DataType_EPC_Item.Visible = true;
                cb_0010_11_DataType_Pwd_Item.Visible = false;
                cb_0010_11_DataType_EPC_Item_SelectedIndexChanged(null, null);
            }
            else
            {
                cb_0010_11_DataType_EPC_Item.Visible = false;
                cb_0010_11_DataType_Pwd_Item.Visible = false;
            }
        }

        private void cb_0010_11_DataType_EPC_Item_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_11_DataType_EPC_Item.SelectedIndex == 0)
            {
                tb_0010_11_WriteDataStart.Text = "0001";
            }
            else if (cb_0010_11_DataType_EPC_Item.SelectedIndex == 1)
            {
                tb_0010_11_WriteDataStart.Text = "0002";
            }
        }

        private void cb_0010_11_DataType_Pwd_Item_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_11_DataType_Pwd_Item.SelectedIndex == 0)
            {
                tb_0010_11_WriteDataStart.Text = "0000";
            }
            else if (cb_0010_11_DataType_Pwd_Item.SelectedIndex == 1)
            {
                tb_0010_11_WriteDataStart.Text = "0002";
            }
        }
        #endregion

        #region 标签锁操作集合

        public void InitLock()
        {
            cb_0010_12_LockArea.SelectedIndex = 0;
            cb_0010_12_LockType.SelectedIndex = 0;
        }

        private void btn_0010_12_Click(object sender, EventArgs e)
        {
            SynchroEPCdata();
            eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            string pwd = tb_0010_11_AccessPwd.Text.Trim();
            int macthStartIndex = Convert.ToInt32(tb_0010_11_MatchStart.Text.Trim());
            eLockArea area = (eLockArea)cb_0010_12_LockArea.SelectedIndex;
            eLockType locktype = (eLockType)cb_0010_12_LockType.SelectedIndex;
            int rt = -1;
            switch (cb_0010_11_MatchType.SelectedIndex)
            {
                case 0: 
                    rt = RFIDReader._Tag6C.Lock(ConnID, (eAntennaNo)AntNum, area, locktype);
                    break;
                case 1: 
                    if (string.IsNullOrEmpty(pwd))
                    {
                        rt = RFIDReader._Tag6C.Lock_MatchEPC(ConnID, (eAntennaNo)AntNum, area, locktype, EPCdata[0], macthStartIndex);
                    }
                    else
                    {
                        rt = RFIDReader._Tag6C.Lock_MatchEPC(ConnID, (eAntennaNo)AntNum, area, locktype, EPCdata[0], macthStartIndex,pwd);
                    }
                    break;
                case 2: 
                    if (string.IsNullOrEmpty(pwd))
                    {
                        rt = RFIDReader._Tag6C.Lock_MatchTID(ConnID, (eAntennaNo)AntNum, area, locktype, EPCdata[1], macthStartIndex);
                    }
                    else
                    {
                        rt = RFIDReader._Tag6C.Lock_MatchTID(ConnID, (eAntennaNo)AntNum, area, locktype, EPCdata[1], macthStartIndex, pwd);
                    }
                    break;
            }
            if (rt == 0)
            {
                MessageBox.Show("OK");
            }
            else
            {
                MessageBox.Show(LanguageHelper.GetResutlCode(1,rt));
            }
            
        }

        #endregion

        #region 销毁标签操作集合

        private void btn_0010_13_Click(object sender, EventArgs e)
        {
            SynchroEPCdata();
            eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            string dpwd = tb_0010_13_DestroyPwd.Text.Trim();
            int macthStartIndex = Convert.ToInt32(tb_0010_11_MatchStart.Text.Trim());
            eLockArea area = (eLockArea)cb_0010_12_LockArea.SelectedIndex;
            eLockType locktype = (eLockType)cb_0010_12_LockType.SelectedIndex;
            int rt = -1;
            switch (cb_0010_11_MatchType.SelectedIndex)
            {
                case 0: 
                    rt = RFIDReader._Tag6C.Destroy(ConnID, (eAntennaNo)AntNum, dpwd);
                    break;
                case 1: 
                    rt = RFIDReader._Tag6C.Destroy_MatchEPC(ConnID, (eAntennaNo)AntNum, dpwd, EPCdata[0], macthStartIndex);
                    break;
                case 2: 
                    rt = RFIDReader._Tag6C.Destroy_MatchTID(ConnID, (eAntennaNo)AntNum, dpwd, EPCdata[1], macthStartIndex);
                    break;
            }
            if (rt == 0)
            {
                MessageBox.Show("OK");
            }
            else
            {
                MessageBox.Show("Failed");
            }
        }

        #endregion

        #region 通用辅助方法
        /// <summary>
        /// 操作标签之前同步标签数据
        /// </summary>
        private void SynchroEPCdata()
        {
            try
            {
                EPCdata[0] = tb_EPC.Text.Trim();
                EPCdata[1] = tb_TID.Text.Trim();
                EPCdata[2] = tb_UserData.Text.Trim();
            }
            catch { }
        }

        /// <summary>
        /// 检查写入数据是否合法
        /// </summary>
        /// <returns>True legal, false is illegal</returns>
        private bool CheckWriteData(out string errMsg)
        {
            bool rt = true;
            errMsg = "";
            if (cb_0010_11_DataType.SelectedIndex == 0)         
            {
                if (tb_0010_11_WriteData.Text.Length != 8)
                {
                    errMsg = _RC.GetString("Test_Tag_Option_Error2");
                    rt = false;
                }
            }
            return rt;
        }

        #endregion


    }
}
