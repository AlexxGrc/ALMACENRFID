using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RFIDReaderAPI.Interface;
using RFIDReaderAPI;
using Microsoft.Win32;
using System.IO.Ports;
using System.Text.RegularExpressions;
using RFIDReaderAPI.MyConnect;
using ALMACENRFID;

namespace ALMACENRFID.MySingleForm.TestForm.Dialog
{
    public partial class AddConnect : BaseDialog
    {
        SingleMainForm contextForm = null;
        List<string> listUsbDevicePath = null;//保存识别的usb设备文件路径
        public AddConnect()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.No;
        }
        public AddConnect(SingleMainForm contextForm)
            : this()
        {
            this.contextForm = contextForm;
            cb_ConnectType.SelectedIndex = 0;
            this.Width = 450;
            this.Height = 200;
        }
        public AddConnect(SingleMainForm contextForm, Int32 connType)
            : this(contextForm)
        {
            InitCom();
            if (connType == 0)
            {
                cb_ConnectType.SelectedIndex = 1;
                cb_ConnectType.SelectedIndex = 0;
            }
            else if (connType == 1)
                cb_ConnectType.SelectedIndex = 1;
            else if (connType == 2)
                cb_ConnectType.SelectedIndex = 2;
            else if (connType == 3)
                cb_ConnectType.SelectedIndex = 3;
        }

        public void InitCom()
        {
            try
            {
                this.cb_ComNum.Items.Clear();
                foreach (string vPortName in SerialPort.GetPortNames())
                {
                    String portName = Regex.Match(vPortName, @"COM[0-9]+").Value;
                    this.cb_ComNum.Items.Add(portName);
                }
                this.cb_usbParam.Items.Clear();
                contextForm.dic_UsbDevicePath_Name.Clear();
                listUsbDevicePath = RFIDReader.GetUsbHidDeviceList();
                for (int i = 0; i < listUsbDevicePath.Count; i++)
                {
                    string path = listUsbDevicePath[i];
                    contextForm.dic_UsbDevicePath_Name.Add(path, "UHF READER " + (i + 1));
                    string name = contextForm.dic_UsbDevicePath_Name[path].ToString();
                    cb_usbParam.Items.Add(name);
                }
            }
            catch { }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            bool isConnect = false;                                 
            if (cb_ConnectType.SelectedIndex == 0)
            {
                if (cb_ComNum.SelectedIndex >= 0 && cb_BPS.SelectedIndex >= 0)
                {
                    this.tb_ConnParam.Text = cb_ComNum.Text + ":" + cb_BPS.Text;
                    isConnect = RFIDReader.CreateSerialConn(this.tb_ConnParam.Text, contextForm);
                }
                // Helper.MyXmlHelper.UpdateInnerText(XMLFIENAME, "Root/AddConnect", "SerialConnect", tb_ConnParam.Text.Trim());
            }
            else if (cb_ConnectType.SelectedIndex == 1)
            {
                isConnect = RFIDReader.CreateTcpConn(tb_ConnParam.Text.Trim(), contextForm);
                Helper.MyXmlHelper.UpdateInnerText(XMLFIENAME, "Root/AddConnect", "TcpConnect", tb_ConnParam.Text.Trim());
            }
            else if (cb_ConnectType.SelectedIndex == 2)
            {
                if (cb_ComNum.SelectedIndex >= 0 && cb_BPS.SelectedIndex >= 0 && !String.IsNullOrEmpty(tb_485Address.Text))
                {
                    this.tb_ConnParam.Text = tb_485Address.Text.Trim() + ":" + cb_ComNum.Text + ":" + cb_BPS.Text;
                    isConnect = RFIDReader.Create485Conn(this.tb_ConnParam.Text, contextForm);
                }
                // Helper.MyXmlHelper.UpdateInnerText(XMLFIENAME, "Root/AddConnect", "_485Connect", tb_ConnParam.Text.Trim());
            }
            else if (cb_ConnectType.SelectedIndex == 3)
            {
                if (cb_usbParam.SelectedIndex >= 0)
                {
                    int deviceIndex = cb_usbParam.SelectedIndex;
                    //tb_ConnParam.Text = cb_usbParam.SelectedItem.ToString();
                    tb_ConnParam.Text = listUsbDevicePath[deviceIndex];
                    isConnect = RFIDReader.CreateUsbConn(listUsbDevicePath[deviceIndex], Handle, contextForm);
                }
            }
            if (isConnect)                                          
            {
                if (!RFIDReader.CheckConnect(tb_ConnParam.Text.Trim()))
                {
                    if (RFIDReader.DIC_CONNECT.ContainsKey(this.tb_ConnParam.Text))
                    {
                        RFIDReader.CloseConn(this.tb_ConnParam.Text);   
                    }
                    this.DialogResult = DialogResult.No;
                    return;
                }
                if (contextForm.IsMultiConnect == false)            
                {
                    contextForm.CloseNowConnect();
                }
                else
                {
                    if (!String.IsNullOrEmpty(contextForm.ConnID))
                    {
                        RFIDReaderAPI.RFIDReader.DIC_CONNECT[contextForm.ConnID].log = null;
                    }
                }
                contextForm.ConnID = tb_ConnParam.Text.Trim();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.No;
            }
        }

        private void cb_ConnectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_ConnectType.SelectedIndex == 0)
            {
                tb_ConnParam.Text = Helper.MyXmlHelper.ReadInnerText(XMLFIENAME, "Root/AddConnect", "SerialConnect");
                if (String.IsNullOrEmpty(tb_ConnParam.Text.Trim()))
                {
                    tb_ConnParam.Text = "COM1:115200";
                }
                tb_485Address.Visible = false;
                cb_BPS.Visible = true;
                cb_ComNum.Visible = true;
                cb_usbParam.Visible = false;
            }
            else if (cb_ConnectType.SelectedIndex == 1)
            {
                tb_ConnParam.Text = Helper.MyXmlHelper.ReadInnerText(XMLFIENAME, "Root/AddConnect", "TcpConnect");
                if (String.IsNullOrEmpty(tb_ConnParam.Text.Trim()))
                {
                    tb_ConnParam.Text = "192.168.1.116:9090";
                }
                tb_485Address.Visible = false;
                cb_BPS.Visible = false;
                cb_ComNum.Visible = false;
                tb_ConnParam.Visible = true;
                cb_usbParam.Visible = false;
            }
            else if (cb_ConnectType.SelectedIndex == 2)
            {
                tb_ConnParam.Text = Helper.MyXmlHelper.ReadInnerText(XMLFIENAME, "Root/AddConnect", "_485Connect");
                if (String.IsNullOrEmpty(tb_ConnParam.Text.Trim()))
                {
                    tb_ConnParam.Text = "1:COM1:115200";
                }
                tb_485Address.Visible = true;
                cb_BPS.Visible = true;
                cb_ComNum.Visible = true;
                cb_usbParam.Visible = false;
            }
            else if (cb_ConnectType.SelectedIndex == 3)
            {
                tb_ConnParam.Text = Helper.MyXmlHelper.ReadInnerText(XMLFIENAME, "Root/AddConnect", "UsbConnect");
                if (String.IsNullOrEmpty(tb_ConnParam.Text.Trim()))
                {
                    tb_ConnParam.Text = "1:COM1:115200";
                }
                tb_485Address.Visible = false;
                cb_BPS.Visible = false;
                cb_ComNum.Visible = false;
                tb_ConnParam.Visible = false;
                cb_usbParam.Visible = true;
            }
        }

        private void cb_ComNum_DropDown(object sender, EventArgs e)
        {
            InitCom();
        }

        private void cb_usbParam_DropDown(object sender, EventArgs e)
        {
            InitCom();
        }
    }
}
