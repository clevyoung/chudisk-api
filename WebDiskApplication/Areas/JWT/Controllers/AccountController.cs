using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebDiskApplication.EFDB;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;



namespace JWTAuthentication.Areas.Jwt.Controllers
{
    public class AccountController : ApiController
    {
        /// <summary>
        /// 이메일, 패스워드 기반으로 로그인하기 및 JWT토큰 생성하기
        /// </summary>
        /// <param name="users">Users 모델</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/account/login")]
        [HttpPost]
        public IHttpActionResult Authenticate([FromBody] Users users)
        {
            using (var db = new WebDiskDBEntities())
            {
                Users user = db.Users.SingleOrDefault(x => x.Email == users.Email && x.Password == users.Password);
                if (user != null)
                {
                    string userRole = Enum.GetName(typeof(WebDiskApplication.Manage.Enums.UserRole), user.UserRole);
                    string token = createToken(users.Email, userRole);
                    return Ok<string>(token);
                }
                else
                {
                    return Unauthorized();
                }
            }
        }

        /// <summary>
        /// 토큰 생성하기 메소드
        /// </summary>
        /// <param name="userEmail">유저 이메일</param>
        /// <param name="userrole">유저 역할</param>
        /// <returns></returns>
        private string createToken(string userEmail, string userRole)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userEmail),
                new Claim(ClaimTypes.Role, userRole)
            };

            const string sec = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(sec));
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature);

            var securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Audience = "https://localhost:44393",
                Issuer = "https://localhost:44393",
                IssuedAt = DateTime.UtcNow,
                Subject = new ClaimsIdentity(claims, "Bearer"),
                SigningCredentials = signingCredentials,
                NotBefore = DateTime.UtcNow.AddMinutes(-1),
                Expires = DateTime.UtcNow.AddHours(12)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(securityTokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
