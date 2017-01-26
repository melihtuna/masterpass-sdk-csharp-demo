using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MasterCard.SDK;
using MasterCard.WalletSDK;
using MasterCard.WalletSDK.Models;
using MasterCard.WalletSDK.Models.All;
using MasterCard.WalletSDK.Models.Common;
using MasterCard.WalletSDK.Models.Switch;
using System.Security.Cryptography;

using allModels = MasterCard.WalletSDK.Models.All;

namespace MasterPassCSharp_Test
{
    [TestClass]
    public class MasterPassService_Test
    {
        
        //This tests the constructor method on MasterPassService, making sure that the class properly
        //extends the common Connector class.  Since consumerKey is private, the only way to test
        //the variable was set properly in the parent class is to utilize the OAuthParametersFactory
        //method, which returns the value of consumerKey.  Since the method is protected, we had
        //to create a mock class that extends MasterPassService, for the purposes of this test. 
        
        [TestMethod]
        public void TestConstructorAndOAuthParametersFactory()
        {
            string originUrl = "originUrl789";
            MasterPassServiceMock mps = new MasterPassServiceMock("consumerkey123", null, originUrl);
            Assert.IsNotNull(mps);

            string returnedKey = mps.GetConsumerKey();
            Assert.AreEqual("consumerkey123", returnedKey);
            Assert.AreEqual("originUrl789", mps.originUrl);

        }



        [TestMethod]
        public void TestOAuthParametersFactory()
        {
            string originUrl = "originUrl789";

            MasterPassServiceMock mps = new MasterPassServiceMock("consumerkey123", null, originUrl);

            OAuthParameters oAuthParams = mps.GetOAuthParameters();
            Assert.AreEqual("eWallet", oAuthParams.get(Connector.REALM));

        }


        //This tests the getAccessToken method, which is expected to do these things:
        //1) Call doRequest, passing along the three parameters provided
        //2) Parse the response into an array and return it
        //The doRequest method is overridden in MasterPassServiceMock, to verify it is called with the right parameters

        [TestMethod]
        public void TestGetAccessTokenReturnsAccessTokenResponse()
        {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            MasterPassServiceMock mps = new MasterPassServiceMock("consumerkey123", privateKey, "originurl789");

            string testAccessUrl = "GetAccessToken_1";
            string requestToken = "4d5e6f";
            string testVerifier = "7g8h9i";

            AccessTokenResponse response = mps.GetAccessToken(testAccessUrl, requestToken, testVerifier);

            Assert.AreEqual("token_4d5e6f", response.AccessToken);
            Assert.AreEqual("secret_7g8h9i", response.OAuthSecret);

        }


        //This tests the very important getRequestTokenAndRedirectUrl method, which performs two operations:
        //1) It calls doRequest, passing the requesturl and callbackurl, and populates the first five attributes 
        //of the return object.  2) It calls getConsumerSignInUrl, which concatenates a series of parameters into
        //a single URL string, which is assigned to the redirectURL attribute of the return object.  
        //The call to doRequest is mocked in MasterPassServiceMock.  But the actual logic in getConsumerSignInUrl
        //is executed and tested in this scenario.  
        [TestMethod]
	    public void TestGetRequestTokenAndRedirectUrl() {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            MasterPassServiceMock mps1 = new MasterPassServiceMock("consumerkey1", privateKey, "originurl789");

		    string requestTokenUrl = "GetRequestToken_1";	// This tells MasterPassServiceMock what to return via doRequest
		    string callbackUrl = "url456";
		    string acceptedCards = "mycard,yourcard,hiscard";
		    string checkoutIdentifier = "project123";
		    string xmlVersion = "v5";
		    Boolean suppressShippingFlag = true;
		    Boolean rewardsProgramFlag = true;
		    Boolean authLevelBasicFlag = true;
		    string shippingProfile = "profile123";

            RequestTokenResponse returnObject = mps1.GetRequestTokenAndRedirectURL(requestTokenUrl, callbackUrl, acceptedCards, checkoutIdentifier, 
                xmlVersion, suppressShippingFlag, rewardsProgramFlag, authLevelBasicFlag, shippingProfile);

            Assert.AreEqual(returnObject.RequestToken, "a1");
		    Assert.AreEqual(returnObject.AuthorizeUrl, "b2");
		    Assert.AreEqual(returnObject.CallbackConfirmed, true);
		    Assert.AreEqual(returnObject.OAuthExpiresIn, "d4");
		    Assert.AreEqual(returnObject.OAuthSecret, "e5");

            var expectedRedirectURL =
            "b2?" + MasterPassService.ACCEPTABLE_CARDS + "=mycard,yourcard,hiscard"
            + "&" + MasterPassService.CHECKOUT_IDENTIFIER + "=project123"
            + "&" + MasterPassService.OAUTH_TOKEN + "=a1"
            + "&" + MasterPassService.VERSION + "=v5"
            + "&" + MasterPassService.AUTH_LEVEL + "=" + MasterPassService.BASIC
            + "&" + MasterPassService.SUPPRESS_SHIPPING_ADDRESS + "=true"
            + "&" + MasterPassService.ACCEPT_REWARDS_PROGRAM + "=true"
            + "&" + MasterPassService.SHIPPING_LOCATION_PROFILE + "=profile123";
		
		    Assert.AreEqual(expectedRedirectURL, returnObject.RedirectUrl);
	    }
	
         //This is the same as the previous test, except the xmlVersion parameter is "V1".  The last four
         //parameters in the redirectURL are expected to be omitted in this scenario.
        [TestMethod]
        public void TestGetRequestTokenAndRedirectUrl_XmlVersion1()
        {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            MasterPassServiceMock mps1 = new MasterPassServiceMock("consumerkey1", privateKey, "originurl789");

            string requestTokenUrl = "GetRequestToken_1";	// This tells MasterPassServiceMock what to return via doRequest
            string callbackUrl = "url456";
            string acceptedCards = "mycard,yourcard,hiscard";
            string checkoutIdentifier = "project123";
            string xmlVersion = "v1";
            Boolean suppressShippingFlag = true;
            Boolean rewardsProgramFlag = true;
            Boolean authLevelBasicFlag = true;
            string shippingProfile = "profile123";

            RequestTokenResponse returnObject = mps1.GetRequestTokenAndRedirectURL(requestTokenUrl, callbackUrl, acceptedCards, checkoutIdentifier,
                xmlVersion, suppressShippingFlag, rewardsProgramFlag, authLevelBasicFlag, shippingProfile);

            Assert.AreEqual(returnObject.RequestToken, "a1");
            Assert.AreEqual(returnObject.AuthorizeUrl, "b2");
            Assert.AreEqual(returnObject.CallbackConfirmed, true);
            Assert.AreEqual(returnObject.OAuthExpiresIn, "d4");
            Assert.AreEqual(returnObject.OAuthSecret, "e5");

            var expectedRedirectURL =
            "b2?" + MasterPassService.ACCEPTABLE_CARDS + "=mycard,yourcard,hiscard"
            + "&" + MasterPassService.CHECKOUT_IDENTIFIER + "=project123"
            + "&" + MasterPassService.OAUTH_TOKEN + "=a1"
            + "&" + MasterPassService.VERSION + "=v1";

            Assert.AreEqual(expectedRedirectURL, returnObject.RedirectUrl);
        }

        //This is the same as the previous test, except the xmlVersion parameter is "V3".  The last two
        //parameters in the redirectURL are expected to be omitted in this scenario.
        [TestMethod]
        public void TestGetRequestTokenAndRedirectUrl_XmlVersion3()
        {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            MasterPassServiceMock mps1 = new MasterPassServiceMock("consumerkey1", privateKey, "originurl789");

            string requestTokenUrl = "GetRequestToken_1";	// This tells MasterPassServiceMock what to return via doRequest
            string callbackUrl = "url456";
            string acceptedCards = "mycard,yourcard,hiscard";
            string checkoutIdentifier = "project123";
            string xmlVersion = "v3";
            Boolean suppressShippingFlag = true;
            Boolean rewardsProgramFlag = true;
            Boolean authLevelBasicFlag = true;
            string shippingProfile = "profile123";

            RequestTokenResponse returnObject = mps1.GetRequestTokenAndRedirectURL(requestTokenUrl, callbackUrl, acceptedCards, checkoutIdentifier,
                xmlVersion, suppressShippingFlag, rewardsProgramFlag, authLevelBasicFlag, shippingProfile);

            Assert.AreEqual(returnObject.RequestToken, "a1");
            Assert.AreEqual(returnObject.AuthorizeUrl, "b2");
            Assert.AreEqual(returnObject.CallbackConfirmed, true);
            Assert.AreEqual(returnObject.OAuthExpiresIn, "d4");
            Assert.AreEqual(returnObject.OAuthSecret, "e5");

            var expectedRedirectURL =
            "b2?" + MasterPassService.ACCEPTABLE_CARDS + "=mycard,yourcard,hiscard"
            + "&" + MasterPassService.CHECKOUT_IDENTIFIER + "=project123"
            + "&" + MasterPassService.OAUTH_TOKEN + "=a1"
            + "&" + MasterPassService.VERSION + "=v3"
            + "&" + MasterPassService.AUTH_LEVEL + "=" + MasterPassService.BASIC
            + "&" + MasterPassService.SUPPRESS_SHIPPING_ADDRESS + "=true";

            Assert.AreEqual(expectedRedirectURL, returnObject.RedirectUrl);
        }

	
        // This tests postShoppingCartData, which is a fairly simple method.  I have some questions about
        // whether there needs to be additional functionality in this method.
	    [TestMethod]
	    public void TestPostShoppingCartData() {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            var mps = new MasterPassServiceMock("consumerkey1", privateKey, "originurl789");
		
		    var shoppingCartUrl = "PostShoppingCartData_1";  // This tells MasterPassServiceMock what to return via doRequest
		    ShoppingCartRequest shoppingCartReq = new ShoppingCartRequest();
            shoppingCartReq.OAuthToken = "token123";
            shoppingCartReq.OriginUrl = "url456";
            shoppingCartReq.ShoppingCart = new allModels.ShoppingCart();

		
		    ShoppingCartResponse returnObject = mps.PostShoppingCartData(shoppingCartUrl, shoppingCartReq);
		
		    var expectedToken = "token123_url456";
		
		    Assert.AreEqual(expectedToken, returnObject.OAuthToken);
	    }

        // This tests postMerchantInitData, which is a fairly simple method.  Very similar to PostShoppingCartData.
        [TestMethod]
        public void TestPostMerchantInitData()
        {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            var mps = new MasterPassServiceMock("consumerkey1", privateKey, "originurl789");

            var merchantInitUrl = "PostMerchantInitData_1";  // This tells MasterPassServiceMock what to return via doRequest
            MerchantInitializationRequest merchantInitReq = new MerchantInitializationRequest();
            merchantInitReq.OAuthToken = "token123";
            merchantInitReq.OriginUrl = "url456";
            merchantInitReq.PreCheckoutTransactionId = "precheckout789";

            MerchantInitializationResponse returnObject = mps.PostMerchantInitData(merchantInitUrl, merchantInitReq);

            var expectedToken = "token123_url456";

            Assert.IsNotNull(returnObject);
            Assert.AreEqual(expectedToken, returnObject.OAuthToken);
        }

	
        //This tests getPaymentShippingResource, which is another simple method.  It takes the token and passes it
        //to doRequest.  DoRequest is mocked, and simply returns the token back out to verify it was called.
	    [TestMethod]
	    public void TestGetPaymentShippingResource() {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            var mps = new MasterPassServiceMock("consumerkey1", privateKey, "originurl789");
		
		    var checkoutResourceUrl = "GetPaymentShippingResource_1";
		    var accessToken = "token123";

            allModels.Checkout returnObject = mps.GetPaymentShippingResource(checkoutResourceUrl, accessToken);
		
		    var expectedResult = "2031782";
		
		    Assert.AreEqual(expectedResult, returnObject.TransactionId);
		
	    }

        // This tests postShoppingCartData, which is a fairly simple method.  I have some questions about
        // whether there needs to be additional functionality in this method, similar to postShoppingCartData.
        [TestMethod]
        public void TestPostCheckoutTransaction()
        {
            AsymmetricAlgorithm privateKey = AsymmetricAlgorithm.Create("privatekey456");
            var mps = new MasterPassServiceMock("consumerkey1", privateKey, "originurl789");
		
		    var postbackurl = "PostCheckoutTransaction_1";  // This tells MasterPassServiceMock what to return via doRequest
		    MerchantTransactions merchantTransactions = new MerchantTransactions();
            MerchantTransaction mt = new MerchantTransaction();
            mt.ApprovalCode = "test123";
            merchantTransactions.MerchantTransactions1.Add(mt);
		
		    MerchantTransactions returnObject = mps.PostCheckoutTransaction(postbackurl, merchantTransactions);
		
		    string expectedResult = "doRequest_called_with_test123";
		
		    Assert.AreEqual(returnObject.MerchantTransactions1[0].ApprovalCode, expectedResult);
        }
		
    }
}
