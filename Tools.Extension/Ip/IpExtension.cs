using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static System.Web.HttpContext;

namespace Tools.Extension.Ip
{
    /// <summary>
    /// Ip扩展
    /// </summary>
    public static class IpExtension
    {
        #region 获得用户IP

        /// <summary>
        /// 获得用户IP
        /// </summary>
        public static string GetUserIp()
        {
            var isErr = false;
            var ip = Current.Request.ServerVariables["HTTP_X_ForWARDED_For"] ?? Current.Request.ServerVariables["REMOTE_ADDR"];
            if (ip.Length > 15)
                isErr = true;
            else
            {
                var temp = ip.Split('.');
                if (temp.Length == 4)
                {
                    foreach (var t in temp)
                    {
                        if (t.Length > 3) isErr = true;
                    }
                }
                else
                    isErr = true;
            }

            return isErr ? "1.1.1.1" : ip;
        }

        #endregion 获得用户IP

        #region 获取当前页面客户端的Ip

        /// <summary>
        /// 获得当前页面客户端的IP
        /// </summary>
        /// <returns>当前页面客户端的IP</returns>
        public static string GetIP()
        {
            string result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]; GetDnsRealHost();
            if (string.IsNullOrEmpty(result))
                result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(result))
                result = HttpContext.Current.Request.UserHostAddress;
            //if (string.IsNullOrEmpty(result) || !Utils.IsIP(result))
            //    return "127.0.0.1";
            return result;
        }

        #endregion 获取当前页面客户端的Ip

        #region 获取客户端IP

        public static string ClientIP
        {
            get
            {
                var isErr = false;
                var ip = "127.0.0.1";
                try
                {
                    ip = HttpContext.Current.Request.ServerVariables["HTTP_X_ForWARDED_For"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    if (ip.Length > 15)
                        isErr = true;
                    else
                    {
                        var temp = ip.Split('.');
                        if (temp.Length == 4)
                        {
                            foreach (var t in temp)
                            {
                                if (t.Length > 3) isErr = true;
                            }
                        }
                        else
                            isErr = true;
                    }
                }
                catch { isErr = false; }

                return isErr ? "1.1.1.1" : ip;
            }
        }

        #endregion 获取客户端IP

        #region 返回指定的服务器变量信息

        /// <summary>
        /// 返回指定的服务器变量信息
        /// </summary>
        /// <param name="strName">服务器变量名</param>
        /// <returns>服务器变量信息</returns>
        public static string GetServerString(this string strName)
        {
            return Current.Request.ServerVariables[strName] ?? "";
        }

        #endregion 返回指定的服务器变量信息

        #region 检查是否为Ip地址

        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIp(this string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        #endregion 检查是否为Ip地址

        #region 测试链接

        /// <summary>
        /// 测试链接
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <returns></returns>
        public static bool TestPing(this string ip)
        {
            using (var p = new Ping())
            {
                var options = new PingOptions { DontFragment = true };
                const string data = "Test Data!";
                var buffer = Encoding.ASCII.GetBytes(data);
                const int timeout = 1000;
                var reply = p.Send(ip, timeout, buffer, options);
                return reply != null && reply.Status == IPStatus.Success;
            }
        }

        #endregion 测试链接

        #region private

        private static string GetDnsRealHost()
        {
            return HttpContext.Current.Request.Url.DnsSafeHost;
        }

        private static string GetUrl(string key)
        {
            var strTxt = new StringBuilder();
            strTxt.Append("785528A58C55A6F7D9669B9534635");
            strTxt.Append("E6070A99BE42E445E552F9F66FAA5");
            strTxt.Append("5F9FB376357C467EBF7F7E3B3FC77");
            strTxt.Append("F37866FEFB0237D95CCCE157A");
            return strTxt.ToString();
        }

        #endregion private
    }
}