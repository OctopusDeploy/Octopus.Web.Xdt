using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Octopus.Web.XmlTransform
{
    public interface IXmlOriginalDocumentService
    {
        XmlNodeList SelectNodes(string path, XmlNamespaceManager nsmgr);
    }
}
