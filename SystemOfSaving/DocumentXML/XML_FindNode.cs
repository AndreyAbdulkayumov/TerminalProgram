using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SystemOfSaving.DocumentXML
{
    public static class XML_FindNode
    {
        public static XmlNode InElement(XmlElement Root, string Name)
        {
            foreach (XmlNode Node in Root)
            {
                if (Node.Attributes.Count > 0)
                {
                    XmlNode attribute = Node.Attributes.GetNamedItem("name");

                    if (attribute != null && attribute.Value == Name)
                    {
                        return Node;
                    }
                }
            }

            throw new Exception("Указанный узел \"" + Name + "\" не найден");
        }

        public static XmlNode InNode(XmlNode Root, string Property)
        {
            foreach (XmlNode Node in Root)
            {
                if (Node.LocalName == Property)
                {
                    return Node;
                }
            }

            throw new Exception("Указанный узел \"" + Property + "\" не найден");
        }
    }
}
