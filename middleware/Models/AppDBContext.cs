using Microsoft.EntityFrameworkCore;

namespace Middleware.Models
{
    public class AppDBContext : DbContext
    {
        public DbSet<Posts> Post {get;set;}
        public DbSet<User> Users {get;set;}
        public AppDBContext (DbContextOptions options) : base (options)
        {

        }
    }
}