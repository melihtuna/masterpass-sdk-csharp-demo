using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using MasterCard.SDK;

namespace MasterCard.SDK.Util
{
    public class URLUtil
    {
        public static string AddQueryParameter(string url, string descriptor, string value, bool considerIgnoreValue, String ignoreValue)
        {
            try
            {
                if (!considerIgnoreValue && value != null && !value.Equals("null") || (ignoreValue != null && value != null && !ignoreValue.Equals(value)))
                {
                    StringBuilder builder = new StringBuilder(url);
                    return builder.Append("&").Append(descriptor).Append("=").Append(encode(value)).ToString();
                }
                else
                {
                    return url;
                }
            }
            catch (MCOpenApiRuntimeException wex)
            {
                throw new MCOpenApiRuntimeException(wex.Message, wex);
            }
        }

        public static string encode(string value)
        {
            return HttpUtility.UrlEncode(value);
        }
    }
}
