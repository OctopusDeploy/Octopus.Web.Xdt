using System.IO;
using NUnit.Framework;

namespace Octopus.Web.XmlTransform.Test
{
    [TestFixture]
    public class XmlTransformableDocumentTest
    {
        [Test]
        public void CanLoadFileWithNs()
        {
            var configurationFileDocument = new XmlTransformableDocument()
            {
                PreserveWhitespace = true
            };

            const string xml = @"<nlog autoReload=""true""
xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <targets>
    <target name=""csv""
xsi:type=""File"">
    <layout xsi:type=""CSVLayout"">
    </layout>
    </target>
    </targets>

    <rules>
    <logger name=""*"" minlevel=""Debug"" writeTo=""csv"" />
    </rules>
</nlog>";
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllText(tempFile, xml);

            try
            {
                configurationFileDocument.Load(tempFile);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Test]
        public void CanLoadFileWithNs2()
        {
            var configurationFileDocument = new XmlTransformableDocument()
            {
                PreserveWhitespace = true
            };

            const string xml = @"<nlog autoReload=""true""
xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <targets>
    <target name=""csv""
xsi:type=""File"">
    <layout xsi:type=""CSVLayout"">
    </layout>
    </target>
    </targets>

    <rules>
    <logger name=""*"" minlevel=""Debug"" writeTo=""csv"" />
    </rules>
</nlog>";
            configurationFileDocument.Load(new StringReader(xml));
        }
    }
}