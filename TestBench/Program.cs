using System;
using System.IO;
using zsyncnet;
namespace TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Zsyncnet TestBench");
            
            ZsyncMake.Make(new FileInfo(@"D:\BSUTest\Input\@ace\ace_advanced_ballistics.dll"));
        }
    }
}