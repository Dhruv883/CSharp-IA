using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Define the relationship between Transaction and User
            modelBuilder.Entity<Transaction>()
                .HasOne<User>() // Specify the relationship type
                .WithMany(u => u.Transactions) // Link back to the User's Transactions
                .HasForeignKey(t => t.UserId) // Specify the foreign key
                .OnDelete(DeleteBehavior.Cascade); // Optionally set delete behavior
        }
    }
}
