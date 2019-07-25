using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Mono.Cecil;

namespace CosturaUnpacker
{
    class Program
    {
        static void Main(string[] args)
        {
            var exe = "";
            if (args.Count() > 0)
                exe = args[0];
            else
            {
                Console.WriteLine("Full exe path + name?");
                exe = Console.ReadLine();
            }
            var app = AssemblyDefinition.ReadAssembly(exe);
            foreach(EmbeddedResource resource in app.MainModule.Resources)
            {
                if (!resource.Name.EndsWith(".compressed"))
                    continue;
                using (var compressedStream = resource.GetResourceStream())
                { 
                    using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            deflateStream.CopyTo(outputStream);
                            try
                            {
                                outputStream.Position = 0;
                                var embeddedAssembly = AssemblyDefinition.ReadAssembly(outputStream);
                                File.WriteAllBytes(embeddedAssembly.MainModule.Name, outputStream.ToArray());
                                Console.WriteLine("Dumping : " + embeddedAssembly.MainModule.Name);
                            }
                            catch
                            {
                                File.WriteAllBytes(resource.Name.Replace(".compressed", "").Replace("costura.", ""), outputStream.ToArray());
                                Console.WriteLine("Dumping : " + resource.Name);
                            }
                        }
                    }
                }
            }
        }
    }
}
