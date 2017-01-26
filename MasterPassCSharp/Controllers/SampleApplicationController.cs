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
using MasterCard.WalletWebContent.Models;


using Com.MasterCard.Sdk;
using Com.MasterCard.Masterpass;
using Com.MasterCard.Masterpass.Merchant;
using Com.MasterCard.Masterpass.Merchant.Model;
using Com.MasterCard.Masterpass.Merchant.Client;
using Com.MasterCard.Masterpass.Merchant.Api;
using Com.MasterCard.Sdk.Core;
using Com.MasterCard.Sdk.Core.Model;
using System.Collections;
using Com.MasterCard.Sdk.Core.Api;
using MasterCard.SDK;
using Com.MasterCard.Merchant.Onboarding.Model;
using Com.MasterCard.Sdk.Core.Exceptions;
using Com.MasterCard.Merchant.Onboarding.Api;

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
        //public MasterPassService service;

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

            MasterCardApiConfiguration.Sandbox = true;
            MasterCardApiConfiguration.ConsumerKey = this.appData.ConsumerKey;
            MasterCardApiConfiguration.PrivateKey = new X509Certificate2(Server.MapPath(this.appData.KeystorePath), this.appData.KeystorePassword).PrivateKey;

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
            try
            {
                RequestTokenResponse requestTokenResponse = Com.MasterCard.Sdk.Core.Api.RequestTokenApi.Create(appData.CallbackUrl);


                appData.requestTokenResponse = requestTokenResponse;
                RequestToken = appData.requestTokenResponse.OauthToken;
                appData.RequestToken = appData.requestTokenResponse.OauthToken;

                //ViewBag.UserSignInUrl = userSignInUrl;
                ViewBag.RequestToken = RequestToken;
                ViewBag.AuthorizeUrl = appData.requestTokenResponse.XoauthRequestAuthUrl;
                ViewBag.OauthExpiresIn = appData.requestTokenResponse.OauthExpiresIn;
                ViewBag.OauthTokenSecret = appData.requestTokenResponse.OauthTokenSecret;
                ViewBag.RedirectUrl = appData.requestTokenResponse.RedirectUrl;

                RequestTokenResponse pairingTokenResponse = Com.MasterCard.Sdk.Core.Api.RequestTokenApi.Create(appData.CallbackUrl);

                ViewBag.PairingToken = pairingTokenResponse.OauthToken;
                ViewBag.PairingAuthorizeUrl = pairingTokenResponse.XoauthRequestAuthUrl;
                ViewBag.PairingOauthExpiresIn = pairingTokenResponse.OauthExpiresIn;
                ViewBag.PairingOauthTokenSecret = pairingTokenResponse.OauthTokenSecret;
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
            Session["data"] = appData;
            return View();
        }

        public ActionResult O2_ShoppingCart()
        {
            try
            {
                //Create an instance of ShoppingCartRequest
                ShoppingCartRequest shoppingCartRequest = new ShoppingCartRequest();
                ShoppingCart shoppingCart = new ShoppingCart();
                List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();
                shoppingCartItems.Add(new ShoppingCartItem
                {
                    Value = 1900L,
                    Description = "This is one item",
                    Quantity = 1
                });
                shoppingCartItems.Add(new ShoppingCartItem
                {
                    ImageURL = "https://somemerchant.com/someimage",
                    Value = 10000L,
                    Description = "Five items",
                    Quantity = 5
                });

                shoppingCart.Subtotal = 11900L;
                shoppingCart.CurrencyCode = "USD";
                shoppingCart.ShoppingCartItem = shoppingCartItems;

                shoppingCartRequest.ShoppingCart = shoppingCart;
                shoppingCartRequest.OAuthToken = RequestToken;

                //Call the ShoppingCartService with required params
                ShoppingCartResponse shoppingCartResponse = ShoppingCartApi.Create(shoppingCartRequest);

                // Set the post transaction URL.
                ViewBag.ShoppingCartUrl = appData.ShoppingCartUrl;
                ViewBag.RewardsProgram = appData.RewardsProgram ? "true" : "false";
                ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
                ViewBag.AcceptedCards = appData.AcceptedCards;

                XmlDocument xmldoc = Serializer<ShoppingCartRequest>.Serialize(shoppingCartRequest);
                XElement shoppingRequestElement = XElement.Parse(xmldoc.OuterXml);
                ViewBag.ShoppingCartRequest = shoppingRequestElement.ToString().Trim();


                if (shoppingCartResponse != null)
                {
                    XmlDocument xmlTransactions = Serializer<ShoppingCartResponse>.Serialize(shoppingCartResponse);
                    XElement shoppingCartElement = XElement.Parse(xmlTransactions.OuterXml);
                    ViewBag.ShoppingCartResponse = AllHtmlEncode(shoppingCartElement.ToString().Trim());
                }
                //ViewBag.AuthHeader = service.AuthHeader;
                ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";
                ViewBag.CallbackDomain = appData.CallbackDomain;
                ViewBag.CallbackUrl = appData.CallbackUrl;
                ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;
                ViewBag.LightboxUrl = appData.LightboxUrl;
                ViewBag.PairingToken = appData.PairingToken;
                ViewBag.PairingCallbackUrl = appData.PairingCallbackUrl;
                ViewBag.RequestToken = appData.requestTokenResponse.OauthToken;
                ViewBag.ShippingSuppression = appData.ShippingSuppression ? "true" : "false";
                //ViewBag.SignatureBaseString = service.SignatureBaseString;


                //Create an instance of MerchantInitializationRequest
                MerchantInitializationRequest merchantInitializationRequest = new MerchantInitializationRequest()
                        .WithOriginUrl("http://localhost")
                        .WithOAuthToken(RequestToken);

                xmldoc = Serializer<MerchantInitializationRequest>.Serialize(merchantInitializationRequest);
                XElement merchantInitRequestElement = XElement.Parse(xmldoc.OuterXml);
                ViewBag.MerchantInitRequest = shoppingRequestElement.ToString().Trim();

                //Call the MerchantInitializationApi Service with required params
                MerchantInitializationResponse merchantInitializationResponse = MerchantInitializationApi.Create(merchantInitializationRequest);

                ViewBag.MerchantInitUrl = appData.MerchantInitUrl;
                XmlDocument xmlTransactionsMI = Serializer<MerchantInitializationResponse>.Serialize(merchantInitializationResponse);
                XElement merchantInitElement = XElement.Parse(xmlTransactionsMI.OuterXml);
                ViewBag.MerchantInitResponse = AllHtmlEncode(merchantInitElement.ToString().Trim());

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

                MerchantInitializationResponse merchantInitResponse;
                MerchantInitializationRequest merchantInitRequest;

                if (isCheckout)
                {
                    ShoppingCartRequest shoppingCartRequest;
                    ShoppingCartResponse shoppingCartResponse;

                    // Get Request Token
                    //appData.requestTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                    appData.requestTokenResponse = Com.MasterCard.Sdk.Core.Api.RequestTokenApi.Create(appData.CallbackUrl);
                    RequestToken = appData.requestTokenResponse.OauthToken;
                    appData.RequestToken = appData.requestTokenResponse.OauthToken;
                    ViewBag.RequestToken = appData.requestTokenResponse.OauthToken;
                    ViewBag.AuthorizationUrl = appData.requestTokenResponse.XoauthRequestAuthUrl;
                    ViewBag.OAuthExpiresIn = appData.requestTokenResponse.OauthExpiresIn;
                    ViewBag.OAuthTokenSecret = appData.requestTokenResponse.OauthTokenSecret;

                    // Post Shopping Cart
                    //Create an instance of ShoppingCartRequest
                    shoppingCartRequest = new ShoppingCartRequest();
                    ShoppingCart shoppingCart = new ShoppingCart();
                    List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();
                    shoppingCartItems.Add(new ShoppingCartItem
                    {
                        Value = 1900L,
                        Description = "This is one item",
                        Quantity = 1
                    });
                    shoppingCartItems.Add(new ShoppingCartItem
                    {
                        ImageURL = "https://somemerchant.com/someimage",
                        Value = 10000L,
                        Description = "Five items",
                        Quantity = 5
                    });

                    shoppingCart.Subtotal = 11900L;
                    shoppingCart.CurrencyCode = "USD";
                    shoppingCart.ShoppingCartItem = shoppingCartItems;

                    shoppingCartRequest.ShoppingCart = shoppingCart;
                    shoppingCartRequest.OAuthToken = appData.RequestToken;

                    //Call the ShoppingCartService with required params
                    shoppingCartResponse = ShoppingCartApi.Create(shoppingCartRequest);

                    appData.ShoppingCartRequest = XElement.Parse(Serializer<ShoppingCartRequest>.Serialize(shoppingCartRequest).OuterXml).ToString();
                    ViewBag.ShoppingCartRequest = appData.ShoppingCartRequest;

                    appData.ShoppingCartResponse = XElement.Parse(Serializer<ShoppingCartResponse>.Serialize(shoppingCartResponse).OuterXml).ToString();
                    ViewBag.ShoppingCartResponse = appData.ShoppingCartResponse;


                    // Get Pairing Token
                    //var pairingTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                    var pairingTokenResponse = Com.MasterCard.Sdk.Core.Api.RequestTokenApi.Create(appData.CallbackUrl);
                    appData.PairingToken = pairingTokenResponse.OauthToken;

                    // Post Merchant Init Data
                    //Create an instance of MerchantInitializationRequest
                    merchantInitRequest = new MerchantInitializationRequest
                    {
                        OriginUrl = "http://localhost",
                        OAuthToken = appData.PairingToken
                    };

                    //Call the MerchantInitializationApi with required params
                    merchantInitResponse = MerchantInitializationApi.Create(merchantInitRequest);


                }
                else
                {
                    //  postMerchantInitData
                    merchantInitRequest = new MerchantInitializationRequest
                    {
                        OriginUrl = "http://localhost",
                        OAuthToken = appData.PairingToken
                    };

                    //Call the MerchantInitializationApi with required params
                    merchantInitResponse = MerchantInitializationApi.Create(merchantInitRequest);
                }

                appData.MerchantInitRequest = Serializer<MerchantInitializationRequest>.Serialize(merchantInitRequest).OuterXml;
                appData.MerchantInitResponse = Serializer<MerchantInitializationResponse>.Serialize(merchantInitResponse).OuterXml;

                ViewBag.AcceptedCards = appData.AcceptedCards;
                ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
                //ViewBag.AuthHeader = service.AuthHeader;
                ViewBag.AuthLevelBasic = appData.AuthLevelBasic ? "true" : "false";

                ViewBag.Checkout = checkout;
                ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;

                ViewBag.ExpressCallbackUrl = appData.ExpressCallbackUrl;
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
                //ViewBag.SignatureBaseString = service.SignatureBaseString;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }


            Session["data"] = appData;
            return View();
        }

        [HttpGet]
        public ActionResult O3_Callback(string mpstatus, string oauth_token, string oauth_verifier, string checkout_resource_url)
        {
            // Save the checkout resource URL to session.
            appData.CheckoutUrl = checkout_resource_url;
            appData.Verifier = oauth_verifier;
            RequestToken = oauth_token;
            appData.RequestToken = oauth_token;

            ViewBag.Mpstatus = mpstatus;
            ViewBag.RequestToken = oauth_token;
            ViewBag.Verifier = oauth_verifier;
            ViewBag.CheckoutUrl = checkout_resource_url;

            Session["data"] = appData;
            return View();
        }

        [HttpGet]
        public ActionResult O3_PairingCallback(string mpstatus, string pairing_token, string pairing_verifier)
        {
            appData.PairingToken = pairing_token;
            appData.PairingVerifier = pairing_verifier;
            ViewBag.PairingToken = pairing_token;
            ViewBag.PairingVerifier = pairing_verifier;

            try
            {

                AccessTokenResponse accessTokenResponse = AccessTokenApi.Create(pairing_token, pairing_verifier);
                string long_access_token = accessTokenResponse.OauthToken; // store for future requests
                appData.longAccessTokenResponse = accessTokenResponse;

                if (appData.longAccessTokenResponse != null)
                {
                    appData.LongAccessToken = appData.longAccessTokenResponse.OauthToken;
                    appData.LongAccessSecret = appData.longAccessTokenResponse.OauthTokenSecret;
                    ViewBag.LongAccessToken = appData.longAccessTokenResponse.OauthToken;
                    ViewBag.LongAccessSecret = appData.longAccessTokenResponse.OauthTokenSecret;
                    HttpCookie cookie = new HttpCookie("longAccessToken", appData.longAccessTokenResponse.OauthToken);
                    Response.Cookies.Add(cookie);
                }

                //ViewBag.AuthHeader = service.AuthHeader;
                //ViewBag.SignatureBaseString = service.SignatureBaseString;
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
            AccessTokenResponse accessTokenResponse = new AccessTokenResponse();

            try
            {
                accessTokenResponse = AccessTokenApi.Create(RequestToken, appData.Verifier);
                appData.accessTokenResponse = accessTokenResponse;
                if (accessTokenResponse != null)
                {
                    appData.AccessToken = accessTokenResponse.OauthToken;
                }

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            ViewBag.CheckoutUrl = appData.CheckoutUrl;
            ViewBag.AccessUrl = appData.AccessUrl;
            ViewBag.AccessToken = accessTokenResponse.OauthToken; //AccessToken;
            ViewBag.TokenSecret = accessTokenResponse.OauthTokenSecret;

            Session["data"] = appData;
            return View();
        }

        public ActionResult O5_PreCheckout(string express)
        {
            try
            {
                bool isExpress = "true".Equals(express);

                PrecheckoutDataRequest preCheckoutDataRequest = GetPrecheckoutDataRequest();

                PrecheckoutDataResponse response = PrecheckoutDataApi.Create(appData.LongAccessToken, preCheckoutDataRequest);

                appData.PreCheckoutRequest = XElement.Parse(Serializer<PrecheckoutDataRequest>.Serialize(preCheckoutDataRequest).OuterXml).ToString();

                if (response.PrecheckoutData.Cards.Card.Count == 0)
                {
                    response.PrecheckoutData.Cards = null;
                }
                if (response.PrecheckoutData.ShippingAddresses.ShippingAddress.Count == 0)
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
                    if (response.PrecheckoutData.ShippingAddresses.ShippingAddress.Count > 0)
                    {
                        appData.PrecheckoutShippingId = response.PrecheckoutData.ShippingAddresses.ShippingAddress.ElementAt(0).AddressId;
                    }
                }
                if (response.PrecheckoutData.Cards != null)
                {
                    if (response.PrecheckoutData.Cards.Card.Count > 0)
                    {
                        appData.PrecheckoutCardid = response.PrecheckoutData.Cards.Card.ElementAt(0).CardId;
                    }
                }


                appData.requestTokenResponse = RequestTokenApi.Create(appData.CallbackUrl);
                RequestToken = appData.requestTokenResponse.OauthToken;
                appData.RequestToken = appData.requestTokenResponse.OauthToken;

                //Create an instance of ShoppingCartRequest
                ShoppingCartRequest shoppingCartRequest = new ShoppingCartRequest();
                ShoppingCart shoppingCart = new ShoppingCart();
                List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();
                shoppingCartItems.Add(new ShoppingCartItem
                {
                    Value = 1900L,
                    Description = "This is one item",
                    Quantity = 1
                });
                shoppingCartItems.Add(new ShoppingCartItem
                {
                    ImageURL = "https://somemerchant.com/someimage",
                    Value = 10000L,
                    Description = "Five items",
                    Quantity = 5
                });

                shoppingCart.Subtotal = 11900L;
                shoppingCart.CurrencyCode = "USD";
                shoppingCart.ShoppingCartItem = shoppingCartItems;

                shoppingCartRequest.ShoppingCart = shoppingCart;
                shoppingCartRequest.OAuthToken = RequestToken;

                //Call the ShoppingCartService with required params
                ShoppingCartResponse shoppingCartResponse = ShoppingCartApi.Create(shoppingCartRequest);

                appData.ShoppingCartRequest = XElement.Parse(Serializer<ShoppingCartRequest>.Serialize(shoppingCartRequest).OuterXml).ToString();
                appData.ShoppingCartResponse = XElement.Parse(Serializer<ShoppingCartResponse>.Serialize(shoppingCartResponse).OuterXml).ToString();

                //Create an instance of MerchantInitializationRequest
                MerchantInitializationRequest merchantInitializationRequest = new MerchantInitializationRequest
                {
                    OriginUrl = "http://localhost",
                    OAuthToken = RequestToken
                };

                //Call the MerchantInitializationApi with required params
                MerchantInitializationResponse merchantInitializationResponse = MerchantInitializationApi.Create(merchantInitializationRequest);

                ViewBag.RequestToken = RequestToken;
                ViewBag.RequestTokenAuthorizationUrl = appData.requestTokenResponse.XoauthRequestAuthUrl;
                ViewBag.RequestTokenExpiresIn = appData.requestTokenResponse.OauthExpiresIn + (appData.requestTokenResponse.OauthExpiresIn != null ? " Seconds" : "");
                ViewBag.RequestTokenSecret = appData.requestTokenResponse.OauthTokenSecret;
                ViewBag.ShoppingCartRequest = appData.ShoppingCartRequest;
                ViewBag.ShoppingCartResponse = appData.ShoppingCartResponse;

                ViewBag.LightboxUrl = appData.LightboxUrl;
                //ViewBag.AuthHeader = service.AuthHeader;
                //ViewBag.SignatureBaseString = service.SignatureBaseString;
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
                ViewBag.IsExpress = isExpress ? "true" : "false";
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

                ViewBag.CheckoutUrl = appData.CheckoutUrl;

                string checkoutId = appData.CheckoutUrl.IndexOf('?') != -1 ? appData.CheckoutUrl.Substring(appData.CheckoutUrl.LastIndexOf('/') + 1).Split('?')[0] : appData.CheckoutUrl.Substring(appData.CheckoutUrl.LastIndexOf('/') + 1);

                Checkout checkout = CheckoutApi.Show(checkoutId, appData.AccessToken);

                Card card = checkout.Card;
                Com.MasterCard.Masterpass.Merchant.Model.Address billingAddress = card.BillingAddress;
                Contact contact = checkout.Contact;
                AuthenticationOptions authOptions = checkout.AuthenticationOptions;
                string preCheckoutTransactionId = checkout.PreCheckoutTransactionId;
                Com.MasterCard.Masterpass.Merchant.Model.RewardProgram rewardProgram = checkout.RewardProgram;
                ShippingAddress shippingAddress = checkout.ShippingAddress;
                string transactionId = checkout.TransactionId;
                string walletId = checkout.WalletID;
                string checkoutData = XElement.Parse(Serializer<Checkout>.Serialize(checkout).OuterXml).ToString();

                appData.TransactionId = checkout.TransactionId;
                appData.PrecheckoutTransactionId = checkout.PreCheckoutTransactionId;

                if (checkout.RewardProgram == null)
                {
                    checkout.RewardProgram = new Com.MasterCard.Masterpass.Merchant.Model.RewardProgram();
                }

                if (authOptions == null)
                {
                    checkout.AuthenticationOptions = new AuthenticationOptions();
                }

                if (shippingAddress == null)
                {
                    checkout.ShippingAddress = new ShippingAddress();
                }


                XmlDocument xmlTransactionsCH = Serializer<Checkout>.Serialize(checkout);
                XElement checkoutElement = XElement.Parse(xmlTransactionsCH.OuterXml);
                ViewBag.CheckoutData = checkoutElement.ToString().Trim();

                return View(checkout);
            }
            catch (Exception ex)
            {
                // Set info needed for displaying the error to the view.
                ViewBag.TransactionUrl = appData.PostbackUrl;
                //if (service != null)
                //    ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.CheckoutUrl = appData.CheckoutUrl;
                ViewBag.ErrorMessage = ex.Message;
                return View(new Checkout
                {
                    Card = new Card
                    {
                        BillingAddress = new Com.MasterCard.Masterpass.Merchant.Model.Address()
                    },
                    Contact = new Contact(),
                    ShippingAddress = new ShippingAddress()
                });
            }
        }


        public ActionResult O6_PostTransaction()
        {
            try
            {

                //Create an instance of MerchantTransactions
                MerchantTransactions objMerchantTransactions = new MerchantTransactions();
                List<MerchantTransaction> merchantTransactions = new List<MerchantTransaction>();
                MerchantTransaction merchantTransaction = new MerchantTransaction
                {
                    TransactionId = appData.TransactionId, // from Step 7
                    PurchaseDate = "2016-08-09T14:52:57.539-05:00",
                    ExpressCheckoutIndicator = false,
                    ApprovalCode = "888888",  // authorization code returned by payment gateway
                    TransactionStatus = "Failure",
                    OrderAmount = 1229L, // total charged to card
                    PreCheckoutTransactionId = appData.PrecheckoutTransactionId,
                    Currency = "USD",
                    ConsumerKey = MasterCardApiConfiguration.ConsumerKey //generated during onboarding process
                };

                merchantTransactions.Add(merchantTransaction);

                objMerchantTransactions._MerchantTransactions = merchantTransactions;

                XmlDocument xmlTransactions = Serializer<MerchantTransactions>.Serialize(objMerchantTransactions);

                ViewBag.PostTransactionSentXml = XElement.Parse(xmlTransactions.OuterXml).ToString();

                //Call the PostbackService with required params
                MerchantTransactions response = PostbackApi.Create(objMerchantTransactions);

                XmlDocument responseXML = Serializer<MerchantTransactions>.Serialize(response);
                if (response != null)
                    ViewBag.PostTransactionReceivedXml = XElement.Parse(responseXML.OuterXml).ToString();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            Session["data"] = appData;
            return View();
        }


        public ActionResult C1_Cart(string xmlVersionDropdown, string shippingSuppressionDropdown, string[] acceptedCardsCheckbox, string privateLabelText, string rewardsDropdown, string authenticationCheckBox, string shippingProfileDropdown)
        {
            try
            {

                ProcessParameters(xmlVersionDropdown, shippingSuppressionDropdown, acceptedCardsCheckbox, privateLabelText, rewardsDropdown, authenticationCheckBox, shippingProfileDropdown);
                //// Our .p12 cert file is stored in our local /Certs folder.
                //// Map this file to get the physical server path.
                //var certPath = Request.MapPath(appData.KeystorePath);

                //// Create the Connector.
                //service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                // Create the user sign-in url.
                //appData.requestTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                RequestTokenResponse requestTokenResponse = Com.MasterCard.Sdk.Core.Api.RequestTokenApi.Create(appData.CallbackUrl);
                appData.requestTokenResponse = requestTokenResponse;

                RequestToken = appData.requestTokenResponse.OauthToken;
                appData.RequestToken = appData.requestTokenResponse.OauthToken;

                // parse Shopping Cart data to populate the checkout cart
                ShoppingCartRequest shoppingCartRequest = ParseShoppingCartFile(RequestToken);
                //ShoppingCartResponse shoppingCartResponse = service.PostShoppingCartData(appData.ShoppingCartUrl, shoppingCartRequest);
                //Call the ShoppingCartService with required params
                ShoppingCartResponse shoppingCartResponse = ShoppingCartApi.Create(shoppingCartRequest);

                //  postMerchantInitData
                MerchantInitializationRequest merchantInitRequest = new MerchantInitializationRequest
                {
                    OriginUrl = "http://localhost",
                    OAuthToken = RequestToken
                };

                //Call the MerchantInitializationApi with required params
                MerchantInitializationResponse merchantInitResponse = MerchantInitializationApi.Create(merchantInitRequest);

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
                ViewBag.CallbackUrl = appData.CartCallbackUrl;
                ViewBag.CallbackDomain = appData.CallbackDomain;
                ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;
                ViewBag.RequestToken = RequestToken;

                //if (service != null)
                //{
                //    ViewBag.AuthHeader = service.AuthHeader;
                //    ViewBag.SignatureBaseString = service.SignatureBaseString;

                //}
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
                //// Our .p12 cert file is stored in our local /Certs folder.
                //// Map this file to get the physical server path.
                //var certPath = Request.MapPath(appData.KeystorePath);

                //// Create the Connector.
                //service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

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

                //appData.accessTokenResponse = service.GetAccessToken(appData.AccessUrl, RequestToken, appData.Verifier);
                appData.accessTokenResponse = AccessTokenApi.Create(RequestToken, appData.Verifier);

                if (appData.accessTokenResponse != null)
                {
                    appData.AccessToken = appData.accessTokenResponse.OauthToken;
                }
                //appData.Checkout = service.GetPaymentShippingResource(appData.CheckoutUrl, appData.AccessToken);
                string checkoutId = appData.CheckoutUrl.IndexOf('?') != -1 ? appData.CheckoutUrl.Substring(appData.CheckoutUrl.LastIndexOf('/') + 1).Split('?')[0] : appData.CheckoutUrl.Substring(appData.CheckoutUrl.LastIndexOf('/') + 1);
                appData.Checkout = CheckoutApi.Show(checkoutId, appData.AccessToken);

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
                ViewBag.Gender = appData.Checkout.Contact.Gender;
                ViewBag.GenderSpecified = !string.IsNullOrEmpty(appData.Checkout.Contact.Gender);
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
                if (appData.Checkout.RewardProgram != null)
                {
                    ViewBag.RewardsName = appData.Checkout.RewardProgram.RewardName;
                    ViewBag.RewardsNumber = appData.Checkout.RewardProgram.RewardNumber;
                    ViewBag.RewardsExpiry = appData.Checkout.RewardProgram.ExpiryYear > 0 ? appData.Checkout.RewardProgram.ExpiryMonth + "/" +
                        appData.Checkout.RewardProgram.ExpiryYear : "";
                }
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
                //if (service != null)
                //    ViewBag.SignatureBaseString = service.SignatureBaseString;
                ViewBag.ErrorMessage = ex.Message;

                Session["data"] = appData;
                return View("C1_Cart");
            }
        }


        public ActionResult C3_OrderComplete()
        {
            //// Our .p12 cert file is stored in our local /Certs folder.
            //// Map this file to get the physical server path.
            //var certPath = Request.MapPath(appData.KeystorePath);

            //// Create the Connector.
            //service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

            // Parse Shopping Cart with no request token to retrieve the subtotal
            ShoppingCartRequest ShoppingCartXML = ParseShoppingCartFile("");

            long total = (Convert.ToInt32(ShoppingCartXML.ShoppingCart.Subtotal) + Tax + ShippingCharge);

            var transactions = new List<MerchantTransaction>();

            // Create a sample transaction using the TransactionId resulting from the checkout process.
            var transaction = new MerchantTransaction
            {
                TransactionId = appData.TransactionId,
                ConsumerKey = appData.ConsumerKey,
                Currency = "USD",
                OrderAmount = total,
                PurchaseDate = "2016-08-09T14:52:57.539-05:00",
                TransactionStatus = "Success",
                ApprovalCode = appData.ApprovalCode
            };
            // Add this transaction to the list.
            transactions.Add(transaction);

            MerchantTransactions merchantTransactions = new MerchantTransactions();
            merchantTransactions._MerchantTransactions = transactions;
            // Submit the transaction receipt.
            MerchantTransactions response = PostbackApi.Create(merchantTransactions);

            XmlDocument responseXML = Serializer<MerchantTransactions>.Serialize(response);

            if (response != null)
                ViewBag.PostTransactionReceivedXml = XElement.Parse(responseXML.OuterXml).ToString();

            Session["data"] = appData;
            return View();
        }

        public ActionResult ExpressCheckout(string cardSubmit, string addressSubmit)
        {
            try
            {
                string longAccessToken = Request.Cookies["longAccessToken"].Value;

                if (!string.IsNullOrEmpty(cardSubmit))
                    appData.PrecheckoutCardid = cardSubmit;
                if (!string.IsNullOrEmpty(addressSubmit))
                    appData.PrecheckoutShippingId = addressSubmit;

                ExpressCheckoutRequest request = GetExpressCheckoutRequest();
                ExpressCheckoutResponse response = ExpressCheckoutApi.Create(appData.LongAccessToken, request);

                appData.ExpressCheckoutRequest = XElement.Parse(Serializer<ExpressCheckoutRequest>.Serialize(request).OuterXml).ToString();
                appData.ExpressCheckoutResponse = XElement.Parse(Serializer<ExpressCheckoutResponse>.Serialize(response).OuterXml).ToString();
                appData.LongAccessToken = response.LongAccessToken;
                Session["LongAccessToken"] = response.LongAccessToken;
                if(response.Checkout != null)
                    appData.TransactionId = response.Checkout.TransactionId;

                appData.ExpressSecurityRequired = false;
                if (response.Errors != null && response.Errors.Error.Count() > 0)
                {
                    foreach (var error in response.Errors.Error)
                    {
                        if (error.Source.Contains("3DS Needed"))
                        {
                            appData.ExpressSecurityRequired = true;
                        }
                    }
                }
                HttpCookie cookie = new HttpCookie("longAccessToken", appData.LongAccessToken);
                Response.Cookies.Add(cookie);

                ViewBag.ExpressCheckoutRequest = appData.ExpressCheckoutRequest;
                ViewBag.ExpressCheckoutResponse = appData.ExpressCheckoutResponse;
                ViewBag.ExpressCheckoutUrl = appData.ExpressCheckoutUrl;
                ViewBag.ExpressSecurityRequired = appData.ExpressSecurityRequired ? "true" : "false";
                ViewBag.RequestToken = RequestToken;
                ViewBag.CallbackDomain = appData.CallbackDomain;
                ViewBag.CheckoutIdentifier = appData.CheckoutIdentifier;
                ViewBag.PreCheckoutTransactionId = appData.PrecheckoutTransactionId;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            ViewBag.LightboxUrl = appData.LightboxUrl;
            //ViewBag.AuthHeader = service.AuthHeader;
            //ViewBag.SignatureBaseString = service.SignatureBaseString;

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
                //var certPath = Request.MapPath(appData.KeystorePath);

                //// Create the Connector.
                //service = MasterPassService.FactoryMethod(appData.ConsumerKey, certPath, appData.KeystorePassword, appData.OriginUrl);

                //var pairingTokenResponse = service.GetRequestToken(appData.RequestUrl, appData.CallbackUrl);
                //appData.PairingToken = pairingTokenResponse.RequestToken;

                RequestTokenResponse requestTokenResponse = Com.MasterCard.Sdk.Core.Api.RequestTokenApi.Create(appData.PairingCallbackUrl);


                appData.PairingToken = requestTokenResponse.OauthToken;

                ViewBag.AllowedLoyaltyPrograms = appData.AllowedLoyaltyPrograms;
                ViewBag.AcceptedCards = appData.AcceptedCards;
                ViewBag.AppBaseUrl = ""; //TODO: Need to find the right value for this
                ViewBag.AuthHeader = requestTokenResponse.XoauthRequestAuthUrl;
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
                //ViewBag.SignatureBaseString = service.SignatureBaseString;

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
            foreach (string type in types)
            {
                if (type.Equals("CARD"))
                {
                    list.Add("CARD");
                }
                else if (type.Equals("PROFILE"))
                {
                    list.Add("PROFILE");
                }
                else if (type.Equals("ADDRESS"))
                {
                    list.Add("ADDRESS");
                }
                else if (type.Equals("REWARD_PROGRAM"))
                {
                    list.Add("REWARD_PROGRAM");
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
            //ShoppingCartXML.OriginUrl = appData.OriginUrl;

            foreach (ShoppingCartItem item in ShoppingCartXML.ShoppingCart.ShoppingCartItem)
            {
                item.Description = TrimDescription(item.Description);
            }
            return ShoppingCartXML;
        }

        //private MerchantInitializationRequest ParseMerchantInitFile(string pairingToken)
        //{
        //    MerchantInitializationRequest request = new MerchantInitializationRequest();
        //    request.OAuthToken = pairingToken;
        //    request.OriginUrl = appData.OriginUrl;
        //    return request;
        //}

        private PrecheckoutDataRequest GetPrecheckoutDataRequest()
        {
            //PairingDataTypes pairingDataTypes = new PairingDataTypes();
            //PrecheckoutDataRequest preCheckoutDataRequest = new PrecheckoutDataRequest();

            //foreach (string pairingDataType in appData.PairingDataTypes)
            //{
            //    PairingDataType type = new PairingDataType();
            //    if (pairingDataType.Equals("CARD"))
            //        type.WithType("CARD");
            //    else if (pairingDataType.Equals("PROFILE"))
            //        type.WithType("PROFILE");
            //    else if (pairingDataType.Equals("ADDRESS"))
            //        type.WithType("ADDRESS");
            //    else if (pairingDataType.Equals("REWARD_PROGRAM"))
            //        type.WithType("REWARD_PROGRAM");
            //    pairingDataTypes.WithPairingDataType(type);
            //}
            //return preCheckoutDataRequest.WithPairingDataTypes(pairingDataTypes);

            return new PrecheckoutDataRequest().WithPairingDataTypes(
                                                                        new PairingDataTypes()
                                                                        .WithPairingDataType(new PairingDataType().WithType("CARD"))
                                                                        .WithPairingDataType(new PairingDataType().WithType("ADDRESS"))
                                                                    );
        }

        private ExpressCheckoutRequest GetExpressCheckoutRequest()
        {
            var shoppingCartRequest = ParseShoppingCartFile(RequestToken);

            ExpressCheckoutRequest expressCheckoutRequest = new ExpressCheckoutRequest();
            expressCheckoutRequest.MerchantCheckoutId = appData.CheckoutIdentifier;
            expressCheckoutRequest.PrecheckoutTransactionId = appData.PrecheckoutTransactionId;
            expressCheckoutRequest.CurrencyCode = "USD";
            expressCheckoutRequest.OrderAmount = (shoppingCartRequest.ShoppingCart.Subtotal + appData.tax).Value;
            expressCheckoutRequest.CardId = appData.PrecheckoutCardid;
            expressCheckoutRequest.DigitalGoods = null;
            expressCheckoutRequest.RewardProgramId = null;
            expressCheckoutRequest.ExtensionPoint = null;
            expressCheckoutRequest.OriginUrl = appData.OriginUrl;
            expressCheckoutRequest.AdvancedCheckoutOverride = false;
            if (!string.IsNullOrWhiteSpace(appData.PrecheckoutShippingId))
            {
                expressCheckoutRequest.ShippingAddressId = appData.PrecheckoutShippingId;
            }
            //if (!string.IsNullOrWhiteSpace(appData.PrecheckoutRewardId))
            //{
            //    expressCheckoutRequest.RewardProgramId = appData.PrecheckoutRewardId;
            //}

            return expressCheckoutRequest;
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

        private string AllHtmlEncode(string value)
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

        public ActionResult MerchantOnboarding_Upload()
        {

            SingleMerchantUpload();
            return View(appData);

        }

        public void SingleMerchantUpload()
        {
            try
            {
                MasterCardApiConfiguration.Sandbox = false;
                MasterCardApiConfiguration.ConsumerKey = this.appData.ProdConsumerKey;
                MasterCardApiConfiguration.PrivateKey = new X509Certificate2(Server.MapPath(this.appData.ProdKeystorePath), this.appData.KeystorePassword).PrivateKey;
                //Create an instance of MerchantUpload
                MerchantUpload merchantUpload = new MerchantUpload()
                        .WithMerchant(new Merchant()
                            .WithCheckoutBrand(new CheckoutBrand()
                                .WithName("Onboarding SDK")
                                .WithSandboxUrl("https://SPMerch58401.com")
                                .WithLogoUrl("http://www.mastercard.us/_globalAssets/img/nav/navl_logo_mastemasterca.png")
                                .WithProductionUrl("https://SPMerch58401.com")
                                .WithDisplayName("Onboarding SDK"))
                            .WithSPMerchantId("KT-OnboardingSDK-4")
                            .WithAction("C")
                            .WithMerchantAcquirer(new MerchantAcquirer().WithAcquirer(new Acquirer()
                                    .WithId("292978156")
                                    .WithAction("C")
                                    .WithName("QATESTACQ")
                                    .WithAssignedMerchantId("MCQA1")
                                    )
                                    .WithMerchantAcquirerBrand(new MerchantAcquirerBrand().WithCardBrand("MASTERCARD")))
                                    .WithAuthOption(new AuthOption().WithCardBrand("MASTER_CARD").WithType("ALL_TRANSACTIONS"))

                            .WithProfile(new Profile()
                                .WithDoingBusAs("Onboarding SDK")
                                .WithName("Onboarding SDK")
                                .WithEmails(new Emails()
                                    .WithEmailAddress("kaushal_tailor@mastercard.com"))
                                .WithPhone(new Phone()
                                    .WithNumber("3734517671")
                                    .WithCountryCode("1"))
                                .WithUrl("https://SPMerch58401.com")
                                .WithAddress(new Com.MasterCard.Merchant.Onboarding.Model.Address()
                                    .WithLine1("898 SPMerch58401")
                                    .WithPostalCode("78090")
                                    .WithCountry("US")
                                    .WithCity("SPMerch58401"))
                                .WithBusinessCategory("test")
                                .WithFedTaxId("211624440")));
                //Call the  with required params
                MerchantDownload merchantDownload = SingleMerchantUploadApi.Create("340302901", merchantUpload);
                appData.MerchantUploadRequest = XElement.Parse(Serializer<MerchantUpload>.Serialize(merchantUpload).OuterXml).ToString();
                appData.MerchantUploadResponse = XElement.Parse(Serializer<MerchantDownload>.Serialize(merchantDownload).OuterXml).ToString();
            }
            catch (SdkErrorResponseException ex)
            {
                if (ex.ErrorObject is Com.MasterCard.Sdk.Core.Model.Errors)
                {
                    var errors = ex.ErrorObject as Com.MasterCard.Sdk.Core.Model.Errors;
                    ViewBag.ErrorMessage = errors.ToJson();
                    // TODO: Handles errors
                }
                else
                {
                    ViewBag.ErrorMessage = ex.Message.ToString();
                }
            }
        }

        public ActionResult MerchantOnboarding_Validate()
        {

            SingleMerchantValidate();
            return View(appData);

        }

        public void SingleMerchantValidate()
        {
            try
            {
                MasterCardApiConfiguration.Sandbox = false;
                MasterCardApiConfiguration.ConsumerKey = this.appData.ProdConsumerKey;
                MasterCardApiConfiguration.PrivateKey = new X509Certificate2(Server.MapPath(this.appData.ProdKeystorePath), this.appData.KeystorePassword).PrivateKey;
                //Create an instance of MerchantUpload
                MerchantUpload merchantUpload = new MerchantUpload()
                                    .WithMerchant(new Merchant()
                                        .WithSPMerchantId("KT-OnboardingSDK-4")
                                        .WithAction("C")
                                        .WithProfile(new Profile()
                                            .WithName("Onboarding SDK")
                                            .WithDoingBusAs("Onboarding SDK")
                                            .WithFedTaxId("211624440")
                                            .WithUrl("https://SPMerch58401.com")
                                            .WithBusinessCategory("test")
                                            .WithAddress(new Com.MasterCard.Merchant.Onboarding.Model.Address()
                                                .WithLine1("898 SPMerch58401")
                                                .WithPostalCode("78090")
                                                .WithCountry("US")
                                                .WithCity("SPMerch58401"))
                                            .WithPhone(new Phone()
                                                .WithNumber("3734517671")
                                                .WithCountryCode("1"))
                                            .WithEmails(new Emails()
                                                .WithEmailAddress("kaushal_tailor@mastercard.com")))
                                        .WithCheckoutBrand(new CheckoutBrand()
                                            .WithName("Onboarding SDK")
                                            .WithDisplayName("Onboarding SDK")
                                            .WithProductionUrl("https://SPMerch58401.com")
                                            .WithSandboxUrl("https://SPMerch58401.com")
                                            .WithLogoUrl("http://www.mastercard.us/_globalAssets/img/nav/navl_logo_mastemasterca.png"))
                                        .WithAuthOption(new AuthOption()
                                            .WithCardBrand("MASTER_CARD")
                                            .WithType("ALL_TRANSACTIONS"))
                                        .WithMerchantAcquirer(new MerchantAcquirer()
                                            .WithAction("C") // Added "C" action in MerchantAcquirer
                                            .WithAcquirer(new Acquirer()
                                                .WithId("531615") // Changed Acquirer Id for Production Environment
                                                .WithName("QATESTACQ")
                                                .WithAssignedMerchantId("MCQA1")
                                                //.WithAction("C") // Removed "C" action from Acquirer
                                                )
                                            .WithMerchantAcquirerBrand(new MerchantAcquirerBrand()
                                                .WithCardBrand("MASTER_CARD") // Changed From MASTERCARD
                                                ))
                                        );

                //Call the  with required params
                ValidateFileResponse merchantDownload = SingleMerchantValidateApi.Create("340302901", merchantUpload);
                appData.MerchantUploadRequest = XElement.Parse(Serializer<MerchantUpload>.Serialize(merchantUpload).OuterXml).ToString();
                appData.MerchantUploadValidateResponse = XElement.Parse(Serializer<ValidateFileResponse>.Serialize(merchantDownload).OuterXml).ToString();
            }
            catch (SdkErrorResponseException ex)
            {
                if (ex.ErrorObject is Com.MasterCard.Sdk.Core.Model.Errors)
                {
                    var errors = ex.ErrorObject as Com.MasterCard.Sdk.Core.Model.Errors;
                    ViewBag.ErrorMessage = errors.ToJson();
                    // TODO: Handles errors
                }
                else
                {
                    ViewBag.ErrorMessage = ex.Message.ToString();
                }
            }
        }
    }
}