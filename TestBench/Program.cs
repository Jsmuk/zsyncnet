using System;
using System.IO;
using System.Text;
using zsyncnet;
namespace TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Zsyncnet TestBench");
            
            ZsyncMake.Make(new FileInfo(@"D:\BSUTest\Input\@ace\ace_advanced_ballistics.dll"));

            ControlFile cf = new ControlFile(new FileStream(@"D:\BSUTest\Input\@ace\ace_advanced_ballistics.dll.zsync",
                FileMode.Open, FileAccess.Read));

        }
    }
}