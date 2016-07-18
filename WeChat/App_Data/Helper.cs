using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeiPay
{
    public class Helper
    {
        public static double priceHelper(string price)
        {
            double tmp;
            double.TryParse(price, out tmp);
            
            return tmp / 100.00;
        }

        public static string timeHelper(string time)
        {
            string[] res;
            string ans = "";
            //XXXX/XX/XX XX:XX:XX
            res = new string[6];

            res[0] = CutString(time, 0, 4);
            res[1] = CutString(time, 4, 2);
            res[2] = CutString(time, 6, 2);
            res[3] = CutString(time, 8, 2);
            res[4] = CutString(time, 10, 2);
            res[5] = CutString(time, 12, 2);

            ans = res[0] + "/" + res[1] + "/" + res[2] + " " + res[3] + ":" + res[4] + ":";
            ans += res[5];
            return ans;
        }
        public static string CutString(string src,int stIndex, int len)
        {
            StringBuilder sr = new StringBuilder();
            int i = 0;
            for (i=0; i<len;i++)
            {
                sr.Append(src[stIndex + i]);
            }
            
            return sr.ToString();
            
        }

        public static string convertTime2String(DateTime time)
        {
           // int ans;
            string stime = time.ToString();
            stime = stime.Replace(' ', '/');
            stime = stime.Replace("/", "");
            stime = stime.Replace(":", "");

            //if (int.TryParse(stime, out ans))
            //    return ans;
            //return 0;
            return stime;
        }
    }

}
