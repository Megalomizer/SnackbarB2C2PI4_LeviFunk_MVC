using Microsoft.EntityFrameworkCore;
using SnackbarB2C2PI4_LeviFunk_ClassLibrary;

namespace SnackbarB2C2PI4_LeviFunk_MVC.Data
{
    public class LibraryDbContext : DbContext
    {
        #region DbSets

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
        public virtual DbSet<Owner> Owners { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }

        #endregion

        // Constructor
        public LibraryDbContext(DbContextOptions<LibraryDbContext> contextOptions) 
            : base(contextOptions)
        {
        }

        // OnConfiguring
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        // OnModelCreate
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Setting OrderProduct as a joined table for Order and Product
            modelBuilder.Entity<Order>()
                .HasMany(e => e.Products)
                .WithMany(e => e.Orders)
                .UsingEntity<OrderProduct>();

            // Setting the currency properties
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(p => p.Cost)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Transaction>()
                .Property(p => p.Cost)
                .HasPrecision(10, 2);

            // Setting Transactions OrderId to OnDelete: do nothing
            modelBuilder.Entity<Transaction>()
                .HasOne(p => p.Order)
                .WithOne(p => p.Transaction)
                .HasForeignKey<Order>(p => p.Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Inheritence
            base.OnModelCreating(modelBuilder);
        }
    }
}
