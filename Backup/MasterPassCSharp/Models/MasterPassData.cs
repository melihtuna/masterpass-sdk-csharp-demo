using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using MasterCard.WalletSDK.Models;
using MasterCard.WalletSDK.Models.All;

using PairingDataTypeType = MasterCard.WalletSDK.Models.All.PairingDataTypeType;

namespace MasterCard.WalletWebContent.Models
{
    public class MasterPassData
    {

        public MasterPassData()
        {
            this.RequestUrl = ConfigurationManager.AppSettings["RequestUrl"];
            this.ShoppingCartUrl = ConfigurationManager.AppSettings["ShoppingCartUrl"];
            this.AccessUrl = ConfigurationManager.AppSettings["AccessURL"];
            this.PostbackUrl = ConfigurationManager.AppSettings["PostbackUrl"];
            this.PreCheckoutUrl = ConfigurationManager.AppSettings["PreCheckoutUrl"];
            this.MerchantInitUrl = ConfigurationManager.AppSettings["MerchantInitUrl"];
            this.LightboxUrl = ConfigurationManager.AppSettings["LightboxUrl"];
            this.PairingCallbackPath = ConfigurationManager.AppSettings["PairingCallbackPath"];
            this.CartCallbackPath = ConfigurationManager.AppSettings["CartCallbackPath"];
            this.CallbackDomain = ConfigurationManager.AppSettings["CallbackDomain"];
            this.CallbackPath = ConfigurationManager.AppSettings["CallbackPath"];
            this.ConnectedCallbackPath = ConfigurationManager.AppSettings["ConnectedCallbackPath"];
            this.ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            this.CheckoutIdentifier = ConfigurationManager.AppSettings["CheckoutIdentifier"];
            this.KeystorePath = ConfigurationManager.AppSettings["KeystorePath"];
            this.KeystorePassword = ConfigurationManager.AppSettings["KeystorePassword"];
            //this.Realm = ConfigurationManager.AppSettings["Realm"];
            this.AuthLevelBasic = ("true").Equals(ConfigurationManager.AppSettings["Auth_Level_Basic"]);
            //this.XmlVersion = ConfigurationManager.AppSettings["XmlVersion"];
            this.ShippingSuppression = ("true").Equals(ConfigurationManager.AppSettings["ShippingSuppression"]);
            this.ShippingProfiles = ConfigurationManager.AppSettings["ShippingProfiles"];
            this.AllowedLoyaltyPrograms = ConfigurationManager.AppSettings["AllowedLoyaltyPrograms"];
        }

        public RequestTokenResponse requestTokenResponse { get; set; }
        public AccessTokenResponse accessTokenResponse { get; set; }
        public AccessTokenResponse longAccessTokenResponse { get; set; }

        public string ApprovalCode = "sample";
        public string AcceptedCards { get; set; }
        public string AccessToken { get; set; }

        [DisplayName("Access URL: ")]
        public string AccessUrl { get; set; }

        public string AllowedLoyaltyPrograms { get; set; }
        public bool AuthLevelBasic { get; set; }
        public string CallbackDomain { get; set; }
        public string CallbackPath { get; set; }

        [DisplayName("Callback URL: ")]
        public string CallbackUrl
        {
            get
            {
                return this.CallbackDomain + this.CallbackPath;
            }
        }

        public string CartCallbackPath { get; set; }
        public object CartCallbackUrl
        {
            get
            {
                return this.CallbackDomain + this.CartCallbackPath;
            }
        }
        public Checkout Checkout { get; set; }

        [DisplayName("Checkout Identifier: ")]
        public string CheckoutIdentifier { get; set; }

        public string CheckoutUrl { get; set; }
        public string CheckoutXML { get; set; }
        public string ConnectedCallbackPath { get; set; }
        public object ConnectedCallbackUrl
        {
            get
            {
                return this.CallbackDomain + this.ConnectedCallbackPath;
            }
        }

        [DisplayName("Consumer Key: ")]
        public string ConsumerKey { get; set; }

        public string ConsumerWalletId { get; set; }

        [DisplayName("Keystore Path: ")]
        public string KeystorePath { get; set; }
        
        [DisplayName("Keystore Password: ")]
        public string KeystorePassword { get; set; }

        [DisplayName("Lightbox URL: ")]
        public string LightboxUrl { get; set; }

        public string LongAccessToken { get; set; }
        public string LongAccessSecret { get; set; }
        public string MerchantInitRequest { get; set; }
        public string MerchantInitResponse { get; set; }

        [DisplayName("Merchant Init URL: ")]
        public string MerchantInitUrl { get; set; }

        public string OriginUrl 
        {
            get
            {
                return this.CallbackDomain;
            }
        }

        public string PairingCallbackPath { get; set; }

        [DisplayName("Pairing Callback URL: ")]
        public string PairingCallbackUrl
        {
            get
            {
                return this.CallbackDomain + this.PairingCallbackPath;
            }
        }

        public List<string> PairingDataTypes { get; set; }
        public string PairingToken { get; set; }
        public string PairingVerifier { get; set; }

        [DisplayName("Postback URL: ")]
        public string PostbackUrl { get; set; }

        public PrecheckoutData PreCheckoutData { get; set; }
        public string PrecheckoutResponse { get; set; }
        public string PreCheckoutRequest { get; set; }
        public string PrecheckoutDataXml { get; set; }
        public string PrecheckoutDataJson { get; set; }
        public string PrecheckoutShippingId { get; set; }
        public string PrecheckoutCardid { get; set; }
        public string PrecheckoutTransactionId { get; set; }

        [DisplayName("PreCheckout URL: ")]
        public string PreCheckoutUrl { get; set; }

        public string Realm { get; set; }
        public string RequestToken { get; set; }

        [DisplayName("Request URL: ")]
        public string RequestUrl { get; set; }

        public Boolean RewardsProgram { get; set; }

        private string shippingProfilesString;
        public string ShippingProfiles
        {
            get
            {
                return shippingProfilesString;
            }
            set
            {
                shippingProfilesString = value;
            }
        }

        public bool ShippingSuppression { get; set; }
        public string ShoppingCartRequest { get; set; }
        public string ShoppingCartResponse { get; set; }

        [DisplayName("Shopping Cart URL: ")]
        public string ShoppingCartUrl { get; set; }

        public long tax = 348;
        public long shipping = 895;
        public string TransactionId { get; set; }

        public string Verifier { get; set; }
        public string WalletName { get; set; }
        public string XmlVersion { get; set; }
    }
}