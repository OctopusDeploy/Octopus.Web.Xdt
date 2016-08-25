using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using NUnit;
using NUnit.Framework;

namespace Octopus.Web.XmlTransform.Test
{
    [TestFixture]
    public class XmlTransformTest
    {
        private readonly ConcurrentBag<string> temporaryDirectories = new ConcurrentBag<string>();

        [OneTimeTearDown]
        public void AfterAllTestsHaveRun()
        {
            foreach (var folder in temporaryDirectories)
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception ex)
                {
                    //not a lot we can do
                    Console.WriteLine($"Failed to delete temporary directory '{folder}' after tests completed: {ex}");
                }
            }
        }

        [Test]
        public void XmlTransform_Support_WriteToStream()
        {
            string src = CreateATestFile("Web.config", "Web.config");
            string transformFile = CreateATestFile("Web.Release.config", "Web.Release.config");
            string destFile = GetTestFilePath("MyWeb.config");

            //execute
            Octopus.Web.XmlTransform.XmlTransformableDocument x = new Octopus.Web.XmlTransform.XmlTransformableDocument();
            x.PreserveWhitespace = true;
            x.Load(src);

            Octopus.Web.XmlTransform.XmlTransformation transform = new Octopus.Web.XmlTransform.XmlTransformation(transformFile);

            bool succeed = transform.Apply(x);

            FileStream fsDestFile = new FileStream(destFile, FileMode.OpenOrCreate);
            x.Save(fsDestFile);

            //verify, we have a success transform
            Assert.AreEqual(true, succeed);

            //verify, the stream is not closed
            Assert.AreEqual(true, fsDestFile.CanWrite, "The file stream can not be written. was it closed?");

            //sanity verify the content is right, (xml was transformed)
            fsDestFile.Dispose();
            string content = File.ReadAllText(destFile);
            Assert.IsFalse(content.Contains("debug=\"true\""));
            
            List<string> lines = new List<string>(File.ReadLines(destFile));
            //sanity verify the line format is not lost (otherwsie we will have only one long line)
            Assert.IsTrue(lines.Count>10);

            //be nice 
            transform.Dispose();
            x.Dispose();
        }

        [Test]
        public void XmlTransform_AttibuteFormatting()
        {
            Transform_TestRunner_ExpectSuccess("AttributeFormatting_source.xml",
                    "AttributeFormatting_transform.xml",
                    "AttributeFormatting_destination.bsl",
                    "AttributeFormatting.Log");
        }

        [Test]
        public void XmlTransform_TagFormatting()
        {
            Transform_TestRunner_ExpectSuccess("TagFormatting_source.xml",
                   "TagFormatting_transform.xml",
                   "TagFormatting_destination.bsl",
                   "TagFormatting.log");
        }

        [Test]
        public void XmlTransform_HandleEdgeCase()
        {
            //2 edge cases we didn't handle well and then fixed it per customer feedback.
            //    a. '>' in the attribute value
            //    b. element with only one character such as <p>
            Transform_TestRunner_ExpectSuccess("EdgeCase_source.xml",
                    "EdgeCase_transform.xml",
                    "EdgeCase_destination.bsl",
                    "EdgeCase.log");
        }

        [Test]
        public void XmlTransform_ErrorAndWarning()
        {
            Transform_TestRunner_ExpectFail("WarningsAndErrors_source.xml",
                    "WarningsAndErrors_transform.xml",
                    "WarningsAndErrors.log");
        }

        private void Transform_TestRunner_ExpectSuccess(string source, string transform, string baseline, string expectedLog)
        {
            string src = CreateATestFile("source.config", source);
            string transformFile = CreateATestFile("transform.config", transform);
            string baselineFile = CreateATestFile("baseline.config", baseline);
            string destFile = GetTestFilePath("result.config");
            TestTransformationLogger logger = new TestTransformationLogger();

            XmlTransformableDocument x = new XmlTransformableDocument();
            x.PreserveWhitespace = true;
            x.Load(src);

            Octopus.Web.XmlTransform.XmlTransformation xmlTransform = new Octopus.Web.XmlTransform.XmlTransformation(transformFile, logger);

            //execute
            bool succeed = xmlTransform.Apply(x);
            x.Save(destFile);
            xmlTransform.Dispose();
            x.Dispose();
            //test
            Assert.AreEqual(true, succeed);
            CompareFiles(baselineFile, destFile);
            CompareMultiLines(ReadResource(expectedLog), logger.LogText);
        }

        private void Transform_TestRunner_ExpectFail(string source, string transform, string expectedLog)
        {
            string src = CreateATestFile("source.config", source);
            string transformFile = CreateATestFile("transform.config", transform);
            string destFile = GetTestFilePath("result.config");
            TestTransformationLogger logger = new TestTransformationLogger();

            XmlTransformableDocument x = new XmlTransformableDocument();
            x.PreserveWhitespace = true;
            x.Load(src);

            Octopus.Web.XmlTransform.XmlTransformation xmlTransform = new Octopus.Web.XmlTransform.XmlTransformation(transformFile, logger);

            //execute
            bool succeed = xmlTransform.Apply(x);
            x.Save(destFile);
            xmlTransform.Dispose();
            x.Dispose();
            //test
            Assert.AreEqual(false, succeed);
            CompareMultiLines(ReadResource(expectedLog), logger.LogText);
        }

        private void CompareFiles(string baseLinePath, string resultPath)
        {
            string bsl;
            using (var stream = File.OpenRead(baseLinePath))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    bsl = sr.ReadToEnd();
                }
            }

            string result;
            using (var stream = File.OpenRead(resultPath))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }

            CompareMultiLines(bsl, result);
        }

        private void CompareMultiLines(string baseline, string result)
        {
            string[] baseLines = baseline.Split(new string[] { global::System.Environment.NewLine },  StringSplitOptions.None);
            string[] resultLines = result.Split(new string[] { global::System.Environment.NewLine },  StringSplitOptions.None);

            for (int i = 0; i < baseLines.Length; i++)
            {
                Assert.AreEqual(baseLines[i], resultLines[i], string.Format("line {0} at baseline file is not matched", i));
            }
        }

        private string ReadResource(string filename)
        {
            Assembly assembly = typeof(XmlTransformTest).GetTypeInfo().Assembly;
            string resourceName = $"{assembly.GetName().Name}.Resources.{filename}";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new MissingManifestResourceException("failed to load " + resourceName);
                }

                using (StreamReader streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private string CreateATestFile(string filename, string resourceName)
        {
            var contents = ReadResource(resourceName);
            var file = GetTestFilePath(filename);
            File.WriteAllText(file, contents);
            return file;
        }

        private string GetTestFilePath(string filename)
        {
            var folder = Path.GetTempPath();
            var guid = Guid.NewGuid().ToString();
            var testRunFolder = Path.Combine(folder, guid);

            Directory.CreateDirectory(testRunFolder);
            temporaryDirectories.Add(testRunFolder);

            return Path.Combine(testRunFolder, filename);
        }
    }
}
