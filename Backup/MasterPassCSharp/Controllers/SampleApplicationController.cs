using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using MasterCard.SDK;
using MasterCard.WalletWebContent.Models;
using MasterCard.WalletSDK;
using MasterCard.WalletSDK.Models;
using MasterCard.WalletSDK.Models.All;
using MasterCard.WalletSDK.Models.Common;
using MasterCard.WalletSDK.Models.Switch;
using allModels = MasterCard.WalletSDK.Models.All;

using Checkout = MasterCard.WalletSDK.Models.All.Checkout;
using PairingDataType = MasterCard.WalletSDK.Models.All.PairingDataType;
using PairingDataTypes = MasterCard.WalletSDK.Models.All.PairingDataTypes;
using PairingDataTypeType = MasterCard.WalletSDK.Models.All.PairingDataTypeType;
using PrecheckoutDataRequest = MasterCard.WalletSDK.Models.All.PrecheckoutDataRequest;
using PrecheckoutDataResponse = MasterCard.WalletSDK.Models.All.PrecheckoutDataResponse;

namespace MasterCard.WalletWebContent.Controllers
{
    /// <summary>
    /// The SampleApplicationController class demonstrates how to interact with the MasterCard API
    /// using the SDK, which is also provided in this solution.
    /// 
    /// Comments have been provided for each MVC Action Method below, demonstrating the 
    /// API's workflow.
    /// </summary>
    public class SampleApplicationController : Controller
    {
        public MasterPassData appData;
        public MasterPassService service;

        // Static vales used int the Checkout simulation
        long ShippingCharge = 895;
        long Tax = 348;


        public SampleApplicationController()
        {

        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (Session["data"] != null)
                this.appData = (MasterPassData)Session["data"];
            else
                this.appData = new MasterPassData();
        }
        
        
        string RequestToken
        {
            get
            {
                if (null != Session["requestToken"])
                    return Session["requestToken"].ToString();
                return string.Empty;
            }
            set
            {
                Session["requestToken"] = value;
            }
        }
        string AccessToken
        {
            get
            {
                if (Session["accessToken"] != null)
                    return Session["accessToken"].ToString();
                return string.Empty;
            }
            set
            {
                Session["accessToken"] = value;
            }
        }


        public ActionResult index()
        {
            Session["data"] = new MasterPassData();
            return View(appData);
        }

        public ActionResult O1_GetRequestToken(string xmlVersionDropdown, string shippingSuppressionDropdown, string[] acceptedCardsCheckbox, string privateLabelText, string rewardsDropdown, string authenticationCheckBox, string shippingProfileDropdown)
        {
            ProcessParameters(xmlVersionDropdown, shippingSuppressionDropdown, acceptedCardsCheckbox, privateLabelText, rewardsDropdown, authenticationCheckBox, shippingProfileDropdown);
            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                // Create the user sign-in url.
                //var requestTokenResponse = service.GetRequestTokenAndRedirectURL(appData.RequestUrl, appData.CallbackUrl, appData.AcceptedCards, appData.CheckoutIdentifier, appData.XmlVersion, appData.ShippingSuppression, appData.RewardsProgram, appData.AuthLevelBasic, shippingProfileDropdown);
                appData.requestTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                RequestToken = appData.requestTokenResponse.RequestToken;
                appData.RequestToken = appData.requestTokenResponse.RequestToken;

                //ViewBag.UserSignInUrl = userSignInUrl;
                ViewBag.RequestToken = RequestToken;
                ViewBag.AuthorizeUrl = appData.requestTokenResponse.AuthorizeUrl;
                ViewBag.OauthExpiresIn = appData.requestTokenResponse.OAuthExpiresIn;
                ViewBag.OauthTokenSecret = appData.requestTokenResponse.OAuthSecret;
                ViewBag.RedirectUrl = appData.requestTokenResponse.RedirectUrl;

                var pairingTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                ViewBag.PairingToken = pairingTokenResponse.RequestToken;
                ViewBag.PairingAuthorizeUrl = pairingTokenResponse.AuthorizeUrl;
                ViewBag.PairingOauthExpiresIn = pairingTokenResponse.OAuthExpiresIn;
                ViewBag.PairingOauthTokenSecret = pairingTokenResponse.OAuthSecret;
            }
            catch (WebException webex)
            {
                ViewBag.ErrorMessage = webex.Message;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            ViewBag.LightboxUrl = appData.LightboxUrl;
            ViewBag.RequestUrl = appData.RequestUrl;
            ViewBag.RewardsProgram = appData.RewardsProgram ? "true" : "false";
            if (service != null)
            {
                ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.AuthHeader = service.AuthHeader;
            }

            Session["data"] = appData;
            return View();
        }

         public ActionResult O2_ShoppingCart()
        {
            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                // Set the post transaction URL.
                ViewBag.ShoppingCartUrl = appData.ShoppingCartUrl;
                ViewBag.RewardsProgram = appData.RewardsProgram ? "true" : "false";
                ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
                ViewBag.AcceptedCards = appData.AcceptedCards;

                //appData.requestTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                //RequestToken = appData.requestTokenResponse.RequestToken;
                //appData.RequestToken = appData.requestTokenResponse.RequestToken;

                ShoppingCartRequest shoppingCartRequest = ParseShoppingCartFile(RequestToken);
                XmlDocument xmldoc = Serializer<ShoppingCartRequest>.Serialize(shoppingCartRequest);
                XElement shoppingRequestElement = XElement.Parse(xmldoc.OuterXml);
                ViewBag.ShoppingCartRequest = shoppingRequestElement.ToString().Trim();

                ShoppingCartResponse shoppingCartResponse = service.PostShoppingCartData(appData.ShoppingCartUrl, shoppingCartRequest);

                if (shoppingCartResponse != null)
                {
                    XmlDocument xmlTransactions = Serializer<ShoppingCartResponse>.Serialize(shoppingCartResponse);
                    XElement shoppingCartElement = XElement.Parse(xmlTransactions.OuterXml);
                    ViewBag.ShoppingCartResponse = service.AllHtmlEncode(shoppingCartElement.ToString().Trim());
                }
                ViewBag.AuthHeader = service.AuthHeader;
                ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";
                ViewBag.CallbackDomain = appData.CallbackDomain;
                ViewBag.CallbackUrl = appData.CallbackUrl;
                ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;
                ViewBag.LightboxUrl = appData.LightboxUrl;
                ViewBag.PairingToken = appData.PairingToken;
                ViewBag.PairingCallbackUrl = appData.PairingCallbackUrl;
                ViewBag.RequestToken = appData.requestTokenResponse.RequestToken;
                ViewBag.ShippingSuppression = appData.ShippingSuppression ? "true" : "false";
                ViewBag.SignatureBaseString = service.SignatureBaseString;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            Session["data"] = appData;
            return View();
        }

         public ActionResult O2_MerchantInit(string checkout)
         {
             Boolean isCheckout = checkout == "true";
             try
             {
                 // Our .p12 cert file is stored in our local /Certs folder.
                 // Map this file to get the physical server path.
                 var certPath = Request.MapPath(appData.KeystorePath);

                 // Create the Connector.
                 service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                 MerchantInitializationResponse merchantInitResponse;
                 MerchantInitializationRequest merchantInitRequest;

                 if (isCheckout)
                 {
                     ShoppingCartRequest shoppingCartRequest;
                     ShoppingCartResponse shoppingCartResponse;

                     // Get Request Token
                     appData.requestTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                     RequestToken = appData.requestTokenResponse.RequestToken;
                     appData.RequestToken = appData.requestTokenResponse.RequestToken;
                     ViewBag.RequestToken = appData.requestTokenResponse.RequestToken;
                     ViewBag.AuthorizationUrl = appData.requestTokenResponse.AuthorizeUrl;
                     ViewBag.OAuthExpiresIn = appData.requestTokenResponse.OAuthExpiresIn;
                     ViewBag.OAuthTokenSecret = appData.requestTokenResponse.OAuthSecret;

                     // Get Pairing Token
                     var pairingTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                     appData.PairingToken = pairingTokenResponse.RequestToken;                     

                     // Post Merchant Init Data
                     merchantInitRequest = this.ParseMerchantInitFile(appData.PairingToken);
                     merchantInitResponse = service.PostMerchantInitData(appData.MerchantInitUrl, merchantInitRequest);

                     // Post Shopping Cart
                     shoppingCartRequest = ParseShoppingCartFile(RequestToken);
                     appData.ShoppingCartRequest = XElement.Parse(Serializer<ShoppingCartRequest>.Serialize(shoppingCartRequest).OuterXml).ToString();
                     ViewBag.ShoppingCartRequest = appData.ShoppingCartRequest;

                     shoppingCartResponse = service.PostShoppingCartData(appData.ShoppingCartUrl, shoppingCartRequest);
                     appData.ShoppingCartResponse = XElement.Parse(Serializer<ShoppingCartResponse>.Serialize(shoppingCartResponse).OuterXml).ToString();
                     ViewBag.ShoppingCartResponse = appData.ShoppingCartResponse;
                 }
                 else
                 {
                     //  postMerchantInitData
                     merchantInitRequest = this.ParseMerchantInitFile(appData.PairingToken);
                     merchantInitResponse = service.PostMerchantInitData(appData.MerchantInitUrl, merchantInitRequest);
                 }

                 appData.MerchantInitRequest = Serializer<MerchantInitializationRequest>.Serialize(merchantInitRequest).OuterXml;
                 appData.MerchantInitResponse = Serializer<MerchantInitializationResponse>.Serialize(merchantInitResponse).OuterXml;
 
                 ViewBag.AcceptedCards = appData.AcceptedCards;
                 ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
                 ViewBag.AuthHeader = service.AuthHeader;
                 ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";
             
                 ViewBag.Checkout = checkout;
                 ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;

                 ViewBag.LightboxUrl = appData.LightboxUrl;

                 ViewBag.MerchantInitRequest = appData.MerchantInitRequest;
                 ViewBag.MerchantInitResponse = appData.MerchantInitResponse;
                 ViewBag.MerchantInitUrl = appData.MerchantInitUrl;

                 ViewBag.PairingCallbackPath = appData.PairingCallbackPath;
                 ViewBag.PairingCallbackUrl = appData.PairingCallbackUrl;
                 ViewBag.PairingToken = appData.PairingToken;
             
                 ViewBag.RequestUrl = appData.RequestUrl;
                 ViewBag.RewardsProgram = appData.RewardsProgram ? "true" : "false";

                 ViewBag.ShippingSuppression = appData.ShippingSuppression ? "true" : "false";
                 ViewBag.ShoppingCartUrl = appData.ShoppingCartUrl;
                 ViewBag.SignatureBaseString = service.SignatureBaseString;
             }
             catch (Exception ex)
             {
                 ViewBag.ErrorMessage = ex.Message;
             }

             
             Session["data"] = appData;
             return View();
         }

        [HttpGet]
        public ActionResult O3_Callback(string oauth_token, string oauth_verifier, string checkout_resource_url)
        {
            // Save the checkout resource URL to session.
            appData.CheckoutUrl = checkout_resource_url;
            appData.Verifier = oauth_verifier;
            RequestToken = oauth_token;
            appData.RequestToken = oauth_token;

            ViewBag.RequestToken = oauth_token;
            ViewBag.Verifier = oauth_verifier;
            ViewBag.CheckoutUrl = checkout_resource_url;

            Session["data"] = appData;
            return View();
        }

        [HttpGet]
        public ActionResult O3_PairingCallback(string pairing_token, string pairing_verifier)
        {
            appData.PairingToken = pairing_token;
            appData.PairingVerifier = pairing_verifier;
            ViewBag.PairingToken = pairing_token;
            ViewBag.PairingVerifier = pairing_verifier;

            try
            {

                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);
                appData.longAccessTokenResponse = service.GetAccessToken(appData.AccessUrl, pairing_token, pairing_verifier);

                if (appData.longAccessTokenResponse != null)
                {
                    appData.LongAccessToken = appData.longAccessTokenResponse.AccessToken;
                    appData.LongAccessSecret = appData.longAccessTokenResponse.OAuthSecret;
                    ViewBag.LongAccessToken = appData.longAccessTokenResponse.AccessToken;
                    ViewBag.LongAccessSecret = appData.longAccessTokenResponse.OAuthSecret;
                    HttpCookie cookie = new HttpCookie("longAccessToken", appData.longAccessTokenResponse.AccessToken);
                    Response.Cookies.Add(cookie);
                }

                ViewBag.AuthHeader = service.AuthHeader;
                ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.AccessUrl = appData.AccessUrl;

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }


            Session["data"] = appData;
            return View();
        }


        public ActionResult O4_GetAccessToken()
        {
            var accessTokenResponse = new AccessTokenResponse();

            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                accessTokenResponse = service.GetAccessToken(appData.AccessUrl, RequestToken, appData.Verifier);
                appData.accessTokenResponse = accessTokenResponse; 
                if (accessTokenResponse != null)
                {
                    appData.AccessToken = accessTokenResponse.AccessToken;
                }

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            ViewBag.CheckoutUrl = appData.CheckoutUrl;
            ViewBag.AccessUrl = appData.AccessUrl;
            ViewBag.AccessToken = accessTokenResponse.AccessToken; //AccessToken;
            ViewBag.TokenSecret = accessTokenResponse.OAuthSecret;

            if (service != null)
            {
                ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.AuthHeader = service.AuthHeader;
            }

            Session["data"] = appData;
            return View();
        }

        public ActionResult O5_PreCheckout()
        {
            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);
                PrecheckoutDataRequest request = GetPrecheckoutDataRequest();
                appData.PreCheckoutRequest = XElement.Parse(Serializer<PrecheckoutDataRequest>.Serialize(request).OuterXml).ToString();
                PrecheckoutDataResponse response = service.GetPrecheckoutData(appData.PreCheckoutUrl, appData.LongAccessToken, request);
                if (response.PrecheckoutData.Contact.FirstName == null && response.PrecheckoutData.Contact.LastName == null)
                {
                    response.PrecheckoutData.Contact = null;
                }
                if (response.PrecheckoutData.RewardPrograms.Count == 0)
                {
                    response.PrecheckoutData.RewardPrograms = null;
                }
                if (response.PrecheckoutData.Cards.Count == 0)
                {
                    response.PrecheckoutData.Cards = null;
                }
                if (response.PrecheckoutData.ShippingAddresses.Count == 0)
                {
                    response.PrecheckoutData.ShippingAddresses = null;
                }
                appData.PreCheckoutData = response.PrecheckoutData;
                appData.LongAccessToken = response.LongAccessToken;
                appData.WalletName = response.PrecheckoutData.WalletName;
                appData.ConsumerWalletId = response.PrecheckoutData.ConsumerWalletId;
                appData.PrecheckoutTransactionId = response.PrecheckoutData.PrecheckoutTransactionId;
                appData.PrecheckoutResponse = XElement.Parse(Serializer<PrecheckoutDataResponse>.Serialize(response).OuterXml).ToString();
                appData.PrecheckoutDataXml = XElement.Parse(Serializer<PrecheckoutDataResponse>.Serialize(response).OuterXml).ToString();
                appData.PrecheckoutDataJson = new JavaScriptSerializer().Serialize(response);

                if (response.PrecheckoutData.ShippingAddresses != null)
                {
                    if (response.PrecheckoutData.ShippingAddresses.Count > 0)
                    {
                        appData.PrecheckoutShippingId = response.PrecheckoutData.ShippingAddresses.ElementAt(0).AddressId;
                    }
                }
                if (response.PrecheckoutData.Cards != null)
                {
                    if (response.PrecheckoutData.Cards.Count > 0)
                    {
                        appData.PrecheckoutCardid = response.PrecheckoutData.Cards.ElementAt(0).CardId;
                    }
                }


                appData.requestTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                RequestToken = appData.requestTokenResponse.RequestToken;
                appData.RequestToken = appData.requestTokenResponse.RequestToken;

                ShoppingCartRequest shoppingCartRequest = ParseShoppingCartFile(RequestToken);
                ShoppingCartResponse shoppingCartResponse = service.PostShoppingCartData(appData.ShoppingCartUrl, shoppingCartRequest);
                appData.ShoppingCartRequest = XElement.Parse(Serializer<ShoppingCartRequest>.Serialize(shoppingCartRequest).OuterXml).ToString();
                appData.ShoppingCartResponse = XElement.Parse(Serializer<ShoppingCartResponse>.Serialize(shoppingCartResponse).OuterXml).ToString();

                ViewBag.RequestToken = RequestToken;
                ViewBag.RequestTokenAuthorizationUrl = appData.requestTokenResponse.AuthorizeUrl;
                ViewBag.RequestTokenExpiresIn = appData.requestTokenResponse.OAuthExpiresIn + (appData.requestTokenResponse.OAuthExpiresIn != null ?  " Seconds" : "");
                ViewBag.RequestTokenSecret = appData.requestTokenResponse.OAuthSecret;
                ViewBag.ShoppingCartRequest = appData.ShoppingCartRequest;
                ViewBag.ShoppingCartResponse = appData.ShoppingCartResponse;

                ViewBag.LightboxUrl = appData.LightboxUrl;
                ViewBag.AuthHeader = service.AuthHeader;
                ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.PreCheckoutRequest = appData.PreCheckoutRequest;
                ViewBag.PreCheckoutUrl = appData.PreCheckoutUrl;
                ViewBag.PreCheckoutResponse = appData.PrecheckoutDataXml;
                ViewBag.PreCheckoutResponseJson = appData.PrecheckoutDataJson;
                ViewBag.RequestURL = appData.RequestUrl;
                ViewBag.ShoppingCartUrl = appData.ShoppingCartUrl;
                ViewBag.ShippingSuppression = appData.ShippingSuppression ? "true" : "false";
                ViewBag.Rewards = appData.RewardsProgram ? "true" : "false";
                ViewBag.CallbackUrl = appData.CallbackUrl;
                ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;
                ViewBag.AcceptedCards = appData.AcceptedCards;
                ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";
                ViewBag.WalletName = appData.WalletName;
                ViewBag.ConsumerWalletId = appData.ConsumerWalletId;
                ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }


            Session["data"] = appData;
            return View();
        }

        public ActionResult O5_ProcessCheckout()
        {
            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                // Retrieve the protected resources from the eWallet (e.g., Shipping Address and Credit Card).
                Checkout checkoutResult = service.GetPaymentShippingResource(appData.CheckoutUrl, appData.AccessToken);
                if (checkoutResult.Contact != null)
                {
                    if (checkoutResult.Contact.DateOfBirth != null)
                    {
                        if (checkoutResult.Contact.DateOfBirth.Year == 0)
                        {
                            checkoutResult.Contact.DateOfBirth = null;
                        }
                    }
                }

                appData.TransactionId = checkoutResult.TransactionId;

                XmlDocument xmlTransactions = Serializer<Checkout>.Serialize(checkoutResult);
                XElement checkoutElement = XElement.Parse(xmlTransactions.OuterXml);

                // Set the checkout data for display.
                ViewBag.CheckoutData = HttpUtility.HtmlEncode(checkoutElement.ToString().Trim());
                ViewBag.TransactionId = appData.TransactionId;

                ViewBag.PostbackUrl = appData.PostbackUrl;
                if (service != null)
                {
                    ViewBag.SignatureBaseString = service.SignatureBaseString;
                    ViewBag.AuthHeader = service.AuthHeader;
                }
                ViewBag.CheckoutUrl = appData.CheckoutUrl;

                Session["data"] = appData;
                return View(checkoutResult);
            }
            catch (Exception ex)
            {
                // Set info needed for displaying the error to the view.
                ViewBag.TransactionUrl = appData.PostbackUrl;
                if (service != null)
                    ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.CheckoutUrl = appData.CheckoutUrl;
                ViewBag.ErrorMessage = ex.Message;
                return View(new Checkout
                {
                    Card = new Card
                    {
                        BillingAddress = new allModels.Address()
                    },
                    Contact = new allModels.Contact(),
                    ShippingAddress = new ShippingAddress()
                });
            }
        }


        public ActionResult O6_PostTransaction()
        {
            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                // Set the post transaction URL.
                ViewBag.PostbackUrl = appData.PostbackUrl;

                // Create a list of transactions to be posted to MasterCard.
                var transactions = new List<MerchantTransaction>();

                // Create a sample transaction using the TransactionId resulting from the checkout process.
                var transaction = new MerchantTransaction
                {
                    TransactionId = appData.TransactionId,
                    ConsumerKey = appData.ConsumerKey,
                    Currency = "USD",
                    OrderAmount = 1000,
                    PurchaseDate = DateTime.Now,
                    TransactionStatus = allModels.TransactionStatus.Success,
                    ApprovalCode = appData.ApprovalCode,
                    PreCheckoutTransactionId = appData.PrecheckoutTransactionId
                };
                // Add this transaction to the list.
                transactions.Add(transaction);

                MerchantTransactions merchantTransactions = new MerchantTransactions();
                merchantTransactions.MerchantTransactions1 = transactions;

                XmlDocument xmlTransactions = Serializer<MerchantTransactions>.Serialize(merchantTransactions);

                ViewBag.PostTransactionSentXml = XElement.Parse(xmlTransactions.OuterXml).ToString();

                // Submit the transaction receipt.
                var result = service.PostCheckoutTransaction(appData.PostbackUrl, merchantTransactions);

                XmlDocument responseXML = Serializer<MerchantTransactions>.Serialize(result);
                if (result != null)
                    ViewBag.PostTransactionReceivedXml = XElement.Parse(responseXML.OuterXml).ToString();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            if (service != null)
            {
                ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.AuthHeader = service.AuthHeader;
            }

            Session["data"] = appData;
            return View();
        }


        public ActionResult C1_Cart(string xmlVersionDropdown, string shippingSuppressionDropdown, string[] acceptedCardsCheckbox, string privateLabelText, string rewardsDropdown, string authenticationCheckBox, string shippingProfileDropdown)
        {
            try {

                ProcessParameters(xmlVersionDropdown, shippingSuppressionDropdown, acceptedCardsCheckbox, privateLabelText, rewardsDropdown, authenticationCheckBox, shippingProfileDropdown);
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                // Create the user sign-in url.
                appData.requestTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                RequestToken = appData.requestTokenResponse.RequestToken;
                appData.RequestToken = appData.requestTokenResponse.RequestToken;

                // parse Shopping Cart data to populate the checkout cart
                ShoppingCartRequest shoppingCartRequest = ParseShoppingCartFile(RequestToken);
                ShoppingCartResponse shoppingCartResponse = service.PostShoppingCartData(appData.ShoppingCartUrl, shoppingCartRequest);
                appData.ShoppingCartRequest = XElement.Parse(Serializer<ShoppingCartRequest>.Serialize(shoppingCartRequest).OuterXml).ToString();
                appData.ShoppingCartResponse = XElement.Parse(Serializer<ShoppingCartResponse>.Serialize(shoppingCartResponse).OuterXml).ToString();

                ViewBag.ShoppingCartItem = shoppingCartRequest.ShoppingCart.ShoppingCartItem;
                ViewBag.SubTotal = (double)shoppingCartRequest.ShoppingCart.Subtotal / 100;
                ViewBag.Shipping = (double)ShippingCharge / 100;
                ViewBag.Tax = (double)Tax / 100;
                ViewBag.Total = (double)(shoppingCartRequest.ShoppingCart.Subtotal + Tax + ShippingCharge) / 100;

                ViewBag.AcceptedCards = appData.AcceptedCards;
                ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
                ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";
                ViewBag.ShippingSuppression = appData.ShippingSuppression ? "true" : "false";
                ViewBag.RewardsProgram = appData.RewardsProgram ? "true" : "false";
                ViewBag.ShippingProfiles = appData.ShippingProfiles;
                ViewBag.XmlVersion = appData.XmlVersion;

                if (service != null)
                {
                    ViewBag.AuthHeader = service.AuthHeader;
                    ViewBag.SignatureBaseString = service.SignatureBaseString;
                    ViewBag.CallbackUrl = appData.CartCallbackUrl;
                    ViewBag.CallbackDomain = appData.CallbackDomain;
                    ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;
                    ViewBag.RequestToken = RequestToken;
                }
            }
            catch (Exception ex)
            {
                // parse Shopping Cart data to populate the checkout cart
                ShoppingCartRequest ShoppingCartXML = ParseShoppingCartFile(RequestToken);
                ViewBag.ShoppingCartItem = ShoppingCartXML.ShoppingCart.ShoppingCartItem;
                ViewBag.SubTotal = (double)ShoppingCartXML.ShoppingCart.Subtotal / 100;
                ViewBag.Shipping = (double)ShippingCharge / 100;
                ViewBag.Tax = (double)Tax / 100;
                ViewBag.Total = (double)(ShoppingCartXML.ShoppingCart.Subtotal + Tax + ShippingCharge) / 100;
                ViewBag.ErrorMessage = ex.Message;
                return View("C1_Cart");
            }

            ViewBag.LightboxUrl = appData.LightboxUrl;

            Session["data"] = appData;           
            return View();
        }

        [HttpGet]
        public ActionResult C2_ReviewOrder(string oauth_token, string oauth_verifier, string checkout_resource_url)
        {
            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                if (oauth_token == null || oauth_verifier == null || checkout_resource_url == null)
                {
                    ShoppingCartRequest ShoppingCartXML = ParseShoppingCartFile("");
                    ViewBag.ShoppingCartItem = ShoppingCartXML.ShoppingCart.ShoppingCartItem;
                    ViewBag.SubTotal = (double)ShoppingCartXML.ShoppingCart.Subtotal / 100;
                    ViewBag.Shipping = (double)ShippingCharge / 100;
                    ViewBag.Tax = (double)Tax / 100;
                    ViewBag.Total = (double)(ShoppingCartXML.ShoppingCart.Subtotal + ShippingCharge + Tax) / 100;
                    return View("C1_Cart");
                }

                RequestToken = oauth_token;
                appData.RequestToken = oauth_token;
                appData.Verifier = oauth_verifier;
                appData.CheckoutUrl = checkout_resource_url;

                appData.accessTokenResponse = service.GetAccessToken(appData.AccessUrl, RequestToken, appData.Verifier);

                if (appData.accessTokenResponse != null)
                {
                    appData.AccessToken = appData.accessTokenResponse.AccessToken;
                }
                appData.Checkout = service.GetPaymentShippingResource(appData.CheckoutUrl, appData.AccessToken);
                appData.CheckoutXML = XElement.Parse(Serializer<Checkout>.Serialize(appData.Checkout).OuterXml).ToString();
                appData.TransactionId = appData.Checkout.TransactionId;
                if (appData.Checkout != null)
                {
                    if (appData.Checkout.Contact != null)
                    {
                        if (appData.Checkout.Contact.DateOfBirth != null)
                        {
                            if (appData.Checkout.Contact.DateOfBirth.Year == 0)
                            {
                                appData.Checkout.Contact.DateOfBirth = null;
                            }
                        }
                    }
                }

                ShoppingCartRequest shoppingCartRequest = ParseShoppingCartFile(RequestToken);
                ViewBag.ShoppingCartItem = shoppingCartRequest.ShoppingCart.ShoppingCartItem;
                ViewBag.SubTotal = (double)shoppingCartRequest.ShoppingCart.Subtotal / 100;
                ViewBag.Shipping = (double)appData.shipping / 100;
                ViewBag.Tax = (double)appData.tax / 100;
                ViewBag.Total = (double)(shoppingCartRequest.ShoppingCart.Subtotal + appData.tax + appData.shipping) / 100;
                ViewBag.RecipientName = appData.Checkout.ShippingAddress.RecipientName;
                ViewBag.RecipientPhoneNumber = appData.Checkout.ShippingAddress.RecipientPhoneNumber;
                ViewBag.ShippingAddressLine1 = appData.Checkout.ShippingAddress.Line1;
                ViewBag.ShippingAddressLine2 = appData.Checkout.ShippingAddress.Line2;
                ViewBag.ShippingAddressCity = appData.Checkout.ShippingAddress.City + " " + appData.Checkout.ShippingAddress.CountrySubdivision + " " +
                    appData.Checkout.ShippingAddress.PostalCode;
                ViewBag.ShippingAddressCountry = appData.Checkout.ShippingAddress.Country;
                ViewBag.Name = appData.Checkout.Contact.FirstName + " " + appData.Checkout.Contact.LastName;
                ViewBag.Gender = appData.Checkout.Contact.Gender == allModels.Gender.M ? "Male" : "Female";
                ViewBag.GenderSpecified = appData.Checkout.Contact.GenderSpecified;
                ViewBag.DOB = appData.Checkout.Contact.DateOfBirth != null ? appData.Checkout.Contact.DateOfBirth.Month + "/" + appData.Checkout.Contact.DateOfBirth.Day + 
                    "/" + appData.Checkout.Contact.DateOfBirth.Year : "";
                ViewBag.NationalId = appData.Checkout.Contact.NationalID;
                ViewBag.Country = appData.Checkout.Contact.Country;
                ViewBag.PhoneNumber = appData.Checkout.Contact.PhoneNumber;
                ViewBag.EmailAddress = appData.Checkout.Contact.EmailAddress;
                ViewBag.CardHolderName = appData.Checkout.Card.CardHolderName;
                ViewBag.CardBrandName = appData.Checkout.Card.BrandName;
                ViewBag.CardExpiry = appData.Checkout.Card.ExpiryMonth + "/" + appData.Checkout.Card.ExpiryYear;
                ViewBag.CardAccountNumber = appData.Checkout.Card.AccountNumber;
                ViewBag.CardLine1 = appData.Checkout.Card.BillingAddress.Line1;
                ViewBag.CardLine2 = appData.Checkout.Card.BillingAddress.Line2;
                ViewBag.CardCity = appData.Checkout.Card.BillingAddress.City + " " + appData.Checkout.Card.BillingAddress.CountrySubdivision + " " +
                    appData.Checkout.Card.BillingAddress.Country;
                ViewBag.CardPostal = appData.Checkout.Card.BillingAddress.PostalCode;
                ViewBag.RewardsName = appData.Checkout.RewardProgram.RewardName;
                ViewBag.RewardsNumber = appData.Checkout.RewardProgram.RewardNumber;
                ViewBag.RewardsExpiry = appData.Checkout.RewardProgram.ExpiryYear > 0 ? appData.Checkout.RewardProgram.ExpiryMonth + "/" +
                    appData.Checkout.RewardProgram.ExpiryYear : "";

                Session["data"] = appData;
                return View();
            }
            catch (Exception ex)
            {
                ShoppingCartRequest ShoppingCartXML = ParseShoppingCartFile(RequestToken);
                ViewBag.ShoppingCartItem = ShoppingCartXML.ShoppingCart.ShoppingCartItem;
                ViewBag.SubTotal = (double)ShoppingCartXML.ShoppingCart.Subtotal / 100;
                ViewBag.Shipping = (double)ShippingCharge / 100;
                ViewBag.Tax = (double)Tax / 100;
                ViewBag.Total = (double)(ShoppingCartXML.ShoppingCart.Subtotal + Tax + ShippingCharge) / 100;
                ViewBag.RewardsProgram = appData.RewardsProgram ? "true" : "false";
                ViewBag.ShippingSuppression = appData.ShippingSuppression ? "true" : "false";
                ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";

                // Set info needed for displaying the error to the view.
                if (service != null)
                    ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.ErrorMessage = ex.Message;

                Session["data"] = appData;
                return View("C1_Cart");
            }
        }


        public ActionResult C3_OrderComplete()
        {
            // Our .p12 cert file is stored in our local /Certs folder.
            // Map this file to get the physical server path.
            var certPath = Request.MapPath(appData.KeystorePath);

            // Create the Connector.
            service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

            // Parse Shopping Cart with no request token to retrieve the subtotal
            ShoppingCartRequest ShoppingCartXML = ParseShoppingCartFile("");

            long total = (ShoppingCartXML.ShoppingCart.Subtotal + Tax + ShippingCharge);

            var transactions = new List<MerchantTransaction>();

            // Create a sample transaction using the TransactionId resulting from the checkout process.
            var transaction = new MerchantTransaction
            {
                TransactionId = appData.TransactionId,
                ConsumerKey = appData.ConsumerKey,
                Currency = "USD",
                OrderAmount = total,
                PurchaseDate = DateTime.Now,
                TransactionStatus = allModels.TransactionStatus.Success,
                ApprovalCode = appData.ApprovalCode
            };
            // Add this transaction to the list.
            transactions.Add(transaction);

            MerchantTransactions merchantTransactions = new MerchantTransactions();
            merchantTransactions.MerchantTransactions1 = transactions;

            XmlDocument xmlTransactions = Serializer<MerchantTransactions>.Serialize(merchantTransactions);

            ViewBag.PostTransactionSentXml = XElement.Parse(xmlTransactions.OuterXml).ToString();

            // Submit the transaction receipt.
            var result = service.PostCheckoutTransaction(appData.PostbackUrl, merchantTransactions);

            XmlDocument responseXML = Serializer<MerchantTransactions>.Serialize(result);

            if (result != null)
                ViewBag.PostTransactionReceivedXml = XElement.Parse(responseXML.OuterXml).ToString();

            Session["data"] = appData;
            return View();
        }



        public ActionResult P1_Pairing(string checkout, string xmlVersionDropdown, string shippingSuppressionDropdown, string[] acceptedCardsCheckbox, 
            string privateLabelText, string rewardsDropdown, string authenticationCheckBox, string shippingProfileDropdown)
        {
            bool isCheckout = "true".Equals(checkout);
            if (!isCheckout)
            {
                ProcessParameters(xmlVersionDropdown, shippingSuppressionDropdown, acceptedCardsCheckbox, privateLabelText, rewardsDropdown, authenticationCheckBox,
                        shippingProfileDropdown);
            }

            try
            {
                // Our .p12 cert file is stored in our local /Certs folder.
                // Map this file to get the physical server path.
                var certPath = Request.MapPath(appData.KeystorePath);

                // Create the Connector.
                service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                var pairingTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                appData.PairingToken = pairingTokenResponse.RequestToken;
                
                ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
                ViewBag.AcceptedCards = appData.AcceptedCards;
                ViewBag.AppBaseUrl = ""; //TODO: Need to find the right value for this
                ViewBag.AuthHeader = service.AuthHeader;
                ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";
                ViewBag.CallbackUrl = isCheckout ? appData.ConnectedCallbackUrl : appData.CallbackUrl;
                ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;
                ViewBag.LightboxUrl = appData.LightboxUrl;
                ViewBag.PairingCallbackPath = appData.PairingCallbackPath;
                ViewBag.PairingCallbackUrl = appData.PairingCallbackUrl;
                ViewBag.PairingToken = appData.PairingToken;
                ViewBag.RequestToken = RequestToken;
                ViewBag.RequestUrl = appData.RequestUrl;
                ViewBag.RewardsProgram = appData.RewardsProgram ? "true" : "false";
                ViewBag.ShippingSuppression = appData.ShippingSuppression ? "true" : "false";
                ViewBag.SignatureBaseString = service.SignatureBaseString;

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            Session["data"] = appData;
            return View();

        }

        public string PairingConfig(string dataTypes)
        {
            string[] types = dataTypes.Split(',');
            List<string> list = new List<string>();
            foreach(string type in types)
            {
                if (type.Equals("CARD"))
                {
                    list.Add(PairingDataTypeType.CARD.ToString());
                }
                else if (type.Equals("PROFILE"))
                {
                    list.Add(PairingDataTypeType.PROFILE.ToString());
                }
                else if (type.Equals("ADDRESS"))
                {
                    list.Add(PairingDataTypeType.ADDRESS.ToString());
                }
                else if (type.Equals("REWARD_PROGRAM"))
                {
                    list.Add(PairingDataTypeType.REWARD_PROGRAM.ToString());
                }
            }
            appData.PairingDataTypes = list;
            Session["data"] = appData;
            return new JavaScriptSerializer().Serialize(appData);
        }

        private void ProcessParameters(string xmlVersionDropdown, string shippingSuppressionDropdown, string[] acceptedCardsCheckbox, string privateLabelText, string rewardsDropdown, string authenticationCheckBox, string shippingProfileDropdown)
        {
            appData.XmlVersion = xmlVersionDropdown;
            appData.ShippingSuppression = string.Equals(shippingSuppressionDropdown, "true", StringComparison.CurrentCultureIgnoreCase) ? true : false;
            appData.RewardsProgram = string.Equals(rewardsDropdown, "true", StringComparison.CurrentCultureIgnoreCase) ? true : false;
            appData.AuthLevelBasic = string.Equals(authenticationCheckBox, "on", StringComparison.CurrentCultureIgnoreCase) ? true : false;

            string acceptedCardsString = "";

            if (acceptedCardsCheckbox != null)
            {
                foreach (string c in acceptedCardsCheckbox)
                {
                    acceptedCardsString = acceptedCardsString + c + ",";
                }
            }

            if (privateLabelText != null && privateLabelText.Trim().Length > 0)
            {
                acceptedCardsString += privateLabelText;
            }
            else if (acceptedCardsString.Length > 0)
            {
                acceptedCardsString = acceptedCardsString.Substring(0, acceptedCardsString.Length - 1);
            }
            appData.AcceptedCards = acceptedCardsString;
        }

        private ShoppingCartRequest ParseShoppingCartFile(string requestToken)
        {
            string ShoppingCartString = System.IO.File.ReadAllText(Server.MapPath("~/resources/shoppingCart.xml"));

            ShoppingCartRequest ShoppingCartXML = Serializer<ShoppingCartRequest>.Deserialize(ShoppingCartString);

            ShoppingCartXML.OAuthToken = requestToken;
            ShoppingCartXML.OriginUrl = appData.OriginUrl;

            foreach (ShoppingCartItem item in ShoppingCartXML.ShoppingCart.ShoppingCartItem)
            {
                item.Description = TrimDescription(item.Description);
            }
            return ShoppingCartXML;
        }

        private MerchantInitializationRequest ParseMerchantInitFile(string pairingToken)
        {
            MerchantInitializationRequest request = new MerchantInitializationRequest();
            request.OAuthToken = pairingToken;
            request.OriginUrl = appData.OriginUrl;
            return request;
        }

        private PrecheckoutDataRequest GetPrecheckoutDataRequest()
        {
            PrecheckoutDataRequest preCheckoutDataRequest = new PrecheckoutDataRequest();
            PairingDataTypes types = new PairingDataTypes();
            foreach (string pairingDataType in appData.PairingDataTypes)
            {
                PairingDataType type = new PairingDataType();
                if(pairingDataType.Equals("CARD"))
                    type.Type = PairingDataTypeType.CARD;
                else if (pairingDataType.Equals("PROFILE"))
                    type.Type = PairingDataTypeType.PROFILE;
                else if (pairingDataType.Equals("ADDRESS"))
                    type.Type = PairingDataTypeType.ADDRESS;
                else if (pairingDataType.Equals("REWARD_PROGRAM"))
                    type.Type = PairingDataTypeType.REWARD_PROGRAM;
                types.PairingDataType.Add(type);
            }
            preCheckoutDataRequest.PairingDataTypes = types.PairingDataType;
            return preCheckoutDataRequest;
        }

        private string TrimDescription(string str)
        {
            if (str.Length >= 95)
            {
                string str1 = str.Substring(0, 95);
                string str2 = str.Substring(95, str.Length - 95);
                str2 = str2.Replace("&amp;", "").Replace("&", "");
                str = str1 + str2;
                str = str.Substring(0, 100);
            }
            return str;
        }
    }
}