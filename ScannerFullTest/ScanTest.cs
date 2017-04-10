using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blackduck.Hub
{
    [TestClass]
    public class ScanTest
    {

        private DirectoryInfo tempProjectDirectory;
        private AppDomain scanDomain;

        [TestInitialize]
        public void SetUp()
        {
            string tempProjectPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            tempProjectDirectory = Directory.CreateDirectory(tempProjectPath);
            ZipFile.ExtractToDirectory(@"TestApp.zip", tempProjectPath);
            scanDomain = AppDomain.CreateDomain("Scanner Test Domain");

        }

        [TestMethod]
        public void TestScan()
        {
            string executablePath = Path.Combine(tempProjectDirectory.FullName, "QuackHackAdmin.exe");
            string outputFilePath = Path.Combine(tempProjectDirectory.FullName, "scanresult.json");
            scanDomain.ExecuteAssembly("Scanner.exe", new string[] {executablePath, outputFilePath });
            string generatedJson = File.ReadAllText(outputFilePath);
            Assert.IsFalse(string.IsNullOrWhiteSpace(generatedJson));
            
        }

        [TestCleanup]
        public void TearDown()
        {
            AppDomain.Unload(scanDomain);
            tempProjectDirectory.Delete(true);
        }
    }
}
