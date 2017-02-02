using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Scanner
{
    class Program
    {
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
            foreach (var refAssembly in targetAssembly.GetReferencedAssemblies())
            {
                Console.WriteLine(refAssembly.FullName + " " + refAssembly.GetPublicKey());
                
            }
            Console.ReadKey();
        }
    }
}
