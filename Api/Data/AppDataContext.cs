using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class AppDataContext : DbContext
{
    public AppDataContext(DbContextOptions options) : base(options)
    {
        
    }


    public DbSet<AppUser> Users {get; set;}

}
