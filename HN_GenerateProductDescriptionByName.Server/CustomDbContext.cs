using Microsoft.EntityFrameworkCore;

namespace HN_GenerateProductDescriptionByName.Server
{
    public class CustomDbContext: DbContext
    {
        public CustomDbContext(DbContextOptions<CustomDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProductDetails> ProductDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductDetails>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
