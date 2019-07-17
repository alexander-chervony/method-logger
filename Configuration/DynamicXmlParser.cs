using System.Dynamic;
using System.Xml.Linq;

namespace Configuration
{
    internal class DynamicXmlParser : DynamicObject
    {
        private readonly XElement element;

        public DynamicXmlParser(string filename)
        {
            element = XElement.Load(filename);
        }

        private DynamicXmlParser(XElement el)
        {
            element = el;
        }

        public string this[string attr]
        {
            get
            {
                if (element == null)
                {
                    return string.Empty;
                }

                return element.Attribute(attr).Value;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (element == null)
            {
                result = null;
                return false;
            }

            XElement sub = element.Element(binder.Name);

            if (sub == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = new DynamicXmlParser(sub);
                return true;
            }
        }

        public override string ToString()
        {
            if (element != null)
            {
                return element.Value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}