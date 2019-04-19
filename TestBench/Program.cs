using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using zsyncnet;
namespace TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Zsyncnet TestBench");
            
            //ZsyncMake.Make(new FileInfo(@"D:\BSUTest\Input\@ace\ace_advanced_ballistics.dll"));            
            
            ZsyncMake.Make(new FileInfo(@"/home/james/tmp/test/web/ace_advanced_ballistics.dll"));
            //var iso = Zsync.Sync(new Uri("http://localhost:8000/netboot.iso.zsync"), new DirectoryInfo(@"D:\tmp\test"));
            //Console.WriteLine($"ISO Total Bytes: {dll}");

            var dll = Zsync.Sync(new Uri("http://localhost:8000/ace_advanced_ballistics.dll.zsync"),
                new DirectoryInfo(@"/home/james/tmp/test/client"));

        }
    }
}