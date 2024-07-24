using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace ALMACENRFID.MyFormTemplet
{
    public class RoundButton : Button
    {
        #region variable

        private Int32 _Radius = 15;

        #endregion

        #region Attributes
        [Description("Circular radius"), Category("behavior")]
        public Int32 Radius
        {
            get { return _Radius; }
            set { _Radius = value; }
        }
        
        #endregion
        public RoundButton() 
        {
            GraphicsPath myPath = new GraphicsPath();
            myPath.AddEllipse(5, 5, _Radius, _Radius);
            this.Region = new Region(myPath);
        }
    }
}
