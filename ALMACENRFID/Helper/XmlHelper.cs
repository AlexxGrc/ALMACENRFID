using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace ALMACENRFID.Helper
{
    /// <summary>
    /// Auxiliary class for XML operations
    /// </summary>
    public class XmlHelper
    {
        #region constant

        /// <summary>
        /// XML file declaration
        /// </summary>
        public const string XML_STATEMENT = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

        private const string NAME = "add";
        private const string KEY = "key";
        private const string VALUE = "value";

        #endregion

        #region 方法
        /// <summary>
        /// Load the Xml file through the file address and get the XmlDocument object
        /// </summary>
        /// <param name="filePath">The system directory where the xml file address is located</param>
        /// <param name="assemblyPath">The assembly address where the file is located [for embedded resources]</param>
        /// <returns>XmlDocument object</returns>
        public static XmlDocument LoadXml(string filePath, string assemblyPath)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                if (File.Exists(filePath))
                {
                    document.Load(filePath);
                }
                else
                {
                    Assembly assembly = Assembly.GetEntryAssembly();
                    Stream stream = assembly.GetManifestResourceStream(assemblyPath);
                    document.Load(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("XmlHelper.LoadXml(string, string) :: " + ex.Message);
                throw ex;
            }
            return document;
        }

        /// <summary>
        /// Generate XML string
        /// </summary>
        /// <param name="root">Root node</param>
        /// <param name="allNodes">All child nodes</param>
        /// <param name="allValues">Data corresponding to all child nodes</param>
        /// <returns></returns>
        public static string CreateXmlStr(string root, string[] allNodes, string[] allValues)
        {
            #region Concatenated string

            //StringBuilder sb = new StringBuilder();
            //sb.Append("<" + headStr + ">");
            //for (int i = 0; i < allNodes.Length; i++)
            //{
            //    sb.Append("<" + allNodes[i] + ">" + allValues[i] + "</" + allNodes[i] + ">");
            //}
            //sb.Append("/" + headStr + ">");
            //return sb.ToString();

            #endregion

            XmlDocument doc = new XmlDocument();
            try
            {
                //Add the declaration of the xml file
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(dec);
                //Add root node
                XmlElement rootNode = doc.CreateElement(root);
                doc.AppendChild(rootNode);
                //Add child node
                for (int i = 0; i < allNodes.Length; i++)
                {
                    XmlElement childNode = doc.CreateElement(allNodes[i]);
                    childNode.InnerText = allValues[i];
                    rootNode.AppendChild(childNode);
                }
                //doc.Save("\\demo.xml");
            }

            catch (Exception ex)
            {
                Debug.WriteLine("XmlHelper.CreateXmlStr(string, string[], string[]) :: " + ex.Message);
                throw ex;
            }
            return doc.InnerXml;
        }

        /// <summary>
        /// Return the corresponding entity collection
        ///     The accessed XML file and the corresponding entity class name must be in the same case, and the data type must be String
        ///     And the writing format of XML must be written as required
        /// </summary>
        /// <typeparam name="T">Generic/typeparam>
        /// <param name="document">XmlDocument object</param>
        /// <returns></returns>
        public static List<T> Select<T>(XmlDocument document)
        {
            List<T> list = new List<T>();
            XmlNode xmlRoot = document.DocumentElement;
            if (xmlRoot != null)
            {
                T obj = default(T);
                Type type = typeof(T);
                PropertyInfo[] propertyInfos = type.GetProperties();
                try
                {
                    foreach (XmlNode xmlNode in xmlRoot.ChildNodes)
                    {
                        if (String.Compare(xmlNode.Name, type.Name, true) == 0)
                        {
                            obj = Activator.CreateInstance<T>();
                            foreach (PropertyInfo propertyInfo in propertyInfos)
                            {
                                String propertyName = propertyInfo.Name;//Get the attribute name
                                object value = xmlNode.Attributes[propertyName].Value;//Get the value of the attribute from XML
                                propertyInfo.SetValue(obj, value, null);//Assign the obtained attribute value to the obj object
                            }
                            list.Add(obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("XmlHelper.Select<T>(XmlDocument) :: " + ex.Message);
                    throw ex;
                }
            }
            return list;
        }

        #endregion
    }
}
