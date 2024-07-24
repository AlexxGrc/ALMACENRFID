using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RFIDReaderAPI;
using RFIDReaderAPI.Interface;
using ALMACENRFID.MyFormTemplet;

using ALMACENRFID;

namespace ALMACENRFID.MySingleForm.TestForm
{
    public partial class RFID_Option : BaseOption
    {
        IAsynchronousMessage contextForm = null;
		
		int minAnt = 100;
        public Int32 rfIndex = 0;
        Dictionary<Int32, List<String>> DIC_RF = new Dictionary<int, List<String>>();

        public RFID_Option()
        {
            InitializeComponent();
        }

        public RFID_Option(string readerID, IAsynchronousMessage contextForm)
            : this()
        {
            this.ConnID = readerID;
            this.contextForm = contextForm;
            this.Text = readerID.Trim() + _RC.GetString("Config_RFID_Option_Title");
        }

        private void RFID_Option_Load(object sender, EventArgs e)
        {
            InitDIC_RF();
            InitComboBox();
            tc_Main.SelectedIndex = -1;
            tc_Main.SelectedIndex = 0;
        }

        private void RFID_Option_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        #region General auxiliary method

        private void InitDIC_RF()
        {
            DIC_RFAddItem(0, 920.625f, 924.375f, 0.25f);
            DIC_RFAddItem(1, 840.625f, 844.375f, 0.25f);
            DIC_RFAddItem(2, 840.625f, 844.375f, 0.25f);
            DIC_RFAddItem(2, 920.625f, 924.375f, 0.25f);
            DIC_RFAddItem(3, 902.75f, 927.25f, 0.5f);
            DIC_RFAddItem(4, 865.7f, 868.1f, 0.6f);
            DIC_RFAddItem(5, 916.8f, 920.4f, 1.2f);
            DIC_RFAddItem(6, 922.25f, 927.75f, 0.25f);
            DIC_RFAddItem(7, 923.125f, 925.125f, 0.25f);        
            DIC_RFAddItem(8, 866.6f, 867.4f, 0.2f);
            DIC_RFAddItem(9, 920.125f, 925.0f, 0.25f);       
            DIC_RFAddItem(10, 917.1f, 923.5f, 0.2f);         
        }

        private void DIC_RFAddItem(Int32 index, float fStart, float fEnd, float jump)
        {
            List<String> listItem = new List<String>();
            while (fStart <= fEnd)
            {
                listItem.Add(fStart.ToString("F3"));
                fStart += jump;
            }
            if (!DIC_RF.ContainsKey(index))
            {
                DIC_RF.Add(index, listItem);
            }
            else
            {
                foreach (String item in listItem)
                {
                    DIC_RF[index].Add(item);
                }
            }
        }

        private void InitComboBox()
        {
            Int32 index = 0;
            cb_0010_05_01.Items.Clear();
            foreach (String item in DIC_RF[rfIndex])
            {
                cb_0010_05_01.Items.Add(new CCBoxItem(item, index));
                index++;
            }
			cb_FrequencySingle.Items.Clear();
			for (int i = 0; i <  DIC_RF[rfIndex].Count; i++)
			{
				cb_FrequencySingle.Items.Add(DIC_RF[rfIndex][i]);
			}
			cb_FrequencySingle.SelectedIndex = 0;
            cb_0010_05_01.MaxDropDownItems = 8;
            // Make the "Name" property the one to display, rather than the ToString() representation.
            cb_0010_05_01.DisplayMember = "Name";
            cb_0010_05_01.ValueSeparator = ", ";
        }

        #endregion

        #region Reader settings

        private void btn_0001_02_Get_Click(object sender, EventArgs e)
        {
            int rs = (int)RFIDReader._ReaderConfig.GetReaderSerialPortParam(ConnID);
            cb_0001_02_00.SelectedIndex = rs;
        }

        private void btn_0001_02_Set_Click(object sender, EventArgs e)
        {
            eBaudrate param = (eBaudrate)cb_0001_02_00.SelectedIndex;
            var rs = RFIDReader._ReaderConfig.SetReaderSerialPortParam(ConnID, param);
            ShowResult(rs);
        }

        private void btn_0001_04_Get_Click(object sender, EventArgs e)
        {
            string rtStr = RFIDReader._ReaderConfig.GetReaderNetworkPortParam(ConnID);
            String[] ipParam = rtStr.Split('|');
            if (ipParam.Length == 3)
            {
                tb_0001_04_00.Text = ipParam[0];
                tb_0001_04_01.Text = ipParam[1];
                tb_0001_04_02.Text = ipParam[2];
            }
            else
            {
                ShowMessage(rtStr);
            }
        }

        private void btn_0001_05_Set_Click(object sender, EventArgs e)
        {
            if (!CheckeData(new List<string>() { tb_0001_04_00.Text.Trim(), tb_0001_04_01.Text.Trim(), tb_0001_04_02.Text.Trim() }))
                return;
            var st = RFIDReader._ReaderConfig.SetReaderNetworkPortParam(ConnID, tb_0001_04_00.Text.Trim(), tb_0001_04_01.Text.Trim(), tb_0001_04_02.Text.Trim());
            ShowResult(st);
        }

        private void btn_0001_06_Get_Click(object sender, EventArgs e)
        {
            tb_0001_06_00.Text = RFIDReader._ReaderConfig.GetReaderMacParam(ConnID);
        }

        private void btn_0001_07_Set_Click(object sender, EventArgs e)
        {
            eWorkMode work = (eWorkMode)(rb_0001_08_00.Checked?0:1);
            List<string> datalist = new List<string>();
            string ip = "";
            string port = "";
            
            if (rb_0001_08_00.Checked) // server
            {
                port = tb_0001_08_01.Text.Trim();
                datalist.Add(port);
            }
            else //client
            {
                port = tb_0001_08_04.Text.Trim();
                ip = tb_0001_08_03.Text.Trim();
                datalist.Add(ip);
                datalist.Add(port);
            }
            if (!CheckeData(datalist)) return;
            var st = RFIDReader._ReaderConfig.SetReaderServerOrClient(ConnID, work, ip, port);
            ShowResult(st);
        }

        private void btn_0001_08_Get_Click(object sender, EventArgs e)
        {
            string searchResult = RFIDReader._ReaderConfig.GetReaderServerOrClient(ConnID);
            if (searchResult.Equals("Error"))
            {
                MessageBox.Show(searchResult);
                return;
            }
            tb_0001_08_01.Text = "";
            tb_0001_08_03.Text = "";
            tb_0001_08_04.Text = "";
            String[] varParam = searchResult.Split('|');
            if (varParam.Length >=2)
            {
                if (varParam[0].Equals("Server"))
                {
                    rb_0001_08_00.Checked = true;
                    rb_0001_08_02.Checked = false;
                    tb_0001_08_01.Text = varParam[1];
                }
                else
                {
                    rb_0001_08_00.Checked = false;
                    rb_0001_08_02.Checked = true;
                    tb_0001_08_03.Text = varParam[1];
                    tb_0001_08_04.Text = varParam[2];
                }
            }
        }

        private void btn_0001_13_Set_Click(object sender, EventArgs e)
        {
            if (CheckeData(new List<string>() { tb_0001_06_00.Text.Trim()}))
            {
                var st = RFIDReader._ReaderConfig.SetReaderMacParam(ConnID, tb_0001_06_00.Text.Trim());
                ShowResult(st);
            }
            
        }

        private void btn_0001_11_Get_Click(object sender, EventArgs e)
        {
            String rtStr = Program.PARAM_SET.GetReaderUTC(ConnID);
            var rs = RFIDReader._ReaderConfig.GetReaderUTC(ConnID);
            if (string.IsNullOrEmpty(rs))
            {
                return;
            }
             tb_0001_10_00.Text = rs;
        }

        private void btn_0001_10_Set_Click(object sender, EventArgs e)
        {
            string date = this.tb_0001_10_00.Text.Trim();
            if (!CheckeData(new List<string>(){date}))
            {
                return;
            }
            int rt = RFIDReader._ReaderConfig.SetReaderUTC(ConnID,date);
            ShowResult(rt);
        }
        private bool CheckeData(List<string> param) {
            foreach (var item in param)
            {
                if (string.IsNullOrEmpty(item))
                {
                    MessageBox.Show("Data is Null!");
                    return false;
                }
            }
            return true;
        }
        private void tb_0001_10_00_MouseClick(object sender, MouseEventArgs e)
        {// Right update time is current time
            if (e.Button == System.Windows.Forms.MouseButtons.Right) 
            {
                tb_0001_10_00.Text = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
            }
        }

        private void btn_0001_15_Set_Click(object sender, EventArgs e)
        {
            string param = tb_0001_15_00.Text.Trim();
            if (!CheckeData(new List<string>() { param })) return;
            var st = RFIDReader._ReaderConfig.SetReader485(ConnID, param);

            ShowResult(st);
        }

        private void btn_0001_16_Get_Click(object sender, EventArgs e)
        {
            tb_0001_15_00.Text = RFIDReader._ReaderConfig.GetReader485(ConnID);

        }

        private void btn_0001_17_Set_Click(object sender, EventArgs e)
        {
          
            var st = RFIDReader._ReaderConfig.SetBreakPointUpload(ConnID, cmb_0001_17_00.SelectedIndex == 1);
            ShowResult(st);
        }

        private void btn_0001_18_Get_Click(object sender, EventArgs e)
        {
            var st = RFIDReader._ReaderConfig.GetBreakPointUpload(ConnID);
            this.cmb_0001_17_00.SelectedIndex = st ? 1 : 0;
        }
        #endregion

        #region GPIO

        private void btn_0001_0C_Get_Click(object sender, EventArgs e)
        {
            String param = cb_0001_0B_00.SelectedIndex.ToString();
            String rtStr = Program.PARAM_SET.GetReaderGPIParam(ConnID, param);
            var st = RFIDReader._ReaderConfig.GetReaderGPIParam(ConnID, (eGPI)cb_0001_0B_00.SelectedIndex);
            Console.WriteLine(st);
            String[] arrParam = rtStr.Split('|');
            if (arrParam.Length == 4)
            {
                try
                {
                    cb_0001_0B_01.SelectedIndex = byte.Parse(arrParam[0]);
                    tb_0001_0B_02.Text = arrParam[1];
                    cb_0001_0B_03.SelectedIndex = byte.Parse(arrParam[2]);
                    tb_0001_0B_04.Text = arrParam[3];
                }
                catch { }
            }
            else if (arrParam.Length == 5)
            {
                
                try
                {
                    cb_0001_0B_01.SelectedIndex = byte.Parse(arrParam[0]);
                    tb_0001_0B_02.Text = arrParam[1];
                    cb_0001_0B_03.SelectedIndex = byte.Parse(arrParam[2]);
                    tb_0001_0B_04.Text = arrParam[3];
                    cb_0001_0B_05.SelectedIndex = byte.Parse(arrParam[4]);
                }
                catch { }
            }
            else
            {
                ShowMessage(rtStr);
            }
        }

        private void btn_0001_0B_Set_Click(object sender, EventArgs e)
        {
            if (cb_0001_0B_01.SelectedIndex > 0 && cb_0001_0B_01.SelectedIndex == cb_0001_0B_03.SelectedIndex)
            {
                MessageBox.Show(_RC.GetString("Config_RFID_0001_0B_Error"));
                return;
            }
            String strParam = "";
            try
            {
                strParam += cb_0001_0B_00.SelectedIndex + "|";
                strParam += cb_0001_0B_01.SelectedIndex + "|";
                strParam += tb_0001_0B_02.Text.Trim() + "|";
                strParam += cb_0001_0B_03.SelectedIndex + "|";
            }
            catch { }
            if (cb_0001_0B_03.SelectedIndex == 6)
            {
                strParam += "1," + tb_0001_0B_04.Text.Trim();
            }
            if (cb_0001_0B_03.SelectedIndex == 0)
            {
                strParam += "2," + cb_0001_0B_05.SelectedIndex;
            }
            strParam = strParam.TrimEnd('|');
            ShowMessage(Program.PARAM_SET.SetReaderGPIParam(ConnID, strParam));
        }

        private void cb_0001_0B_00_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_0001_0C_Get_Click(null, null);
        }

        private void cb_0001_0B_03_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0001_0B_03.SelectedIndex == 6)
            {
                tb_0001_0B_04.Visible = true;
                lb_DelayTime.Visible = true;
                lb_DelayTime_1.Visible = true;
            }
            else
            {
                tb_0001_0B_04.Visible = false;
                lb_DelayTime.Visible = false;
                lb_DelayTime_1.Visible = false;
            }
            if (cb_0001_0B_03.SelectedIndex == 0)
            {
                lb_upload.Visible = true;
                cb_0001_0B_05.Visible = true;
            }
            else
            {
                lb_upload.Visible = false;
                cb_0001_0B_05.Visible = false;
            }
        }

        private void btn_0001_09_Hight_Click(object sender, EventArgs e)
        {
            Dictionary<eGPO, eGPOState> param = new Dictionary<eGPO, eGPOState>();
            
            if (chk_GPO_1.Checked)
            {
                param.Add(eGPO._1, (eGPOState)cmb_GPO_1.SelectedIndex);
            }
            if (chk_GPO_2.Checked)
            {
                param.Add(eGPO._2, (eGPOState)cmb_GPO_2.SelectedIndex);
            }
            if (chk_GPO_3.Checked)
            {
                param.Add(eGPO._3, (eGPOState)cmb_GPO_3.SelectedIndex);
            }
            if (chk_GPO_4.Checked)
            {
                param.Add(eGPO._4, (eGPOState)cmb_GPO_4.SelectedIndex);
            }

            int st = RFIDReader._ReaderConfig.SetReaderGPOState(ConnID, param);
            ShowResult(st);
        }

        private void btn_0001_0A_GET_Click(object sender, EventArgs e)
        {
            string rtParam = RFIDReader._ReaderConfig.GetReaderGPIState(ConnID);
            String[] arrParam = rtParam.Split('&');
            foreach (String item in arrParam)
            {
                String[] arrItem = item.Split(',');
                if (arrItem.Length == 2)
                {
                    Control[] arrControl = gb_GPI.Controls.Find("btn_GPI_" + arrItem[0], true);
                    if (arrControl.Length > 0)
                    {
                        if (arrItem[1].Equals("Low"))
                        {
                            ((Button)arrControl[0]).BackColor = Color.DimGray;
                        }
                        else
                        {
                            ((Button)arrControl[0]).BackColor = Color.Red;
                        }
                    }
                }
            }
        }

        private void btn_0001_0E_Get_Click(object sender, EventArgs e)
        {
            var rtParam = RFIDReader._ReaderConfig.GetReaderWG(ConnID);
            String[] arrParam = rtParam.Split('|');
            if (arrParam.Length == 3)
            {
                try
                {
                    if (arrParam[0].Equals("Error") || arrParam[2].Equals("Error"))
                    {
                        MessageBox.Show(rtParam);
                        return;
                    }

                    cb_0001_0D_00.SelectedIndex = arrParam[0].Equals("Close") ? 0 : 1;
                    cb_0001_0D_01.SelectedIndex = arrParam[1].Equals("Wiegand26") ? 0 : arrParam[1].Equals("Wiegand34") ? 1 : 2;
                    cb_0001_0D_02.SelectedIndex = arrParam[2].Equals("end_of_the_EPC_data") ? 0 : 1;
                }
                catch { }
            }
        }

        private void btn_0001_0D_Get_Click(object sender, EventArgs e)
        {
            if (cb_0001_0D_00.SelectedIndex<0||cb_0001_0D_01.SelectedIndex<0||cb_0001_0D_02.SelectedIndex<0)
            {
                MessageBox.Show("Data is Null");
                return;
            }
            var st = RFIDReader._ReaderConfig.SetReaderWG(ConnID, (eWiegandSwitch)cb_0001_0D_00.SelectedIndex, (eWiegandFormat)cb_0001_0D_01.SelectedIndex, (eWiegandDetails)cb_0001_0D_02.SelectedIndex);
            ShowResult(st);
        }

        #endregion

        #region RFID settings

        private void btn_Set_0010_01_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ConnID))
            {
                ShowMessage(_RC.GetString("Main_Success"));
            }
        }

        private void btn_Get_0010_02_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ConnID))
            {
                ShowMessage(Program.RFID_OPTION.GetReaderPower(ConnID));
            }

        }

        private void btn_0010_01_Set_Click(object sender, EventArgs e)
        {
             Dictionary<int, int> sendParam  = new Dictionary<int, int>();
            try
            {
                if (chk_0010_01_01.Checked)
                {
                    sendParam.Add(1, cb_0010_01_01.SelectedIndex);
                }
                if (chk_0010_01_02.Checked)
                {
                    sendParam.Add(2, cb_0010_01_02.SelectedIndex);
                }
                if (chk_0010_01_03.Checked)
                {
                    sendParam.Add(3, cb_0010_01_03.SelectedIndex);
                }
                if (chk_0010_01_04.Checked)
                {
                    sendParam.Add(4, cb_0010_01_04.SelectedIndex);
                }
                if (chk_0010_01_05.Checked)
                {
                    sendParam.Add(5, cb_0010_01_05.SelectedIndex);
                }
                if (chk_0010_01_06.Checked)
                {
                    sendParam.Add(6, cb_0010_01_06.SelectedIndex);
                }
                if (chk_0010_01_07.Checked)
                {
                    sendParam.Add(7, cb_0010_01_07.SelectedIndex);
                }
                if (chk_0010_01_08.Checked)
                {
                    sendParam.Add(8, cb_0010_01_08.SelectedIndex);
                }
                if (chk_0010_01_09.Checked)
                {
                    sendParam.Add(9, cb_0010_01_09.SelectedIndex);
                }
                if (chk_0010_01_10.Checked)
                {
                    sendParam.Add(10, cb_0010_01_10.SelectedIndex);
                }
                if (chk_0010_01_11.Checked)
                {
                    sendParam.Add(11, cb_0010_01_11.SelectedIndex);
                }
                if (chk_0010_01_12.Checked)
                {
                    sendParam.Add(12, cb_0010_01_12.SelectedIndex);
                }
                if (chk_0010_01_13.Checked)
                {
                    sendParam.Add(13, cb_0010_01_13.SelectedIndex);
                }
                if (chk_0010_01_14.Checked)
                {
                    sendParam.Add(14, cb_0010_01_14.SelectedIndex);
                }
                if (chk_0010_01_15.Checked)
                {
                    sendParam.Add(15, cb_0010_01_15.SelectedIndex);
                }
                if (chk_0010_01_16.Checked)
                {
                    sendParam.Add(16, cb_0010_01_16.SelectedIndex);
                }
                if (chk_0010_01_17.Checked)
                {
                    sendParam.Add(17, cb_0010_01_17.SelectedIndex);
                }
                if (chk_0010_01_18.Checked)
                {
                    sendParam.Add(18, cb_0010_01_18.SelectedIndex);
                }
                if (chk_0010_01_19.Checked)
                {
                    sendParam.Add(19, cb_0010_01_19.SelectedIndex);
                }
                if (chk_0010_01_20.Checked)
                {
                    sendParam.Add(20, cb_0010_01_20.SelectedIndex);
                }
                if (chk_0010_01_21.Checked)
                {
                    sendParam.Add(21, cb_0010_01_21.SelectedIndex);
                }
                if (chk_0010_01_22.Checked)
                {
                    sendParam.Add(22, cb_0010_01_22.SelectedIndex);
                }
                if (chk_0010_01_23.Checked)
                {
                    sendParam.Add(23, cb_0010_01_23.SelectedIndex);
                }
                if (chk_0010_01_24.Checked)
                {
                    sendParam.Add(24, cb_0010_01_24.SelectedIndex);
                }
            }
            catch { }
            var rtStr = RFIDReader._RFIDConfig.SetANTPowerParam(ConnID, sendParam);
            if (rtStr==0)
            {
                MessageBox.Show("OK");
            }
            else
            {
                MessageBox.Show("Error:"+rtStr);
            }
        }

        private void btn_0010_02_Get_Click(object sender, EventArgs e)
        {
            Dictionary<Int32, Int32> antList = RFIDReader._RFIDConfig.GetANTPowerParam(ConnID);
            if (antList.Count == 0) return;
            try
            {
                foreach (int index in antList.Keys)
                {
                    switch (index)
                    {
                        case 1:
                            cb_0010_01_01.SelectedIndex = antList[index];
                            break;
                        case 2:
                            cb_0010_01_02.SelectedIndex = antList[index];
                            break;
                        case 3:
                            cb_0010_01_03.SelectedIndex = antList[index];
                            break;
                        case 4:
                            cb_0010_01_04.SelectedIndex = antList[index];
                            break;
                        case 5:
                            cb_0010_01_05.SelectedIndex = antList[index];
                            break;
                        case 6:
                            cb_0010_01_06.SelectedIndex = antList[index];
                            break;
                        case 7:
                            cb_0010_01_07.SelectedIndex = antList[index];
                            break;
                        case 8:
                            cb_0010_01_08.SelectedIndex = antList[index];
                            break;
                        case 9:
                            cb_0010_01_09.SelectedIndex = antList[index];
                            break;
                        case 10:
                            cb_0010_01_10.SelectedIndex = antList[index];
                            break;
                        case 11:
                            cb_0010_01_11.SelectedIndex = antList[index];
                            break;
                        case 12:
                            cb_0010_01_12.SelectedIndex = antList[index];
                            break;
                        case 13:
                            cb_0010_01_13.SelectedIndex = antList[index];
                            break;
                        case 14:
                            cb_0010_01_14.SelectedIndex = antList[index];
                            break;
                        case 15:
                            cb_0010_01_15.SelectedIndex = antList[index];
                            break;
                        case 16:
                            cb_0010_01_16.SelectedIndex = antList[index];
                            break;
                        case 17:
                            cb_0010_01_17.SelectedIndex = antList[index];
                            break;
                        case 18:
                            cb_0010_01_18.SelectedIndex = antList[index];
                            break;
                        case 19:
                            cb_0010_01_19.SelectedIndex = antList[index];
                            break;
                        case 20:
                            cb_0010_01_20.SelectedIndex = antList[index];
                            break;
                        case 21:
                            cb_0010_01_21.SelectedIndex = antList[index];
                            break;
                        case 22:
                            cb_0010_01_22.SelectedIndex = antList[index];
                            break;
                        case 23:
                            cb_0010_01_23.SelectedIndex = antList[index];
                            break;
                        case 24:
                            cb_0010_01_24.SelectedIndex = antList[index];
                            break;
                    }

                }
            }
            catch { }
        }

        private void btn_0010_05_Set_Click(object sender, EventArgs e)
        {
            String sendParam = "";
            if (cb_0010_05_00.SelectedIndex == 1)
            {
                sendParam += cb_0010_05_00.SelectedIndex;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (CCBoxItem item in cb_0010_05_01.CheckedItems)
                {
                    sb.Append(item.Value).Append(",");
                }
                sendParam += cb_0010_05_00.SelectedIndex + "|" + sb.ToString().TrimEnd(',');
            }
            String rtStr = SetReaderWorkFrequency(ConnID, sendParam);
            ShowMessage(rtStr);
        }

        public String SetReaderWorkFrequency(string connID, string param)
        {
            string rt = "-2|Timeout";
            try
            {
                RFIDReaderAPI.Protocol.BaseFrame bf = new RFIDReaderAPI.Protocol.Frame_0010_05(param);
                RFIDReader.DIC_CONNECT[connID].SendSingleFrame(bf);
                RFIDReaderAPI.Protocol.BaseFrame rtFrame = RFIDReader.DIC_CONNECT[connID].WaitResponse<RFIDReaderAPI.Protocol.Frame_0010_05>();
                if (rtFrame != null) rt = rtFrame.GetReturnData();
            }
            catch (Exception)
            {
                rt = "-2|Timeout";
            }
            return rt;
        }

        private void btn_0010_06_Get_Click(object sender, EventArgs e)
        {
            string rtStr = RFIDReader._RFIDConfig.GetReaderWorkFrequency(ConnID);
            String[] varStr = rtStr.Split('|');
            if (varStr.Length > 0)
            {

                try
                {
                    InitComboBox();
                    if (varStr[0].Equals("Auto"))
                    {
                        cb_0010_05_00.SelectedIndex = 1;
                    }
                    else
                    {
                        cb_0010_05_00.SelectedIndex = 0;
                    }
                    
                    foreach (String item in varStr[2].Split(','))
                    {
                        for (int i = 0; i < cb_0010_05_01.Items.Count; i++)
                        {
                            var str = cb_0010_05_01.Items[i].ToString();
                            if (str.Contains(item))
                            {
                                int lastindex = str.LastIndexOf(":");
                                string tempStr = str.Substring(lastindex+1, str.Length - lastindex-1);
                                cb_0010_05_01.SetItemChecked(Convert.ToInt32(tempStr), true);
                            }
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message );
                }
                      
            }
            else
            {
                ShowMessage(rtStr);
            }
        }

        private void cb_0010_05_00_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_0010_05_00.SelectedIndex == 1)
            {
                cb_0010_05_01.Enabled = false;
            }
            else
            {
                cb_0010_05_01.Enabled = true;
            }
        }
		

		
        private void btn_0010_07_Set_Click(object sender, EventArgs e)
        {
            Int32 iParam = 0;
            string param = string.Empty;
            foreach (var item in gb_ANTEnable.Controls)
            {
                if (item is QQCheckBox)
                {
                    QQCheckBox cb = (QQCheckBox)item;
                    if (cb.Checked)
                    {
                        iParam += Int32.Parse(cb.Tag.ToString());
                    }
                }
            }
            param = iParam.ToString();

            //add antenna param
            string addAntParam = GetAddAntParam();
            if (!addAntParam.Equals(""))
            {
                param += "|25," + addAntParam;
            }

            String rtStr = Program.RFID_OPTION.SetReaderANT(ConnID, param);
            // String rtStr = Program.RFID_OPTION.SetReaderANT(ConnID, "3|1,2-7-13&2,1-2-4-8&");
            ShowMessage(rtStr);
        }

        /// <summary>
        /// get added antenna param
        /// </summary>
        /// <returns></returns>
        public String GetAddAntParam()
        {
            int addAntNum = 0;
            if (ANT9.Checked)
            {
                addAntNum += Int32.Parse(ANT9.Tag.ToString());
            }
            if (ANT10.Checked)
            {
                addAntNum += Int32.Parse(ANT10.Tag.ToString());
            }
            if (ANT11.Checked)
            {
                addAntNum += Int32.Parse(ANT11.Tag.ToString());
            }
            if (ANT12.Checked)
            {
                addAntNum += Int32.Parse(ANT12.Tag.ToString());
            }
            if (ANT13.Checked)
            {
                addAntNum += Int32.Parse(ANT13.Tag.ToString());
            }
            if (ANT14.Checked)
            {
                addAntNum += Int32.Parse(ANT14.Tag.ToString());
            }
            if (ANT15.Checked)
            {
                addAntNum += Int32.Parse(ANT15.Tag.ToString());
            }
            if (ANT16.Checked)
            {
                addAntNum += Int32.Parse(ANT16.Tag.ToString());
            }
            if (ANT17.Checked)
            {
                addAntNum += Int32.Parse(ANT17.Tag.ToString());
            }
            if (ANT18.Checked)
            {
                addAntNum += Int32.Parse(ANT18.Tag.ToString());
            }
            if (ANT19.Checked)
            {
                addAntNum += Int32.Parse(ANT19.Tag.ToString());
            }
            if (ANT20.Checked)
            {
                addAntNum += Int32.Parse(ANT20.Tag.ToString());
            }
            if (ANT21.Checked)
            {
                addAntNum += Int32.Parse(ANT21.Tag.ToString());
            }
            if (ANT22.Checked)
            {
                addAntNum += Int32.Parse(ANT22.Tag.ToString());
            }
            if (ANT23.Checked)
            {
                addAntNum += Int32.Parse(ANT23.Tag.ToString());
            }
            if (ANT24.Checked)
            {
                addAntNum += Int32.Parse(ANT24.Tag.ToString());
            }

            if (addAntNum != 0)
            {
                return Convert.ToString(addAntNum, 16).PadLeft(4, '0');
            }
            else
            {
                return "";
            }

        }

        private void btn_0010_08_Get_Click(object sender, EventArgs e)
        {
            String rtStr = Program.RFID_OPTION.GetReaderANT(ConnID);
            byte index = 255;
            String[] arr_Param = rtStr.Split('|');
			
            if (byte.TryParse(arr_Param[0], out index))
            {
                foreach (var item in gb_ANTEnable.Controls)
                {
                    if (item is QQCheckBox)
                    {
                        QQCheckBox cb = (QQCheckBox)item;
                        if ((index & byte.Parse(cb.Tag.ToString())) == byte.Parse(cb.Tag.ToString()))
                        {
                            cb.Checked = true;
                        }
                        else
                        {
                            cb.Checked = false;
                        }
                    }
                }
            }
            else
            {
                ShowMessage(rtStr);
            }
            if (arr_Param.Length > 1)
            {
                string[] str = arr_Param[1].Split('&');
                foreach (string item in str)
                {
                    string[] aaa = item.Split(',');
                    if (aaa[0].Equals("25"))
                    {
                        int addAntNum = Convert.ToInt32(aaa[1], 16);
                        if (addAntNum >= Convert.ToInt32(ANT24.Tag.ToString()))
                        {
                            ANT24.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT24.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT23.Tag.ToString()))
                        {
                            ANT23.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT23.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT22.Tag.ToString()))
                        {
                            ANT22.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT22.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT21.Tag.ToString()))
                        {
                            ANT21.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT21.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT20.Tag.ToString()))
                        {
                            ANT20.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT20.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT19.Tag.ToString()))
                        {
                            ANT19.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT19.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT18.Tag.ToString()))
                        {
                            ANT18.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT18.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT17.Tag.ToString()))
                        {
                            ANT17.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT17.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT16.Tag.ToString()))
                        {
                            ANT16.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT16.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT15.Tag.ToString()))
                        {
                            ANT15.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT15.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT14.Tag.ToString()))
                        {
                            ANT14.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT14.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT13.Tag.ToString()))
                        {
                            ANT13.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT13.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT12.Tag.ToString()))
                        {
                            ANT12.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT12.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT11.Tag.ToString()))
                        {
                            ANT11.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT11.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT10.Tag.ToString()))
                        {
                            ANT10.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT10.Tag.ToString());
                        }
                        if (addAntNum >= Convert.ToInt32(ANT9.Tag.ToString()))
                        {
                            ANT9.Checked = true;
                            addAntNum -= Convert.ToInt32(ANT9.Tag.ToString());
                        }
                    }
                }
            }
            

        }

        private void btn_0010_0B_Set_Click(object sender, EventArgs e)
        {
            
            int st = RFIDReader._RFIDConfig.SetEPCBaseBandParam(ConnID, cb_0010_0B_00.SelectedIndex == 6 ? 255 : cb_0010_0B_00.SelectedIndex, cb_0010_0B_01.SelectedIndex, cb_0010_0B_02.SelectedIndex, cb_0010_0B_03.SelectedIndex);
            ShowResult(st);
        }

        private static void ShowResult(int st)
        {
            if (st == 0)
            {
                MessageBox.Show("OK");
            }
            else
            {
                MessageBox.Show("Error");
            }
        }

        private void btn_0010_0C_Get_Click(object sender, EventArgs e)
        {
            string rsStr = RFIDReader._RFIDConfig.GetEPCBaseBandParam(ConnID);
            string[] varStr = rsStr.Split('|');
            if (varStr.Length == 4)
            {
                try
                {
                    if (varStr[0].Equals("255"))
                    {
                        cb_0010_0B_00.SelectedIndex = 6;
                    }
                    else
                    {
                        cb_0010_0B_00.SelectedIndex = Byte.Parse(varStr[0]);
                    }
                    cb_0010_0B_01.SelectedIndex = Byte.Parse(varStr[1]);
                    cb_0010_0B_02.SelectedIndex = Byte.Parse(varStr[2]);
                    cb_0010_0B_03.SelectedIndex = Byte.Parse(varStr[3]);
                }
                catch { }
            }
            else
            {
                ShowMessage(rsStr);
            }

        }

        private void btn_0010_09_Set_Click(object sender, EventArgs e)
        {
            string txt01 = tb_0010_09_01.Text.Trim();
            string txt00 = tb_0010_09_00.Text.Trim();
            if (string.IsNullOrEmpty(txt01) || string.IsNullOrEmpty(txt00))
            {
                MessageBox.Show("Data is Null!");
            }
            int st = RFIDReader._RFIDConfig.SetTagUpdateParam(ConnID, Convert.ToInt32(txt00), Convert.ToInt32(txt01));
            ShowResult(st);
        }

        private void btn_0010_0A_Get_Click(object sender, EventArgs e)
        {
            string rtStr = RFIDReader._RFIDConfig.GetTagUpdateParam(ConnID);
            String[] varStr = rtStr.Split('|');
            if (varStr.Length == 2)
            {
                tb_0010_09_00.Text = varStr[0];
                tb_0010_09_01.Text = varStr[1];
            }
        }

        private void btn_0010_0D_Set_Click(object sender, EventArgs e)
        {
           
            if (string.IsNullOrEmpty(tb_0010_0D_01.Text.Trim()))
            {
                MessageBox.Show("Time is Null!");
                return;
            }
            var rt = RFIDReader._RFIDConfig.SetReaderAutoSleepParam(ConnID, cb_0010_0D_00.SelectedIndex == 1, tb_0010_0D_01.Text.Trim());
            ShowResult(rt);
        }

        private void btn_0010_0E_Get_Click(object sender, EventArgs e)
        {
            string rt = RFIDReader._RFIDConfig.GetReaderAutoSleepParam(ConnID);
            String[] varStr = rt.Split('|');
            if (varStr.Length == 2)
            {
                if (varStr[0].Equals("Close")) cb_0010_0D_00.SelectedIndex = 0;
                else cb_0010_0D_00.SelectedIndex = 1;
                tb_0010_0D_01.Text = varStr[1];
            }
            else
            {
                if (varStr[0].Equals("Close")) cb_0010_0D_00.SelectedIndex = 0;
            }
        }
        // Antenna VSWR check
        private void btn_0101_05_Click(object sender, EventArgs e)
        {

            if (btn_0101_00_Click())
            {
                eAntennaNo antno = RFIDReaderAPI.RFIDReader._Tag6C.GeteAntennaNo(minAnt + "");
                string rt = RFIDReaderAPI.RFIDReader._RFIDConfig.GetAntennaStandingWaveRatio(ConnID, (int)antno);

                string[] array_rt = rt.Split('|');
                if (array_rt.Length >= 2)
                {
                    int v = Convert.ToInt32(array_rt[0]) - Convert.ToInt32(array_rt[1]);
                    if (v < 5)
                    {
                        ShowMessage(_RC.GetString("RFID_Option_UnconnectedInfinity"));
                    }
                    else
                    {
                        var vswr = 1 + (2.0 / Math.Pow(10, v / 46.0));
                        ShowMessage(_RC.GetString("RFID_Option_Messge1") + rt + "\n" +_RC.GetString("RFID_Option_Messge2") + Math.Round(vswr, 3));
                    }
                }
            }
            else
            {
                MessageBox.Show(_RC.GetString("RFID_Option_Messge3"));
            }



        }
        #endregion

        #region Reset

        private void btn_RestoreFactory_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == ShowQuestion(_RC.GetString("Main_Show_Question_Restore")))
            {
                ShowResult(RFIDReader._ReaderConfig.SetReaderRestoreFactory(ConnID));

               
            }
        }

        #endregion

        private void btn_Set_0010_03_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ConnID))
            {
                string param = cb_0010_03.SelectedIndex.ToString();
                eRF_Range rf = (eRF_Range)cb_0010_03.SelectedIndex;
                int rt = RFIDReader._RFIDConfig.SetReaderRF(ConnID, rf);
                if (rt == 0)
                {
                    btn_0010_06_Get_Click(null, null);
                    MessageBox.Show("OK");
                }
                else
                {
                    MessageBox.Show(rt+"");
                }
                rfIndex = cb_0010_03.SelectedIndex;
				InitComboBox();
            }
        }

        private void btn_0010_04_Get_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ConnID))
            {
                eRF_Range rt = RFIDReader._RFIDConfig.GetReaderRF(ConnID);
                this.cb_0010_03.SelectedIndex = (int)rt;
                rfIndex = (int)rt;
                
            }
        }



        #region Parameter initialization

        private void InitRFIDControl()
        {
            foreach (var item in gb_SetANTPower.Controls)
            {
                CheckBox cb = item as CheckBox;
                if (cb != null)
                {
                    int index = Int32.Parse(cb.Name.Substring(cb.Name.Length - 2));
                    if (index > antCount)
                    {
                        cb.Enabled = false;
                    }
                }
            }
            foreach (var item in gb_ANTEnable.Controls)
            {
                CheckBox cb = item as CheckBox;
                if (cb != null)
                {
                    int index = Int32.Parse(cb.Name.Substring(cb.Name.Length - 2));
                    if (index > antCount)
                    {
                        cb.Enabled = false;
                    }
                }
            }
            #region Add antenna enable
            if (antCount >= Int32.Parse(ANT9.Name.ToString().Substring(3)))
            {
                ANT9.Enabled = true;
            }
            else
            {
                ANT9.Checked = false;
                ANT9.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT10.Name.ToString().Substring(3)))
            {
                ANT10.Enabled = true;
            }
            else
            {
                ANT10.Checked = false;
                ANT10.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT11.Name.ToString().Substring(3)))
            {
                ANT11.Enabled = true;
            }
            else
            {
                ANT11.Checked = false;
                ANT11.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT12.Name.ToString().Substring(3)))
            {
                ANT12.Enabled = true;
            }
            else
            {
                ANT12.Checked = false;
                ANT12.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT13.Name.ToString().Substring(3)))
            {
                ANT13.Enabled = true;
            }
            else
            {
                ANT13.Checked = false;
                ANT13.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT14.Name.ToString().Substring(3)))
            {
                ANT14.Enabled = true;
            }
            else
            {
                ANT14.Checked = false;
                ANT14.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT15.Name.ToString().Substring(3)))
            {
                ANT15.Enabled = true;
            }
            else
            {
                ANT15.Checked = false;
                ANT15.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT16.Name.ToString().Substring(3)))
            {
                ANT16.Enabled = true;
            }
            else
            {
                ANT16.Checked = false;
                ANT16.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT17.Name.ToString().Substring(3)))
            {
                ANT17.Enabled = true;
            }
            else
            {
                ANT17.Checked = false;
                ANT17.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT18.Name.ToString().Substring(3)))
            {
                ANT18.Enabled = true;
            }
            else
            {
                ANT18.Checked = false;
                ANT18.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT19.Name.ToString().Substring(3)))
            {
                ANT19.Enabled = true;
            }
            else
            {
                ANT19.Checked = false;
                ANT19.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT20.Name.ToString().Substring(3)))
            {
                ANT20.Enabled = true;
            }
            else
            {
                ANT20.Checked = false;
                ANT20.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT21.Name.ToString().Substring(3)))
            {
                ANT21.Enabled = true;
            }
            else
            {
                ANT21.Checked = false;
                ANT21.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT22.Name.ToString().Substring(3)))
            {
                ANT22.Enabled = true;
            }
            else
            {
                ANT22.Checked = false;
                ANT22.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT23.Name.ToString().Substring(3)))
            {
                ANT23.Enabled = true;
            }
            else
            {
                ANT23.Checked = false;
                ANT23.Enabled = false;
            }
            if (antCount >= Int32.Parse(ANT24.Name.ToString().Substring(3)))
            {
                ANT24.Enabled = true;
            }
            else
            {
                ANT24.Checked = false;
                ANT24.Enabled = false;
            }
            #endregion
			this.cb_ANTSingle.Items.Clear();
			for (int i = 0; i < antCount; i++)
			{
				cb_ANTSingle.Items.Add(i+1);
			}
			cb_ANTSingle.SelectedIndex = 0;
            btn_0010_02_Get_Click(null, null);
            btn_0010_04_Get_Click(null, null);
            btn_0010_0C_Get_Click(null, null);
            btn_0010_06_Get_Click(null, null);
            btn_0010_08_Get_Click(null, null);
            btn_0010_0A_Get_Click(null, null);
            btn_0010_0E_Get_Click(null, null);
        }

        private void InitReaderSetControl()
        {
            btn_0001_02_Get_Click(null, null);
            btn_0001_04_Get_Click(null, null);
            btn_0001_06_Get_Click(null, null);
            btn_0001_08_Get_Click(null, null);
            btn_0001_11_Get_Click(null, null);
            btn_0001_16_Get_Click(null, null);
            btn_0001_18_Get_Click(null, null);
        }


        #endregion

        //When the tab is switched, initialize the data in the query
        private void tc_Main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tc_Main.SelectedIndex == 0)
            {
                InitReaderProerty();
                InitRFIDControl();
            }
            else if (tc_Main.SelectedIndex == 1)
            {
                InitReaderProerty();
                InitReaderSetControl();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            try
            {
                var rtStr = RFIDReader._ReaderConfig.GetReaderSelfCheck(ConnID);
                String[] ipParam = rtStr.Split('|');
                if (ipParam.Length == 2)
                {
                    try
                    {
                        cmbSelfCheck.SelectedIndex = 1;

                        tbxSelfCheckIP.Text = ipParam[1];
                    }
                    catch { }
                }
                else if (ipParam.Length == 1)
                {
                    try
                    {
                        cmbSelfCheck.SelectedIndex = 0;
                    }
                    catch { }
                }
                else
                {
                    ShowMessage(rtStr);
                }
            }
            catch
            { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var st = RFIDReader._ReaderConfig.SetReaderSelfCheck(ConnID, cmbSelfCheck.SelectedIndex == 1, tbxSelfCheckIP.Text.Trim());
            ShowResult(st);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (cb_0010_07_01.Enabled) cb_0010_07_01.Checked = true;
            if (cb_0010_07_02.Enabled) cb_0010_07_02.Checked = true;
            if (cb_0010_07_03.Enabled) cb_0010_07_03.Checked = true;
            if (cb_0010_07_04.Enabled) cb_0010_07_04.Checked = true;
            if (cb_0010_07_05.Enabled) cb_0010_07_05.Checked = true;
            if (cb_0010_07_06.Enabled) cb_0010_07_06.Checked = true;
            if (cb_0010_07_07.Enabled) cb_0010_07_07.Checked = true;
            if (cb_0010_07_08.Enabled) cb_0010_07_08.Checked = true;
            if (ANT9.Enabled) ANT9.Checked = true;
            if (ANT10.Enabled) ANT10.Checked = true;
            if (ANT11.Enabled) ANT11.Checked = true;
            if (ANT12.Enabled) ANT12.Checked = true;
            if (ANT13.Enabled) ANT13.Checked = true;
            if (ANT14.Enabled) ANT14.Checked = true;
            if (ANT15.Enabled) ANT15.Checked = true;
            if (ANT16.Enabled) ANT16.Checked = true;
            if (ANT17.Enabled) ANT17.Checked = true;
            if (ANT18.Enabled) ANT18.Checked = true;
            if (ANT19.Enabled) ANT19.Checked = true;
            if (ANT20.Enabled) ANT20.Checked = true;
            if (ANT21.Enabled) ANT21.Checked = true;
            if (ANT22.Enabled) ANT22.Checked = true;
            if (ANT23.Enabled) ANT23.Checked = true;
            if (ANT24.Enabled) ANT24.Checked = true;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (cb_0010_07_01.Enabled) cb_0010_07_01.Checked = false;
            if (cb_0010_07_02.Enabled) cb_0010_07_02.Checked = false;
            if (cb_0010_07_03.Enabled) cb_0010_07_03.Checked = false;
            if (cb_0010_07_04.Enabled) cb_0010_07_04.Checked = false;
            if (cb_0010_07_05.Enabled) cb_0010_07_05.Checked = false;
            if (cb_0010_07_06.Enabled) cb_0010_07_06.Checked = false;
            if (cb_0010_07_07.Enabled) cb_0010_07_07.Checked = false;
            if (cb_0010_07_08.Enabled) cb_0010_07_08.Checked = false;
            if (ANT9.Enabled) ANT9.Checked = false;
            if (ANT10.Enabled) ANT10.Checked = false;
            if (ANT11.Enabled) ANT11.Checked = false;
            if (ANT12.Enabled) ANT12.Checked = false;
            if (ANT13.Enabled) ANT13.Checked = false;
            if (ANT14.Enabled) ANT14.Checked = false;
            if (ANT15.Enabled) ANT15.Checked = false;
            if (ANT16.Enabled) ANT16.Checked = false;
            if (ANT17.Enabled) ANT17.Checked = false;
            if (ANT18.Enabled) ANT18.Checked = false;
            if (ANT19.Enabled) ANT19.Checked = false;
            if (ANT20.Enabled) ANT20.Checked = false;
            if (ANT21.Enabled) ANT21.Checked = false;
            if (ANT22.Enabled) ANT22.Checked = false;
            if (ANT23.Enabled) ANT23.Checked = false;
            if (ANT24.Enabled) ANT24.Checked = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            foreach (var item in gb_SetANTPower.Controls)
            {
                CheckBox cb = item as CheckBox;
                if (cb != null)
                {
                    if (cb.Enabled)
                    {
                        cb.Checked = true;
                    }
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            foreach (var item in gb_SetANTPower.Controls)
            {
                CheckBox cb = item as CheckBox;
                if (cb != null)
                {
                    if (cb.Enabled)
                    {
                        cb.Checked = false;
                    }
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            foreach (var item in gb_SetANTPower.Controls)
            {
                ComboBox cb = item as ComboBox;
                if (cb != null)
                {
                    if (cb.Enabled)
                    {
                        cb.SelectedIndex = 33;
                    }
                }
            }
        }

		private void btn_0101_00_Click(object sender, EventArgs e)
		{
			string addAntParam = "";
			
			string param = "";

			minAnt = cb_ANTSingle.SelectedIndex + 1;

			if (cb_0010_05_01.CheckedItems.Count>0)
			{
				var item = cb_0010_05_01.CheckedItems[0];
				var index = cb_FrequencySingle.SelectedIndex;
				if (minAnt > 8)
				{
					addAntParam = "|1," + Convert.ToString((int)RFIDReaderAPI.RFIDReader._Tag6C.GeteAntennaNo(minAnt+""), 16).PadLeft(4, '0');
					param = "0|" + index + addAntParam;
				}
				else
				{
					int firstindex =  cb_0010_05_01.SelectedIndex;
					param = (int)RFIDReaderAPI.RFIDReader._Tag6C.GeteAntennaNo(minAnt + "") + "|" + index;
				}
			}
			else
			{
				MessageBox.Show("Please select a frequency");
				return;
			}
			
			if (!String.IsNullOrEmpty(ConnID))
			{
				ShowMessage(Program.TEST_OPTION.SendCarrier(ConnID, param));
			}
		}
		private bool btn_0101_00_Click()
		{
			//CheckedAnt();
			string addAntParam = "";

			string param = "";

			minAnt = cb_ANTSingle.SelectedIndex + 1;

			
			//var item = cb_0010_05_01.CheckedItems[0];
			var index = cb_FrequencySingle.SelectedIndex;
			if (minAnt > 8)
			{
				addAntParam = "|1," + Convert.ToString((int)RFIDReaderAPI.RFIDReader._Tag6C.GeteAntennaNo(minAnt + "")/256, 16).PadLeft(4, '0');
				param = "0|" + index + addAntParam;
			}
			else
			{
				int firstindex = cb_0010_05_01.SelectedIndex;
				param = (int)RFIDReaderAPI.RFIDReader._Tag6C.GeteAntennaNo(minAnt + "") + "|" + index;
			}
			

			if (!String.IsNullOrEmpty(ConnID))
			{
                string rt = Program.TEST_OPTION.SendCarrier(ConnID, param);
				if (!rt.StartsWith("0"))
				{
					return false;
				}
			}
			return true;
		}
		
    }

    public class CCBoxItem
    {
        private int val;
        public int Value
        {
            get { return val; }
            set { val = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public CCBoxItem()
        {
        }

        public CCBoxItem(string name, int val)
        {
            this.name = name;
            this.val = val;
        }

        public override string ToString()
        {
            return string.Format("name: '{0}', value: {1}", name, val);
        }
    }
}
