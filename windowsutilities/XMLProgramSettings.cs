/*
 *  
 *  Started     Completion      Version     By                  Company
 *  8/24/07     8/26/07         1.0         Jeremy Hiestand     HystWare (HystWare.com)
 *  Notes:      Simple XML style file replacement for the older INI file system
 * 
 *  Copyright 2007 all rights reserved
 * 
 *  Please retain all copyright notices, this class library is free to use
 *  for any lawful purpose's, author assumes no responsibility for any damages
 * 
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace WindowsUtilities
{
    sealed public class XMLSettings
    {

        private XmlDocument docXmlSettings = null;
        private string xmlFileName;

        //Overrided constructor, creates a new XMLDocument and attempts to load the xml
        //file pased by the FileName field, if it fails an exception is thrown
        public XMLSettings(string FileName)
        {            
            try
            {
                this.docXmlSettings = new XmlDocument();
                this.xmlFileName = FileName;

                //If the File does not exists create it
                if (!File.Exists(this.xmlFileName))
                {
                    //Set the root element to <Document></Document>
                    XmlElement docNode = this.docXmlSettings.CreateElement("Document");
                    this.docXmlSettings.AppendChild(docNode);
                    this.docXmlSettings.Save(FileName);
                }
                
                this.docXmlSettings.Load(this.xmlFileName);
            }
            catch
            {
                //To get here the XML file has to be malformed
                //Throw a new exception showing so
                throw new FileNotFoundException("Error phrasing " + 
                    "file :" + this.xmlFileName);
            }
        }

        #region Public Properties

        public string FileName
        {
            get { return xmlFileName; }
            set { this.xmlFileName = value; }
        }
        
        #endregion

        public override string ToString()
        {
            return this.xmlFileName;
        }

        /*
        WriteXMLString is the Base method for all the WriteXMLValue methods, upon calling a
        WriteXMLValue method it converts the Value into a string laterial and calls the
        WriteXMLString method, if an error occurs in writing it returns false.
        */

        public bool WriteXMLString(string Parent, string Key, string Value)
        {

            try
            {
                XmlElement rootElement = this.docXmlSettings.DocumentElement;
                XmlNode xmlKeyNode = rootElement.SelectSingleNode("/Document/" +
                                                                  Parent + "/" + Key);
                
                if (xmlKeyNode != null) //Is there a child node with the name of "Key"
                {
                    xmlKeyNode.InnerText = Value;
                    this.docXmlSettings.Save(this.xmlFileName);
                }
                else //No child or Parent node was found, create new Parent / child node with the name of "Key"
                {
                    XmlNode xmlNewNode;
                    XmlNode XmlParentNode = rootElement.SelectSingleNode("/Document" + 
                                                                         "/" + Parent);

                    //Check to see if there is a Parent node with the correct name
                    if (XmlParentNode == null)
                    {
                        XmlParentNode = docXmlSettings.DocumentElement; //Select the Top node (DocumentNode)
                        xmlNewNode = docXmlSettings.CreateElement(Parent);
                        XmlParentNode.AppendChild(xmlNewNode);
                    }

                    //Now create the child node and set its innertext to "Value"
                    XmlParentNode = rootElement.SelectSingleNode("/Document" +
                                                                 "/" + Parent);
                    xmlNewNode = docXmlSettings.CreateElement(Key);
                    xmlNewNode.InnerText = Value;
                    XmlParentNode.AppendChild(xmlNewNode);

                    this.docXmlSettings.Save(this.xmlFileName);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WriteXMLValue(string Parent, string Key, Int16 Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, Int32 Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, Int64 Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, float Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, decimal Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, bool Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, byte Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, UInt16 Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, UInt32 Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        public bool WriteXMLValue(string Parent, string Key, UInt64 Value)
        {
            return WriteXMLString(Parent, Key, Value.ToString());
        }

        /*
        ReadXMLString is the Base method for all the ReadXML(Value) methods, upon calling a
        ReadXML(Value) method it converts the Value into a string laterial and calls the
        ReadXMLString method, if an error occurs in readung it returns the "default" value.
        */

        public string ReadXMLString(string Parent, string Key, string Default)
        {
            try
            {
                XmlElement rootElement = this.docXmlSettings.DocumentElement;
                XmlNode xmlKeyNode = rootElement.SelectSingleNode("/Document/" +
                                                                  Parent + "/" + Key);
                if (xmlKeyNode != null) //Is there a child node with the name of "Key"
                {
                    return xmlKeyNode.InnerText;
                }

                //Key node was not found, return the Default string
                return Default;
            }
            catch
            {
                //Processing error, return the Default string
                return Default;
            }
        }

        public Int16 ReadXMLint16(string Parent, string Key, Int16 Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return Int16.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public int ReadXMLint32(string Parent, string Key, int Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return int.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public Int64 ReadXMLint64(string Parent, string Key, Int64 Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return Int64.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public float ReadXMLfloat(string Parent, string Key, float Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return float.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public decimal ReadXMLdecimal(string Parent, string Key, decimal Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return decimal.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public bool ReadXMLbool(string Parent, string Key, bool Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return bool.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public byte ReadXMLbyte(string Parent, string Key, byte Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return byte.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public UInt16 ReadXMLuint16(string Parent, string Key, UInt16 Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return UInt16.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public UInt32 ReadXMLuint32(string Parent, string Key, UInt32 Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return UInt32.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }

        public UInt64 ReadXMLuint64(string Parent, string Key, UInt64 Default)
        {
            try
            {
                //Will return the correct XML value if the node is found otherwise
                //the ReadXMLString will return the default value back
                return UInt64.Parse(ReadXMLString(Parent, Key, Default.ToString()));
            }
            catch
            {
                //Processing error, the xml nodes innertext was more then like not "convertable"
                //to a value type so return the default value.
                return Default;
            }
        }
    }

}
