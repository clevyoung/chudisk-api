using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web;


namespace WebDiskApplication.Areas.JWT.AuthFilter
{
    public class JwtAuthenticationFilterAttribute : Attribute, IAuthenticationFilter
    {
        
        public const string SUPPORTED_TOKEN_SCHEME = "Bearer";

        private readonly string _audience = "https://localhost:44393";

        private readonly string _validIssuer = "https://localhost:44393";

        public bool AllowMultiple { get { return false; } }

        public bool SendChallenge { get; set; }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
          
            var authHeader = context.Request.Headers.Authorization;


            if(authHeader == null)
            {
                return;
            }

            var tokenType = authHeader.Scheme;
            if(!tokenType.Equals(SUPPORTED_TOKEN_SCHEME))
            {
                return;
            }

            var credentials = authHeader.Parameter;

            if(string.IsNullOrEmpty(credentials))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", context.Request);
                return;
            }

            try
            {
                IPrincipal principal = await ValidateCredentialsAsync(credentials, context.Request, cancellationToken);

                if(principal == null)
                {
                    context.ErrorResult = new AuthenticationFailureResult("Invaild security token", context.Request);
                }
                else
                {
                    context.Principal = principal;
                }

            }
            catch(Exception stex) when(stex is SecurityTokenValidationException || stex is SecurityTokenExpiredException || stex is SecurityTokenNoExpirationException || stex is SecurityTokenNotYetValidException)
            {
                context.ErrorResult = new AuthenticationFailureResult("Security token lifetime error", context.Request);
            }
            catch(Exception)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid security token", context.Request);
            }

        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            if(SendChallenge)
            {
                context.Result = new AddChallengeOnUnauthorizedResult(
                                    new AuthenticationHeaderValue(SUPPORTED_TOKEN_SCHEME),
                                    context.Result);
            }
            return Task.FromResult(0);
        }

        public async Task<IPrincipal> ValidateCredentialsAsync(string credentials, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            var isValidJwt = jwtHandler.CanReadToken(credentials);

            if(!isValidJwt)
            {
                return null;
            }
            const string sec = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(sec));

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudiences = new[] { _audience },

                ValidateIssuer = true,
                ValidIssuers = new[] { _validIssuer },

                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = new[] { securityKey },

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(5),

                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role,
                AuthenticationType = SUPPORTED_TOKEN_SCHEME
            };

            SecurityToken validatedToken = new JwtSecurityToken();
            ClaimsPrincipal principal = jwtHandler.ValidateToken(credentials, validationParameters, out validatedToken);


            return await Task.FromResult(principal);

        }

      
    }

    
}