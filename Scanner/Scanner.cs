using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Blackduck.Hub
{
    class Scanner
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

			ScannerJsonBuilder builder = ScannerJsonBuilder.NewInstance();



            Assembly targetAssembly = Assembly.LoadFile(target);
            Console.WriteLine(targetAssembly.FullName+" "+targetAssembly.GetName().GetPublicKey());
			//Retrieve the referenced assembly

			builder.ProjectName = targetAssembly.GetName().Name;
			builder.Release = targetAssembly.GetName().Version.ToString();

			builder.AddDirectory(targetAssembly.GetName().Name, new FileInfo(target).FullName);

            foreach (var refAssemblyName in targetAssembly.GetReferencedAssemblies())
            {

				//Load the assembly to get its path
				try
				{
					Assembly refAssembly = Assembly.Load(refAssemblyName);
					string path = refAssembly.Location.ToString();

					Console.WriteLine(refAssembly.FullName + " " + refAssemblyName.GetPublicKey());

					if (!string.IsNullOrWhiteSpace(path))
					{
						string sha1 = computeSha1(path);
						builder.AddFile(refAssembly.FullName, path, new FileInfo(path).Length, sha1);
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
