using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Blackduck.Hub
{
    [TestClass]
    public class ScanTest
    {
        private JObject scanOutputJson;

        [TestInitialize]
        public void DoScan()
        {
            string tempProjectPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            DirectoryInfo tempProjectDirectory = Directory.CreateDirectory(tempProjectPath);
            AppDomain scanDomain = AppDomain.CreateDomain("Scanner Test Domain");
            try
            {
                ZipFile.ExtractToDirectory(@"TestApp.zip", tempProjectPath);

                string executablePath = Path.Combine(tempProjectDirectory.FullName, "QuackHackAdmin.exe");
                string outputFilePath = Path.Combine(tempProjectDirectory.FullName, "scanresult.json");
                scanDomain.ExecuteAssembly("Scanner.exe", new string[] { executablePath, outputFilePath });
                string scanResultJsonText = File.ReadAllText(outputFilePath);
                Assert.IsFalse(string.IsNullOrWhiteSpace(scanResultJsonText));
                //Construct the Json
                scanOutputJson = JsonConvert.DeserializeObject<JObject>(scanResultJsonText);

            }
            finally
            {
                AppDomain.Unload(scanDomain);
                tempProjectDirectory.Delete(true);
            }
        }

        /// <summary>
        /// Checks for an expected .NET assembly referenced from the test application.
        /// </summary>
        [TestMethod]
        public void TestExpectedDotNetAssembly()
        {

            //Test for an expected native .Net assembly
            JToken strapUpFormsInfo = scanOutputJson.SelectToken("$.scanNodeList[?(@.name == 'StrapUp.Forms.dll - StrapUp.FormsPlugin.Abstractions[1.0.0.0]')]");
            Assert.IsNotNull(strapUpFormsInfo);
            Assert.AreEqual("FILE", strapUpFormsInfo["type"].ToString());
            Assert.AreEqual("f4ef17258b2d39b116614df1dee17679d450a169", strapUpFormsInfo["signatures"]["FILE_SHA1"].ToString());
            Assert.AreEqual(JTokenType.Integer, strapUpFormsInfo["size"].Type);
            Assert.AreEqual("16384", strapUpFormsInfo["size"].ToString());
        }

        /// <summary>
        /// Checks for an expected Native .DLL referenced by P-Invoke from the test application
        /// </summary>
        [TestMethod]
        public void TestExpectedNativeDll()
        {
            JToken freeImageInfo = scanOutputJson.SelectToken("$.scanNodeList[?(@.name == 'FreeImage.dll - FreeImage[3, 17, 0, 0]')]");
            Assert.IsNotNull(freeImageInfo);
            Assert.AreEqual("FILE", freeImageInfo["type"].ToString());
            Assert.AreEqual("fc4e67de92eb34b9c89ca37a1a4592066b9193c6", freeImageInfo["signatures"]["FILE_SHA1"].ToString());
            Assert.AreEqual(JTokenType.Integer, freeImageInfo["size"].Type);
            Assert.AreEqual("6402560", freeImageInfo["size"].ToString());

        }

        /// <summary>
        /// Two assemblies referenced from the application code are not included in the test environment.
        /// The failure to access these assemblies should be documented as problems in the resulting scan file
        /// </summary>
        [TestMethod]
        public void TestScanProblemReporting()
        {
            var actualMessages = scanOutputJson.SelectTokens("scanProblemList..problem..message").Select(t => t.ToString()).ToList();
            Assert.AreEqual(2, actualMessages.Count);

            var expectedMessages = new HashSet<String>() {
                "Could not load file or assembly 'Xamarin.Forms.Core, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. The system cannot find the file specified.",
                "Could not load file or assembly 'Xamarin.Forms.Xaml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. The system cannot find the file specified."
            };

            var unexpectedMessages = actualMessages.Except(expectedMessages).ToList();
            Assert.AreEqual(0, unexpectedMessages.Count, "Unexpected problem messages: " + (unexpectedMessages.Count > 0 ? unexpectedMessages.Aggregate(string.Join) : ""));

            var notFoundMessages = expectedMessages.Except(actualMessages).ToList();
            Assert.AreEqual(0, notFoundMessages.Count, "Expected problem messages not found: " + (notFoundMessages.Count > 0 ? notFoundMessages.Aggregate(string.Join) : ""));
        }
    }
}
