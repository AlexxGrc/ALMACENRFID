using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace ALMACENRFID.Helper
{
    public class MyXmlHelper
    {
        public MyXmlHelper()
        {
        }

        public static String GetXMLPath(string name)
        {
            String path = "";
            try
            {
                if (!string.IsNullOrEmpty(name))
                {

                    path = Environment.CurrentDirectory + "\\" + name;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AssemblyHelper.GetXMLPath(string)->" + ex.Message);
                // throw ex;
            }
            return path;
        }

        /// <summary>
        /// Read data
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="node">Parent node</param>
        /// <param name="attribute">Read node</param>
        /// <returns>string</returns>
        public static string ReadInnerText(string path, string rootNode, string selectNode)
        {
            string str1 = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetXMLPath(path));
                XmlNodeList nodeList = doc.SelectSingleNode(rootNode).ChildNodes;//Take all the child nodes of the NewDataSet node
                foreach (XmlNode xn in nodeList)//Traverse all child nodes
                {
                    XmlElement xe = (XmlElement)xn;//Convert the child node type to XmlElement type
                    if (xe.Name == selectNode)
                    {
                        str1 = xe.InnerText;
                        continue;
                    }
                }
                return str1;
            }
            catch (Exception ex) { string aaa = ex.ToString(); }
            finally
            {
            }
            return str1;
        }


        public static void UpdateInnerText(string path, string rootNode, string selectNode, string text)
        {
            try
            {
                Boolean isSearch = false;
                XmlDocument doc = new XmlDocument();
                string filPath = GetXMLPath(path);
                doc.Load(filPath);
                XmlNode xn = doc.SelectSingleNode(rootNode);
                XmlNodeList nodeList = xn.ChildNodes;//Get all child nodes of the NewDataSet node
                foreach (XmlNode xxn in nodeList)//Traverse all child nodes
                {
                    XmlElement xe = (XmlElement)xxn;//Convert the child node type to XmlElement type
                    if (xe.Name == selectNode)
                    {
                        xe.InnerText = text;
                        isSearch = true;
                        continue;
                    }
                }
                if (!isSearch)
                {
                    XmlElement newxe = doc.CreateElement(selectNode);
                    newxe.InnerText = text;
                    xn.AppendChild(newxe);
                }
                doc.Save(filPath);
            }
            catch (Exception ex) { string aaa = ex.ToString(); }
        }

    }
}
