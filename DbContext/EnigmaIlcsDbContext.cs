using Microsoft.EntityFrameworkCore;
using project_ilcs.Models;

namespace project_ilcs
{
    public class EnigmaIlcsDbContext : DbContext
    {
        public EnigmaIlcsDbContext(DbContextOptions<EnigmaIlcsDbContext> options) : base(options)
        {

        }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Products> Products { get; set; }
    }
}
