using GoodsAssignment.Models;
using Microsoft.EntityFrameworkCore;

namespace GoodsAssignment.Data
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BPType> BPTypes { get; set; }
        public DbSet<BusinessPartner> BusinessPartners { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<SaleOrder> SaleOrders { get; set; }
        public DbSet<SaleOrderLine> SaleOrderLines { get; set; }
        public DbSet<SaleOrderLineComment> SaleOrderLineComments { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users table
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().HasData(
                new User { ID = 1, UserName = "U1", FullName = "User 1", Password = "P1", Active = true },
                new User { ID = 2, UserName = "U2", FullName = "User 2", Password = "P2", Active = false }
            );

            // BPType table
            modelBuilder.Entity<BPType>().HasData(
                new BPType { TypeCode = "C", TypeName = "Customer" },
                new BPType { TypeCode = "V", TypeName = "Vendor" }
            );

            // BusinessPartners table
            modelBuilder.Entity<BusinessPartner>().HasOne<BPType>()
                .WithMany()
                .HasForeignKey(bp => bp.BPType)
                .HasPrincipalKey(bt => bt.TypeCode);
            modelBuilder.Entity<BusinessPartner>().HasData(
                new BusinessPartner { BPCode = "C0001", BPName = "Customer 1", BPType = "C", Active = true },
                new BusinessPartner { BPCode = "C0002", BPName = "Customer 2", BPType = "C", Active = false },
                new BusinessPartner { BPCode = "V0001", BPName = "Vendor 1", BPType = "V", Active = true },
                new BusinessPartner { BPCode = "V0002", BPName = "Vendor 2", BPType = "V", Active = false }
            );

            // Items table
            modelBuilder.Entity<Item>().HasData(
                new Item { ItemCode = "Itm1", ItemName = "Item 1", Active = true },
                new Item { ItemCode = "Itm2", ItemName = "Item 2", Active = true },
                new Item { ItemCode = "Itm3", ItemName = "Item 3", Active = false }
            );

            // SaleOrders table
            modelBuilder.Entity<SaleOrder>().HasOne<BusinessPartner>()
                .WithMany()
                .HasForeignKey(so => so.BPCode)
                .HasPrincipalKey(bp => bp.BPCode);
            modelBuilder.Entity<SaleOrder>().HasOne<User>()
               .WithMany()
               .HasForeignKey(so => so.CreatedBy)
               .HasPrincipalKey(u => u.ID);
            modelBuilder.Entity<SaleOrder>().HasOne<User>()
               .WithMany()
               .HasForeignKey(so => so.LastUpdatedBy)
               .HasPrincipalKey(u => u.ID);

            // SaleOrderLines table
            modelBuilder.Entity<SaleOrderLine>().Property(sol => sol.Quantity).HasPrecision(38,18);
            modelBuilder.Entity<SaleOrderLine>().HasOne<SaleOrder>()
                .WithMany()
                .HasForeignKey(sol => sol.DocID)
                .HasPrincipalKey(so => so.ID);
            modelBuilder.Entity<SaleOrderLine>().HasOne<Item>()
                .WithMany()
                .HasForeignKey(sol => sol.ItemCode)
                .HasPrincipalKey(i => i.ItemCode);

            // SaleOrderLineComments table
            modelBuilder.Entity<SaleOrderLineComment>().HasOne<SaleOrder>()
                .WithMany()
                .HasForeignKey(solc => solc.DocID)
                .HasPrincipalKey(so => so.ID).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SaleOrderLineComment>().HasOne<SaleOrderLine>()
                .WithMany()
                .HasForeignKey(solc => solc.LineID)
                .HasPrincipalKey(sol => sol.LineID);

            // PurchaseOrders table
            modelBuilder.Entity<PurchaseOrder>().HasOne<BusinessPartner>()
                .WithMany()
                .HasForeignKey(po => po.BPCode)
                .HasPrincipalKey(bp => bp.BPCode);
            modelBuilder.Entity<PurchaseOrder>().HasOne<User>()
               .WithMany()
               .HasForeignKey(po => po.CreatedBy)
               .HasPrincipalKey(u => u.ID);
            modelBuilder.Entity<PurchaseOrder>().HasOne<User>()
               .WithMany()
               .HasForeignKey(po => po.LastUpdatedBy)
               .HasPrincipalKey(u => u.ID);

            // PurchaseOrderLines table
            modelBuilder.Entity<PurchaseOrderLine>().Property(pol => pol.Quantity).HasPrecision(38,18);
            modelBuilder.Entity<PurchaseOrderLine>().HasOne<PurchaseOrder>()
                .WithMany()
                .HasForeignKey(pol => pol.DocID)
                .HasPrincipalKey(po => po.ID);
            modelBuilder.Entity<PurchaseOrderLine>().HasOne<Item>()
                .WithMany()
                .HasForeignKey(pol => pol.ItemCode)
                .HasPrincipalKey(i => i.ItemCode);
        }

    }
}
