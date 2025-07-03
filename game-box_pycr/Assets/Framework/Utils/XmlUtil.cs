using System;
using System.Xml;

namespace Framework
{
    public class XmlUtil
    {
        /// <summary>
        /// Get Xml document by file path
        /// </summary>
        /// <param name="filePath">Absolute file path</param>
        /// <param name="action">Xml document callback</param>
        public static void ReadXml(string filePath, Action<XmlDocument> action)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            action.Invoke(doc);
        }

        /// <summary>
        /// Get Xml document by xml file content
        /// </summary>
        /// <param name="text">File content</param>
        /// <param name="action">Xml document callback</param>
        public static void ReadXmlByText(string text, Action<XmlDocument> action)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            action.Invoke(doc);
        }

        /// <summary>
        /// Get Xml document node by name
        /// </summary>
        /// <param name="doc">Xml document</param>
        /// <param name="key">Node name</param>
        public static XmlNode GetRootNode(XmlDocument doc, string key)
        {
            XmlNode xn = doc.SelectSingleNode(key);
            return xn;
        }

        /// <summary>
        /// Traverse and process node elements
        /// </summary>
        /// <param name="node">Xml node</param>
        /// <param name="callback">Node callback</param>
        public static void GetNodeChildrenForEach(XmlNode node, Action<XmlElement> callback)
        {
            XmlNodeList xnl = node.ChildNodes;
            foreach (XmlNode xNode in xnl)
            {
                XmlElement element = (XmlElement)xNode;
                callback?.Invoke(element);
            }
        }

        /// <summary>
        /// Get Xml document node element by name
        /// </summary>
        /// <param name="node">Xml node</param>
        /// <param name="name">Element name</param>
        public static XmlElement GetNodeChildByName(XmlNode node, string name)
        {
            XmlNodeList xnl = node.ChildNodes;
            foreach (XmlNode xNode in xnl)
            {
                XmlElement element = (XmlElement)xNode;
                string key = element.Name;
                if (key == name)
                {
                    return element;
                }
            }
            return null;
        }
    }
}
