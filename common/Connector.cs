using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security;
using System.Web;


namespace MasterCard.SDK
{
    /// <summary>
    /// This class defines the base functionality of the MasterCard OpenAPI.
    /// </summary>
    public class Connector
    {
        public static string EMPTY_STRING = "";
        public static string EQUALS = "=";
        public static string AMP = "&";
        public static string QUESTION_MARK = "?";
        public static string MESSAGE = "Message";
        public static string HTTP_CODE = "HttpCode";
        public static string COLON_2X_BACKSLASH = "://";
        public static int SC_MULTIPLE_CHOICES = 300;
        public static string REALM = "realm";
        public static string OAUTH_CALLBACK = "oauth_callback";
        public static string OAUTH_CONSUMER_KEY = "oauth_consumer_key";
        public static string OAUTH_VERSION = "oauth_version";
        public static string OAUTH_SIGNATURE = "oauth_signature";
        public static string OAUTH_SIGNATURE_METHOD = "oauth_signature_method";
        public static string OAUTH_TIMESTAMP = "oauth_timestamp";
        public static string OAUTH_NONCE = "oauth_nonce";
        public static string OAUTH_BODY_HASH = "oauth_body_hash";
        public static string ONE_POINT_ZERO = "1.0";
        public static string RSA_SHA1 = "RSA-SHA1";
        public static string OAUTH_START_STRING = "OAuth ";
        public static string COMMA = ",";
        public static string DOUBLE_QOUTE = "\"";
        public static string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };
        public const String HTML_TAG = "<html>";
        public const String BODY_OPENING_TAG = "<body>";
        public const String BODY_CLOSING_TAG = "</body>";

        public const String NULL_RESPONSE_PARAMETERS_ERROR = "ResponseParameters can not be null.";
        public const String NULL_PARAMETERS_ERROR = "Null parameters passed to method call";
        public const String NULL_PRIVATEKEY_ERROR_MESSAGE = "Private Key is null";

        public const String CONTENT_TYPE = "content-type";
        public const String CONTENT_LENGTH = "content-length";
        public const String APPLICATION_XML = "application/xml; charset=UTF-8";
        public const String AUTHORIZATION = "Authorization";

        //private static Random random = new Random();
        private static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        private static UTF8Encoding encoder = new UTF8Encoding();

        public string SignatureBaseString { get { return _signatureBaseString; } }
        public String AuthHeader { get { return _authHeader; } }
        private string ConsumerKey { get; set; }
        private AsymmetricAlgorithm privateKey { get; set; }
        private string _signatureBaseString;
        private String _authHeader;


        /// <summary>
        /// This constructor allows the caller to provide a preloaded private key for use 
        /// when OAuth calls are made.
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="privateKey"></param>
        public Connector(string consumerKey, AsymmetricAlgorithm privateKey)
        {
            this.ConsumerKey = consumerKey;
            this.privateKey = privateKey;

            // Turns the handling of a 100 HTTP server response ON
            System.Net.ServicePointManager.Expect100Continue = true;
        }

        /// <summary> 
        /// This method will HTML encode all special characters in the string parameter
        /// </summary>
        /// <returns>The parameter string that was passed in with all special characters HTML encoded</returns>
        
        public string AllHtmlEncode(string value)
        {
            // call the normal HtmlEncode method first
            char[] chars = value.ToCharArray();  //HttpUtility.HtmlEncode(value).ToCharArray();
            StringBuilder encodedValue = new StringBuilder();

            // Encode all the multi byte characters that the normal encoder misses
            foreach (char c in chars)
            {
                if ((int)c > 127) // above normal ASCII
                    encodedValue.Append("&#" + (int)c + ";");
                else
                    encodedValue.Append(c);
            }
            return encodedValue.ToString();
        }

        /******************** protected and support methods ***************************************************************************************************************************/
        protected virtual OAuthParameters OAuthParametersFactory()
        {
            OAuthParameters oparams = new OAuthParameters();
            oparams.addParameter(OAUTH_CONSUMER_KEY, ConsumerKey);
            oparams.addParameter(OAUTH_NONCE, getNonce());
            oparams.addParameter(OAUTH_TIMESTAMP, getTimestamp());
            oparams.addParameter(OAUTH_SIGNATURE_METHOD, RSA_SHA1);
            oparams.addParameter(OAUTH_VERSION, ONE_POINT_ZERO);
            return oparams;
        }

        /// <summary>
        /// This method receives a name value collection and returns the value of the 
        /// specified parameter.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected static string ParseResponseParameter(NameValueCollection collection, string parameter)
        {
            string value = (collection[parameter] ?? "").Trim();
            return (value.Length > 0) ? value : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestMethod"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        protected Dictionary<string, string> doRequest(string url, string requestMethod, string body)
        {
            return this.doRequest(url, requestMethod, OAuthParametersFactory(), body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestMethod"></param>
        /// <returns></returns>
        protected Dictionary<string, string> doRequest(string url, string requestMethod)
        {
            return this.doRequest(url, requestMethod, OAuthParametersFactory(), null);
        }

        /// <summary>
        /// Method to handle all OpenApi connection details.
        /// </summary>
        /// <param name="httpsURL"></param>
        /// <param name="requestMethod"></param>
        /// <param name="oparams"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> doRequest(String httpsURL, String requestMethod,
                            OAuthParameters oparams, String body)
        {
            try
            {
                if (privateKey == null)
                {
                    throw new MCOpenApiRuntimeException(NULL_PRIVATEKEY_ERROR_MESSAGE);
                }
                if (body != null && body.Length > 0)
                {
                    oparams = setOauthBodyHashParameter(body, oparams);
                }

                HttpWebRequest con = setupConnection(httpsURL, requestMethod, oparams, body);

                if (body != null)
                {
                    writeBodyToConnection(body, con);
                }

                return checkForErrorsAndReturnRepsonse(con);
            }
            catch (Exception e)
            {
                throw new MCOpenApiRuntimeException(e.Message, e);
            }
        }

        /* -------- private Methods ------------------------------------------------------------------------------------------------------------------ */

        /// <summary>
        /// Method to add the Oauth body hash to the OAuthParameters
        /// </summary>
        /// <param name="body"></param>
        /// <param name="oparams"></param>
        /// <returns></returns>
        private OAuthParameters setOauthBodyHashParameter(String body, OAuthParameters oparams)
        {
            byte[] bodyStringBytes = encoder.GetBytes(body);
            SHA1 sha = new SHA1CryptoServiceProvider();
            string encodedHash = Convert.ToBase64String(sha.ComputeHash(bodyStringBytes));
            oparams.addParameter(OAUTH_BODY_HASH, encodedHash);
            return oparams;
        }

        /// <summary>
        /// Setup the HttpWebRequest and connection headers
        /// </summary>
        /// <param name="httpsURL"></param>
        /// <param name="requestMethod"></param>
        /// <param name="oparams"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private HttpWebRequest setupConnection(String httpsURL,
            String requestMethod, OAuthParameters oparams, String body)
        {
            Uri url = new Uri(httpsURL);
            HttpWebRequest con = (HttpWebRequest)WebRequest.Create(url);
            con.Method = requestMethod;
            con.Headers.Add(AUTHORIZATION, buildAuthHeaderString(httpsURL, requestMethod, oparams));
            con.UserAgent = "MC Open API OAuth Framework v1.0-C#";
            con.Accept = APPLICATION_XML;
            

            // Setting the Content Type header depending on the body
            if (body != null)
            {
                con.ContentType = APPLICATION_XML;
                //con.ContentLength = body.Length;
                con.ContentLength = Encoding.UTF8.GetByteCount(body);
            }
            return con;
        }

        /// <summary>
        /// Builds the Authorization header 
        /// </summary>
        /// <param name="httpsURL"></param>
        /// <param name="requestMethod"></param>
        /// <param name="oparams"></param>
        /// <returns></returns>
        private string buildAuthHeaderString(String httpsURL, String requestMethod, OAuthParameters oparams)
        {
            generateAndSignSignature(httpsURL, requestMethod, oparams);
            StringBuilder buffer = new StringBuilder();
            buffer.Append(OAUTH_START_STRING);
            buffer = parseParameters(buffer, oparams);
            this._authHeader = buffer.ToString();
            return buffer.ToString();
        }

        /// <summary>
        /// Method to build a comma delimited list of key/value string for the signature base string and authorization header
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="oparams"></param>
        /// <returns></returns>
        private StringBuilder parseParameters(StringBuilder buffer, OAuthParameters oparams)
        {
            foreach (KeyValuePair<string, SortedSet<string>> key in oparams.getBaseParameters())
            {
                SortedSet<string> value = key.Value;
                parseSortedSetValues(buffer, key.Key, value);
                buffer.Append(COMMA);
            }
            buffer.Remove(buffer.Length - 1, 1);
            return buffer;
        }

        /// <summary>
        /// Helper method to build the comma delimited list of key/values in a sorted set
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="key"></param>
        /// <param name="paramap"></param>
        /// <returns></returns>
        private StringBuilder parseSortedSetValues(StringBuilder buffer, String key, SortedSet<string> paramap)
        {
            foreach (string value in paramap.Keys)
            {
                buffer.Append(key).Append(EQUALS).Append(DOUBLE_QOUTE).Append(UrlEncodeRfc3986(value)).Append(DOUBLE_QOUTE);
            }
            return buffer;
        }
        /// <summary>
        /// Splits the responseParameters string into Key/Value pairs in the returned Dictionary 
        /// </summary>
        /// <param name="responseParameters"></param>
        /// <returns></returns>
        protected Dictionary<string, string> parseOAuthResponseParameters(string responseParameters)
        {
            if (responseParameters == null)
            {
                throw new MCOpenApiRuntimeException(NULL_RESPONSE_PARAMETERS_ERROR);
            }

            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] parameters = responseParameters.Split('&');

            foreach (string value in parameters)
            {
                String[] keyValue = value.Split('=');
                if (keyValue.Length == 2)
                {
                    result.Add(keyValue[0], keyValue[1]);
                }
            }
            // if the keyValue length is not 2 then they will be ignored
            return result;
        }

        /// <summary>
        /// Generates and signs the signature base string
        /// </summary>
        /// <param name="httpsURL"></param>
        /// <param name="requestMethod"></param>
        /// <param name="oparams"></param>
        /// <returns></returns>
        private string generateAndSignSignature(String httpsURL, String requestMethod, OAuthParameters oparams)
        {
            OAuthParameters sbsParams = new OAuthParameters();
            sbsParams.putAll(oparams.getBaseParameters());

            string realm = null;
            if (sbsParams.get(REALM) != EMPTY_STRING)
            {
                realm = sbsParams.get(REALM);
                sbsParams.remove(REALM, null);
            }
            String baseString;
            baseString = generateSignatureBaseString(httpsURL, requestMethod, sbsParams);
            _signatureBaseString = baseString;

            String signature;
            signature = sign(baseString, privateKey);
            oparams.addParameter(OAUTH_SIGNATURE, signature);
            if (realm != null)
            {
                sbsParams.put(REALM, realm);
            }
            return signature;
        }

        /// <summary>
        /// Method to signthe signature base string. 
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="KeyStore"></param>
        /// <returns></returns>
        private string sign(string baseString, AsymmetricAlgorithm KeyStore)
        {
            byte[] baseStringBytes = encoder.GetBytes(baseString);

            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)KeyStore;
            // Hash the data
            SHA1Managed sha1 = new SHA1Managed();
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] hash = sha1.ComputeHash(baseStringBytes);

            // Sign the hash
            byte[] SignedHashValue = csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
            return Convert.ToBase64String(SignedHashValue);
        }

        /// <summary>
        /// Generates the signature base string
        /// </summary>
        /// <param name="httpsURL"></param>
        /// <param name="requestMethod"></param>
        /// <param name="oparams"></param>
        /// <returns></returns>
        private string generateSignatureBaseString(string httpsURL, string requestMethod, OAuthParameters oparams)
        {
            Uri requestUri = parseUrl(httpsURL);
            String encodedBaseString;
            encodedBaseString = UrlEncodeRfc3986(requestMethod.ToUpper()) + AMP + UrlEncodeRfc3986(normalizeUrl(requestUri)) + AMP + UrlEncodeRfc3986(normalizeParameters(httpsURL, oparams));
            return encodedBaseString;
        }

        /// <summary>
        /// Normalizes the request URL before adding it to the signature base string
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static String normalizeUrl(Uri uri)
        {
            return uri.Scheme + COLON_2X_BACKSLASH + uri.Host + uri.AbsolutePath;
        }

        /// <summary>
        /// Normalize the OAuth parameters as they are added to the signature base string
        /// </summary>
        /// <param name="httpUrl"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        private static String normalizeParameters(String httpUrl, OAuthParameters requestParameters)
        {
            // add the querystring to the base string (if one exists)
            if (httpUrl.IndexOf(QUESTION_MARK) > 0)
            {
                NameValueCollection queryParameters = HttpUtility.ParseQueryString(httpUrl.Substring(httpUrl.IndexOf(QUESTION_MARK) + 1));
                foreach (string key in queryParameters)
                {
                    requestParameters.put(key, UrlEncodeRfc3986(queryParameters[key]));
                }
            }
            // piece together the base string, encoding each key and value
            StringBuilder paramString = new StringBuilder();
            foreach (KeyValuePair<string, SortedSet<string>> kvp in requestParameters.getBaseParameters())
            {
                if (kvp.Value.Count == 0)
                {
                    continue; // Skip if key doesn't have any values
                }
                if (paramString.Length > 0)
                {
                    paramString.Append(AMP);
                }
                int tempCounter = 0;
                foreach (string value in kvp.Value.Keys)
                {
                    paramString.Append(UrlEncodeRfc3986(kvp.Key)).Append(EQUALS).Append((value));
                    if (tempCounter != kvp.Value.Count - 1)
                    {
                        paramString.Append(AMP);
                    }
                    tempCounter++;
                }
            }
            return paramString.ToString();
        }

        /// <summary>
        /// Converts a string URL to a Uri class
        /// </summary>
        /// <param name="httpsURL"></param>
        /// <returns></returns>
        private Uri parseUrl(string httpsURL)
        {
            // validate the request url
            if ((httpsURL == null) || (httpsURL.Length == 0))
            {
                throw new MCOpenApiRuntimeException(NULL_PARAMETERS_ERROR);
            }
            return new Uri(httpsURL);
        }

        /// <summary>
        /// Reads the Http response and checks the response for errors.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private Dictionary<string, string> checkForErrorsAndReturnRepsonse(HttpWebRequest connection)
        {
            try
            {
                HttpWebResponse webResp = (HttpWebResponse)connection.GetResponse();

                if ((int)webResp.StatusCode >= SC_MULTIPLE_CHOICES)
                {
                    String message = readResponse(webResp);
                    // Cut the html off of the error message and leave the body
                    if (message.Contains(HTML_TAG))
                    {
                        message = message.Substring(message.IndexOf(BODY_OPENING_TAG) + 6, message.IndexOf(BODY_CLOSING_TAG));
                    }
                    throw new MCOpenApiRuntimeException(message);
                }
                else
                {
                    Dictionary<String, String> responseMap = new Dictionary<String, String>();
                    responseMap.Add(MESSAGE, readResponse(webResp));
                    responseMap.Add(HTTP_CODE, webResp.StatusCode.ToString());

                    return responseMap;
                }
            }
            catch (WebException wex)
            {
                String message = readResponse((HttpWebResponse)wex.Response);
                // Cut the html off of the error message and leave the body
                if (message.Contains(HTML_TAG))
                {
                    message = message.Substring(message.IndexOf(BODY_OPENING_TAG) + 6, message.IndexOf(BODY_CLOSING_TAG));
                }
                throw new MCOpenApiRuntimeException(message);
            }
        }

        /// <summary>
        /// Read the response from the Http reponse
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string readResponse(HttpWebResponse response)
        {
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            return responseFromServer;
        }

        /// <summary>
        /// Writes the body to an estiblished HttpWebRequest
        /// </summary>
        /// <param name="body"></param>
        /// <param name="con"></param>
        private void writeBodyToConnection(String body, HttpWebRequest con)
        {
            byte[] encodedBody = encoder.GetBytes(body);
            Stream newStream = con.GetRequestStream();

            newStream.Write(encodedBody, 0, encodedBody.Length);
            newStream.Close();
        }

        /// <summary>
        /// Generates a 8 character Nonce
        /// </summary>
        /// <returns></returns>
        private static String getNonce()
        {
            int length = 8;
            var nonceString = new StringBuilder();
            byte[] randomBytes = new byte[sizeof(int)];

            for (int i = 0; i < length; i++)
            {
                random.GetBytes(randomBytes);
                int val = BitConverter.ToInt32(randomBytes, 0);
                val &= 0x7fffffff; // Bitwise or to produce absolute value numbers
                nonceString.Append(validChars[val % validChars.Length]);
            }

            return nonceString.ToString();
        }

        //private static String getNonce()
        //{
        //    int length = 17;
        //    var nonceString = new StringBuilder();
        //    for (int i = 0; i < length; i++)
        //    {
        //        nonceString.Append(validChars[random.Next(0, validChars.Length - 1)]);
        //    }
        //    return nonceString.ToString();
        //}

        /// <summary>
        /// Generates a timestamp
        /// </summary>
        /// <returns></returns>
        private static String getTimestamp()
        {
            long ticks = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
            ticks /= 10000000; //Convert windows ticks to seconds
            return ticks.ToString();
        }

        /// <summary>
        /// URL encodes to the RFC3986 specification
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UrlEncodeRfc3986(string value)
        {
            StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));
            for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                escaped.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
            }
            return escaped.ToString();
        }
    }

}
