using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MasterCard.SDK;
using MasterCard.WalletSDK;
using MasterCard.WalletSDK.Models;
using MasterCard.WalletSDK.Models.All;
using MasterCard.WalletSDK.Models.Common;
using MasterCard.WalletSDK.Models.Switch;
using System.Security.Cryptography;

using Address = MasterCard.WalletSDK.Models.All.Address;
using Checkout = MasterCard.WalletSDK.Models.All.Checkout;
using DateOfBirth = MasterCard.WalletSDK.Models.All.DateOfBirth;
using Gender = MasterCard.WalletSDK.Models.All.Gender;

namespace MasterPassCSharp_Test
{
    public class MasterPassServiceMock : MasterPassService
    {

        public MasterPassServiceMock(string consumerKey, AsymmetricAlgorithm privateKey, string originUrl) : base(consumerKey, privateKey, originUrl)
        {
        }

        public string GetConsumerKey()
        {
            OAuthParameters oap = this.OAuthParametersFactory();
            return oap.get(OAUTH_CONSUMER_KEY);
        }

        public OAuthParameters GetOAuthParameters()
        {
            return this.OAuthParametersFactory();
        }

        protected override Dictionary<string, string> doRequest(String httpsURL, String requestMethod, OAuthParameters oparams, String body)
        {
            Dictionary<string, string> returnDict = new Dictionary<string, string>();
            returnDict.Add(Connector.HTTP_CODE, "200");

            switch (httpsURL)
            {
                case "GetAccessToken_1":
                    var token_response = "token_" + oparams.get("oauth_token");
                    var secret_response = "secret_" + oparams.get("oauth_verifier");

                    returnDict.Add(Connector.MESSAGE, "oauth_token=" + token_response + "&oauth_token_secret=" + secret_response);
                    
                    break;

            	case "GetRequestToken_1":
                    var param1 = MasterPassService.OAUTH_TOKEN + Connector.EQUALS + "a1";
                    var param2 = MasterPassService.XOAUTH_REQUEST_AUTH_URL + Connector.EQUALS + "b2";
                    var param3 = MasterPassService.OAUTH_CALLBACK_CONFIRMED + Connector.EQUALS + "true";
                    var param4 = MasterPassService.OAUTH_EXPIRES_IN + Connector.EQUALS + "d4";
                    var param5 = MasterPassService.OAUTH_TOKEN_SECRET + Connector.EQUALS + "e5";

                    var msg = param1 + Connector.AMP + param2 + Connector.AMP + param3 + Connector.AMP + param4 + Connector.AMP + param5;
                    
                    returnDict.Add(Connector.MESSAGE, msg);

                    break;

                case "PostShoppingCartData_1":
                    ShoppingCartRequest shoppingCartReq = Serializer<ShoppingCartRequest>.Deserialize(body);
                    var requestToken = shoppingCartReq.OAuthToken;
                    var originUrl = shoppingCartReq.OriginUrl;
                    var returnToken = requestToken + "_" + originUrl;

                    returnDict.Add(Connector.MESSAGE, "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><ShoppingCartResponse><OAuthToken>"+ returnToken +"</OAuthToken></ShoppingCartResponse>");

                    break;

                case "PostMerchantInitData_1":
                    MerchantInitializationRequest merchantInitReq = Serializer<MerchantInitializationRequest>.Deserialize(body);
                    var requestToken2 = merchantInitReq.OAuthToken;
                    var originUrl2 = merchantInitReq.OriginUrl;
                    var returnToken2 = requestToken2 + "_" + originUrl2;

                    returnDict.Add(Connector.MESSAGE, "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MerchantInitializationResponse><OAuthToken>" + returnToken2 + "</OAuthToken></MerchantInitializationResponse>");

                    break;

                case "GetPaymentShippingResource_1":
                    var token_2 = oparams.get("Token");

                    Checkout checkout = new Checkout();
                    Card card = new Card();
                    card.BrandId = "master";
                    card.BrandName = "MasterCard";
                    card.AccountNumber = "5453010000064154";
                    Address billingAddress = new Address();
                    billingAddress.City = "Niagara falls";
                    billingAddress.Country = "CA";
                    billingAddress.CountrySubdivision = "CA-ON";
                    billingAddress.Line1 = "5943 Victoria Ave";
                    billingAddress.PostalCode = "L2G 3L7";
                    card.BillingAddress = billingAddress;
                    card.CardHolderName = "JOE Test";
                    card.ExpiryMonth = 5;
                    card.ExpiryYear = 2017;
                    checkout.Card = card;
                    AuthenticationOptions options = new AuthenticationOptions();
                    options.AuthenticateMethod = "test_authenticatemethod";
                    options.CardEnrollmentMethod = "test_cardenrollmentmethod";
                    options.CAvv = "test_cavv";
                    options.EciFlag = "test_eciflag";
                    options.MasterCardAssignedID = "test_mastercardassignedid";
                    options.PaResStatus = "test_paresstatus";
                    options.SCEnrollmentStatus = "test_scenrollmentstatus";
                    options.SignatureVerification = "test_signatureverification";
                    options.Xid = "test_xid";
                    checkout.AuthenticationOptions = options;
                    Contact contact = new Contact();
                    contact.Country = "CA";
                    DateOfBirth dateOfBirth = new DateOfBirth();
                    dateOfBirth.Day = 1;
                    dateOfBirth.Month = 1;
                    dateOfBirth.Year = 1;
                    contact.DateOfBirth = dateOfBirth;
                    contact.EmailAddress = "test_emailaddress";
                    contact.FirstName = "test_firstname";
                    contact.Gender = Gender.M;
                    contact.GenderSpecified = true;
                    contact.LastName = "test_lastname";
                    contact.MiddleName = "test_middlename";
                    contact.NationalID = "test_nationalid";
                    contact.PhoneNumber = "test_phonenumber";
                    checkout.Contact = contact;
                    checkout.PreCheckoutTransactionId = "2031782";
                    RewardProgram rewardProgram = new RewardProgram();
                    rewardProgram.ExpiryMonth = 1;
                    rewardProgram.ExpiryYear = 1;
                    checkout.RewardProgram = rewardProgram;
                    ShippingAddress shippingAddress = new ShippingAddress();
                    shippingAddress.City = "Niagara falls";
                    shippingAddress.Country = "CA";
                    shippingAddress.CountrySubdivision = "CA-ON";
                    shippingAddress.Line1 = "5943 Victoria Ave";
                    shippingAddress.PostalCode = "L2G 3L7";
                    shippingAddress.RecipientName = "JOE  Test";
                    shippingAddress.RecipientPhoneNumber = "1-2061113333";
                    checkout.ShippingAddress = shippingAddress;
                    checkout.TransactionId = "2031782";
                    checkout.WalletID = "test_walletid";

                    var msg2 = Serializer<Checkout>.Serialize(checkout).OuterXml;

                    //var msg2 = "<?xml version='1.0' encoding='UTF-8'?>" +
                    //        "<Checkout>" + 
                    //        "<Card>" + 
                    //        "<BrandId>master</BrandId>" +
                    //        "<BrandName>MasterCard</BrandName>" +
                    //        "<AccountNumber>5453010000064154</AccountNumber>" +
                    //        "<BillingAddress><City>Niagara falls</City><Country>CA</Country>" +
                    //        "<CountrySubdivision>CA-ON</CountrySubdivision>" +
                    //        "<Line1>5943 Victoria Ave</Line1><Line2/><PostalCode>L2G 3L7</PostalCode>" +
                    //        "</BillingAddress>" +
                    //        "<CardHolderName>JOE Test</CardHolderName><ExpiryMonth>5</ExpiryMonth>" +
                    //        "<ExpiryYear>2017</ExpiryYear>" + 
                    //        "</Card>" + 
                    //        "<AccessToken>"+ token_2 +"</AccessToken>" +
                    //        "<TransactionId>2031782</TransactionId>" +
                    //        "<Contact><FirstName>JOE</FirstName><LastName>Test</LastName><Country>US</Country>" +
                    //        "<EmailAddress>joe.test@email.com</EmailAddress><PhoneNumber>1-9876543210</PhoneNumber>" +
                    //        "</Contact>" +
                    //        "<ShippingAddress><City>Seattle</City><Country>US</Country>" +
                    //        "<CountrySubdivision>US-WA</CountrySubdivision><Line1>1326 5th Ave SE</Line1><Line2/><PostalCode>98101</PostalCode>" +
                    //        "<RecipientName>JOE  Test</RecipientName><RecipientPhoneNumber>1-2061113333</RecipientPhoneNumber>" +
                    //        "</ShippingAddress><PayPassWalletIndicator>101</PayPassWalletIndicator>" + 
                    //        "</Checkout>";

                    returnDict.Add(Connector.MESSAGE, msg2);
                    break;

                case "PostCheckoutTransaction_1":
                    MerchantTransactions merchantTransactions = Serializer<MerchantTransactions>.Deserialize(body);
                    var approvalCode = "doRequest_called_with_" + merchantTransactions.MerchantTransactions1[0].ApprovalCode;

                    var msg3 = "<?xml version='1.0' encoding='UTF-8'?>" + 
                            "<MerchantTransactions><MerchantTransactions>" + 
                            "<TransactionId>2007408</TransactionId>" + 
                            "<ConsumerKey>cLb0tKkEJhGTITp_6ltDIibO5Wgbx4rIldeXM_jRd4b0476c!414f4859446c4a366c726a327474695545332b353049303d</ConsumerKey>" + 
                            "<Currency>USD</Currency><OrderAmount>76239</OrderAmount><PurchaseDate>2014-05-07T16:40:13.847-05:00</PurchaseDate>" +
                            "<TransactionStatus>Success</TransactionStatus><ApprovalCode>" + approvalCode + "</ApprovalCode>" + 
                            "</MerchantTransactions></MerchantTransactions>";

                    returnDict.Add(Connector.MESSAGE, msg3);
                    break;
            }

            return returnDict;
        }




    }
}
