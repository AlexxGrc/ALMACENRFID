using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using ALMACENRFID.Helper;
using ALMACENRFID.Forms;
using ALMACENRFID.Win32;
using ALMACENRFID;

namespace ALMACENRFID.MyFormTemplet
{
    /// <summary>
    /// Theme settings applied to menu controls
    /// </summary>
    public class QQToolStripRenderer : ToolStripRenderer
    {
        #region Override Methods

        /// <summary>
        /// Draw menu background
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.AffectedBounds;
            g.SmoothingMode = SmoothingMode.HighQuality;

            int Rgn = NativeMethods.CreateRoundRectRgn(1, 1, rect.Width, rect.Height, 2, 2);
            NativeMethods.SetWindowRgn(e.ToolStrip.Handle, Rgn, true);

            Image bgk = AssemblyHelper.GetImage("\\QQ\\ContextMenu\\menu_bkg.png");
            Image board = AssemblyHelper.GetImage("\\QQ\\ContextMenu\\menu_bkg_board.png");

            g.DrawImage(bgk, new Rectangle(0, 0, 28, 5), new Rectangle(4, 4, 28, 5), GraphicsUnit.Pixel);
            g.DrawImage(bgk, new Rectangle(0, 5, 28, rect.Height - 10), new Rectangle(4, 8, bgk.Height - 2, 14), GraphicsUnit.Pixel);
            g.DrawImage(bgk, new Rectangle(0, rect.Height - 5, 28, 5), new Rectangle(4, bgk.Height - 9, 28, 5), GraphicsUnit.Pixel);
            
            g.DrawImage(board, new Rectangle(28, 0, rect.Width - 32, 5), new Rectangle(10, 4, board.Width - 35, 5), GraphicsUnit.Pixel);
            g.DrawImage(board, new Rectangle(rect.Width - 4, 0, 8, 5), new Rectangle(board.Width - 8, 4, 8, 5), GraphicsUnit.Pixel);
            g.DrawImage(board, new Rectangle(rect.Width - 4, 5, 8, rect.Height - 10), new Rectangle(board.Width - 8, 10, 8, 12), GraphicsUnit.Pixel);
            g.DrawImage(board, new Rectangle(rect.Width - 4, rect.Height - 5, 8, 5), new Rectangle(board.Width - 8, board.Height - 9, 8, 5), GraphicsUnit.Pixel);
            g.DrawImage(board, new Rectangle(28, rect.Height - 4, rect.Width - 32, 5), new Rectangle(10, board.Height - 8, board.Width - 35, 5), GraphicsUnit.Pixel);
            g.DrawImage(board, new Rectangle(28, 5, rect.Width - 32, rect.Height - 9), new Rectangle(10, 10, 32, 12), GraphicsUnit.Pixel);

            base.OnRenderToolStripBackground(e);

        }

        /// <summary>
        /// Draw an arrow on the ToolStripItem.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            Image arrow = AssemblyHelper.GetImage("\\QQ\\ContextMenu\\menu_arrow.png");
            Rectangle imgPoint = new Rectangle(
                e.ArrowRectangle.X + 4,
                (e.ArrowRectangle.Height - arrow.Height) / 2,
                arrow.Width,
                arrow.Height);
            Rectangle imgRect = new Rectangle(0, 0, arrow.Width, arrow.Height);
            g.DrawImage(arrow, imgPoint, imgRect, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Hide the drawing of the Border in the parent class
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }

        /// <summary>
        /// Mouse overdated state
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            ToolStripItem item = e.Item;
            ToolStrip toolstrip = e.ToolStrip;

            if (toolstrip is ToolStrip)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                if (item.Selected)
                {
                    item.ForeColor = Color.White;
                    Image select = AssemblyHelper.GetImage("\\QQ\\ContextMenu\\menu_highlight.png");
                    Rectangle rect = new Rectangle(
                        item.ContentRectangle.X + 1,
                        item.ContentRectangle.Y,
                        item.ContentRectangle.Width - 1,
                        item.ContentRectangle.Height);
                    Rectangle imgRect = new Rectangle(0, 0, select.Width - 1, select.Height);
                    g.DrawImage(select, rect, imgRect, GraphicsUnit.Pixel);
                }
                else
                {
                    item.ForeColor = Color.Black;
                }
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        /// <summary>
        /// Horizontal line
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (!e.Vertical)
            {
                Image cutling = AssemblyHelper.GetImage("\\QQ\\ContextMenu\\menu_cutling.png");
                Rectangle rect = new Rectangle(
                    e.Item.ContentRectangle.X + 25,
                    e.Item.ContentRectangle.Y,
                    e.Item.ContentRectangle.Width - 25,
                    cutling.Height);
                e.Graphics.DrawImage(cutling, rect);
            }
            else
            {
                base.OnRenderSeparator(e);
            }
        }

        /// <summary>
        /// Selected item
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Image check = AssemblyHelper.GetImage("\\QQ\\ContextMenu\\menu_check.png");
            e.Graphics.DrawImage(check, e.ImageRectangle);
        }

        /// <summary>
        /// Center the icon
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            //base.OnRenderItemImage(e);
            int offset = 28;

            Image icon = e.Image;
            Rectangle iconRect = e.ImageRectangle;
            Graphics g = e.Graphics;

            if (e.ToolStrip is ContextMenuStrip)
            {
                ContextMenuStrip contextMenuStrip = e.ToolStrip as ContextMenuStrip;
                iconRect.X = (offset - icon.Width) / 2;
                if (icon.Width < contextMenuStrip.ImageScalingSize.Width ||
                    icon.Height < contextMenuStrip.ImageScalingSize.Height)
                {
                    g.DrawImage(icon, iconRect);
                }
                else
                {
                    Rectangle iconPoint = new Rectangle(
                        iconRect.X,
                        2, 
                        contextMenuStrip.ImageScalingSize.Width + 3, 
                        contextMenuStrip.ImageScalingSize.Height + 2);

                    Rectangle imgRect = new Rectangle(0, 0, icon.Width, icon.Height);
                    g.DrawImage(icon, iconPoint, imgRect, GraphicsUnit.Pixel);
                }
            }
        }

        #endregion
    }
}
