                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RFIDReaderAPI.Interface;
using RFIDReaderAPI;
using System.Threading;
using RFIDReaderAPI.Models;
using ALMACENRFID.MyFormTemplet;
using System.Runtime.InteropServices;
using System.Reflection;
using ALMACENRFID.Helper;
using ALMACENRFID.Entity;
using ALMACENRFID;

namespace ALMACENRFID
{
    public delegate void WriteDebug(string msg);                                              // Output debugging information
    public delegate void AddTag(Tag_Model tag_6C, DataGridViewRow dgvr, Boolean isNew);       // Update label information invoke
    public delegate void FlushState();                                                        // Update real-time status invoke
    public delegate void UpdateGPI(RoundButton rb, Color cl);
    public delegate void TCPPortConn(String connID);                                          // Callback when the reader actively connects to the server
    public partial class SingleMainForm : _360Form, IAsynchronousMessage
    {
        #region property
        private eAntennaNo antNo;
        private eReadType readType;
        public static Boolean IsEnglish = false;
        public Boolean IsMultiConnect = false;         // Whether it is a connection of multiple readers
        private const int MAX_DEBUG_MESSAGE = 1000;    // When debugging messages are greater than 1000, it will be cleared automatically
        private int nowDebugMessageCount = 0;
        private bool IsFlush = false;                  // Whether to continue to cyclically refresh the state (end the refresh thread)
        private bool IsStartRead = false;              // Is the label being read
        private bool IsWriteDebug = false;             // Whether to enable DEBUG mode
        private bool IsShowTag = true;                 // Whether to display label data in DataGridView
        public String readVarParam_6C = "";            // Optional parameters when reading the label, which can be saved and read by the configuration file
        public String readVarParam_6B = "";
        public String readVarParam_GB = "";
        public string ConnID = "";                      // ID of the current connection
        Dictionary<String, DataGridViewRow> dic_Rows = new Dictionary<string, DataGridViewRow>(); // Display label data in DataGridView
        DateTime TJ_Run_Start = DateTime.Now;           // Start reading label time, used to count label reading speed
        long TJ_LastTagcount = 0;                       // When reading tags continuously, the total number of tags read in the last second is used to count the real-time rate of tag reading
        System.Windows.Forms.Timer[] TJ_GPI_Timers = new System.Windows.Forms.Timer[4];// Timer object to control GPI lights
        List<RoundButton> TJ_List_GPI_Button = new List<RoundButton>();               // GPI light button collection
        public String ReadGPOParam = "";                    // Read tag control GPO
        public String ReadGPOParam_Stop = "";
        public object ReadGPOObject = new object();         // Control the lock object when reading tags

        MySingleForm.SearchDevice sDevice = null;            // Search device window object

        public Dictionary<string, string> dic_UsbDevicePath_Name = new Dictionary<string, string>(); //usb device path and device name dictionary
        public ReadParam readP = new ReadParam();
        [DllImport("kernel32.dll", EntryPoint = "Beep")]
        //The first parameter refers to the frequency, the larger the higher, the second parameter refers to the length of the ring
        public static extern int Beep(
        int dwFreq,
        int dwDuration
        );
        #endregion
       

        #region Reader capability

        private int minDB = 0;                                             // Minimum power
        private int maxDB = 36;                                            // Maximum power
        private int antCount = 2;                                          // Number of antennas
        private List<Int32> bandList = new List<Int32>();                  // Frequency band list
        private List<Int32> RFIDProtocolList = new List<Int32>();          // RFID protocol list

        #endregion

        #region Constructor
        public SingleMainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        } 
        #endregion

        #region Form event

        private void SingleMainForm_Shown(object sender, EventArgs e)
        {
            tsmi_SerialConn_Click(null, null);
        }

        private void SingleMainForm_Load(object sender, EventArgs e)
        {
            Rectangle clientRectangle = ClientRectangle;
            Point clientPoint = PointToScreen(new Point(0, 0));
            clientRectangle.Offset(clientPoint.X - Left, clientPoint.Y - Top - 5);
            Region = new Region(clientRectangle);
            this.Height = 650;
            this.Width = 1024;
            ReadConfig();
            String verStr = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			verStr = verStr.Substring(0, verStr.LastIndexOf('.'));
			verStr = verStr.Substring(0, verStr.LastIndexOf('.'));
            this.Text = _RC.GetString("Main_Title") + " " + verStr;
            CheckEnableButton();
            InitGPI();
            
        }
        // Form close event
        private void SingleMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.RFID_OPTION.StopReader(ConnID);
        }
        #endregion

        #region Initialization process
        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            InitReaderProerty();

            CheckEnableButton();

            #region Antenna capacity

            foreach (var item in gb_ReadControl.Controls)
            {
                CheckBox cb = item as CheckBox;
                if (cb != null)
                {
                    cb.BackColor = Color.Transparent;
                    string AntNum = cb.Name.ToString().Substring(3);
                    int index = Int32.Parse(AntNum);
                    if (index > antCount)
                    {
                        cb.Enabled = false;
                        cb.Checked = false;
                    }
                    else
                    {
                        cb.Enabled = true;
                    }
                }
            }
            #endregion

         
            dgv_Tags.AutoGenerateColumns = false;
            Type type = dgv_Tags.GetType();
            System.Reflection.PropertyInfo pi = type.GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dgv_Tags, true, null);

        }
        
        /// <summary>
        /// Get reader properties
        /// </summary>
        private void InitReaderProerty()
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
        /// <summary>
        /// Bind the data source to the table
        /// </summary>
        private void InitDataGridView()
        {
            if (dgv_Tags.InvokeRequired)
            {
                dgv_Tags.BeginInvoke(new MethodInvoker(InitDataGridView), null);
                return;
            }
            try
            {
                Int32 scrollHeight = dgv_Tags.FirstDisplayedScrollingRowIndex;
                BindingSource bs = new BindingSource();
                bs.DataSource = RFIDReaderAPI.RFIDReader.DIC_CONNECT[ConnID].dic_NowTags.Values;
                bs.SuspendBinding();
                dgv_Tags.DataSource = bs;
                dgv_Tags.FirstDisplayedScrollingRowIndex = scrollHeight;
            }
            catch { }
        }

        #endregion

        #region  DataGridView operation methods and events

        //  Add a single label
        public void AddSingleTag(Tag_Model tag_6C, DataGridViewRow dgvr, Boolean isNew)
        {
            if (this.dgv_Tags.InvokeRequired)
            {
                this.dgv_Tags.BeginInvoke(new AddTag(AddSingleTag), tag_6C, dgvr, isNew);
                return;
            }
            try
            {
                if (!isNew)
                {
                    Int64 newStr = (Int64)dgvr.Cells["clm_TotalCount"].Value + 1;
                    dgvr.Cells["clm_TotalCount"].Value = newStr;
                    if (tag_6C.ANT_NUM <= 24)
                    {
                        dgvr.Cells["clm_ANT" + tag_6C.ANT_NUM].Value = (Int64)dgvr.Cells["clm_ANT" + tag_6C.ANT_NUM].Value + 1;
                    }
                    dgvr.Cells["clm_UserData"].Value = tag_6C.UserData;
                    dgvr.Cells["clm_ReserveData"].Value = tag_6C.TagetData;
                    dgvr.Cells["clm_RSSI"].Value = tag_6C.RSSI;
                    dgvr.Cells["clm_ReadTime"].Value = tag_6C.ReadTime;
                    dgvr.Cells["clm_G2V2Data"].Value = tag_6C.G2V2Data;
                    dgvr.Cells["clm_G2V2Challenge"].Value = tag_6C.G2V2Challenge;
					dgvr.Cells["clm_EpcData"].Value = tag_6C.EpcData;

					dgvr.Cells["clm_EMTemp"].Value = tag_6C.EM_Temperature;
					dgvr.Cells["clm_RFTemp"].Value = tag_6C.RFMicron_Temperature;
                }
                else
                {
                    dgv_Tags.Rows.Add(dgvr);
                }
                this.led_Tag_ReadCount.Text = RFIDReader.DIC_CONNECT[ConnID].ProcessCount.ToString();
            }
            catch { }
        }

        // Cell click event
        private void dgv_Tags_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                    {
                        dgv_Tags.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
                    }
                    else if (e.RowIndex >= 0 && e.ColumnIndex < 0)
                    {
                        dgv_Tags.Rows[e.RowIndex].Selected = true;
                        dgv_Tags.CurrentCell = dgv_Tags.Rows[e.RowIndex].Cells[0];
                    }
                }
                catch { }
            }
        }
        // Cell click event
        private void dgv_Tags_MouseDown(object sender, MouseEventArgs e)
        {
            dgv_Tags.CurrentCell = null;
        }
        //Right-click menu to open
        private void cmsForGridView_Opening(object sender, CancelEventArgs e)
        {
            if (GetNowSelectRow() == null || IsStartRead == true)
            {
                cms_tsmi_Copy.Enabled = false;
                cms_tsmi_WriteEpc.Enabled = false;
                cms_tsmi_WriteUserData.Enabled = false;
            }
            else
            {
                cms_tsmi_Copy.Enabled = true;
                cms_tsmi_WriteEpc.Enabled = true;
                cms_tsmi_WriteUserData.Enabled = true;
            }
        }
        // write EPC
        private void cms_tsmi_WriteEpc_Click(object sender, EventArgs e)
        {
            tsb_Write_EPC_Click(null, null);
        }
        // write user data
        private void cms_tsmi_WriteUserData_Click(object sender, EventArgs e)
        {
            tsb_Write_UserData_Click(null, null);
        }
        //Copy cell
        private void cms_tsmi_Copy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(dgv_Tags.SelectedCells[0].Value.ToString());
            }
            catch { }
        }
        // clear the list
        private void cms_tsmi_ClearList_Click(object sender, EventArgs e)
        {
            tsb_ClearList_Click(null, null);
        }
        // Get the currently selected column
        private DataGridViewRow GetNowSelectRow()
        {
            DataGridViewRow dgr_Select = null;
            if (dgv_Tags.CurrentCell != null)
            {
                dgr_Select = dgv_Tags.CurrentRow;
            }
            else if (dgv_Tags.SelectedRows.Count > 0)
            {
                dgr_Select = dgv_Tags.SelectedRows[0];
            }
            return dgr_Select;
        }

        #endregion

        #region General auxiliary method
        /// <summary>
        /// Close current connection
        /// </summary>
        public void CloseNowConnect()
        {
            if (!String.IsNullOrEmpty(ConnID))
            {
                try
                {
                    if (IsStartRead)         //Disconnect while reading tags
                    {
                        RFIDReader._RFIDConfig.Stop(ConnID);
                        IsStartRead = false;
                    }
                    RFIDReaderAPI.RFIDReader.CloseConn(ConnID);
                    ConnID = "";
                    tssl_NowConnID.Text = "---";
                }
                catch { }
            }
            CheckEnableButton();
        }
        /// <summary>
        /// Check if the reader is currently connected
        /// </summary>
        private Boolean CheckNowConnect()
        {
            if (String.IsNullOrEmpty(ConnID))
            {
                ShowMessage(_RC.GetString("Main_Show_NoConnect"));
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        ///The call event of the reader actively connecting to the server, single reader mode
        /// </summary>
        private void TCPPortConnectting(string newConnID)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new TCPPortConn(TCPPortConnectting), newConnID);
                return;
            }
            if (!String.IsNullOrEmpty(ConnID))
            {
                if (DialogResult.OK == ShowQuestion(_RC.GetString("Main_Show_Question_NewConnect")))
                {
                    CloseNowConnect();

                    //Do not reconnect, so the success rate of reconnect is not high
                    RFIDReader.CreateTcpConn(newConnID, this);        //reconnect
                    this.ConnID = newConnID;
                    tssl_NowConnID.Text = ConnID;
                    Init();
                }
                else
                {
                   
                }
            }
            else
            {
                CloseNowConnect();
                this.ConnID = newConnID;
                tssl_NowConnID.Text = ConnID;
                Init();
            }
        }
       
        /// <summary>
        /// Get antenna number
        /// </summary>
        /// <returns></returns>
        private Int32 GetAntNum()
        {
            Int32 antNUM = 0;
            foreach (var item in gb_ReadControl.Controls)               // Get antenna number
            {
                CheckBox control = item as CheckBox;
                if (control != null && control.Checked)
                {
                    if (Convert.ToInt32(control.Name.Substring(3)) <=8)
                    {
                        antNUM += Int32.Parse(control.Tag.ToString());
                    }
                }
            }
            return antNUM;
        }
       
        /// <summary>
        /// Update status bar
        /// </summary>
        public void StartFlush()
        {
            if (IsFlush == true) return;
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object o)
            {
                IsFlush = true;
                long flushCount = 0;
                while (IsFlush)
                {
                    if (flushCount % 2 == 0) FlushState();
                    flushCount++;
                    System.Threading.Thread.Sleep(500);
                    // Application.DoEvents();
                }
            }));
        }
       
        
        /// <summary>
        /// Update button controllability
        /// </summary>
        private void CheckEnableButton()
        {
            #region Restore control state

            foreach (ToolStripItem item in menuStrip.Items)
            {
                item.Enabled = true;
            }
            foreach (ToolStripItem item in tsm_Main.Items)
            {
                item.Enabled = true;
            }

            #endregion

            if (String.IsNullOrEmpty(ConnID))
            {
                foreach (ToolStripItem item in menuStrip.Items)
                {
                    item.Enabled = false;
                }
                tsmi_Connect.Enabled = true;
                tsmi_SearchDevice.Enabled = true;
                tsmi_Language.Enabled = true;
                foreach (ToolStripItem item in tsm_Main.Items)
                {
                    item.Enabled = false;
                }
            }
            else
            {
                if (IsStartRead)
                {

                    foreach (ToolStripItem item in menuStrip.Items)
                    {
                        item.Enabled = false;
                    }
                    tsmi_Connect.Enabled = true;
                    tsmi_Helper.Enabled = true;
                    tsb_Read_Epc.Enabled = false;
                    tsb_Read_EPCTID.Enabled = false;
                    tsb_Read_Global.Enabled = false;
                    tsb_Write_EPC.Enabled = false;
                    tsb_Write_UserData.Enabled = false;
                    tsb_WriteGlobal.Enabled = false;
                }
            }

        }
        /// <summary>
        /// Get tag read parameters
        /// </summary>
        public String GetReadTagParam(String varParam)
        {
            String rt = "";

            #region Get antenna number & single reading/cyclic reading

            Int32 antNUM = 0;
            Int32 singleOrWhile = -1;
            int count = 0;
            foreach (var item in gb_ReadControl.Controls)
            {
                CheckBox control = item as CheckBox;
                if (control != null && control.Checked)
                {
                    if (Convert.ToInt32(control.Name.Substring(3)) <= 8)
                    {
                        antNUM += Int32.Parse(control.Tag.ToString());
                        if (count==0)
                        {
                            antNo = (eAntennaNo)Int32.Parse(control.Tag.ToString());
                        }
                        else
                        {
                            antNo = antNo | (eAntennaNo)Int32.Parse(control.Tag.ToString());
                        }
                        count++;
                    }
                }
            }
            foreach (var item in gb_ReadControl.Controls)               
            {
                CheckBox control = item as CheckBox;
                if (control != null && control.Checked)
                {
                    if (Convert.ToInt32(control.Name.Substring(3)) > 8)
                    {
                        if (count>0)
                        {
                            antNo = antNo | RFIDReader._Tag6C.GeteAntennaNo(control.Name.Substring(3));
                        }
                        else
                        {
                            antNo = RFIDReader._Tag6C.GeteAntennaNo(control.Name.Substring(3));
                        }
                        count++;
                    }
                }
            }
            foreach (var item in gb_ReadType.Controls)
            {
                RadioButton control = item as RadioButton;
                if (control != null && control.Checked)
                {
                    singleOrWhile = Byte.Parse(control.Tag.ToString());
                    this.readType = (eReadType)singleOrWhile;
                }
            }
            rt += (Byte)antNUM + "|" + singleOrWhile + "|";

            #endregion


            #region Optional parameters

            string addAntParam = GetAddAntParam();

            if (!String.IsNullOrEmpty(varParam))
            {
                rt += varParam;
                if (antCount > 8)
                {
                    if (varParam.Contains(","))
                    {
                        if (rb_6c.Checked)
                        {
                            rt += "&10," + addAntParam;
                        }
                        else if (rb_6b.Checked)
                        {
                            rt += "&3," + addAntParam;
                        }
                        else if (rb_gb.Checked)
                        {
                            rt += "&6," + addAntParam;
                        }
                    }
                    else
                    {
                        if (rb_6c.Checked)
                        {
                            rt += "|10," + addAntParam;
                        }
                        else if (rb_6b.Checked)
                        {
                            rt += "|3," + addAntParam;
                        }
                        else if (rb_gb.Checked)
                        {
                            rt += "|6," + addAntParam;
                        }
                    }
                }
            }
            else
            {
                if (antCount > 8)
                {
                    if (rb_6c.Checked)
                    {
                        rt += "10," + addAntParam;
                    }
                    else if (rb_6b.Checked)
                    {
                        rt += "3," + addAntParam;
                    }
                    else if (rb_gb.Checked)
                    {
                        rt += "6," + addAntParam;
                    }
                }
            }

            #endregion

            rt = rt.TrimEnd('|');
            return rt;
        }

        /// <summary>
        /// Get increase antenna parameters
        /// </summary>
        /// <returns></returns>
        public String GetAddAntParam()
        {
            int addAntNum = 0;
            foreach (var item in gb_ReadControl.Controls)              
            {
                CheckBox control = item as CheckBox;
                if (control != null && control.Checked)
                {
                    if (Convert.ToInt32(control.Name.Substring(3)) > 8)
                    {
                        addAntNum += Int32.Parse(control.Tag.ToString());
                    }
                }
            }

            if (addAntNum != 0)
            {
                return Convert.ToString(addAntNum, 16).PadLeft(4, '0');
            }
            else
            {
                return "0000";
            }

        }

        #endregion

        #region Menu bar event

        #region Switch connection

        // Open serial connection
        private void tsmi_SerialConn_Click(object sender, EventArgs e)
        {
            if (!IsMultiConnect) { CloseNowConnect(); }
            MySingleForm.TestForm.Dialog.AddConnect addConnForm = new MySingleForm.TestForm.Dialog.AddConnect(this, 0);
            DialogResult dr = addConnForm.ShowDialog(this);
            this.Focus();
            if (dr == DialogResult.OK)
            {
                tssl_NowConnID.Text = ConnID;
                if (ConnID.Length > 60) 
                {
                    tssl_NowConnID.Text = dic_UsbDevicePath_Name[ConnID];
                }
                Init();
            }
            else if (dr == DialogResult.No)
            {
                ShowMessage(_RC.GetString("Main_Show_Connect_Failure"));
            }
        }
        // Open TCP connection
        private void tsmi_TCPClient_Click(object sender, EventArgs e)
        {
            if (!IsMultiConnect) { CloseNowConnect(); }
            MySingleForm.TestForm.Dialog.AddConnect addConnForm = new MySingleForm.TestForm.Dialog.AddConnect(this, 1);
            DialogResult dr = addConnForm.ShowDialog(this);
            this.Focus();
            if (dr == DialogResult.OK)
            {
                tssl_NowConnID.Text = ConnID;
                Init();
            }
            else if (dr == DialogResult.No)
            {
                ShowMessage(_RC.GetString("Main_Show_Connect_Failure"));
            }
        }
        // Open 485 connection
        private void tsmi_485Conn_Click(object sender, EventArgs e)
        {
            if (!IsMultiConnect) { CloseNowConnect(); }
            MySingleForm.TestForm.Dialog.AddConnect addConnForm = new MySingleForm.TestForm.Dialog.AddConnect(this, 2);
            DialogResult dr = addConnForm.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                tssl_NowConnID.Text = ConnID;
                Init();
            }
            else if (dr == DialogResult.No)
            {
                ShowMessage(_RC.GetString("Main_Show_Connect_Failure"));
            }
        }
        //Open the USB connection
        private void uSB模式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsMultiConnect) { CloseNowConnect(); }
            MySingleForm.TestForm.Dialog.AddConnect addConnForm = new MySingleForm.TestForm.Dialog.AddConnect(this, 3);
            DialogResult dr = addConnForm.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                tssl_NowConnID.Text = dic_UsbDevicePath_Name[ConnID];
                Init();
            }
            else if (dr == DialogResult.No)
            {
                ShowMessage(_RC.GetString("Main_Show_Connect_Failure"));
            }
        }

        // Start the TCP server
        private void tsmi_TCPServer_Click(object sender, EventArgs e)
        {
            MySingleForm.TestForm.TCP_Server serverForm = new MySingleForm.TestForm.TCP_Server(ConnID, this);
            serverForm.ShowDialog(this);
            this.Focus();
        }

        // Multi-reader polling
        public bool searchDeviceConnect(string connParam)
        {
            try
            {
                if (!IsMultiConnect) { CloseNowConnect(); }
                bool isConnect = RFIDReader.CreateTcpConn(connParam, this);
                if (isConnect)
                {
                    ConnID = connParam;
                    tssl_NowConnID.Text = ConnID;
                    Init();
                    this.Focus();
                    return true;
                }
                else
                {
                    ShowMessage(_RC.GetString("Main_Show_Connect_Failure"));
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }

        private void tsmi_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region Configuration

        // Get cached data
        private void tsmi_GetCacheTag_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                // Query reader breakpoint resume tag cache
                string st =  RFIDReader._ReaderConfig.GetBreakPointCacheTag(ConnID);
                MessageBox.Show(st);
            }
        }
        // Clear cache data
        private void tsmi_ClearCacheTag_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                int rt = RFIDReader._ReaderConfig.ClearBreakPointCache(ConnID);
                if (rt == 0)
                {
                    ShowMessage("OK!");
                }
                else
                {
                    ShowMessage("Failed|"+rt);
                }
                
            }
        }

        // advanced
        private void tsmi_AdvancedSettings_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                MySingleForm.TestForm.RFID_Option form_Opion = new MySingleForm.TestForm.RFID_Option(ConnID, this);
                form_Opion.ShowDialog(this);
                this.Focus();
            }
        }
        #endregion

        #region help 

        
        // Reader information
        private void tsmi_GetReaderMsg_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                try
                {
                    String vStr = Program.PARAM_SET.GetReaderInformation(ConnID);
                    String[] arrStr = vStr.Split('|');
                    if (arrStr.Length == 3)
                    {
                        UInt32 sdTime = 1;
                        if (UInt32.TryParse(arrStr[2], out sdTime))
                        {
                            UInt32 iDay = sdTime / (24 * 60 * 60);
                            UInt32 iHours = (sdTime % (24 * 60 * 60)) / (60 * 60);
                            UInt32 iMinute = ((sdTime % (24 * 60 * 60)) % (60 * 60)) / 60;
                            UInt32 iSecond = ((sdTime % (24 * 60 * 60)) % (60 * 60)) % 60;
                            // "Application software version: {0}\r\n Reader name: {1}\r\n Reader power-on time: {2} days {3} hours {4} minutes"
                            String resultStr = String.Format(_RC.GetString("Main_Version_Application"), arrStr[0], arrStr[1].Substring(2, arrStr[1].Length - 2), iDay, iHours, iMinute);
                            ShowSearchResult(resultStr);
                        }
                    }
                    else
                    {
                        MessageBox.Show(vStr);
                    }
                }
                catch { }
            }
        }
        // Get baseband software version
        private void tsmi_GetBaseBandVersion_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                String vStr = Program.PARAM_SET.GetReaderBaseBandSoftVersion(ConnID);
                ShowSearchResult(_RC.GetString("Main_Version_BaseBand") + vStr);
            }
        }
        private void tsmt_SNGet_Click(object sender, EventArgs e)
        {
            string sn = Program.TEST_OPTION.Get_0101_33(ConnID);
            if (sn.StartsWith("-"))
            {
                MessageBox.Show("Software baseband does not support or communication timeout！");
                return;
            }
            MessageBox.Show("serial number:" + (sn));
        }
        public string HexStringToASCII(string hexstring)
        {
            string s = hexstring;
            byte[] buff = new byte[s.Length / 2];
            int index = 0;
            for (int i = 0; i < s.Length; i += 2)
            {
                buff[index] = Convert.ToByte(s.Substring(i, 2), 16);
                ++index;
            }
            string result = Encoding.Default.GetString(buff);
            return result;
        }
        #endregion

        #endregion

        #region Interface implementation
        /// <summary>
        /// Output debugging information
        /// </summary>
        /// <param name="msg">Debug information</param>
        public void WriteDebugMsg(string msg)
        {
            if (!IsWriteDebug) return;
            if (tb_DebugMsg.InvokeRequired)
            {
                tb_DebugMsg.BeginInvoke(new WriteDebug(WriteDebugMsg), msg);
                return;
            }
            tb_DebugMsg.AppendText(msg + Environment.NewLine);
            nowDebugMessageCount++;
        }
        /// <summary>
        /// Output log message
        /// </summary>
        /// <param name="msg"></param>
        public void WriteLog(string msg)
        {
        }
        /// <summary>
        /// The reader actively connects in the TCP client mode
        /// </summary>
        /// <param name="connID"></param>
        public void PortConnecting(String connID)
        {
            if (IsMultiConnect)                     
            {
                WriteDebugMsg("New Reader Connected：" + connID);
            }
            else                                    
            {
                TCPPortConnectting(connID);
            }
        }
        /// <summary>
        /// The TCP partner actively closes the connection
        /// </summary>
        public void PortClosing(String connID)
        {
            if (this.ConnID.Equals(connID))
            {
                CloseNowConnect();
                Init();
            }
            WriteDebugMsg(connID + "Disconnect...");
        }
        /// <summary>
        ///  Label information output
        /// </summary>
        /// <param name="tag_Model"></param>
        public void OutPutTags(Tag_Model tag_Model)
        {
            if (!IsShowTag) return;
            if (tag_Model == null || tag_Model.Result != 0x00) return;
            bool isNew = false;
            DataGridViewRow dgvr = null;
            lock (dic_Rows)
            {
                try
                {
                    if (!dic_Rows.ContainsKey(tag_Model.EPC + "|" + tag_Model.TID))
                    {
                        dgvr = new DataGridViewRow();
                        dgvr.CreateCells(dgv_Tags, new object[] { tag_Model.ReaderName, tag_Model.TagType, 
                            tag_Model.EPC, tag_Model.TID, tag_Model.UserData, tag_Model.TagetData, 
                            tag_Model.TotalCount, tag_Model.ANT1_COUNT, tag_Model.ANT2_COUNT, 
                            tag_Model.ANT3_COUNT, tag_Model.ANT4_COUNT, tag_Model.ANT5_COUNT, 
                            tag_Model.ANT6_COUNT, tag_Model.ANT7_COUNT, tag_Model.ANT8_COUNT, 
                            tag_Model.ANT9_COUNT, tag_Model.ANT10_COUNT, tag_Model.ANT11_COUNT, 
                            tag_Model.ANT12_COUNT, tag_Model.ANT13_COUNT, tag_Model.ANT14_COUNT, 
                            tag_Model.ANT15_COUNT, tag_Model.ANT16_COUNT, tag_Model.ANT17_COUNT, 
                            tag_Model.ANT18_COUNT, tag_Model.ANT19_COUNT, tag_Model.ANT20_COUNT, 
                            tag_Model.ANT21_COUNT, tag_Model.ANT22_COUNT, tag_Model.ANT23_COUNT, 
                            tag_Model.ANT24_COUNT, 
                            tag_Model.RSSI, tag_Model.Frequency, tag_Model.Phase, tag_Model.ReadTime,
                            tag_Model.G2V2Challenge,tag_Model.G2V2Data,
							tag_Model.EM_Temperature, tag_Model.RFMicron_Temperature});
                        dic_Rows.Add(tag_Model.EPC + "|" + tag_Model.TID, dgvr);
                        isNew = true;
                    }
                    else
                    {
                        dgvr = dic_Rows[tag_Model.EPC + "|" + tag_Model.TID];
                    }
                }
                catch { }
            }
            AddSingleTag(tag_Model, dgvr, isNew);
        }
        /// <summary>
        ///  Read tag end message
        /// </summary>
        public void OutPutTagsOver()
        {
            // lb_tagTotalCount.Text = dic_Rows.Count.ToString();
            this.led_Tag_Count.Text = dic_Rows.Count.ToString();
            IsFlush = false;
        }
        /// <summary>
        /// GPIO trigger message
        /// </summary>

        public void GPIControlMsg(GPI_Model gpi_model)
        {
            if (gpi_model.StartOrStop == 0)
            {
                GPI_Start(gpi_model.GpiIndex - 1, gpi_model.GpiState);  //Subtract one for compatibility, change GPIindex to start from 1
            }
            else
            {
                GPI_End(gpi_model.GpiIndex - 1, gpi_model.GpiState);
            }
        }

		public void EventUpload(CallBackEnum type, object param)
		{
			//throw new NotImplementedException();
		}

        /// <summary>
        /// Real-time data display
        /// </summary>
        public void FlushState()
        {
            if (this.lb_ReceiveCount.InvokeRequired)
            {
                this.dgv_Tags.BeginInvoke(new FlushState(FlushState), null);
                return;
            }
            Monitor.Enter(dgv_Tags);
            try
            {
                long nowTagCount = RFIDReader.DIC_CONNECT[ConnID].ProcessCount;
                this.lb_ReceiveCount.Text = nowTagCount.ToString();
                this.tssl_CacheSize.Text = RFIDReader.DIC_CONNECT[ConnID].receiveBufferManager.DataCount.ToString();
                this.led_Tag_Count.Text = dic_Rows.Count.ToString();
                Int32 totalMinutes = (Int32)((DateTime.Now - TJ_Run_Start).TotalMilliseconds / 1000);
                led_Time.Text = totalMinutes.ToString();
                long l_Speed = Math.Abs(nowTagCount - TJ_LastTagcount);
                led_Speed.Text = l_Speed.ToString();
                TJ_LastTagcount = nowTagCount;

                float cpuLoad = pc_Processor.NextValue();
                tssl_CPULoad.Text = cpuLoad.ToString("F2") + "%";           // CPU%
            }
            catch { }
            Monitor.Exit(dgv_Tags);
        }

        /// <summary>
        /// Display query results
        /// </summary>
        /// <param name="msg">Result information</param>
        public void ShowSearchResult(String msg)
        {
            MySingleForm.TestForm.Dialog.SearchResult srForm = new MySingleForm.TestForm.Dialog.SearchResult(msg);
            srForm.Show();
        }

        #endregion

        #region Toolbar events

        #region read EPC
        /// <summary>
        /// read EPC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsb_Read_Epc_Click(object sender, EventArgs e)
        {
            Program.RFID_OPTION.StopReader(ConnID);
            tsb_ClearList_Click(null, null);            
            int st = -1;
            GetReadTagParam("");
            if (rb_6c.Checked)
            {
              
                st = RFIDReader._Tag6C.GetEPC(ConnID, antNo, readType);
            }
            else if (rb_6b.Checked)
            {
                st = RFIDReader._Tag6B.Get6B(ConnID, antNo, readType,e6BReaderContent.TID);
            }
            else if (rb_gb.Checked)
            {
                st = RFIDReader._TagGB.GetEPC(ConnID, antNo, readType);
            }
            if (st!=0) { ShowMessage(LanguageHelper.GetResutlCode(0,st)); return; }
            tsb_Read_Enable();
        } 
        #endregion

        #region read EPC&TID
        private void tsb_Read_EPCTID_Click(object sender, EventArgs e)
        {
            RFIDReader._RFIDConfig.Stop(ConnID);
            tsb_ClearList_Click(null, null);            
            GetReadTagParam("");
            int st = -1;
            if (rb_6c.Checked)
            {
                st = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType);
            }
            else if (rb_6b.Checked)
            {
                st = RFIDReader._Tag6B.Get6B(ConnID, antNo, readType, e6BReaderContent.TID);
            }
            else if (rb_gb.Checked)
            {
                st = RFIDReader._TagGB.GetEPC_TID(ConnID, antNo, readType);
            }
            if (st != 0) { ShowMessage(LanguageHelper.GetResutlCode(0, st)); return; }
            tsb_Read_Enable();
        } 
        #endregion

        #region Advanced reading

        private void tsb_Read_Global_Click(object sender, EventArgs e)
        {
            int rs = -1;
            tsb_ClearList_Click(null, null);            
            GetReadTagParam("");
            if (rb_6c.Checked)                          // 6C
            {
                #region read 6C
                MySingleForm.TestForm.ReadTag_Param readTagParam = new MySingleForm.TestForm.ReadTag_Param(this, 0);
                DialogResult rt = readTagParam.ShowDialog(this);
                this.Focus();
                if (rt == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(readVarParam_6C))
                    {
                        return;
                    }
                    string[] datalist = readVarParam_6C.Split('&');
                    List<bool> checkList = new List<bool>() { false, false, false, false, false,false };
                    foreach (var item in datalist)
                    {

                        string[] strs = item.Split(',');
                        string[] temparams = strs[1].Split('_');
                        if (strs[0].Equals("1")) // 匹配读
                        {
                            readP.MatchType = Convert.ToInt32(temparams[0]);
                            readP.MatchCode =  temparams[2];
                            readP.MatchIndex = Convert.ToInt32(temparams[1]);
                            checkList[4] = true; 
                        }
                        else if (strs[0].Equals("2")) // 读 TID
                        {
                            readP.TIDReadType= Convert.ToInt32(temparams[0]);
                            readP.TIDReadLen= Convert.ToInt32(temparams[1]);
                            checkList[0] = true;
                        }
                        else if (strs[0].Equals("3")) // UserData
                        {
                            readP.UserDataLen = Convert.ToInt32(temparams[1], 16);
                            readP.UserDataStartIndex = Convert.ToInt32(temparams[0]);
                            checkList[1] = true;
                        }
                        else if (strs[0].Equals("4")) // Reserved area
                        {
                            readP.ReserveDataLen = Convert.ToInt32(temparams[1], 16);
                            readP.UserDataStartIndex = Convert.ToInt32(temparams[0]);
                            checkList[2] = true;
                        }
                        else if (strs[0].Equals("5")) // Access password
                        {
                            readP.pwd = temparams[0];
                            checkList[3] = true;
                        }
                        else if (strs[0].Equals("6")) // EPCData
                        {
                            readP.EPCDataLen = Convert.ToInt32(temparams[1], 16);
                            readP.EPCDataStartIndex = Convert.ToInt32(temparams[0]);
                            checkList[5] = true;
                        }
                    }
                    #region Match read
                    if (checkList[4])  // Match read
                    {
                        if (checkList[3]) // With access password
                        {
                            #region Access password matching read
                            if (checkList[0])  // TID
                            {
                                if (checkList[1]) // UserData
                                {
                                    if (checkList[2]) // TID、UserData和 Reserved
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData_ReservedData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.ReserveStartIndex, readP.ReserveDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                                    }
                                    else // TID和UserData
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                                    }
                                }
                                else if (checkList[2]) // read TID and Reserved
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                                }
                                else if (checkList[5]) // TID and EPCData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                                }
                                else // only read TID
                                {
                                    if (readP.TIDReadLen>0&&readP.TIDReadType == 1)
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType,lenght:readP.TIDReadLen, matchType: readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, matchCode: readP.MatchCode, matchWordStartIndex: readP.MatchIndex, accessPassword: readP.pwd);
                                    }
                                    else
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType, matchType: readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, matchCode: readP.MatchCode, matchWordStartIndex: readP.MatchIndex, accessPassword: readP.pwd);
                                    }
                                    
                                }
                            }
                            else if (checkList[1]) // UserData
                            {
                                if (checkList[2]) // UserData and Reserved // No such function
                                {
                                    MessageBox.Show(_RC.GetString("ReadTag_Error01"));
                                    return;
                                }
                                else // only read UserData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                                }
                            }
                            else if (checkList[2]) // only read Reserved
                            {
                                rs = RFIDReader._Tag6C.GetEPC_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                            }
                            else if (checkList[5]) // only read EPCData
                            {
                                rs = RFIDReader._Tag6C.GetEPC_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                            }
                            else // match read EPC
                            {
                                rs = RFIDReader._Tag6C.GetEPC(ConnID, antNo, readType, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex, readP.pwd);
                            }
                            #endregion
                        }
                        else // Without access password
                        {
                            #region Matching read, without access password
                            if (checkList[0])  // TID
                            {
                                if (checkList[1]) // UserData
                                {
                                    if (checkList[2]) // TID、UserData和 Reserved
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData_ReservedData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.ReserveStartIndex, readP.ReserveDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                                    }
                                    else // TID and UserData
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                                    }
                                }
                                else if (checkList[2]) // read TID and Reserved
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                                }
                                else if (checkList[5]) // TID and EPCData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                                }
                                else // only read TID
                                {

                                    if (readP.TIDReadLen > 0 && readP.TIDReadType == 1)
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType, lenght: readP.TIDReadLen, matchType: readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, matchCode: readP.MatchCode, matchWordStartIndex: readP.MatchIndex);
                                    }
                                    else
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType, matchType: readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, matchCode: readP.MatchCode, matchWordStartIndex: readP.MatchIndex);
                                    }
                                    
                                }
                            }
                            else if (checkList[1]) // UserData
                            {
                                if (checkList[2]) // UserData and Reserved // No such function
                                {
                                    MessageBox.Show(_RC.GetString("ReadTag_Error01"));
                                    return;
                                }
                                else // only read UserData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                                }
                            }
                            else if (checkList[2]) //only read Reserved
                            {
                                rs = RFIDReader._Tag6C.GetEPC_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                            }
                            else if (checkList[5]) // only read EPCData
                            {
                                rs = RFIDReader._Tag6C.GetEPC_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                            }
                            else // mathch read EPC
                            {
                                rs = RFIDReader._Tag6C.GetEPC(ConnID, antNo, readType, readP.MatchType == 1 ? eMatchCode.EPC : eMatchCode.TID, readP.MatchCode, readP.MatchIndex);
                            }
                            #endregion
                        }


                    }
                    #endregion
                    else
                    {
                        #region Mismatch, normal card reading
                        if (checkList[3]) // Access password
                        {
                            if (checkList[0])  // TID
                            {
                                if (checkList[1]) // UserData
                                {
                                    if (checkList[2]) // TID、UserData和 Reserved
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData_ReservedData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.ReserveStartIndex, readP.ReserveDataLen,accessPassword: readP.pwd);
                                    }
                                    else // TID and UserData
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, accessPassword: readP.pwd);
                                    }
                                }
                                else if (checkList[2]) // read TID和Reserved
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen, accessPassword: readP.pwd);
                                }
                                else if (checkList[5]) // TID and EPCData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen, accessPassword: readP.pwd);
                                }
                                else // only read TID
                                {
                                    if (readP.TIDReadLen > 0 && readP.TIDReadType == 1)
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType, readP.TIDReadLen, accessPassword: readP.pwd);
                                    }
                                    else
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType, accessPassword: readP.pwd);
                                    }

                                }
                            }
                            else if (checkList[1]) // UserData
                            {
                                if (checkList[2]) // UserData and Reserved // No such function
                                {
                                    MessageBox.Show(_RC.GetString("ReadTag_Error01"));
                                    return;
                                }
                                else // only read UserData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, accessPassword: readP.pwd);
                                }
                            }
                            else if (checkList[2]) //only read  Reserved
                            {
                                rs = RFIDReader._Tag6C.GetEPC_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen, accessPassword: readP.pwd);
                            }
                            else if (checkList[5]) // only read  EPCData
                            {
                                rs = RFIDReader._Tag6C.GetEPC_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen, accessPassword: readP.pwd);
                            }
                            else // match read EPC
                            {
                                rs = RFIDReader._Tag6C.GetEPC(ConnID, antNo, readType, accessPassword: readP.pwd);
                            }
                        }
                        else // Without access password
                        {
                            if (checkList[0])  // TID
                            {
                                if (checkList[1]) // UserData
                                {
                                    if (checkList[2]) // TID、UserData和 Reserved
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData_ReservedData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen, readP.ReserveStartIndex, readP.ReserveDataLen);
                                    }
                                    else // TID and UserData
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen);
                                    }
                                }
                                else if (checkList[2]) // read TID和Reserved
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen);
                                }
                                else if (checkList[5]) // TID and EPCData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_TID_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen);
                                }
                                else // 只读TID
                                {
                                    if (readP.TIDReadLen > 0 && readP.TIDReadType == 1)
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType, readP.TIDReadLen);
                                    }
                                    else
                                    {
                                        rs = RFIDReader._Tag6C.GetEPC_TID(ConnID, antNo, readType);
                                    }

                                }
                            }
                            else if (checkList[1]) // UserData
                            {
                                if (checkList[2]) // UserData and Reserved // No such function
                                {
                                    MessageBox.Show(_RC.GetString("ReadTag_Error01"));
                                    return;
                                }
                                else // only read UserData
                                {
                                    rs = RFIDReader._Tag6C.GetEPC_UserData(ConnID, antNo, readType, readP.UserDataStartIndex, readP.UserDataLen);
                                }
                            }
                            else if (checkList[2]) //only read Reserved
                            {
                                rs = RFIDReader._Tag6C.GetEPC_ReservedData(ConnID, antNo, readType, readP.ReserveStartIndex, readP.ReserveDataLen);
                            }
                            else if (checkList[5]) // only read EPCData
                            {
                                rs = RFIDReader._Tag6C.GetEPC_EpcData(ConnID, antNo, readType, readP.EPCDataStartIndex, readP.EPCDataLen);
                            }
                            else // match read EPC
                            {
                                rs = RFIDReader._Tag6C.GetEPC(ConnID, antNo, readType);
                            }
                        }
                        
                        #endregion
                    } 
                        
                    
                    if (rs!=0)
                    {
                        MessageBox.Show(LanguageHelper.GetResutlCode(0, rs));
                    }
                }
                else
                {
                    return;
                }
                #endregion
            }
            else if (rb_6b.Checked)                     // 6B
            {
                #region read 6B
                MySingleForm.TestForm.ReadTag_Param readTagParam = new MySingleForm.TestForm.ReadTag_Param(this, 1);
                DialogResult rt = readTagParam.ShowDialog(this);
                this.Focus();
                if (rt == DialogResult.OK)
                {
                    string[] strs = readVarParam_6B.Split('|');
                    string[] tempparams = strs[1].Split('&');
                    if (strs[0].Equals("0")) // only read TID
                    {
                        rs = RFIDReader._Tag6B.Get6B(ConnID, antNo, readType, e6BReaderContent.TID);
                    }
                    else if (strs[0].Equals("1")) // Read user area and TID area
                    {
                        string[] temparam = tempparams[0].Split(',')[1].Split('_');
                        rs = RFIDReader._Tag6B.Get6B_UserData(ConnID, antNo, readType, e6BReaderContent.TID_UserData, Convert.ToInt32(temparam[0]), Convert.ToInt32(temparam[1]));
                    }
                    else
                    {
                        string[] temparam = tempparams[0].Split(',')[1].Split('_');
                        if (tempparams.Length == 2) // only read user area
                        {
                            rs = RFIDReader._Tag6B.Get6B_UserData_MatchTID(ConnID, antNo, readType, e6BReaderContent.UserData, Convert.ToInt32(temparam[0]), Convert.ToInt32(temparam[1]), tempparams[1].Split(',')[1]);
                        }
                        else
                        {
                            rs = RFIDReader._Tag6B.Get6B_UserData(ConnID, antNo, readType, e6BReaderContent.UserData, Convert.ToInt32(temparam[0]), Convert.ToInt32(temparam[1]));
                        }
                    }
                }
                if (rs != 0)
                {
                    MessageBox.Show("ErrorCode:" + rs);
                    return;
                } 
                #endregion
            }
            else if (rb_gb.Checked)                    // GB
            {
                #region read GB
                MySingleForm.TestForm.ReadTag_Param readTagParam = new MySingleForm.TestForm.ReadTag_Param(this, 2);
                DialogResult rt = readTagParam.ShowDialog(this);
                this.Focus();
                if (rt == DialogResult.OK)
                {

                    if (string.IsNullOrEmpty(readVarParam_GB))
                    {
                        return;
                    }
                    string[] dataarrary = readVarParam_GB.Split('&');
                    foreach (var item in dataarrary)
                    {
                        string[] tempStr = item.Split(',');
                        string[] paramsdata = tempStr[1].Split('_');
                        if (tempStr[0].Equals("1")) // match
                        {
                            if (paramsdata[0].Equals("00")) // TID
                            {
                                if (paramsdata[1].Equals("0000")) // Do not set the match start bit
                                {
                                    rs = RFIDReader._TagGB.GetEPC_TID_MatchTID(ConnID, antNo, readType, paramsdata[2]);
                                }
                                else
                                {
                                    rs = RFIDReader._TagGB.GetEPC_TID_MatchTID(ConnID, antNo, readType, paramsdata[2], Convert.ToInt32(paramsdata[1]));
                                }
                            }
                            else if (paramsdata[0].Equals("10")) //EPC
                            {
                                if (paramsdata[1].Equals("0000")) // Do not set the match start bit
                                {
                                    rs = RFIDReader._TagGB.GetEPC_TID_MatchEPC(ConnID, antNo, readType, paramsdata[2]);
                                }
                                else
                                {
                                    rs = RFIDReader._TagGB.GetEPC_TID_MatchEPC(ConnID, antNo, readType, paramsdata[2], Convert.ToInt32(paramsdata[1]));
                                }
                            }
                            else  
                            {
                                rs = RFIDReader._TagGB.GetEPC_TID(ConnID, antNo, readType);
                               
                            }
                        }
                        else if (tempStr[0].Equals("2")) // TID
                        {
                            if (paramsdata[0].Equals("00"))
                            {
                                rs = RFIDReader._TagGB.GetEPC_TID(ConnID, antNo, readType);
                            }
                            else
                            {
                                rs = RFIDReader._TagGB.GetEPC_TID(ConnID, antNo, readType, Convert.ToInt32(paramsdata[1]));
                            }
                        }
                        else if (tempStr[0].Equals("3"))  // UserData
                        {
                            if (paramsdata.Length == 4) // match password
                            {

                                rs = RFIDReader._TagGB.GetEPC_TID_UserData(ConnID, antNo, readType, Convert.ToInt32(paramsdata[0]), paramsdata[1].Length, paramsdata[2], paramsdata[3]);
                            }
                            else
                            {
                                rs = RFIDReader._TagGB.GetEPC_TID_UserData(ConnID, antNo, readType, Convert.ToInt32(paramsdata[0]), paramsdata[1].Length, paramsdata[2]);
                            }
                        }
                    }

                } 
                #endregion
            }
            if (rs==0)
            {
                tsb_Read_Enable();
            }
            else
            {
                ShowMessage("Error Code:" + rs);
            }
        }
        // Read control status
        private void tsb_Read_Enable()
        {
            RFIDReader.DIC_CONNECT[ConnID].ProcessCount = 0;  
            TJ_LastTagcount = 0;
            StartFlush();                                   
            if (rb_While.Checked)
            {
                IsStartRead = true;
                CheckEnableButton();
            }
        }
        #endregion

        #region write EPC

        private void tsb_Write_EPC_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = GetNowSelectRow();
            if (dgvr != null)
            {
                GetReadTagParam("");
                Int32 antNUM = (int)antNo;
                String[] args = new String[] { "", "", "" };
                args[0] = dgvr.Cells["clm_EPC"].Value.ToString();
                args[1] = dgvr.Cells["clm_TID"].Value.ToString();
                args[2] = dgvr.Cells["clm_UserData"].Value.ToString();

                
                if (dgvr.Cells["clm_TagType"].Value.Equals("6C"))           // write 6C
                {
                    MySingleForm.TestForm.FunctionForm.FunctionWriteEpc formWriteEPC = new MySingleForm.TestForm.FunctionForm.FunctionWriteEpc(ConnID, args, antNUM, GetAddAntParam(), this);
                    formWriteEPC.ShowDialog(this);
                    this.Focus();
                }
                else if (dgvr.Cells["clm_TagType"].Value.Equals("GB"))
                {
                    MySingleForm.TestForm.FunctionForm.FunctionWriteEpc_GB formWriteEPC = new MySingleForm.TestForm.FunctionForm.FunctionWriteEpc_GB(ConnID, args, antNUM, GetAddAntParam(), this);
                    formWriteEPC.ShowDialog(this);
                    this.Focus();
                }
                else
                {
                    ShowMessage(_RC.GetString("Main_Show_Write_Error2"));
                }
            }
            else
            {
                ShowMessage(_RC.GetString("Main_Show_Write_Error1"));
            }
        } 
        #endregion

        #region write UserData

        private void tsb_Write_UserData_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvr = GetNowSelectRow();
            if (dgvr != null)
            {
                GetReadTagParam("");
                Int32 antNUM = (int)antNo;
                String[] args = new String[] { "", "", "" };
                args[0] = dgvr.Cells["clm_EPC"].Value.ToString();
                args[1] = dgvr.Cells["clm_TID"].Value.ToString();
                args[2] = dgvr.Cells["clm_UserData"].Value.ToString();
                antNUM = GetAntNum();
                if (dgvr.Cells["clm_TagType"].Value.Equals("6C"))           // write 6C
                {
                    MySingleForm.TestForm.FunctionForm.FunctionWriteUserData formWriteUserData = new MySingleForm.TestForm.FunctionForm.FunctionWriteUserData(ConnID, args, antNUM, GetAddAntParam(), this);
                    formWriteUserData.ShowDialog(this);
                    this.Focus();
                }
                else if (dgvr.Cells["clm_TagType"].Value.Equals("GB"))
                {
                    MySingleForm.TestForm.FunctionForm.FunctionWriteUserData_GB formWriteUserData = new MySingleForm.TestForm.FunctionForm.FunctionWriteUserData_GB(ConnID, args, antNUM, GetAddAntParam(), this);
                    formWriteUserData.ShowDialog(this);
                    this.Focus();
                }
                else
                {
                    ShowMessage(_RC.GetString("Main_Show_Write_Error2"));
                }
            }
            else
            {
                ShowMessage(_RC.GetString("Main_Show_Write_Error1"));
            }
        } 
        #endregion

        #region Advanced writing

        private void tsb_WriteGlobal_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                String[] args = new String[] { "", "", "" };
                DataGridViewRow dgvr = GetNowSelectRow();
                if (dgvr != null)                        
                {
                    args[0] = dgvr.Cells["clm_EPC"].Value.ToString();
                    args[1] = dgvr.Cells["clm_TID"].Value.ToString();
                    args[2] = dgvr.Cells["clm_UserData"].Value.ToString();
                }
                GetReadTagParam("");
                Int32 antNUM = (int)antNo;
                
                if (rb_6c.Checked)
                {
                    MySingleForm.TestForm.Tag_Option formTagOption = new MySingleForm.TestForm.Tag_Option(ConnID, args, antNUM, GetAddAntParam(), this);
                    formTagOption.ShowDialog(this);
                    this.Focus();
                }
                else if (rb_6b.Checked)
                {
                    MySingleForm.TestForm.Tag_Option6B formTagOption = new MySingleForm.TestForm.Tag_Option6B(ConnID, args, antNUM, GetAddAntParam(), this);
                    formTagOption.ShowDialog(this);
                    this.Focus();
                }
                else if (rb_gb.Checked)
                {
                    MySingleForm.TestForm.Tag_OptionGB formTagOption = new MySingleForm.TestForm.Tag_OptionGB(ConnID, args, antNUM, GetAddAntParam(), this);
                    formTagOption.ShowDialog(this);
                    this.Focus();
                }
            }
        } 
        #endregion

        #region Select column

        private void tsddb_SelectColumn_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

            ToolStripMenuItem tsmi = e.ClickedItem as ToolStripMenuItem;
            if (tsmi != null)
            {
                if (!tsmi.Checked)
                {
                    dgv_Tags.Columns["clm_" + e.ClickedItem.Text].Visible = true;
                }
                else
                {
                    dgv_Tags.Columns["clm_" + e.ClickedItem.Text].Visible = false;
                }
            }
        } 
        #endregion

        #region Restart the reader

        private void tsb_ResetReader_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                Program.PARAM_SET.ReSetReader(ConnID);
                
            }
        } 
        #endregion

        #region Stop

        private void tsb_Stop_Click(object sender, EventArgs e)
        {
            IsStartRead = false;
            IsFlush = false;
            RFIDReader._Tag6C.Stop(ConnID);
            CheckEnableButton();
        } 
        #endregion

        #region Clear list

        private void tsb_ClearList_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckNowConnect())
                {
                    lock (dic_Rows)
                    {
                        dgv_Tags.Rows.Clear();
                        dic_Rows.Clear();
                    }
                    TJ_Run_Start = DateTime.Now;
                    RFIDReaderAPI.RFIDReader.DIC_CONNECT[ConnID].ClearTagData();
                }
                led_Speed.Text = "0";
                led_Tag_Count.Text = "0";
                led_Tag_ReadCount.Text = "0";
                led_Time.Text = "0";
            }
            catch { }
        } 
        #endregion

        #region Close the connection
        private void tsb_CloseNowConnect_Click(object sender, EventArgs e)
        {
            CloseNowConnect();
        } 
        #endregion

        #endregion

        #region GPI control method
        /// <summary>
        /// 
        /// </summary>
        public void InitGPI()
        {
            TJ_List_GPI_Button.Add(btn_GPI_0);
            TJ_List_GPI_Button.Add(btn_GPI_1);
            TJ_List_GPI_Button.Add(btn_GPI_2);
            TJ_List_GPI_Button.Add(btn_GPI_3);
        }
        /// <summary>
        /// Change GPI status
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="cl"></param>
        public void UpdateGPIState(RoundButton rb, Color cl)
        {
            if (rb.InvokeRequired)
            {
                rb.BeginInvoke(new UpdateGPI(UpdateGPIState), rb, cl);
                return;
            }
            rb.Invalidate();
            rb.BackColor = cl;
        }
        /// <summary>
        /// GPI trigger start
        /// </summary>
        /// <param name="gpiIndex">GPI port subscript</param>
        /// <param name="gpiState">GPI status, 0 low 1 high</param>
        public void GPI_Start(Int32 gpiIndex, Int32 gpiState)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object o)
            {
                if (TJ_List_GPI_Button[gpiIndex].Tag.Equals("0")) return;
                lock (TJ_List_GPI_Button[gpiIndex])
                {
                    TJ_List_GPI_Button[gpiIndex].Tag = "0";
                }
                while (TJ_List_GPI_Button[gpiIndex].Tag.Equals("0"))
                {
                    if (gpiState == 0)
                    {
                        TJ_List_GPI_Button[gpiIndex].BackColor = Color.Red;
                    }
                    else
                    {
                        TJ_List_GPI_Button[gpiIndex].BackColor = Color.DimGray;
                    }
                    lock (TJ_List_GPI_Button[gpiIndex])
                    {
                        if (Monitor.Wait(TJ_List_GPI_Button[gpiIndex], 500))              
                        {
                            break;
                        }
                    }
                    if (TJ_List_GPI_Button[gpiIndex].Tag.Equals("0"))
                    {
                        TJ_List_GPI_Button[gpiIndex].BackColor = Color.White;
                    }
                    else
                    {
                        break;
                    }
                    lock (TJ_List_GPI_Button[gpiIndex])
                    {
                        if (Monitor.Wait(TJ_List_GPI_Button[gpiIndex], 500))              
                        {
                            break;
                        }
                    }
                }
            }), new object());
        }
        /// <summary>
        /// GPI trigger end
        /// </summary>
        /// <param name="gpiIndex"></param>
        /// <param name="gpiState"></param>
        public void GPI_End(Int32 gpiIndex, Int32 gpiState)
        {
            lock (TJ_List_GPI_Button[gpiIndex])
            {
                Monitor.PulseAll(TJ_List_GPI_Button[gpiIndex]);
            }
            lock (TJ_List_GPI_Button[gpiIndex])
            {
                TJ_List_GPI_Button[gpiIndex].Tag = "1";
            }
            if (gpiState == 0)
            {
                TJ_List_GPI_Button[gpiIndex].BackColor = Color.Red;
            }
            else
            {
                TJ_List_GPI_Button[gpiIndex].BackColor = Color.DimGray;
            }
        }

        #endregion

        #region Main page event

        private void btn_TagOption_Click(object sender, EventArgs e)
        {
            if (CheckNowConnect())
            {
                String[] args = new String[] { "", "", "" };
                if (dgv_Tags.SelectedRows.Count > 0)                        
                {
                    DataGridViewRow dgvr = dgv_Tags.SelectedRows[0];
                    args[0] = dgvr.Cells["clm_EPC"].Value.ToString();
                    args[1] = dgvr.Cells["clm_TID"].Value.ToString();
                    args[2] = dgvr.Cells["clm_UserData"].Value.ToString();
                }
                Int32 antNUM = 0;
                foreach (var item in gb_ReadControl.Controls)               
                {
                    CheckBox control = item as CheckBox;
                    if (control != null && control.Checked)
                    {
                        if (Convert.ToInt32(control.Name.Substring(3)) <= 8)
                        {
                            antNUM += Int32.Parse(control.Tag.ToString());
                        }
                    }
                }
                if (rb_6c.Checked)
                {
                    MySingleForm.TestForm.Tag_Option formTagOption = new MySingleForm.TestForm.Tag_Option(ConnID, args, antNUM, GetAddAntParam(), this);
                    formTagOption.ShowDialog(this);
                    this.Focus();
                }
                else if (rb_6b.Checked)
                {
                    MySingleForm.TestForm.Tag_Option6B formTagOption = new MySingleForm.TestForm.Tag_Option6B(ConnID, args, antNUM, GetAddAntParam(), this);
                    formTagOption.ShowDialog(this);
                    this.Focus();
                }
            }
        }

        #endregion

        #region Configuration file operation

        public void SaveConfig()
        {
            Helper.MyXmlHelper.UpdateInnerText(XMLFIENAME, "Root/" + this.Name, "ReadVarParam_6C", readVarParam_6C);
            Helper.MyXmlHelper.UpdateInnerText(XMLFIENAME, "Root/" + this.Name, "ReadVarParam_6B", readVarParam_6B);
        }

        public void ReadConfig()
        {
            readVarParam_6C = Helper.MyXmlHelper.ReadInnerText(XMLFIENAME, "Root/" + this.Name, "ReadVarParam_6C");
            readVarParam_6B = Helper.MyXmlHelper.ReadInnerText(XMLFIENAME, "Root/" + this.Name, "ReadVarParam_6B");
        }

        #endregion

        #region Search device

        private void tsmi_SearchDevice_Click(object sender, EventArgs e)
        {
            if (sDevice == null)
            {
                sDevice = new MySingleForm.SearchDevice(this);
                sDevice.Show();
            }
            else
            {
                if (sDevice.IsDisposed)
                {
                    sDevice = new MySingleForm.SearchDevice(this);
                    sDevice.Show();
                }
                sDevice.Focus();
            }

        }

        #endregion

        #region Language

        private void tsmi_Language_EN_Click(object sender, EventArgs e)
        {
            try
            {
                Helper.ConfigHelper.SetValue("Language", "en");
                Application.Restart();
            }
            catch { }
        }


        private void tsmi_Language_CN_Click(object sender, EventArgs e)
        {
            try
            {
                Helper.ConfigHelper.SetValue("Language", "cn");
                Application.Restart();
            }
            catch { }
        }

        #endregion

        #region select all
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var item in gb_ReadControl.Controls)
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
        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var item in gb_ReadControl.Controls)
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
        #endregion

	}

    public class DataGridViewPlus : DataGridView
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            try { base.OnPaint(e); }
            catch { Invalidate(); }
        }
    }

    [Flags]
    public enum SoundFlags
    {
        /// <summary>play synchronously (default)</summary>
        SND_SYNC = 0x0000,
        /// <summary>play asynchronously</summary>
        SND_ASYNC = 0x0001,
        /// <summary>silence (!default) if sound not found</summary>
        SND_NODEFAULT = 0x0002,
        /// <summary>pszSound points to a memory file</summary>
        SND_MEMORY = 0x0004,
        /// <summary>loop the sound until next sndPlaySound</summary>
        SND_LOOP = 0x0008,
        /// <summary>don’t stop any currently playing sound</summary>
        SND_NOSTOP = 0x0010,
        /// <summary>Stop Playing Wave</summary>
        SND_PURGE = 0x40,
        /// <summary>don’t wait if the driver is busy</summary>
        SND_NOWAIT = 0x00002000,
        /// <summary>name is a registry alias</summary>
        SND_ALIAS = 0x00010000,
        /// <summary>alias is a predefined id</summary>
        SND_ALIAS_ID = 0x00110000,
        /// <summary>name is file name</summary>
        SND_FILENAME = 0x00020000,
        /// <summary>name is resource name or atom</summary>
        SND_RESOURCE = 0x00040004
    }
}