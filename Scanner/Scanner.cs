using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using System.Linq;
namespace Blackduck.Hub
{
    static class Scanner
    {
        private static readonly SHA1Cng sha1 = new SHA1Cng();

        private static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> elements)
        {
            foreach (T element in elements)
                queue.Enqueue(element);
        }

        static void Main(string[] args)
        {

            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Argument required. Whaddaya want me to scan?");
                return;
            }


            string target = Path.GetFullPath(args[0]);
            Console.WriteLine("Scanning " + target + "...");

            ScannerJsonBuilder builder = ScannerJsonBuilder.NewInstance();

            Assembly targetAssembly = Assembly.LoadFile(target);
            Console.WriteLine(targetAssembly.GetName().Name + " " + targetAssembly.GetName().GetPublicKey());

            //Prime the output model
            builder.ProjectName = targetAssembly.GetName().Name;
            builder.Release = targetAssembly.GetName().Version.ToString();
            builder.AddDirectory(targetAssembly.GetName().Name, new FileInfo(target).FullName);

            //The files already scanned
            ISet<string> scannedPaths = new HashSet<string>();
            //The files found that need scanning
            Queue<AssemblyName> assembliesToScan = new Queue<AssemblyName>();
            //... Starting with the assemblies referenced from the one we're scanning
            assembliesToScan.EnqueueAll(targetAssembly.GetReferencedAssemblies());

            while (assembliesToScan.Count > 0)
            {
                AssemblyName refAssemblyName = assembliesToScan.Dequeue();
                Assembly refAssembly = Assembly.Load(refAssemblyName);
                string path = Path.GetFullPath(refAssembly.Location);
                if (scannedPaths.Contains(path))
                    continue;

                try
                {
                    scannedPaths.Add(path);
                    //Note the file informatoin
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                    string name = string.IsNullOrWhiteSpace(fvi.ProductName) ? refAssembly.GetName().Name : fvi.ProductName;
                    //We'll make our file names more descriptive than just the actual file name.
                    string fileName = ($"{Path.GetFileName(path)} - {name}[{fvi.ProductVersion}]");
                    Console.WriteLine(fileName);

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        string sha1 = computeSha1(path);
                        builder.AddFile(fileName, path, new FileInfo(path).Length, sha1);
                    }

                    assembliesToScan.EnqueueAll(refAssembly.GetReferencedAssemblies());

                    var pInvokePaths = PInvokeSearcher.findPInvokePaths(refAssembly);
                    foreach (string nativeDllPath in pInvokePaths.FoundPaths.Where(fp => !scannedPaths.Contains(fp)))
                    {
                        FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(nativeDllPath);
                        String dllFileName = ($"{Path.GetFileName(nativeDllPath)} - {dllVersionInfo.ProductName}[{dllVersionInfo.ProductVersion}]");
                        Console.WriteLine("NATIVE: " + dllFileName);
                        builder.AddFile(dllFileName, nativeDllPath, new FileInfo(nativeDllPath).Length, computeSha1(nativeDllPath));
                        scannedPaths.Add(nativeDllPath);
                    }
                    
                }
                catch (FileNotFoundException e)
                {
                    Console.Error.WriteLine(e.Message);
                }

            }


            if (args.Length < 2)
                builder.Write(Console.Out);
            else using (var fileWriter = new StreamWriter(args[1], false))
                {
                    builder.Write(fileWriter);
                }

        }

        private static string computeSha1(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] hash = sha1.ComputeHash(fs);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }



    }


}
