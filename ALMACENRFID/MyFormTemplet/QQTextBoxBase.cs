﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

using ALMACENRFID.Helper;
using ALMACENRFID.Forms;
using ALMACENRFID.Win32;

namespace ALMACENRFID.MyFormTemplet
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxBitmap(typeof(TextBox))]
    public class QQTextBoxBase : TextBox
    {
        #region variable
        /// <summary>
        /// Watermark text
        /// </summary>
        private string _waterText = string.Empty;
        /// <summary>
        /// Watermark text color
        /// </summary>
        private Color _waterColor = Color.DarkGray;
        #endregion

        #region Attributes
        /// <summary>
        /// 
        /// </summary>
        [Description("Watermark text"), Category("Custom attribute")]
        public string WaterText
        {
            get { return this._waterText; }
            set
            {
                this._waterText = value;
                base.Invalidate();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("Watermark text"), Category("Custom attribute")]
        public Color WaterColor
        {
            get {return this._waterColor;}
            set
            {
                this._waterColor = value;
                base.Invalidate();
            }
        }
        #endregion

        #region method
        /// <summary>
        /// Draw a watermark
        /// </summary>
        private void WmPaintWater(ref Message m)
        {
            using (Graphics g = Graphics.FromHwnd(base.Handle))
            {
                if (this.Text.Length == 0 &&
                    !string.IsNullOrEmpty(this._waterText) &&
                    !this.Focused)
                {
                    TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
                    if (this.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
                    {
                        flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                    }
                    TextRenderer.DrawText(g, this._waterText, new Font("宋体", 8.5f), this.ClientRectangle, this._waterColor, flags);
                }
            }
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == (int)WindowsMessage.WM_PAINT)
                this.WmPaintWater(ref m);//绘制水印
        }
        #endregion
    }
}
