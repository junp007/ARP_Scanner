using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ArpCs
{
    public class ARP
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int dstIp, int srcIp, byte[] mac, ref int macLen);

        /// <summary>
        /// ARPパケットを送信する
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="MACAddress"></param>
        /// <returns></returns>
        public static bool SendARP(string IPAddress, out string MACAddress)
        {
            return SendARP(IPAddressStr2Num(IPAddress, true), out MACAddress);
        }

        /// <summary>
        /// ARPパケットを送信する(IPアドレス数値バージョン)
        /// </summary>
        /// <param name="IPAddressNum"></param>
        /// <param name="MACAddress"></param>
        /// <returns></returns>
        public static bool SendARP(int IPAddressNum, out string MACAddress)
        {
            var mac = new byte[6];
            var macLen = mac.Length;
            if (SendARP(IPAddressNum, 0, mac, ref macLen) == 0) {
                MACAddress = string.Format("{0:x2}-{1:x2}-{2:x2}-{3:x2}-{4:x2}-{5:x2}", mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
                return true;
            } else {
                MACAddress = "";
                return false;
            }
        }

        /// <summary>
        /// IPアドレスを数値に変換する
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static int IPAddressStr2Num(string IPAddress, bool reverse = false)
        {
            string[] token = IPAddress.Split(new char[] { '.' });
            if (token.Length != 4) {
                throw new Exception("Invalid IP Address type." + IPAddress);
            }

            int[] octets = new int[] { int.Parse(token[0]), int.Parse(token[1]), int.Parse(token[2]), int.Parse(token[3]) };
            foreach (int octet in octets) {
                if (octet < 0 || octet > 0xff) {
                    throw new Exception("Octet is out of range(0 ~ 255)." + IPAddress);
                }
            }

            int ret = 0;
            if (reverse) {
                for (int i = octets.Length - 1; i >= 0; --i) {
                    ret <<= 8;
                    ret += octets[i];
                }
            } else {
                for (int i = 0; i < octets.Length; ++i) {
                    ret <<= 8;
                    ret += octets[i];
                }
            }
            return ret;
        }

        /// <summary>
        /// 数値をIPAddressに変換する
        /// </summary>
        /// <param name="IPAddressNum"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static string IPAddressNum2Str(int IPAddressNum, bool reverse = false)
        {
            string ret = "";
            int[] octet = new int[4];
            for (int i = 0; i < 4; ++i) {
                if (reverse) {
                    octet[i] = (IPAddressNum >> i * 8) & 0xff;
                } else {
                    octet[i] = (IPAddressNum >> (3 - i) * 8) & 0xff;
                }
                ret += octet[i];
                if (i != 3) {
                    ret += ".";
                }
            }

            return ret;
        }
    }
}
