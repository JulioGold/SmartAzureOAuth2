using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Globalization;

namespace SmartAzureOAuth2
{
    internal class OAuth2Token
    {
        public readonly string AccessToken;
        public readonly DateTime ExpiresOn;

        public OAuth2Token(string accessToken, DateTime expiresOn)
        {
            AccessToken = accessToken;
            ExpiresOn = expiresOn;
        }
    }

    public class AzureOAuth2Token
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _appKey;
        private Lazy<OAuth2Token> _oAuth2Token;

        public AzureOAuth2Token(string tenantId, string clientId, string appKey)
        {
            _tenantId = tenantId;
            _clientId = clientId;
            _appKey = appKey;
        }

        public string AcquireToken()
        {
            // TODO: Check what is correct to use, DateTime.Now or DateTime.UtcNow
            if (_oAuth2Token == null || _oAuth2Token.Value.ExpiresOn <= DateTime.Now)
            {
                AuthenticationContext authenticationContext = new AuthenticationContext(String.Format(CultureInfo.InvariantCulture, "https://login.windows.net/{0}", _tenantId));

                ClientCredential clientCredential = new ClientCredential(_clientId, _appKey);

                AuthenticationResult authenticationResult = null;

                try
                {
                    // In case of change, we had a other resource: https://management.azure.com/
                    authenticationResult = authenticationContext.AcquireTokenAsync("https://management.core.windows.net/", clientCredential).GetAwaiter().GetResult();
                }
                catch (AdalException)
                {
                    //May be retry
                }

                _oAuth2Token = new Lazy<OAuth2Token>(() => {
                    return new OAuth2Token(authenticationResult.AccessToken, authenticationResult.ExpiresOn.DateTime);
                });
            }

            return _oAuth2Token.Value.AccessToken;
        }
    }
}
