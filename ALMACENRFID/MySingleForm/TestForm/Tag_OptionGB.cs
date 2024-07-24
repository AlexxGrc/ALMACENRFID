
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
    public partial class Tag_OptionGB : BaseOption
    {
        IAsynchronousMessage contextForm = null;
        Int32 AntNum = 0;                          // 所选的天线编号
        String addAntParam = string.Empty;         //新增9-24天线配置参数

        String[] EPCdata = new String[] { "", "", "" };  // EPC, TID, UserData

        public Dictionary<Int32, int> dic_DataType = new Dictionary<Int32, int>()         // Write data type
        {
            {0,16},
            {1,32},
            {2,0},
            {3,1},
            {4,2},
            {5,3},
            {6,4},
            {7,5},
            {8,6},
            {9,7},
            {10,8},
            {11,9},
            {12,10},
            {13,11},
            {14,12},
            {15,13},
            {16,14},
            {17,15},
        };
        public Dictionary<Int32, int> dic_AreaType = new Dictionary<Int32, int>()         // Write data type
        {
            {0,16},
            {1,32},
            {2,0+48},
            {3,1+48},
            {4,2+48},
            {5,3+48},
            {6,4+48},
            {7,5+48},
            {8,6+48},
            {9,7+48},
            {10,8+48},
            {11,9+48},
            {12,10+48},
            {13,11+48},
            {14,12+48},
            {15,13+48},
            {16,14+48},
            {17,15+48},
        };
        public Dictionary<Int32, byte> dic_LockType = new Dictionary<Int32, byte>()         // Lock Type
        {
            {0,0x00},
            {1,0x01},
            {2,0x02},
            {3,0x03},
            {4,0x17},
            {5,0x18},
            {6,0x19}
        };

        public Tag_OptionGB()
        {
            InitializeComponent();
        }

        public Tag_OptionGB(string readerID, String[] epcData, Int32 antNum, String AddAntParam, IAsynchronousMessage contextForm)
            : this()
        {
            this.ConnID = readerID;
            this.EPCdata = epcData;
            this.contextForm = contextForm;
            this.AntNum = antNum;
            this.addAntParam = AddAntParam;
            this.Text = readerID.Trim() + _RC.GetString("Test_Tag_Option_Title3");
            this.tb_EPC.Text = this.EPCdata[0];
            this.tb_TID.Text = this.EPCdata[1];
            this.tb_UserData.Text = this.EPCdata[2];
            Init();
        }

        public void Init() 
        {
            InitReaderProerty();
        }
        //  Write label
        private void btn_0010_51_Click(object sender, EventArgs e)
        {
            SynchroEPCdata();
            int macthTypeIndex = cb_0010_51_MatchType.SelectedIndex;
            string pwd = tb_0010_51_WritePassword.Text.Trim();
            int macthStartIndex = Convert.ToInt32(tb_0010_51_MatchStart.Text.Trim());
            int dataTypeIndex = cb_0010_51_DataType.SelectedIndex;
            eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            string writeData = tb_0010_51_WriteData.Text.Trim();
            int rt = -1;
            //RFIDReader._TagGB.WriteUserData
            if (dataTypeIndex==0) // EPC
            {
                switch (macthTypeIndex)
                {
                    case 0: 
                        rt = RFIDReader._TagGB.WriteEPC(ConnID, antNo, writeData);
                        break;
                    case 1: // Macth EPC
                        if (string.IsNullOrEmpty(pwd))
                        {
                            rt = RFIDReader._TagGB.WriteEPC_MatchEPC(ConnID, antNo, writeData, EPCdata[0], macthStartIndex);
                        }
                        else
                        {
                            rt = RFIDReader._TagGB.WriteEPC_MatchEPC(ConnID, antNo, writeData, EPCdata[0], macthStartIndex, pwd);
                        }
                        break;
                    case 2: // Macth TID
                        if (string.IsNullOrEmpty(pwd))
                        {
                            rt = RFIDReader._TagGB.WriteEPC_MatchTID(ConnID, antNo, writeData, EPCdata[1], macthStartIndex);
                        }
                        else
                        {
                            rt = RFIDReader._TagGB.WriteEPC_MatchTID(ConnID, antNo, writeData, EPCdata[1], macthStartIndex, pwd);
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (dataTypeIndex == 1) // Safety Zone
            {
                switch (macthTypeIndex)
                {
                    case 0: 
                        if (cb_0010_51_DataType_Pwd_Item.SelectedIndex == 0)
                        {
                            rt = RFIDReader._TagGB.WriteDestroyPassWord(ConnID, antNo, writeData, pwd);
                        }
                        else 
                        {
                            rt = RFIDReader._TagGB.WriteAccessPassWord(ConnID, antNo, writeData, pwd);
                        }
                        break;
                    case 2: // Macth EPC
                        if (cb_0010_51_DataType_Pwd_Item.SelectedIndex == 0) 
                        {
                            rt = RFIDReader._TagGB.WriteDestroyPassWord_MatchEPC(ConnID, antNo, writeData, EPCdata[0], macthStartIndex, pwd);
                        }
                        else 
                        {
                            rt = RFIDReader._TagGB.WriteAccessPassWord_MatchEPC(ConnID, antNo, writeData, EPCdata[0], macthStartIndex, pwd);
                        }
                        break;
                    case 1: // Macth TID
                       if (cb_0010_51_DataType_Pwd_Item.SelectedIndex == 0) 
                        {
                            rt = RFIDReader._TagGB.WriteDestroyPassWord_MatchTID(ConnID, antNo, writeData, EPCdata[1], macthStartIndex, pwd);
                        }
                        else 
                        {
                            rt = RFIDReader._TagGB.WriteAccessPassWord_MatchTID(ConnID, antNo, writeData, EPCdata[1], macthStartIndex, pwd);
                        }
                        break;
                    default:
                        break;
                }
            }
            else // User area & self user area
            {
                switch (macthTypeIndex)
                {
                    case 0: 
                        rt = RFIDReader._TagGB.WriteUserData(ConnID, antNo, dic_DataType[dataTypeIndex], writeData, 0);
                        break;
                    case 2: // Macth EPC
                        if (string.IsNullOrEmpty(pwd))
                        {
                            rt = RFIDReader._TagGB.WriteUserData_MatchEPC(ConnID, antNo, dic_DataType[dataTypeIndex], writeData, 0, EPCdata[0], macthStartIndex);
                        }
                        else
                        {
                            rt = RFIDReader._TagGB.WriteUserData_MatchEPC(ConnID, antNo, dic_DataType[dataTypeIndex], writeData, 0, EPCdata[0], macthStartIndex, pwd);
                        }
                        break;
                    case 1: // Macth TID
                        if (string.IsNullOrEmpty(pwd))
                        {
                            rt = RFIDReader._TagGB.WriteUserData_MatchTID(ConnID, antNo, dic_DataType[dataTypeIndex], writeData, 0, EPCdata[1], macthStartIndex);
                        }
                        else
                        {
                            rt = RFIDReader._TagGB.WriteUserData_MatchTID(ConnID, antNo, dic_DataType[dataTypeIndex], writeData, 0, EPCdata[1], macthStartIndex, pwd);
                        }
                        break;
                }
            }
            if (rt==0)
            {
                MessageBox.Show("OK!");
            }
            else
            {
                MessageBox.Show("Failed!");
            }
            
        }

        // Lock Tag
        private void btn_0010_52_Click(object sender, EventArgs e)
        {
            int macthTypeIndex = cb_0010_51_MatchType.SelectedIndex;
            string pwd = tb_0010_52_LockPassword.Text.Trim();
            int macthStartIndex = Convert.ToInt32(tb_0010_51_MatchStart.Text.Trim());
            int lockArea = cb_0010_52_LockArea.SelectedIndex;
            int lockTypeIndex = cb_0010_52_LockType.SelectedIndex;
            eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            int rt = -1;
            SynchroEPCdata();
           
            
            switch (macthTypeIndex)
            {
                case 0: 
                    if (string.IsNullOrEmpty(pwd)) {
                        rt = RFIDReader._TagGB.Lock(ConnID, antNo, (eLockAreaGB)dic_AreaType[lockArea], (eLockTypeGB)dic_LockType[lockTypeIndex]);
                    }
                    else
                    {
                        rt = RFIDReader._TagGB.Lock(ConnID, antNo, (eLockAreaGB)dic_AreaType[lockArea], (eLockTypeGB)dic_LockType[lockTypeIndex],pwd);
                    }
                    
                    break;
                case 2: // Macth EPC
                    if (string.IsNullOrEmpty(pwd))
                    {
                        rt = RFIDReader._TagGB.Lock_MatchEPC(ConnID, antNo, (eLockAreaGB)dic_AreaType[lockArea], (eLockTypeGB)dic_LockType[lockTypeIndex], EPCdata[0], macthStartIndex);
                    }
                    else
                    {
                        rt = RFIDReader._TagGB.Lock_MatchEPC(ConnID, antNo, (eLockAreaGB)dic_AreaType[lockArea], (eLockTypeGB)dic_LockType[lockTypeIndex], EPCdata[0], macthStartIndex, pwd);
                    }
                    break;
                case 1: // Macth TID
                    if (string.IsNullOrEmpty(pwd))
                    {
                        rt = RFIDReader._TagGB.Lock_MatchTID(ConnID, antNo, (eLockAreaGB)dic_AreaType[lockArea], (eLockTypeGB)dic_LockType[lockTypeIndex], EPCdata[1], macthStartIndex);
                    }
                    else
                    {
                        rt = RFIDReader._TagGB.Lock_MatchTID(ConnID, antNo, (eLockAreaGB)dic_AreaType[lockArea], (eLockTypeGB)dic_LockType[lockTypeIndex], EPCdata[1], macthStartIndex, pwd);
                    }
                    break;
                default:
                    break;
            }
            
           
            if (rt == 0)
            {
                MessageBox.Show("OK!");
            }
            else
            {
                MessageBox.Show("Failed!");
            }

        }

        // Destroy the label
        private void btn_0010_53_Click(object sender, EventArgs e)
        {
            SynchroEPCdata();
            int macthTypeIndex = cb_0010_51_MatchType.SelectedIndex;
            string pwd = tb_0010_53_DestroyPwd.Text.Trim();
            int macthStartIndex = Convert.ToInt32(tb_0010_51_MatchStart.Text.Trim());
            eAntennaNo antNo = RFIDReader._Tag6C.GeteAntennaNo(AntNum + "");
            int rt = -1;
            if (string.IsNullOrEmpty(pwd))
            {
                MessageBox.Show("password can not be blank");
                return;
            }
            switch (macthTypeIndex)
            {
                case 0: 
                    rt = RFIDReader._TagGB.Destroy(ConnID, antNo, pwd);
                    break;
                case 2: // Macth EPC
                    rt = RFIDReader._TagGB.Destroy_MatchEPC(ConnID, antNo, pwd, EPCdata[0], macthStartIndex);
                    break;
                case 1: // Macth TID
                    rt = RFIDReader._TagGB.Destroy_MatchTID(ConnID, antNo, pwd, EPCdata[1], macthStartIndex);
                    break;
                default:
                    break;
            }
            if (rt==0)
            {
                MessageBox.Show("OK!");
            }
            else
            {
                MessageBox.Show("Error:"+rt);
            }
           
        }

        private void cb_0010_51_DataType_Pwd_Item_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_51_DataType_Pwd_Item.SelectedIndex == 0)
            {
                tb_0010_51_WriteDataStart.Text = "0000";
            }
            else if (cb_0010_51_DataType_Pwd_Item.SelectedIndex == 1)
            {
                tb_0010_51_WriteDataStart.Text = "0002";
            }
        }


        #region 写入事件

        private void cb_0010_51_DataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_51_DataType.SelectedIndex == 0)    
            {
                cb_0010_11_DataType_EPC_Item.Visible = true;
                cb_0010_51_DataType_Pwd_Item.Visible = false;
            }
            else if(cb_0010_51_DataType.SelectedIndex == 1)
            {
                cb_0010_11_DataType_EPC_Item.Visible = false;
                cb_0010_51_DataType_Pwd_Item.Visible = true;
            }
            else
            {
                cb_0010_51_DataType_Pwd_Item.Visible = false;
                cb_0010_11_DataType_EPC_Item.Visible = false;
            }
            if (cb_0010_51_DataType.SelectedIndex >= 2) 
            {
                tb_0010_51_WriteDataStart.Text = "0004";
            }
        }


        private void cb_0010_11_DataType_EPC_Item_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_11_DataType_EPC_Item.SelectedIndex == 0) 
            {
                tb_0010_51_WriteDataStart.Text = "0000";
            }
            else if (cb_0010_11_DataType_EPC_Item.SelectedIndex == 1)
            {
                tb_0010_51_WriteDataStart.Text = "0001";
            }
        }


        #endregion



        #region 辅助通用方法
        /// <summary>
        /// Synchronize tag data before operating the tag
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
        /// Check if the write data is legal
        /// </summary>
        /// <returns>True legal, false is illegal</returns>
        private bool CheckWriteData(out string errMsg)
        {
            bool rt = true;
            errMsg = "";
            if (!String.IsNullOrEmpty(tb_0010_51_WritePassword.Text) && tb_0010_51_WritePassword.Text.Length != 8)
            {
                errMsg = _RC.GetString("Test_Tag_Option_Error2");
                rt = false;
            }
            return rt;
        }

        #endregion

        private void Tag_OptionGB_Load(object sender, EventArgs e)
        {
            cb_0010_51_DataType.SelectedIndex = 0;
            cb_0010_51_MatchType.SelectedIndex = 0;
        }

        

        
    }
}
