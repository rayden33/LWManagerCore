using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWManagerCore.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<LeaseContract> LeaseContracts { get; set; } = null!;
        public DbSet<ArchiveLeaseContract> ArchiveLeaseContracts { get; set; }
        public DbSet<ReturnedLeaseContract> ReturnedLeaseContracts { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductReference> ProductReferences { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<ReturnedProduct> ReturnedProducts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Properties.Settings.Default.DatabaseFileName}");
        }
    }
}
