using System.Xml;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MasterCard.SDK
{
    /// <summary>
    /// This generic class provides methods to serializes an object of type T marked as Serializable
    /// to XML or deserialize an object of type T from XML.
    /// </summary>
    /// <typeparam name="T">The type of object being serialized/deserialized.</typeparam>
    public static class Serializer<T>
    {
        /// <summary>
        /// This method serializes the type of object specified by T into an
        /// XmlDocument object and returns the result to the user.
        /// </summary>
        /// <param name="objectToSerialize">Object to be serialized.</param>
        /// <returns>Returns the object T as an XmlDocument.</returns>
        public static XmlDocument Serialize(T objectToSerialize)
        {
            XmlSerializer serializer = new XmlSerializer(objectToSerialize.GetType());
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new Utf8StringWriter(sb);

            serializer.Serialize(writer, objectToSerialize);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sb.ToString());

            return doc;
        }

        /// <summary>
        /// This method overloads the Deserialize(XmlDocument) method by allowing the user
        /// to pass a string, which it converts to an XmlDocument and then calling the 
        /// Deserialize method. 
        /// </summary>
        /// <param name="xml">The xml payload to be deserialized.</param>
        /// <returns>Returns an object of type T, which has been deserialized from the supplied XML input string.</returns>
        public static T Deserialize(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return Deserialize(doc);
        }

        /// <summary>
        /// This method receives an XmlDocument, deserializes the XML as an object of 
        /// type T, and returns the result to the caller.
        /// </summary>
        /// <param name="doc">The XML to be deserialized.</param>
        /// <returns>Returns an object of type T, which has been deserialized from the supplied XML input.</returns>
        public static T Deserialize(XmlDocument doc)
        {
            //Assuming doc is an XML document containing a serialized object and objType is a System.Type set to the type of the object.
            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            object obj = serializer.Deserialize(reader);
            // Then you just need to cast obj into whatever type it is eg:
            T myObj = (T)obj;

            return myObj;
        }
    }

    /// <summary>
    /// Custom UTF8 encoder.
    /// </summary>
    public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder sb)
            : base(sb)
        {
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
