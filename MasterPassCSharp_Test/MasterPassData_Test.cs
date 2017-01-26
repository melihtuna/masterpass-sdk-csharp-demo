using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MasterCard.WalletWebContent.Models;

namespace MasterPassCSharp_Test
{
    [TestClass]
    public class MasterPassData_Test
    {
        private string requestUrl = "test_requesturl";
        private string shoppingCartUrl = "test_shoppingcarturl";
        private string accessUrl = "test_accessurl";
        private string postbackUrl = "test_postbackurl";
        private string precheckoutUrl = "test_precheckouturl";
        private string merchantInitUrl = "test_merchantiniturl";
        private string lightboxUrl = "test_lightboxurl";
        private string pairingCallbackPath = "test_pairingcallbackpath";
        private string cartCallbackPath = "test_cartcallbackpath";
        private string callbackDomain = "test_callbackdomain";
        private string callbackPath = "test_callbackpath";
        private string connectedCallbackPath = "test_connectedcallbackpath";
        private string consumerKey = "test_consumerkey";
        private string checkoutIdentifier = "test_checkoutidentifier";
        private string keystorePath = "test_keystorepath";
        private string keystorePassword = "test_keystorepassword";
        private string realm = "test_realm";
        private string authLevelBasic = "test_authlevelbasic";
        private string xmlVersion = "test_xmlversion";
        private string shippingSuppression = "test_shippingsuppression";
        private string shippingProfiles = "test_shippingprofiles";
        private string allowedLoyaltyPrograms = "test_allowedloyaltyprograms";

        [TestInitialize]
        public void setUp()
        {
            ConfigurationManager.AppSettings["RequestUrl"] = requestUrl;
            ConfigurationManager.AppSettings["ShoppingCartUrl"] = shoppingCartUrl;
            ConfigurationManager.AppSettings["AccessUrl"] = accessUrl;
            ConfigurationManager.AppSettings["PostbackUrl"] = postbackUrl;
            ConfigurationManager.AppSettings["PrecheckoutUrl"] = precheckoutUrl;
            ConfigurationManager.AppSettings["MerchantInitUrl"] = merchantInitUrl;
            ConfigurationManager.AppSettings["LightboxUrl"] = lightboxUrl;
            ConfigurationManager.AppSettings["PairingCallbackPath"] = pairingCallbackPath;
            ConfigurationManager.AppSettings["CartCallbackPath"] = cartCallbackPath;
            ConfigurationManager.AppSettings["CallbackDomain"] = callbackDomain;
            ConfigurationManager.AppSettings["CallbackPath"] = callbackPath;
            ConfigurationManager.AppSettings["ConnectedCallbackPath"] = connectedCallbackPath;
            ConfigurationManager.AppSettings["ConsumerKey"] = consumerKey;
            ConfigurationManager.AppSettings["CheckoutIdentifier"] = checkoutIdentifier;
            ConfigurationManager.AppSettings["KeystorePath"] = keystorePath;
            ConfigurationManager.AppSettings["KeystorePassword"] = keystorePassword;
            ConfigurationManager.AppSettings["Realm"] = realm;
            ConfigurationManager.AppSettings["Auth_Level_Basic"] = authLevelBasic;
            ConfigurationManager.AppSettings["XmlVersion"] = xmlVersion;
            ConfigurationManager.AppSettings["ShippingSuppression"] = shippingSuppression;
            ConfigurationManager.AppSettings["ShippingProfiles"] = shippingProfiles;
            ConfigurationManager.AppSettings["AllowedLoyaltyPrograms"] = allowedLoyaltyPrograms;

        }

        [TestMethod]
        public void testConstructorPopulatesAttributesFromConfigurationManager()
        {

            MasterPassData sad = new MasterPassData();
            Assert.AreEqual(requestUrl, sad.RequestUrl);
            Assert.AreEqual(shoppingCartUrl, sad.ShoppingCartUrl);
            Assert.AreEqual(accessUrl, sad.AccessUrl);
            Assert.AreEqual(postbackUrl, sad.PostbackUrl);
            Assert.AreEqual(precheckoutUrl, sad.PreCheckoutUrl);
            Assert.AreEqual(merchantInitUrl, sad.MerchantInitUrl);
            Assert.AreEqual(lightboxUrl, sad.LightboxUrl);
            Assert.AreEqual(pairingCallbackPath, sad.PairingCallbackPath);
            Assert.AreEqual(cartCallbackPath, sad.CartCallbackPath);
            Assert.AreEqual(callbackDomain, sad.CallbackDomain);
            Assert.AreEqual(callbackPath, sad.CallbackPath);
            Assert.AreEqual(connectedCallbackPath, sad.ConnectedCallbackPath);
            Assert.AreEqual(consumerKey, sad.ConsumerKey);
            Assert.AreEqual(checkoutIdentifier, sad.CheckoutIdentifier);
            Assert.AreEqual(keystorePath, sad.KeystorePath);
            Assert.AreEqual(keystorePassword, sad.KeystorePassword);
            //Assert.AreEqual(realm, sad.Realm);
            Assert.AreEqual(authLevelBasic.Equals("true"), sad.AuthLevelBasic);
            //Assert.AreEqual(xmlVersion, sad.XmlVersion);
            Assert.AreEqual(shippingSuppression.Equals("true"), sad.ShippingSuppression);
            Assert.AreEqual(shippingProfiles, sad.ShippingProfiles);
            Assert.AreEqual(allowedLoyaltyPrograms, sad.AllowedLoyaltyPrograms);
            Assert.AreEqual(sad.CallbackUrl, callbackDomain + callbackPath);
            Assert.AreEqual(sad.PairingCallbackUrl, callbackDomain + pairingCallbackPath);
            Assert.AreEqual(sad.CartCallbackUrl, callbackDomain + cartCallbackPath);
            Assert.AreEqual(sad.ConnectedCallbackUrl, callbackDomain + connectedCallbackPath);

        }

    }
}
