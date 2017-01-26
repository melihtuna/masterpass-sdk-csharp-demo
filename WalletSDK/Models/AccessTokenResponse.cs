using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterCard.WalletSDK.Models
{
    public class AccessTokenResponse
    {
        public string RequestToken { get; set; }
        public string Verifier { get; set; }
        public string CheckoutResourceUrl { get; set; }
        public string AccessToken { get; set; }
        public string PaymentShippingResource { get; set; }
        public string OAuthSecret { get; set; }
        public string AccessTokenCallAuthHeader { get; set; }
        public string AccessTokenCallSignatureBaseString { get; set; }

    }
}
