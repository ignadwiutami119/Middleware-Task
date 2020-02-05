using System.Collections.Generic;

namespace Middleware.Models
{
    public class User
    {
        public int id {get;set;}
        public string username {get;set;}
        public string password {get;set;}

        public ICollection<Posts> Posts {get;set;}
    }
}