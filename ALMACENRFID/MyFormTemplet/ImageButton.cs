using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ALMACENRFID.MyFormTemplet
{
    public partial class ImageButton : UserControl
    {
        public ImageButton()
        {
            this.InitializeComponent();
        }

        private void ImageButton_Load(object sender, EventArgs e)
        {
            this.pImage.Image = this.backgroundimage;

            this.lblTemp.Parent = this.pImage;
            this.lblTemp.Width = this.pImage.Width;
            this.lblTemp.Left = (int)Math.Round((double)((((double)this.pImage.Width) / 2.0) - (((double)this.lblTemp.Width) / 2.0)));
            this.lblTemp.Top = (int)Math.Round((double)((((double)this.pImage.Height) / 2.0) - (((double)this.lblTemp.Height) / 2.0)));
        }

        private void ImageButton_Resize(object sender, EventArgs e)
        {
            this.lblTemp.Left = (int)Math.Round((double)((((double)this.pImage.Width) / 2.0) - (((double)this.lblTemp.Width) / 2.0)));
            this.lblTemp.Top = (int)Math.Round((double)((((double)this.pImage.Height) / 2.0) - (((double)this.lblTemp.Height) / 2.0)));
        }

        #region attributes

        private Image backgroundimage;
        private Image mouseoverimage;
        private Image mouseclickimage;

        [Category("Important attribute"), Description("The background image displayed by the button by default")]
        public override Image BackgroundImage
        {
            get
            {
                return this.pImage.Image;
            }
            set
            {
                this.pImage.Image = value;
                this.backgroundimage = value;
            }
        }

        [Category("Important attribute"), Description("The text to display in the button")]
        public string ButtonText
        {
            get
            {
                return this.lblTemp.Text;
            }
            set
            {
                this.lblTemp.Text = value;
            }
        }

        [Category("Important attribute"), Description("Image displayed when the button is clicked")]
        public Image MouseClickImage
        {
            get
            {
                return this.mouseclickimage;
            }
            set
            {
                this.mouseclickimage = value;
            }
        }

        [Category("Important attribute"), Description("The image that appears when the mouse is over the button")]
        public Image MouseOverImage
        {
            get
            {
                return this.mouseoverimage;
            }
            set
            {
                this.mouseoverimage = value;
            }
        }

        #endregion

        #region event

        public delegate void ClickEventHandler(object sender, EventArgs e);
        public event ClickEventHandler Click;

        private void OnClick(object sender, EventArgs e)
        {
            ClickEventHandler clickEvent = this.Click;
            if (clickEvent != null)
            {
                clickEvent(this, e);
            }
        }

        #region Mouse event

        private void Mouse_Enter(object sender, EventArgs e)
        {
            //this.Cursor = Cursors.Hand;
            this.pImage.Image = this.mouseoverimage;
        }

        private void Mouse_Leave(object sender, EventArgs e)
        {
            //this.Cursor = Cursors.Arrow;
            this.pImage.Image = this.backgroundimage;
        }

        private void Mouse_Down(object sender, MouseEventArgs e)
        {
            //this.Cursor = Cursors.Hand;
            this.pImage.Image = this.mouseclickimage;
        }

        private void Mouse_Hover(object sender, EventArgs e)
        {
            this.pImage.Image = this.mouseoverimage;
        }

        private void Mouse_Up(object sender, MouseEventArgs e)
        {
            this.pImage.Image = this.backgroundimage;
        }

        #endregion

        private void lblTemp_TextChanged(object sender, EventArgs e)
        {
            this.lblTemp.Parent = this.pImage;
            this.lblTemp.Width = this.pImage.Width;
            this.lblTemp.Left = (int)Math.Round((double)((((double)this.pImage.Width) / 2.0) - (((double)this.lblTemp.Width) / 2.0)));
            this.lblTemp.Top = (int)Math.Round((double)((((double)this.pImage.Height) / 2.0) - (((double)this.lblTemp.Height) / 2.0)));
        }

        private void pbImage_BackgroundImageChanged(object sender, EventArgs e)
        {
            this.pImage.Refresh();
        }

        #endregion
    }

    //ImageButton.Designer.cs  
    partial class ImageButton
    {
        /// <summary>
        /// Required designer variable。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up all resources in use
        /// </summary>
        /// <param name="disposing">True if the managed resource should be released; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code generated by the component designer

        /// <summary>
        /// Component Designer supports the required methods - don't
        /// Use the code editor to modify the contents of this method。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.pImage = new PictureBox();
            this.lblTemp = new Label();
            ((ISupportInitialize)this.pImage).BeginInit();
            base.SuspendLayout();
            this.pImage.Dock = DockStyle.Fill;
            this.pImage.Location = new Point(0, 0);
            this.pImage.Name = "pbImage";
            this.pImage.Size = new Size(0x80, 40);
            this.pImage.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pImage.TabIndex = 0;
            this.pImage.TabStop = false;
            this.pImage.MouseLeave += new EventHandler(this.Mouse_Leave);
            this.pImage.Click += new EventHandler(this.OnClick);
            this.pImage.MouseDown += new MouseEventHandler(this.Mouse_Down);
            this.pImage.BackgroundImageChanged += new EventHandler(this.pbImage_BackgroundImageChanged);
            this.pImage.MouseHover += new EventHandler(this.Mouse_Hover);
            this.pImage.MouseUp += new MouseEventHandler(this.Mouse_Up);
            this.pImage.MouseEnter += new EventHandler(this.Mouse_Enter);
            this.lblTemp.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.lblTemp.AutoSize = true;
            this.lblTemp.BackColor = Color.Transparent;
            this.lblTemp.Location = new Point(13, 15);
            this.lblTemp.Name = "lblTemp";
            this.lblTemp.Size = new Size(0x65, 12);
            this.lblTemp.TabIndex = 1;
            this.lblTemp.Text = "Please enter the displayed text";
            this.lblTemp.TextAlign = ContentAlignment.MiddleCenter;
            this.lblTemp.MouseLeave += new EventHandler(this.Mouse_Leave);
            this.lblTemp.Click += new EventHandler(this.OnClick);
            this.lblTemp.MouseDown += new MouseEventHandler(this.Mouse_Down);
            this.lblTemp.MouseEnter += new EventHandler(this.Mouse_Enter);
            this.lblTemp.MouseHover += new EventHandler(this.Mouse_Hover);
            this.lblTemp.MouseUp += new MouseEventHandler(this.Mouse_Up);
            this.lblTemp.TextChanged += new EventHandler(this.lblTemp_TextChanged);
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.Controls.Add(this.lblTemp);
            base.Controls.Add(this.pImage);
            base.Name = "ImageButton";
            base.Size = new Size(0x80, 40);
            base.Load += new EventHandler(this.ImageButton_Load);
            base.Resize += new EventHandler(this.ImageButton_Resize);
            base.MouseEnter += new EventHandler(this.Mouse_Enter);
            base.MouseLeave += new EventHandler(this.Mouse_Leave);
            ((ISupportInitialize)this.pImage).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private Label lblTemp;
        private PictureBox pImage;

        #endregion
    }
}
