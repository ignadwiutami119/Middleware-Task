using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Middleware.Models;

namespace Middleware.Controllers {
    [Route ("/user")]
    [ApiController]
    public class UserControllers : ControllerBase {

        public AppDBContext _appDbContext;
        private IConfiguration Configuration;
        public UserControllers (AppDBContext appDBContext, IConfiguration configuration) {
            _appDbContext = appDBContext;
            Configuration = configuration;
        }

        [Authorize]
        [HttpGet ("welcome")]
        public IActionResult welcome () {
            var token = System.IO.File.ReadAllText ("Token.txt");
            var JwtSecurityTokenHandler = new JwtSecurityTokenHandler ();
            var securityToken = JwtSecurityTokenHandler.ReadToken (token) as JwtSecurityToken;
            var email = securityToken?.Claims.First (Claim => Claim.Type == "email").Value;
            var sub = securityToken?.Claims.First (Claim => Claim.Type == "sub").Value;
            var jti = securityToken?.Claims.First (Claim => Claim.Type == "jti").Value;

            if (securityToken == null) {
                return Unauthorized ();
            }

            return Ok (new {
                message = "Welcome"
            });
        }

        [HttpPost ("register")]
        public IActionResult Post ([FromBody] User user) {
            var db = from x in _appDbContext.Users select x.username;
            foreach (var data in db) {
                if (user.username == data) {
                    return Ok ("account already exist");
                } else {
                    _appDbContext.Users.Add (user);
                    break;
                }
            }
            _appDbContext.SaveChanges ();
            return Ok ("Succesfully Register");
        }

        [HttpPost ("login")]
        public IActionResult Login ([FromBody] User input) {
            IActionResult response = Unauthorized ();
            var user = AuthenticatedUser (input);
            if (user != null) {
                var token = GenerateJwtToken (user);
                TextWriter tkn = new StreamWriter ("Token.txt", true);
                tkn.WriteLine (token);
                tkn.Close ();
                Console.WriteLine("Succesfully login, your token is : \n");
                return Ok (new { token = token });
            }
            return Ok ();
        }
        private User AuthenticatedUser (User input) {
            User user = null;
            var db = from x in _appDbContext.Users select new { id = x.id, uname = x.username, pass = x.password };
            foreach (var data in db) {
                if (input.username == data.uname && input.password == data.pass) {
                    user = new User {
                    id = data.id,
                    username = input.username,
                    password = input.password
                    };
                    return user;
                }
            }
            return user;
        }

        private object GenerateJwtToken (User user) {
            var securityKey = new SymmetricSecurityKey (System.Text.Encoding.UTF8.GetBytes (Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials (securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new [] {
                new Claim (JwtRegisteredClaimNames.Sub, Convert.ToString (user.id)),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid ().ToString ()),
            };
            var token = new JwtSecurityToken (
                issuer: Configuration["Jwt:Issuer"],
                audience : Configuration["Jwt:Audience"],
                claims,
                expires : DateTime.Now.AddMinutes (120),
                signingCredentials : credentials
            );

            var encodedToken = new JwtSecurityTokenHandler ().WriteToken (token);
            return encodedToken;
        }
    }
}