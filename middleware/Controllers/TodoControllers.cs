using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware.Models;

namespace Middleware.Controllers {
    [Route ("/todo")]
    public class TodoControllers : ControllerBase {
        public AppDBContext AppDBContext { get; set; }
        public TodoControllers (AppDBContext appDBContext) {
            AppDBContext = appDBContext;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get () {
            var token = System.IO.File.ReadAllLines ("Token.txt").Last ();
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler ();
            var securityToken = jwtSecurityTokenHandler.ReadToken (token) as JwtSecurityToken;
            var sub = securityToken.Claims.First (u => u.Type == "sub").Value;
            var x = from a in AppDBContext.Post where a.UserId == Convert.ToInt32 (sub) select a;
            return Ok (x);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Post ([FromBody] Posts posts) {
            var token = System.IO.File.ReadAllLines ("Token.txt").Last ();
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler ();
            var securityToken = jwtSecurityTokenHandler.ReadToken (token) as JwtSecurityToken;
            AppDBContext.Add (posts);
            AppDBContext.SaveChanges ();
            return Ok (AppDBContext.Post.Include (u => u.User).ToList ());
        }

        [Authorize]
        [HttpPatch ("{id}")]
        public IActionResult Update (int id, [FromBody] Posts todoRequest) {
            var token = System.IO.File.ReadAllLines ("Token.txt").Last ();
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler ();
            var securityToken = jwtSecurityTokenHandler.ReadToken (token) as JwtSecurityToken;
            var todo = AppDBContext.Post.Find (id);
            todo.list = todoRequest.list;
            AppDBContext.SaveChanges ();
            return Ok (AppDBContext.Post.Include ("User").ToList ());
        }

        [Authorize]
        [HttpPatch]
        [Route ("done/{id}")]
        public IActionResult Done (int id) {
            var token = System.IO.File.ReadAllLines ("Token.txt").Last ();
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler ();
            var securityToken = jwtSecurityTokenHandler.ReadToken (token) as JwtSecurityToken;
            var todo = AppDBContext.Post.Find (id);
            todo.status = true;
            AppDBContext.SaveChanges ();
            return Ok (AppDBContext.Post.Include ("User").ToList ());
        }

        [Authorize]
        [HttpDelete ("delete/{id}")]
        public ActionResult<string> Delete (int id) {
            var token = System.IO.File.ReadAllLines ("Token.txt").Last ();
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler ();
            var securityToken = jwtSecurityTokenHandler.ReadToken (token) as JwtSecurityToken;
            var activity = AppDBContext.Post.Find (id);
            AppDBContext.Attach (activity);
            AppDBContext.Remove (activity);
            AppDBContext.SaveChanges ();
            return Ok ($"Activity that has id: {id} was deleted");
        }

        [Authorize]
        [HttpDelete ("clear")]
        public ActionResult<string> Clear (int id) {
            var token = System.IO.File.ReadAllLines ("Token.txt").Last ();
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler ();
            var securityToken = jwtSecurityTokenHandler.ReadToken (token) as JwtSecurityToken;
            var sub = securityToken.Claims.First (a => a.Type == "sub").Value;
            var a = from item in AppDBContext.Post where item.UserId == Convert.ToInt32 (sub) select item;
            AppDBContext.Post.RemoveRange (a);
            AppDBContext.SaveChanges ();
            return Ok ("all data sucesfully deleted");
        }
    }
}