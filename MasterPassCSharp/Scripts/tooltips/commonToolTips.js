$(document).ready(function() {
	// -----------  Index Page--------------------------------------------------------------------------//
	
	$('#hostFile').qtip(ToolTipWithLink('/Tooltips/HostFile.html','Host File',650));
	// Config Data  -----------------------------------------------------------------------------------//
	$('#conKey').qtip(ToolTipWithLink( '/Tooltips/ConsumerKey.html','Consumer Key',900));
	$('#chkId').qtip(ToolTipWithLink('/Tooltips/CheckoutID.html','Checkout Identifier',900));
	$('#keyStorepath').qtip(ToolTip('Private Key is managed by the merchant and corresponding public key is stored on MasterPass side. Key exchange happens on MasterCard developer zone (https://developer.mastercard.com). Every call to MasterPass has to be signed by the merchant\'s private key.',900));
	$('#keyStorepass').qtip(ToolTip('Private Key is managed by the merchant and corresponding public key is stored on MasterPass side. Key exchange happens on MasterCard developer zone (https://developer.mastercard.com). Every call to MasterPass has to be signed by the merchant\'s private key.',900));
	$('#callback').qtip(ToolTipWithLink('/Tooltips/CallbackURL.html','Callback URL',900));
	
	// Redirect Options  -----------------------------------------------------------------------------------//	
	$('#xmlversion').qtip(ToolTip('MasterPass api version: Should be v5',''));
	$('#shippingsuppression').qtip(ToolTip('This parameter will allow merchants to suppress shipping address. Please refer to the integration guide for more details.',''));
	$('#rewards').qtip(ToolTip('This parameter will allow merchants to retrieve consumer reward details. Please refer to the integration guide for more details.',''));
	$('#authlevel').qtip(ToolTip('This parameter will allow merchants to retrieve consumer reward details. Please refer to the integration guide for more details.',''));
	$('#shippingprofile').qtip(ToolTip('Shipping profile id as specified in your profile. Please refer to the integration guide for more details.',''));
	$('#acceptedcards').qtip(ToolTip('Card types accepted by merchant. Please refer to the integration guide for the IDs associated with these card types.',''));
	$('#privatelabel').qtip(ToolTip('ID for Private Label card. Please refer to the integration guide for more details.',''));
	$('#walletSelector').qtip(ToolTip('Used to test the wallet sector flag that the partner wallets will use',''));
	$('#postbackVersion').qtip(ToolTip('This option will override the postback URL and XML to the version specified in this dropdown (only for OAuth Flow)',100));

	//Common tooltips   -----------------------------------------------------------------------------------//
	$('#authHeader').qtip(ToolTip('Authorization Header',''));
	$('#sbs').qtip(ToolTip('Signature Base String',''));
	$('#requestEndpoint').qtip(ToolTip('Endpoint to the request the request token from MasterPass services. <br>Sandbox URL: https://sandbox.api.mastercard.com/oauth/consumer/v1/request_token<br>Production URL: https://api.mastercard.com/oauth/consumer/v1/request_token',600));
	$('#shopEndpoint').qtip(ToolTip('Endpoint to the post the shopping cart data to MasterPass services.<br>Sandbox URL: https://sandbox.api.mastercard.com/online/v1/shopping-cart <br>Production URL: https://api.mastercard.com/online/v1/shopping-cart',600));
	$('#accessEndpoint').qtip(ToolTip('Endpoint to the request the access token from MasterPass services.<br>Sandbox URL: https://sandbox.api.mastercard.com/oauth/consumer/v1/access_token <br>Production URL: https://api.mastercard.com/oauth/consumer/v1/access_token',600));
	$('#postbackEndpoint').qtip(ToolTip('Endpoint to the post the post transaction data to MasterPass services.<br>Sandbox URL: https://api.mastercard.com/online/v2/transaction <br>Production URL: https://api.mastercard.com/online/v2/transaction',500));

	// Request Token  -----------------------------------------------------------------------------------//
	$('#requestToken').qtip(ToolTip('...',''));
	$('#authorizeurl').qtip(ToolTip('...',''));
	$('#tokenexpires').qtip(ToolTip('...',''));
	$('#oauthSecret').qtip(ToolTip('...',''));
	
	// Shopping Cart  -----------------------------------------------------------------------------------//
	$('#ShoppingXML').qtip(ToolTip('...',''));
	$('#ShoppingResponse').qtip(ToolTip('...',''));
	$('#redirectURL').qtip(ToolTip('...',''));
	
	// Callback  -----------------------------------------------------------------------------------//
	$('#requestToken').qtip(ToolTip('...',''));
	$('#verifier').qtip(ToolTip('...',''));
	$('#checkoutURL').qtip(ToolTip('...',''));
	$('#returnProfileName').qtip(ToolTip('...',''));
			
	//Access Token  -----------------------------------------------------------------------------------//
	$('#accessToken').qtip(ToolTip('...',''));
	$('#oauthSecret').qtip(ToolTip('...',''));	
			
	// Checkout  -----------------------------------------------------------------------------------//
	$('#checkoutEndpoint').qtip(ToolTip('...',''));		
	$('#chekoutXML').qtip(ToolTip('...',''));			
	$('#sampleForm').qtip(ToolTip('...',''));		
		
	// Postback  -----------------------------------------------------------------------------------//
	$('#postbackReceived').qtip(ToolTip('...',''));			
	$('#postbackSent').qtip(ToolTip('...',''));
			
});
	
	
function ToolTipWithLink(ttUrl,text,width) {
	  toolTipCfg = {
			  content: { 
				  url: window.location.protocol + "//" + window.location.host + ttUrl,
				  title: {
					   text: text,
					   button: 'Close'
				   		}
                    },
		   style: { 
			      width: width
		   		},  
            
		   show: { 
                when: { event: 'click' } 
                },
		   hide: { 
                when: { event: 'click' } 
                },
//		   show: 'mouseover',
//		   hide: 'mouseout',
		   position: {  
                adjust: { screen: true } 
                }
	  }

	  return toolTipCfg
	}


function ToolTip(text,width) {
	  toolTipCfg = {
			  content: text,
			  show: 'mouseover',
			  hide: 'mouseout',
			  position: {
			  	      corner: {
			  	         target: 'topRight',
			  	         tooltip: 'bottomLeft'
			  	      }
			  	   },
			  style: { 
			  	   	  width: width,
			  	      name: 'red', // Inherit from preset style
			  	      tip: 'bottomLeft'
			  	   }
		}
	  return toolTipCfg
	}