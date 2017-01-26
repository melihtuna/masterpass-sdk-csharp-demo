using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MasterCard.SDK;
using MasterCard.WalletSDK.Models;
using MasterCard.WalletSDK.Models.All;
using MasterCard.WalletSDK.Models.Common;
using MasterCard.WalletSDK.Models.Switch;
using MasterCard.SDK.Util;

using Checkout = MasterCard.WalletSDK.Models.All.Checkout;
using PrecheckoutDataRequest = MasterCard.WalletSDK.Models.All.PrecheckoutDataRequest;
using PrecheckoutDataResponse = MasterCard.WalletSDK.Models.All.PrecheckoutDataResponse;

namespace MasterCard.WalletSDK
{
    public class MasterPassService : Connector
    {
        public const String NULL_RESPONSE_PARAMETERS_ERROR = "ResponseParameters can not be null.";

	    //Request Token Response
	    public const String XOAUTH_REQUEST_AUTH_URL = "xoauth_request_auth_url";
	    public const String OAUTH_CALLBACK_CONFIRMED = "oauth_callback_confirmed";
	    public const String OAUTH_EXPIRES_IN = "oauth_expires_in";
	
	    //Request Token Response
	    public const String OAUTH_TOKEN_SECRET = "oauth_token_secret";
	
	    // Callback URL parameters
	    public const String OAUTH_TOKEN = "oauth_token";
	    public const String OAUTH_VERIFIER = "oauth_verifier";
        public const String CHECKOUT_RESOURCE_URL = "checkout_resource_url";
	
        // OAuth Parameters
        public const String ORIGIN_URL = "origin_url";

        // Redirect Parameters
	    public const String CHECKOUT_IDENTIFIER = "checkout_identifier";
        public const String ACCEPTABLE_CARDS = "acceptable_cards";
        public const String VERSION = "version";
        public const String SUPPRESS_SHIPPING_ADDRESS = "suppress_shipping_address";
        public const String ACCEPT_REWARDS_PROGRAM = "accept_reward_program";
        public const String SHIPPING_LOCATION_PROFILE = "shipping_location_profile";
        public const String WALLET_SELECTOR = "wallet_selector_bypass";
        public const String DEFAULT_XMLVERSION = "v1";
        public const String AUTH_LEVEL = "auth_level";
        public const String BASIC = "basic";
        public static Regex _XML_VERSION_REGEX = new Regex(@"v[0-9]+");
        public const String DEFAULT_XML_VERSION = "v1";
        
        public const String APPROVAL_CODE = "sample";

        public string originUrl { get; set; }

        public MasterPassService(string consumerKey, AsymmetricAlgorithm privateKey, string originUrl) : base(consumerKey, privateKey)
        {
            this.originUrl = originUrl;
        }

        public static MasterPassService FactoryMethod(string consumerKey, string keystorePath, string keystorePassword, string originUrl)
        {
            return new MasterPassService(consumerKey, GetPrivateKey(keystorePath, keystorePassword), originUrl);
        }

        protected static AsymmetricAlgorithm GetPrivateKey(string keystorePath, string keystorePassword)
        {
            X509Certificate2 mcIssuedCertificate = new X509Certificate2(keystorePath, keystorePassword);
            return mcIssuedCertificate.PrivateKey;
        }


        protected override OAuthParameters OAuthParametersFactory()
        {
            OAuthParameters oap = base.OAuthParametersFactory();
            oap.put(MasterPassService.REALM, "eWallet");
            //oap.put(MasterPassService.ORIGIN_URL, this.originUrl);
            return oap;
        }

        /// <summary>
        /// SDK: 
        /// This method captures the Checkout Resource URL and Request Token Verifier
        /// and uses these to request the Access Token.
        /// </summary>
        public AccessTokenResponse GetAccessToken(string accessTokenUrl, string requestToken, string oauthVerifier)
        {
            var oauthParams = OAuthParametersFactory();
            oauthParams.addParameter("oauth_token", requestToken);
            oauthParams.addParameter("oauth_verifier", oauthVerifier);

            var returnParams = doRequest(accessTokenUrl, WebRequestMethods.Http.Post, oauthParams, null);

            var message = returnParams[Connector.MESSAGE];
            var oparams = parseOAuthResponseParameters(message);
            AccessTokenResponse accessTokenResponse = new AccessTokenResponse();

            accessTokenResponse.AccessToken = oparams[MasterPassService.OAUTH_TOKEN];
            accessTokenResponse.OAuthSecret = oparams[MasterPassService.OAUTH_TOKEN_SECRET];

            return accessTokenResponse;
        }


        /// <summary>
        /// SDK: 
        /// This method handles the request token request and generation of the redirect URL.
        /// </summary>
        public RequestTokenResponse GetRequestTokenAndRedirectURL(string requestTokenUrl, string callbackUrl,
            string acceptedCards, string checkoutIdentifier, string xmlVersion, Boolean suppressShippingFlag, 
            Boolean rewardsProgramFlag, Boolean Auth_Level_Basic, string shippingProfile)
        {
            RequestTokenResponse requestTokenResponse = GetRequestToken(requestTokenUrl, callbackUrl);
            requestTokenResponse.RedirectUrl = GetConsumerSignInUrl(requestTokenResponse.RequestToken, requestTokenResponse.AuthorizeUrl,
                                                                    acceptedCards, checkoutIdentifier, xmlVersion, suppressShippingFlag,
                                                                    Auth_Level_Basic, rewardsProgramFlag, shippingProfile);
            return requestTokenResponse;
        }


        /// <summary>
        /// SDK: 
        /// This method takes a ShoppingCartRequest object, posts the data to MasterCard and returns
        /// a ShoppingCartResponse class.
        /// </summary>
        public ShoppingCartResponse PostShoppingCartData(string shoppingCartUrl, ShoppingCartRequest shoppingCartData)
        {
            // Question - does this method need to pass OAUTH_BODY_HASH to doRequest? --no, doRequest does it automatically
            // Question - do we need to catch a WebException, or does Connector.doRequest take care of it?

            XmlDocument xmldoc = Serializer<ShoppingCartRequest>.Serialize(shoppingCartData);
            string xml = xmldoc.OuterXml;
            Dictionary<string,string> requestResponse = doRequest(shoppingCartUrl, WebRequestMethods.Http.Post, xml);

            string message = requestResponse[Connector.MESSAGE];

            ShoppingCartResponse shoppingCartResponse = Serializer<ShoppingCartResponse>.Deserialize(message);            
            return shoppingCartResponse;
        }


        /// <summary>
        /// SDK: 
        /// This method takes a ShoppingCartRequest object, posts the data to MasterCard and returns
        /// a ShoppingCartResponse class.
        /// </summary>
        public MerchantInitializationResponse PostMerchantInitData(string merchantInitUrl, MerchantInitializationRequest merchantInitData)
        {
            // Question - does this method need to pass OAUTH_BODY_HASH to doRequest?
            // Question - do we need to catch a WebException, or does Connector.doRequest take care of it?

            XmlDocument xmldoc = Serializer<MerchantInitializationRequest>.Serialize(merchantInitData);
            Dictionary<string, string> requestResponse = doRequest(merchantInitUrl, WebRequestMethods.Http.Post, xmldoc.OuterXml);

            string message = requestResponse[Connector.MESSAGE];

            MerchantInitializationResponse merchantInitResponse = Serializer<MerchantInitializationResponse>.Deserialize(message);           
            return merchantInitResponse;
        }



        /// <summary>
        /// SDK: 
        /// This method retrieves the payment and shipping information
        /// for the current user/session.
        /// </summary>
        /// <returns>Returns a string (of XML) that contains the user's payment and 
        /// shipping information.</returns>
        public Checkout GetPaymentShippingResource(string checkoutResourceUrl, string accessToken)
        {
            var oauthParams = OAuthParametersFactory();
            oauthParams.addParameter("oauth_token", accessToken);

            Dictionary<string,string> requestResponse = doRequest(checkoutResourceUrl, WebRequestMethods.Http.Get, oauthParams, null);

            string message = requestResponse[Connector.MESSAGE];
            
            // Deserialize the result into our Checkout object.
            Checkout checkout = Serializer<Checkout>.Deserialize(message);

            // Return the checkout information to the caller.
            return checkout;
        }


        /// <summary>
        /// This method submits the receipt transaction list to MasterCard as a final step
        /// in the Wallet process.
        /// </summary>
        /// <param name="merchantTransactions"></param>
        /// <returns></returns>
        public MerchantTransactions PostCheckoutTransaction(String postbackUrl, MerchantTransactions merchantTransactions)
        {
            // Question - does this method need to pass OAUTH_BODY_HASH to doRequest?
            // Question - do we need to catch a WebException, or does Connector.doRequest take care of it?

            XmlDocument xmldoc = Serializer<MerchantTransactions>.Serialize(merchantTransactions);
            Dictionary<string, string> requestResponse = doRequest(postbackUrl, WebRequestMethods.Http.Post, xmldoc.OuterXml);

            string message = requestResponse[Connector.MESSAGE];

            MerchantTransactions returnTransactions = Serializer<MerchantTransactions>.Deserialize(message);

            return returnTransactions;
        }


        /// <summary>
        /// SDK: 
        /// Get the user's request token and store it in the current user session.
        /// </summary>
        public RequestTokenResponse GetRequestToken(string requestTokenURL, string callbackUrl)
        {
            var oauthParams = OAuthParametersFactory();
            oauthParams.addParameter(OAUTH_CALLBACK, callbackUrl);
            //oauthParams.addParameter(REALM, "eWallet");

            var resp = doRequest(requestTokenURL, WebRequestMethods.Http.Post, oauthParams, null);
            var returnParams = parseOAuthResponseParameters(resp[Connector.MESSAGE]);

            var response = new RequestTokenResponse();
            response.AuthorizeUrl = HttpUtility.UrlDecode(returnParams["xoauth_request_auth_url"]);
            response.RequestToken = returnParams["oauth_token"];
            response.CallbackConfirmed = ("true" == returnParams["oauth_callback_confirmed"]);
            response.OAuthExpiresIn = returnParams["oauth_expires_in"];
            response.OAuthSecret = returnParams["oauth_token_secret"];
            return response;
        }

        public PrecheckoutDataResponse GetPrecheckoutData(string precheckoutUrl, string longAccessToken, PrecheckoutDataRequest preCheckoutDataRequest)
        {
            var oauthParams = OAuthParametersFactory();
            oauthParams.addParameter(OAUTH_TOKEN, longAccessToken);

            XmlDocument xmldoc = Serializer<PrecheckoutDataRequest>.Serialize(preCheckoutDataRequest);
            Dictionary<string, string> requestResponse = doRequest(precheckoutUrl, WebRequestMethods.Http.Post, oauthParams, xmldoc.OuterXml);
            string message = requestResponse[Connector.MESSAGE];
            return Serializer<PrecheckoutDataResponse>.Deserialize(message);
        }

        private string GetConsumerSignInUrl(string requestToken, string authorizationUrl,
            string acceptedCards, string checkoutIdentifier, String xmlVersion, Boolean suppressShippingFlag, 
            Boolean authLevelBasicFlag, Boolean rewardsProgramFlag, string shippingProfile)
        {
            string paramString = EMPTY_STRING;

            xmlVersion = xmlVersion.ToLower();
            Match match = _XML_VERSION_REGEX.Match(xmlVersion);

            // Use v1 if xmlVersion does not match correct patern
            if (!match.Success)
            {
                xmlVersion = DEFAULT_XML_VERSION;
            }

            // Construct MasterCard specific Auth URL parameters according to what 
            // the caller has provided for accepted cards and international shipping.
            //Dictionary<string, string> masterCardAuthUrlParams = new Dictionary<string, string>();

            // Add the acceptable cards parameter if the call has provided a list of cards.
            if (!string.IsNullOrEmpty(acceptedCards))
                paramString += GetParamString(ACCEPTABLE_CARDS, acceptedCards, true);                

            // Add the checkout identifier.
            paramString += GetParamString(CHECKOUT_IDENTIFIER, checkoutIdentifier);

            paramString += GetParamString(OAUTH_TOKEN, requestToken);

            // Add the version
            paramString += GetParamString(VERSION, xmlVersion);

            if (!xmlVersion.Equals(DEFAULT_XML_VERSION))
            {
                if (authLevelBasicFlag)
                {
                    paramString += GetParamString(AUTH_LEVEL, BASIC);
                }
                // Add the shipping suppression flag
                if (suppressShippingFlag)
                {
                    paramString += GetParamString(SUPPRESS_SHIPPING_ADDRESS, suppressShippingFlag.ToString().ToLower());
                }

            }
            if (Convert.ToInt16(xmlVersion.Substring(1)) >= 4)
            {
                if (rewardsProgramFlag)
                {
                    paramString += GetParamString(ACCEPT_REWARDS_PROGRAM, true.ToString().ToLower());
                }


                // Add a shipping profile only if the shipping suppression is false and there is a redirectShippingProfile
                if (shippingProfile != null && shippingProfile.Length > 0)
                {
                    paramString += GetParamString(SHIPPING_LOCATION_PROFILE, shippingProfile);
                }
            }

            // Construct the Auth URL w/parameters and return it to the caller.
            return HttpUtility.UrlDecode(authorizationUrl + paramString);
        }


	    /**
	     * SDK:
	     * Method to create the URL with GET Parameters
	     *
	     * @param $key
	     * @param $value
	     * @param $firstParam
	     *
	     * @return string
	     */
	    private string GetParamString(string key, string value, bool firstParam = false) {
		    var paramString = EMPTY_STRING;
			
		    if (firstParam) {
			    paramString += QUESTION_MARK;
		    } else {
			    paramString += AMP;
		    }
		    paramString += key + EQUALS + value;
			
		    return paramString;
	    }



    }
}
