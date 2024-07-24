using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ALMACENRFID.Helper;
using ALMACENRFID.Forms;
using ALMACENRFID.Win32;
using ALMACENRFID;

namespace ALMACENRFID.MyFormTemplet
{
    /// <summary>
    /// 
    /// </summary>
    public partial class QQTextBoxEx : UserControl
    {
        #region variable
        private QQTextBoxBase BaseText;

        private Image _borderImage = AssemblyHelper.GetImage("\\QQ\\TextBox\\normal.png");
        private Cursor _cursor = Cursors.IBeam;
        private EMouseState _mouseState = EMouseState.Normal;
        private EMouseState _iconMouseState = EMouseState.Normal;
        private bool _iconIsButton;
        private Image _icon;
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public QQTextBoxEx()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer, true);
            InitializeComponent();
            this.InitEvents();
            this.BackColor = Color.Transparent;
            this.UpdateStyles();
        }

        #endregion

        #region Custom event && method for firing events
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler IconClick;
        /// <summary>
        /// 
        /// </summary>
        private void OnIconClick()
        {
            if (this.IconClick != null)
                this.IconClick(this, EventArgs.Empty);
        }
        #endregion

        #region Attributes
        /// <summary>
        /// 
        /// </summary>
        [Description("Specifies the maximum number of characters that can be entered in the edit control"), Category("behavior")]
        public virtual int MaxLength
        {
            get { return this.BaseText.MaxLength; }
            set { this.BaseText.MaxLength = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Whether the text of the control edit control can span multiple lines."), Category("behavior")]
        public virtual bool Multiline
        {
            get { return this.BaseText.Multiline; }
            set { this.BaseText.Multiline = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Indicates the character that will be displayed for the password input of the single-line edit control."), Category("behavior")]
        public char IsPasswordChat
        {
            get { return this.BaseText.PasswordChar; }
            set { this.BaseText.PasswordChar = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Controls whether the text in the edit control can be changed."), Category("behavior")]
        public virtual bool ReadOnly
        {
            get { return this.BaseText.ReadOnly; }
            set { this.BaseText.ReadOnly = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Indicates whether the text in the edit control is displayed with the default password characters."), Category("behavior")]
        public virtual bool IsSystemPasswordChar
        {
            get { return this.BaseText.UseSystemPasswordChar; }
            set { this.BaseText.UseSystemPasswordChar = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Indicates whether the multi-line edit control wraps automatically"), Category("behavior")]
        public virtual bool WordWrap
        {
            get { return this.BaseText.WordWrap; }
            set { this.BaseText.WordWrap = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("The font used to display the text in the control."), Category("Exterior")]
        new public virtual Font Font
        {
            get { return this.BaseText.Font; }
            set { this.BaseText.Font = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("The foreground color of this component for displaying text."), Category("Exterior")]
        new public virtual Color ForeColor
        {
            get { return this.BaseText.ForeColor; }
            set { this.BaseText.ForeColor = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("A line of text in a multiline edit as an array of string values."), Category("Exterior")]
        public virtual string[] Lines
        {
            get { return this.BaseText.Lines; }
            set { this.BaseText.Lines = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Indicates which scroll bars will be displayed for this control for multi-line edit controls."), Category("Exterior")]
        public virtual ScrollBars ScrollBars
        {
            get { return this.BaseText.ScrollBars; }
            set { this.BaseText.ScrollBars = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Indicates how the text of the edit control should be aligned"), Category("Exterior")]
        public virtual HorizontalAlignment TextAlign
        {
            get { return this.BaseText.TextAlign; }
            set { this.BaseText.TextAlign = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Text box icon"), Category("Custom attribute")]
        public virtual Image Icon
        {
            get { return this._icon; }
            set
            {
                this._icon = value;
                base.Invalidate(this.IconRect);
                this.PositionTextBox();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Whether the icon of the text box is a button"), Category("Custom attribute")]
        public virtual bool IconIsButton
        {
            get { return this._iconIsButton; }
            set { this._iconIsButton = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Watermark text"), Category("Custom attribute")]
        public virtual string WaterText
        {
            get { return this.BaseText.WaterText; }
            set { this.BaseText.WaterText = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Watermark color"), Category("Custom attribute")]
        public virtual Color WaterColor
        {
            get { return this.BaseText.WaterColor; }
            set { this.BaseText.WaterColor = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Browsable(true)]
        public override string Text
        {
            get { return this.BaseText.Text; }
            set { this.BaseText.Text = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override Cursor Cursor
        {
            get { return this._cursor; }
            set { this._cursor = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override Size MinimumSize
        {
            get { return new Size(20, 24); }
            set { base.MinimumSize = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual EMouseState MouseState
        {
            get { return this._mouseState; }
            set
            {
                this._mouseState = value;
                base.Invalidate();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual EMouseState IconMouseState
        {
            get { return this._iconMouseState; }
            set
            {
                this._iconMouseState = value;
                base.Invalidate();
            }
        }
        /// <summary>
        /// Icon drawing area
        /// </summary>
        protected virtual Rectangle IconRect
        {
            get { return new Rectangle(3, 3, 20, 20); }
        }
        #endregion

        #region Method
        /// <summary>
        /// Load event
        /// </summary>
        private void InitEvents()
        {
            this.BaseText.MouseMove += new MouseEventHandler(BaseText_MouseMove);
            this.BaseText.MouseLeave += new EventHandler(BaseText_MouseLeave);
            this.BaseText.KeyDown += new KeyEventHandler(BaseText_KeyDown);
            this.BaseText.KeyPress += new KeyPressEventHandler(BaseText_KeyPress);
            this.BaseText.KeyUp += new KeyEventHandler(BaseText_KeyUp);
        }

        /// <summary>
        /// Design interface
        /// </summary>
        private void InitializeComponent()
        {
            this.BaseText = new MyFormTemplet.QQTextBoxBase();
            this.SuspendLayout();
            // 
            // BaseText
            // 
            this.BaseText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BaseText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BaseText.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BaseText.Location = new System.Drawing.Point(3, 4);
            this.BaseText.Margin = new System.Windows.Forms.Padding(0);
            this.BaseText.Name = "BaseText";
            this.BaseText.Size = new System.Drawing.Size(172, 18);
            this.BaseText.TabIndex = 0;
            this.BaseText.WaterColor = System.Drawing.Color.DarkGray;
            this.BaseText.WaterText = "";
            // 
            // TextBoxEx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.BaseText);
            this.Name = "TextBoxEx";
            this.Size = new System.Drawing.Size(178, 24);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        /// <summary>
        /// Offset text box
        /// </summary>
        protected virtual void PositionTextBox()
        {
            if (this._icon != null)
            {
                int position = 23;
                this.BaseText.Width -= position; 
                this.BaseText.Location = new Point(
                    this.BaseText.Location.X + position, 
                    this.BaseText.Location.Y);
            }
        }

        #endregion

        #region Event
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BaseText_MouseLeave(object sender, EventArgs e)
        {
            this.MouseState = EMouseState.Leave;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BaseText_MouseMove(object sender, MouseEventArgs e)
        {
            this.MouseState = EMouseState.Move;
        }

        private void BaseText_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseText_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseText_KeyDown(object sender, KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// When the size of the text box changes, replace the type of the text box with multiple lines of text.
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (this.Height > 26)
                this.BaseText.Multiline = true;
            else
                this.BaseText.Multiline = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            switch (this._mouseState)
            {
                case EMouseState.Move:
                    using (Image hotLine = AssemblyHelper.GetImage("\\QQ\\TextBox\\move.png"))
                    {
                        DrawHelper.RendererBackground(g, this.ClientRectangle, hotLine, true);
                    }
                    break;
                default:
                    DrawHelper.RendererBackground(g, this.ClientRectangle, this._borderImage, true);
                    break;
            }
            if (this._icon != null)
            {
                Rectangle iconRect = this.IconRect;
                if (this._iconMouseState == EMouseState.Down)
                {
                    iconRect.X += 1;
                    iconRect.Y += 1;
                }
                g.DrawImage(this._icon, iconRect, 0, 0, this._icon.Width, this._icon.Height, GraphicsUnit.Pixel);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.MouseState = EMouseState.Move;
            if (this._icon != null && this.IconRect.Contains(e.Location))
            {
                if (this._iconIsButton)
                    this.Cursor = Cursors.Hand;
                else
                    this.Cursor = Cursors.Default;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (this._icon != null && this._iconIsButton)
            {
                if (e.Button == MouseButtons.Left && this.IconRect.Contains(e.Location))
                {
                    this.IconMouseState = EMouseState.Down;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (this._icon != null && this._iconIsButton)
            {
                this.IconMouseState = EMouseState.Up;
                if (e.Button == MouseButtons.Left && this.IconRect.Contains(e.Location))
                    this.OnIconClick();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.MouseState = EMouseState.Leave;
            this.Cursor = Cursors.Default;
        }
        #endregion
    }
}
