using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;

namespace ALMACENRFID
{
    public class _RC
    {
        /// <summary>
        /// Set current culture by name
        /// </summary>
        /// <param name="name">name</param>
        public static void SetCurrentCulture(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = "en-US";
                }

                Thread.CurrentThread.CurrentCulture = new CultureInfo(name);
            }
            catch { }
        }

        /// <summary>
        /// Get string by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>current language string</returns>
        public static string GetString(string id)
        {
            string strCurLanguage = "";

            try
            {
                ResourceManager rm = new ResourceManager("SimpleReader.Resource.Resource", Assembly.GetExecutingAssembly());
                CultureInfo ci = Thread.CurrentThread.CurrentCulture;
                strCurLanguage = rm.GetString(id, ci);
            }
            catch
            {
                strCurLanguage = "No id:" + id + ", please add.";
            }

            return strCurLanguage;
        }

    }
}
