using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;

using ALMACENRFID.Helper;
using ALMACENRFID.Forms;
using ALMACENRFID;

namespace ALMACENRFID.MyFormTemplet
{
    /// <summary>
    /// Button
    /// </summary>
    [DefaultEvent("Click")]
    [DefaultProperty("Text")]
    [ToolboxBitmap(typeof(Button))]
    public class QQButton : Button
    {
        #region variable
        /// <summary>
        /// Mouse status
        /// </summary>
        private EMouseState _mouseState = EMouseState.Normal;
        /// <summary>
        /// Text alignment
        /// </summary>
        private TextFormatFlags _textAlign = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
        /// <summary>
        /// Button image by default
        /// </summary>
        private Image _normalImage = null;
        /// <summary>
        /// Picture when the mouse is pressed
        /// </summary>
        private Image _downImage = null;
        /// <summary>
        /// Mouse over the picture
        /// </summary>
        private Image _moveImage = null;
        /// <summary>
        /// Did you press the mouse?
        /// </summary>
        private bool _isShowBorder = true;

        #endregion

        #region Constructor
        
        public QQButton()
        {
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.DoubleBuffer |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.Opaque, false);
            base.BackColor = Color.Transparent;
            this.UpdateStyles();
        }

        #endregion

        #region Attributes
        /// <summary>
        /// Default size
        /// </summary>
        protected override Size DefaultSize
        {
            get { return new Size(75, 28); }
        }
        /// <summary>
        /// Default image
        /// </summary>
        [Description("Default image")]
        public virtual Image NormalImage
        {
            get
            {
                if (this._normalImage == null)
                    this._normalImage = AssemblyHelper.GetImage("\\QQ\\Button\\normal.png");
                return this._normalImage;
            }
            set
            {
                this._normalImage = value;
                base.Invalidate();
            }
        }
        /// <summary>
        /// Picture when the mouse is pressed
        /// </summary>
        [Description("Picture when the mouse is pressed")]
        public virtual Image DownImage
        {
            get
            {
                if (this._downImage == null)
                    this._downImage = AssemblyHelper.GetImage("\\QQ\\Button\\down.png");
                return this._downImage;
            }
            set
            {
                this._downImage = value;
                base.Invalidate();
            }
        }
        /// <summary>
        /// Mouse over the picture
        /// </summary>
        [Description("Mouse over the picture")]
        public virtual Image MoveImage
        {
            get
            {
                if (this._moveImage == null)
                    this._moveImage = AssemblyHelper.GetImage("\\QQ\\Button\\highlight.png");
                return this._moveImage;
            }
            set
            {
                this._moveImage = value;

                base.Invalidate();
            }
        }
        /// <summary>
        /// Whether to display a lighted border
        /// </summary>
        [Description("Whether to display a lighted border")]
        public virtual bool IsShowBorder
        {
            get { return this._isShowBorder; }
            set { this._isShowBorder = value; }
        }
        /// <summary>
        /// Text related to the control
        /// </summary>
        [DefaultValue("QQButton")]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                base.Invalidate(this.TextRect);
            }
        }
        /// <summary>
        /// Picture displayed on the button
        /// </summary>
        [Description("Picture displayed on the button")]
        public virtual new Image Image
        {
            get { return base.Image; }
            set
            {
                base.Image = value;
                base.Invalidate();
            }
        }

        /// <summary>
        /// Alignment of text on the button
        /// </summary>
        [Description("Alignment of text on the button")]
        new public ContentAlignment TextAlign
        {
            get { return base.TextAlign; }
            set
            {
                base.TextAlign = value;
                switch (base.TextAlign)
                {
                    case ContentAlignment.BottomCenter:
                        this._textAlign = TextFormatFlags.Bottom | 
                                          TextFormatFlags.HorizontalCenter | 
                                          TextFormatFlags.SingleLine;
                        break;
                    case ContentAlignment.BottomLeft:
                        this._textAlign = TextFormatFlags.Bottom | 
                                          TextFormatFlags.Left | 
                                          TextFormatFlags.SingleLine;
                        break;
                    case ContentAlignment.BottomRight:
                        this._textAlign = TextFormatFlags.Bottom | 
                                          TextFormatFlags.Right | 
                                          TextFormatFlags.SingleLine;
                        break;
                    case ContentAlignment.MiddleCenter:
                        this._textAlign = TextFormatFlags.SingleLine | 
                                          TextFormatFlags.HorizontalCenter | 
                                          TextFormatFlags.VerticalCenter;
                        break;
                    case ContentAlignment.MiddleLeft:
                        this._textAlign = TextFormatFlags.Left | 
                                          TextFormatFlags.VerticalCenter | 
                                          TextFormatFlags.SingleLine;
                        break;
                    case ContentAlignment.MiddleRight:
                        this._textAlign = TextFormatFlags.Right | 
                                          TextFormatFlags.VerticalCenter | 
                                          TextFormatFlags.SingleLine;
                        break;
                    case ContentAlignment.TopCenter:
                        this._textAlign = TextFormatFlags.Top | 
                                          TextFormatFlags.HorizontalCenter | 
                                          TextFormatFlags.SingleLine;
                        break;
                    case ContentAlignment.TopLeft:
                        this._textAlign = TextFormatFlags.Top | 
                                          TextFormatFlags.Left | 
                                          TextFormatFlags.SingleLine;
                        break;
                    case ContentAlignment.TopRight:
                        this._textAlign = TextFormatFlags.Top | 
                                          TextFormatFlags.Right | 
                                          TextFormatFlags.SingleLine;
                        break;
                }
                base.Invalidate(this.TextRect);
            }
        }

        /// <summary>
        /// The entire button area
        /// </summary>
        internal Rectangle AllRect
        {
            get { return new Rectangle(0, 0, this.Width, this.Height); }
        }
        /// <summary>
        /// Text area
        /// </summary>
        internal Rectangle TextRect
        {
            get { return new Rectangle(2, 2, this.AllRect.Width - 4, this.AllRect.Height - 4); }
        }
        /// <summary>
        /// Mouse status
        /// </summary>
        internal EMouseState MouseState
        {
            get { return this._mouseState; }
            set
            {
                this._mouseState = value;
                base.Invalidate();
            }
        }

        #endregion

        #region Override method
        /// <summary>
        ///  System.Windows.Forms.Form.Paint 
        /// </summary>
        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            //g.DrawImage(AssemblyHelper.GetAssemblyImage("Button.all_btn_White-side.png"), this.AllRect);
            if (base.Enabled)
            {
                if (this.IsShowBorder)
                {
                    if (this.Focused)//得到焦点的时候，绘制边框
                    {
                        using (Image light = AssemblyHelper.GetImage("QQ.Button.Light.png"))
                        {
                            //g.DrawImage(light, this.AllRect);
                            DrawHelper.RendererBackground(g, this.AllRect, light, true);
                        }
                    }
                }

                switch (this.MouseState)
                {
                    case EMouseState.Leave:
                    case EMouseState.Normal:
                        if (base.Focused)
                        {
                            if (this.IsShowBorder)
                            {
                                using (Image focus = AssemblyHelper.GetImage("QQ.Button.focus.png"))
                                {
                                    DrawHelper.RendererBackground(g, this.TextRect, focus, true);
                                }
                            }
                            else
                            {
                                DrawHelper.RendererBackground(g, this.TextRect, this.NormalImage, true);
                            }
                        }
                        else
                        {
                            DrawHelper.RendererBackground(g, this.TextRect, this.NormalImage, true);
                        }
                        break;
                    case EMouseState.Up:
                    case EMouseState.Move:
                        DrawHelper.RendererBackground(g, this.TextRect, this.MoveImage, true);
                        break;
                    case EMouseState.Down:
                        DrawHelper.RendererBackground(g, this.TextRect, this.DownImage, true);
                        break;
                }

                
                TextRenderer.DrawText(g, this.Text, this.Font, this.TextRect, this.ForeColor, this._textAlign);
                
            }
            else
            {
                using (Image gray = AssemblyHelper.GetImage("QQ.Button.gray.png"))
                {
                    DrawHelper.RendererBackground(g, this.TextRect, gray, true);
                }
                TextRenderer.DrawText(g, this.Text, this.Font, this.TextRect, Color.Gray, this._textAlign);
            }
        }
        /// <summary>
        ///  System.Windows.Forms.Form.MouseEnter 
        /// </summary>
        /// <param name="e"> System.EventArgs。</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.MouseState = EMouseState.Move;
        }
        /// <summary>
        ///  System.Windows.Forms.Form.MouseLeave 
        /// </summary>
        /// <param name="e"> System.EventArgs</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.MouseState = EMouseState.Leave;
        }
        /// <summary>
        ///  System.Windows.Forms.Form.MouseDown 
        /// </summary>
        /// <param name="mevent"> System.Windows.Forms.MouseEventArgs</param>
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            this.MouseState = EMouseState.Down;
        }
        /// <summary>
        ///  System.Windows.Forms.Form.MouseUp 
        /// </summary>
        /// <param name="mevent"> System.Windows.Forms.MouseEventArgs</param>
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            this.MouseState = EMouseState.Up;
        }
        #endregion
    }
}
