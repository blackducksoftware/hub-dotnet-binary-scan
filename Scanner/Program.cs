using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.Reflection;
using System.IO;

namespace Scanner
{
    class Program
    {
        private static readonly SHA1Cng sha1 = new SHA1Cng();

        static void Main(string[] args)
        {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Argument required. Whaddaya want me to scan?");
                return;
            }
            string target = args[0];
            Console.WriteLine("Scanning " + target + "...");
            Assembly targetAssembly = Assembly.LoadFile(target);
            Console.WriteLine(targetAssembly.FullName+" "+targetAssembly.GetName().GetPublicKey());
            //Retrieve the referenced assembly
            foreach (var refAssemblyName in targetAssembly.GetReferencedAssemblies())
            {

                //Load the assembly to get its path
                Assembly refAssembly = Assembly.Load(refAssemblyName);
                string path =refAssembly.Location.ToString();

 
                Console.Write(refAssembly.FullName + " " + refAssemblyName.GetPublicKey() + " -> ");
                Console.Write(path + " [" );

                if (!string.IsNullOrWhiteSpace(path))
                {
                    string sha1 = computeSha1(path);
                    Console.Write(sha1);
                }
                Console.WriteLine("]");


            }
            Console.ReadKey();
        }

        private static String computeSha1(String path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] hash = sha1.ComputeHash(fs);
                return Convert.ToBase64String(hash);
            }
        }
    }

  
}
