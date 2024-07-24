using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALMACENRFID.Entity
{
    public class ReadParam
    {
        #region Matching read parameters
        /// <summary>
        /// Match type EPC||TID
        /// </summary>
        public int MatchType { get; set; }
        /// <summary>
        /// Match start index
        /// </summary>
        public int MatchIndex { get; set; }
        /// <summary>
        /// Match code
        /// </summary>
        public string MatchCode { get; set; } 
        #endregion

        #region TID parameters
        /// <summary>
        /// Read mode
        /// </summary>
        public int TIDReadType { get; set; }
        /// <summary>
        /// Read length
        /// </summary>
        public int TIDReadLen { get; set; }
        #endregion

        #region User area parameters
        /// <summary>
        /// initial address
        /// </summary>
        public int UserDataStartIndex { get; set; }
        /// <summary>
        /// Read length
        /// </summary>
        public int UserDataLen { get; set; }
        #endregion
        #region User area parameters
        /// <summary>
        /// initial address
        /// </summary>
        public int ReserveStartIndex { get; set; }
        /// <summary>
        /// Read length
        /// </summary>
        public int ReserveDataLen { get; set; }
        #endregion
        #region EPCData parameters
          /// <summary>
        /// initial address
        /// </summary>
        public int EPCDataStartIndex { get; set; }
        /// <summary>
        /// Read length
        /// </summary>
        public int EPCDataLen { get; set; }
        #endregion
        #region access password
        public string pwd { get; set; }
        #endregion
    }
}
