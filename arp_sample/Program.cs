using System;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading.Tasks;
using ArpCs;

namespace ARP_001
{
    class Program
    {
        static void Main(string[] args)
        {
            Parallel.For(1, 255, i =>
                {
                var ipStr = string.Format("192.168.0.{0}", i);
                string res;
                ARP.SendARP(ipStr, out res);
                Console.WriteLine(string.Format("{0}へARP要求 : ", ipStr) + res);
                });
            Console.WriteLine("何かのキーを押してください。");
            Console.ReadKey();
        }
    }
}