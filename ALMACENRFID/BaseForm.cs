
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using ALMACENRFID;
using ALMACENRFID.Forms;
using ALMACENRFID.Win32;

namespace ALMACENRFID
{
    public partial class BaseForm : Form
    {
        protected static string XMLFIENAME = "ReaderConfig.xml";

        #region variable
        /// <summary>
        /// System button
        /// </summary>
        protected Forms.ESysButton _sysButton = Forms.ESysButton.Normal;
        /// <summary>
        /// The mouse state of the close button
        /// </summary>
        protected EMouseState _closeState = EMouseState.Normal;
        /// <summary>
        /// The mouse state of the maximize button
        /// </summary>
        protected EMouseState _maxState = EMouseState.Normal;
        /// <summary>
        /// The mouse state of the minimize button
        /// </summary>
        protected EMouseState _minState = EMouseState.Normal;
        /// <summary>
        /// Record form size
        /// </summary>
        protected Size _formSize = Size.Empty;
        /// <summary>
        /// Record form position
        /// </summary>
        protected Point _formPoint = Point.Empty;
        /// <summary>
        /// Specify how the form window is displayed
        /// </summary>
        protected FormWindowState _windowState = FormWindowState.Normal;
        /// <summary>
        /// Whether to allow the window size to be changed
        /// </summary>
        protected bool _isResize = true;
        /// <summary>
        /// Whether to show icon
        /// </summary>
        protected bool _showIcon = true;
        /// <summary>
        /// Is it transparent
        /// </summary>
        private bool _isTransfer = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize a new instance of the Paway.Windows.Forms.FormBase class.
        /// </summary>
        public BaseForm()
        {
            this.SetStyle(
               ControlStyles.AllPaintingInWmPaint |
               ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw |
               ControlStyles.Selectable |
               ControlStyles.ContainerControl |
               ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.Opaque, false);
            this.UpdateStyles();
            this.Initialize();
        }
        #endregion

        #region Attributes
        /// <summary>
        /// Whether to allow the window size to be changed
        /// </summary>
        [Description("Whether to allow the window size to be changed"), DefaultValue(true)]
        public virtual bool IsResize
        {
            get { return this._isResize; }
            set { _isResize = value; }
        }
        /// <summary>
        /// Specify how the form window is displayed
        /// </summary>
        [Description("Specify how the form window is displayed")]
        public virtual new FormWindowState WindowState
        {
            get { return this._windowState; }
            set
            {
                this._windowState = value;
                switch (this._windowState)
                {
                    case FormWindowState.Normal:
                        base.WindowState = FormWindowState.Normal;
                        break;
                    case FormWindowState.Minimized:
                        base.WindowState = FormWindowState.Minimized;
                        break;
                    case FormWindowState.Maximized:
                        this._formSize = this.Size;
                        this._formPoint = this.Location;
                        this.Location = new Point(0, 0);
                        this.Width = Screen.PrimaryScreen.WorkingArea.Width;
                        this.Height = Screen.PrimaryScreen.WorkingArea.Height;
                        break;
                }
            }
        }
        /// <summary>
        /// Minimum value of the window size
        /// </summary>
        public override Size MinimumSize
        {
            get { return new Size(140, 40); }
            set { base.MinimumSize = value; }
        }
        /// <summary>
        /// Transparent or not
        /// </summary>
        public bool IsTransfer
        {
            get { return this._isTransfer; }
            set { this._isTransfer = value; }
        }
        /// <summary>
        /// Encapsulate the information needed when creating the control.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                if (!this._isTransfer)
                    return base.CreateParams;
                else
                {
                    CreateParams param = base.CreateParams;
                    param.ExStyle = 0x00080000;
                    return param;
                }
            }
        }
        /// <summary>
        /// Whether to show icon
        /// </summary>
        public virtual new bool ShowIcon
        {
            get { return this._showIcon; }
            set
            {
                this._showIcon = value;
                base.Invalidate(this.TitleBarRect);
            }
        }
        /// <summary>
        /// Form title text
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                base.Invalidate(this.TitleBarRect);
            }
        }

        /// <summary>
        /// System control buttons
        /// </summary>
        [Description("Display and hide of system control buttons")]
        public ESysButton SysButton
        {
            get { return this._sysButton; }
            set
            {
                this._sysButton = value;
                base.Invalidate(this.TitleBarRect);
            }
        }
        /// <summary>
        /// System control button area
        /// </summary>
        protected virtual Rectangle SysBtnRect
        {
            get { return Rectangle.Empty; }
        }
        /// <summary>
        /// Title bar area
        /// </summary>
        protected virtual Rectangle TitleBarRect
        {
            get { return new Rectangle(0, 0, this.Width, 30); }
        }
        /// <summary>
        /// Close button area
        /// </summary>
        protected virtual Rectangle CloseRect
        {
            get { return Rectangle.Empty; }
        }
        /// <summary>
        /// Minimize button area
        /// </summary>
        protected virtual Rectangle MiniRect
        {
            get { return Rectangle.Empty; }
        }
        /// <summary>
        /// Maximise button area
        /// </summary>
        protected virtual Rectangle MaxRect
        {
            get { return Rectangle.Empty; }
        }
        /// <summary>
        /// Icon display area
        /// </summary>
        protected virtual Rectangle IconRect
        {
            get { return new Rectangle(4, 4, 16, 16); }
        }
        /// <summary>
        /// Title text display area
        /// </summary>
        protected virtual Rectangle TextRect
        {
            get
            {
                int width = this.TitleBarRect.Width - this.IconRect.Width - 15;
                int height = this.TitleBarRect.Height - 10;
                Rectangle textRect = new Rectangle(8, 2, width, height);
                if (this.ShowIcon)
                    textRect.X = this.IconRect.Width + 8;
                return textRect;
            }
        }
        /// <summary>
        /// The current mouse state of the close button
        /// </summary>
        protected EMouseState CloseState
        {
            get { return this._closeState; }
            set
            {
                this._closeState = value;
                base.Invalidate(this.CloseRect);
            }
        }
        /// <summary>
        /// The current mouse state of the maximize button
        /// </summary>
        protected EMouseState MaxState
        {
            get { return this._maxState; }
            set
            {
                this._maxState = value;
                base.Invalidate(this.MaxRect);
            }
        }
        /// <summary>
        /// The current mouse state of the minimize button
        /// </summary>
        protected EMouseState MinState
        {
            get { return this._minState; }
            set
            {
                this._minState = value;
                base.Invalidate(this.MiniRect);
            }
        }
        #endregion

        #region method

        /// <summary>
        /// Initialization window
        /// </summary>
        private void Initialize()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
        }

        /// <summary>
        /// Drag window size
        /// </summary>
        /// <param name="m"></param>
        private void WmNcHitTest(ref Message m)
        {
            if (this.WindowState != FormWindowState.Maximized)
            {
                int wparam = m.LParam.ToInt32();

                Point point = new Point(
                    NativeMethods.LOWORD(wparam),
                    NativeMethods.HIWORD(wparam));

                point = PointToClient(point);
                if (_isResize)
                {
                    if (point.X <= 3)
                    {
                        if (point.Y <= 3)
                            m.Result = (IntPtr)WM_NCHITTEST.HTTOPLEFT;
                        else if (point.Y > Height - 3)
                            m.Result = (IntPtr)WM_NCHITTEST.HTBOTTOMLEFT;
                        else
                            m.Result = (IntPtr)WM_NCHITTEST.HTLEFT;
                    }
                    else if (point.X >= Width - 3)
                    {
                        if (point.Y <= 3)
                            m.Result = (IntPtr)WM_NCHITTEST.HTTOPRIGHT;
                        else if (point.Y >= Height - 3)
                            m.Result = (IntPtr)WM_NCHITTEST.HTBOTTOMRIGHT;
                        else
                            m.Result = (IntPtr)WM_NCHITTEST.HTRIGHT;
                    }
                    else if (point.Y <= 3)
                    {
                        m.Result = (IntPtr)WM_NCHITTEST.HTTOP;
                    }
                    else if (point.Y >= Height - 3)
                    {
                        m.Result = (IntPtr)WM_NCHITTEST.HTBOTTOM;
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                }
                else
                {
                    base.WndProc(ref m);
                }
            }
        }

        /// <summary>
        /// Set the image as a form, the transparency area is determined by the opacity value
        /// </summary>
        /// <param name="bitmap">Transparent bitmap</param>
        /// <param name="opacity">Transparency value 0~255</param>
        public void SetBitmap(Bitmap bitmap, byte opacity)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

            // The ideia of this is very simple,
            // 1. Create a compatible DC with screen;
            // 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
            // 3. Call the UpdateLayeredWindow.

            IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
            IntPtr memDc = NativeMethods.CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
                oldBitmap = NativeMethods.SelectObject(memDc, hBitmap);

                SIZE size = new SIZE(bitmap.Width, bitmap.Height);
                POINT pointSource = new POINT(0, 0);
                POINT topPos = new POINT(Left, Top);
                BLENDFUNCTION blend = new BLENDFUNCTION();
                blend.BlendOp = Consts.AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = opacity;
                blend.AlphaFormat = Consts.AC_SRC_ALPHA;

                NativeMethods.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Consts.ULW_ALPHA);
            }
            finally
            {
                NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    NativeMethods.SelectObject(memDc, oldBitmap);
                    //Windows.DeleteObject(hBitmap); // The documentation says that we have to use the Windows.DeleteObject... but since there is no such method I use the normal DeleteObject from Win32 GDI and it's working fine without any resource leak.
                    NativeMethods.DeleteObject(hBitmap);
                }
                NativeMethods.DeleteDC(memDc);
            }
        }

        #endregion

        #region Override Methods
        /// <summary>
        /// Raises the System.Windows.Forms.Form.Paint event.
        /// </summary>
        /// <param name="e">System.Windows.Forms.PaintEventArgs containing event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            if (this.ShowIcon)//Painting icon
            {
                Bitmap iconImage = this.Icon.ToBitmap();
                g.DrawImage(iconImage, this.IconRect);
            }
            //Draw title text
            if (!string.IsNullOrEmpty(this.Text))
            {
                TextRenderer.DrawText(g, this.Text, new Font("宋体", 9f, FontStyle.Bold), this.TextRect, this.ForeColor, TextFormatFlags.VerticalCenter);
            }
        }
        /// <summary>
        /// Raises the System.Windows.Forms.Form.Load event.
        /// </summary>
        /// <param name="e">System.EventArgs containing event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.DesignMode)
            {
                switch (this.StartPosition)
                {
                    case FormStartPosition.CenterParent:
                        this.Location = new Point(
                           (this.Parent.Width - this.Width) / 2,
                           (this.Parent.Height - this.Height) / 2);
                        break;
                    case FormStartPosition.CenterScreen:
                        this.Location = new Point(
                            (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2,
                            (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);
                        break;
                    case FormStartPosition.Manual:
                    case FormStartPosition.WindowsDefaultBounds:
                    case FormStartPosition.WindowsDefaultLocation:
                        break;
                }
            }
        }
        /// <summary>
        /// Raises System.Windows.Forms.Form.SizeChanged.
        /// </summary>
        /// <param name="e">System.EventArgs containing event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            base.Padding = new Padding(3, 26, 3, 3);
        }
        /// <summary>
        /// Process Windows messages.
        /// </summary>
        /// <param name="m">The WindowsMessage to be processed.</param>
        protected override void WndProc(ref Message m)
        {
            try
            {
                switch (m.Msg)
                {
                    case (int)WindowsMessage.WM_NCPAINT:
                    case (int)WindowsMessage.WM_NCCALCSIZE:
                        break;
                    case (int)WindowsMessage.WM_NCACTIVATE:
                        if (m.WParam == (IntPtr)0)
                        {
                            m.Result = (IntPtr)1;
                        }
                        break;
                    case (int)WindowsMessage.WM_NCHITTEST:
                        base.WndProc(ref m);
                        this.WmNcHitTest(ref m);
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            catch { }
        }
        /// <summary>
        /// Raises System.Windows.Forms.Form.MouseDown.
        /// </summary>
        /// <param name="e">System.Windows.Forms.MouseEventArgs containing event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Point point = e.Location;
            if (e.Button == MouseButtons.Left)
            {
                if (this.CloseRect.Contains(point))
                    this.CloseState = EMouseState.Down;
                else if (this.MiniRect.Contains(point))
                    this.MinState = EMouseState.Down;
                else if (this.MaxRect.Contains(point))
                    this.MaxState = EMouseState.Down;
            }
            if (this.WindowState != FormWindowState.Maximized)
            {
                if (e.Button == MouseButtons.Left && !this.SysBtnRect.Contains(e.Location))
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 274, 61440 + 9, 0);
                }
            }
        }
        /// <summary>
        /// Raises System.Windows.Forms.Form.MouseMove.
        /// </summary>
        /// <param name="e">System.Windows.Forms.MouseEventArgs containing event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
                return;
            Point point = e.Location;
            if (this.CloseRect.Contains(point))
                this.CloseState = EMouseState.Move;
            else
                this.CloseState = EMouseState.Normal;
            if (this.MiniRect.Contains(point))
                this.MinState = EMouseState.Move;
            else
                this.MinState = EMouseState.Normal;
            if (this.SysButton == ESysButton.Normal)
            {
                if (this.MaxRect.Contains(point))
                    this.MaxState = EMouseState.Move;
                else
                    this.MaxState = EMouseState.Normal;
            }
        }
        /// <summary>
        /// Raises System.Windows.Forms.Form.MouseLeave.
        /// </summary>
        /// <param name="e">System.EventArgs containing event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.CloseState = EMouseState.Normal;
            this.MaxState = EMouseState.Normal;
            this.MinState = EMouseState.Normal;
        }
        /// <summary>
        /// Raises System.Windows.Forms.Form.MouseUp.
        /// </summary>
        /// <param name="e">System.Windows.Forms.MouseEventArgs containing event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left)
                return;
            Point point = e.Location;
            if (this.CloseRect.Contains(point))
            {
                this.CloseState = EMouseState.Move;
                this.Close();
            }
            else
            {
                this.CloseState = EMouseState.Normal;
            }
            if (this.MiniRect.Contains(point))
            {
                this.MinState = EMouseState.Move;
                this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                this.MinState = EMouseState.Normal;
            }
            if (this.MaxRect.Contains(point))
            {
                this.MaxState = EMouseState.Move;
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.Size = this._formSize;
                    this.Location = this._formPoint;
                    this.WindowState = FormWindowState.Normal;
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }
            else
            {
                this.MaxState = EMouseState.Normal;
            }
        }

        #endregion

        private void BaseForm_Load(object sender, EventArgs e)
        {
            Rectangle clientRectangle = ClientRectangle;
            Point clientPoint = PointToScreen(new Point(0, 0));
            clientRectangle.Offset(clientPoint.X - Left, clientPoint.Y - Top);
            Region = new Region(clientRectangle);
        }

        #region General auxiliary method
        /// <summary>
        /// Show alert message
        /// </summary>
        /// <param name="msg"></param>
        protected void ShowMessage(String msg)
        {
            MessageBox.Show(msg, _RC.GetString("Main_Tip"), MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        /// <summary>
        /// Show error message
        /// </summary>
        /// <param name="msg"></param>
        protected void ShowError(String msg)
        {
            MessageBox.Show(msg, _RC.GetString("Main_Tip"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        /// <summary>
        /// Show question message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected DialogResult ShowQuestion(String msg)
        {
            return MessageBox.Show(msg, _RC.GetString("Main_Tip"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        }
        /// <summary>
        /// Obtain a hexadecimal string (Byte)
        /// </summary>
        protected String GetHexStringByByte(byte iParam)
        {
            String rt = "";
            rt = RFIDReaderAPI.MyHelper.MyString.ByteToString(iParam);
            return rt;
        }
        /// <summary>
        /// Get a hexadecimal string (Uint16)
        /// </summary>
        protected String GetHexStringByUInt16(UInt16 iParam)
        {
            String rt = "";
            rt = RFIDReaderAPI.MyHelper.MyString.ByteToString(GetReverseU16(iParam));
            return rt;
        }
        /// <summary>
        /// Get a hexadecimal string (Uint32)
        /// </summary>
        protected String GetHexStringByUInt32(UInt32 iParam)
        {
            String rt = "";
            rt = RFIDReaderAPI.MyHelper.MyString.ByteToString(GetReverseU32(iParam));
            return rt;
        }
        /// <summary>
        /// Get U16 in network byte order
        /// </summary>
        /// <param name="data">UInt16</param>
        /// <returns>2字节Byte</returns>
        protected byte[] GetReverseU16(ushort data)
        {
            byte[] rt = new byte[2];
            rt = BitConverter.GetBytes(data);
            Array.Reverse(rt);
            return rt;
        }
        /// <summary>
        /// Get U32 in network byte order
        /// </summary>
        /// <param name="data">UInt32</param>
        /// <returns>4 Bytes</returns>
        protected byte[] GetReverseU32(UInt32 data)
        {
            byte[] rt = new byte[4];
            rt = BitConverter.GetBytes(data);
            Array.Reverse(rt);
            return rt;
        }
        #endregion

    }
}
