using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALMACENRFID.Helper
{
    public static class LanguageHelper
    {
        public static int LanguageType = 1;         // 0 Chinese     1 English
        static bool isIntit = false;

        public static Dictionary<int, string> WriteResultCode = new Dictionary<int, string>() 
        {
            {0,"0|OK"},
            {1,"1|天线端口参数错误"},        // Antenna port parameter error
            {2,"2|选择参数错误"},            // Wrong selection parameter
            {3,"3|写入参数错误"},            // Write parameter error
            {4,"4|CRC校验错误"},             // CRC checksum error
            {5,"5|功率不足"},        // Insufficient power
            {6,"6|数据区溢出"},       // Data area overflow
            {7,"7|数据区被锁定"},       // Data area is locked
            {8,"8|访问密码错误"},       // Access password error
            {9,"9|其他标签错误"},       // Other tag errors
            {10,"10|标签丢失"},       // tag is missing
            {11,"11|读写器发送指令错误"}       // The reader sends a command error
        };
       
        /// <summary>
        /// Return result dictionary
        /// </summary>
        public static Dictionary<int, string> ReadResultCode = new Dictionary<int, string>() 
        {
            {0,"0|OK"},
            {1,"1|天线端口参数错误"},        // Antenna port parameter error
            {2,"2|选择读取参数错误"},        // Wrong selection parameter
            {3,"3|TID读取参数错误"},        // TID read parameter error
            {4,"4|用户数据区读取参数错误"},        // User data area read parameter error
            {5,"5|保留区读取参数错误"},        // Error reading parameter in reserved area
            {6,"6|其他参数错误"}             // failure
        };


        static void Intit() {
            if (LanguageType == 1)
            {
                for (int i = 1; i < WriteResultCode.Keys.Count; i++)
                {
                    try
                    {
                        WriteResultCode[i] = ALMACENRFID.Properties.Resources.ResourceManager.GetString("Frame_0010_11_" + i);
                    }
                    catch { }
                }
                for (int i = 1; i < ReadResultCode.Keys.Count; i++)
                {
                    try
                    {
                        ReadResultCode[i] = ALMACENRFID.Properties.Resources.ResourceManager.GetString("Frame_0010_10_" + i);
                    }
                    catch { }
                }
            }
            isIntit = true;
        }
        public static string GetResutlCode(int resultType, int key) {
            if (!isIntit)
            {
                Intit();
            }
            string result = "";
            if (key < 0 ) return "-2|TimeOut"; 
            if (resultType == 0) //read
            {
                result = ReadResultCode[key];
            }
            else // write
            {
                result = WriteResultCode[key];
            }
            return result;
        }
    }
}
