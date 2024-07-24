using System;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace ALMACENRFID
{
    /// <summary>
    /// 对当前程序集的操作类
    /// </summary>
    public class AssemblyHelper
    {
        #region 常量
        /// <summary>
        /// 程序集的名称
        /// </summary>
        private static string CurrentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        #endregion

        #region 变量
        /// <summary>
        /// 当前程序集
        /// </summary>
        private static Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

        #endregion

        #region 方法
        /// <summary>
        /// 在嵌入的资源文件中查找相应的图片
        /// </summary>
        /// <param name="name">资源图片的文件名称+扩展名</param>
        /// <returns></returns>
        public static Image GetImage2(string name)
        {
            Image image = null;
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    StringBuilder sb = new StringBuilder();
                    if (name[0] != '.')
                        sb.Append(AssemblyHelper.CurrentAssemblyName + "." + name);
                    else
                        sb.Append(AssemblyHelper.CurrentAssemblyName + name);
                    using (Stream stream = CurrentAssembly.GetManifestResourceStream(sb.ToString()))
                    {
                        if (stream == null)
                            throw new Exception("No se pudo cargar el archivo de recursos, motivo del error: puede perderse" + CurrentAssemblyName + ".dll");
                        else
                            image = Image.FromStream(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AssemblyHelper.GetImage(string)->" + ex.Message);
                throw ex;
            }
            return image;
        }


        public static Image GetImage(string name)
        {

            string filePath= "C:\\Users\\AlxCN\\Desktop\\almacenrfid\\ALMACENRFID\\" + AssemblyHelper.CurrentAssemblyName + name;
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"No se pudo encontrar el archivo: {filePath}");
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetImage error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
