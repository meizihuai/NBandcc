using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace NBandcc
{
    public class DeviceHelper
    {
        public static string[] GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                List<string> list = new List<string>();
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        list.Add(IpEntry.AddressList[i].ToString());
                    }
                }
                return list.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>  
        /// 获取本机MAC地址  
        /// </summary>  
        /// <returns>本机MAC地址</returns>  
        public static string GetMacAddress()
        {
            try
            {
                IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                if (nics == null || nics.Length < 1)
                {
                    return "";
                }
                foreach (NetworkInterface adapter in nics)
                {
                    PhysicalAddress address = adapter.GetPhysicalAddress();
                    byte[] bytes = address.GetAddressBytes();
                    string mac = "";
                    if (bytes == null || bytes.Length == 0)
                    {
                        return "";
                    }
                    else
                    {
                        foreach(var d in bytes)
                        {
                            mac = mac + d.ToString("X2");
                        }
                        return mac;
                    }               
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
    }
}
