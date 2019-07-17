using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBandcc
{
    public class Utils
    {
        public static string FormatDateTime(string time)
        {
            try
            {
                if (string.IsNullOrEmpty(time)) return null;
                DateTime t = DateTime.Parse(time);
                return t.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static bool IsNumberic(string str)
        {
            //Regex regExp = new Regex("^[0-9]*$");
            //return regExp.IsMatch(str);
            if (string.IsNullOrEmpty(str)) return false;
            try
            {
                double d = double.Parse(str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
