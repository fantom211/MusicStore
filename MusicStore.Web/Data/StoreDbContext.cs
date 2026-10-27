using Microsoft.EntityFrameworkCore;
using MusicStore.Web.Models;

namespace MusicStore.Web.Data;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<DailySpin> DailySpins => Set<DailySpin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Category).HasConversion<string>().HasMaxLength(32);
            entity.Property(p => p.Price).HasPrecision(12, 2);
            entity.Property(p => p.ImageUrl).HasMaxLength(500);
            entity.HasIndex(p => p.Category);
            entity.HasIndex(p => p.IsActive);
            entity.HasData(ProductSeeder.GetDemoProducts());
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.Property(c => c.UserId).HasMaxLength(64).IsRequired();
            entity.HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // Один и тот же товар не может лежать в корзине пользователя дважды.
            entity.HasIndex(c => new { c.UserId, c.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.UserId).HasMaxLength(64).IsRequired();
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(o => o.ItemsTotal).HasPrecision(12, 2);
            entity.Property(o => o.TotalAmount).HasPrecision(12, 2);
            entity.OwnsOne(o => o.Address, address =>
            {
                address.Property(a => a.City).HasMaxLength(100).IsRequired();
                address.Property(a => a.Street).HasMaxLength(100).IsRequired();
                address.Property(a => a.House).HasMaxLength(20).IsRequired();
                address.Property(a => a.Apartment).HasMaxLength(20);
                address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                address.Property(a => a.RecipientName).HasMaxLength(100);
                address.Property(a => a.Phone).HasMaxLength(50);
            });
            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.CreatedAt);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(12, 2);
            entity.Property(i => i.Subtotal).HasPrecision(12, 2);
            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(i => i.OrderId);
        });

        modelBuilder.Entity<DailySpin>(entity =>
        {
            entity.Property(s => s.UserId).HasMaxLength(64).IsRequired();
            entity.Property(s => s.SymbolsJson).HasMaxLength(100).IsRequired();
            entity.Property(s => s.SpinDate).HasColumnType("date");
            // Гарантия «одна прокрутка в сутки на пользователя» на уровне базы:
            // параллельные запросы не смогут вставить вторую строку за тот же день.
            entity.HasIndex(s => new { s.UserId, s.SpinDate }).IsUnique();
            entity.HasOne(s => s.UsedInOrder)
                .WithMany()
                .HasForeignKey(s => s.UsedInOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

/// <summary>Демонстрационные товары, чтобы каталог работал сразу после настройки базы.</summary>
public static class ProductSeeder
{
    public static List<Product> GetDemoProducts() => new()
    {
        new Product
        {
            Id = 1, Name = "Fender Player Stratocaster", Category = ProductCategory.Guitars,
            Description = "Электрогитара, клён, 3 сингла. Классический звук для рока и блюза.",
            Price = 85000m, DiscountPercent = 10, StockQuantity = 5,
            ImageUrl = "/images/products/product-strat.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 2, Name = "Yamaha Pacifica 112V", Category = ProductCategory.Guitars,
            Description = "Электрогитара для начинающих и продолжающих, хамбакер + 2 сингла.",
            Price = 42000m, DiscountPercent = 0, StockQuantity = 8,
            ImageUrl = "/images/products/product-pacifica.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 3, Name = "Ernie Ball Regular Slinky (10-46)", Category = ProductCategory.Strings,
            Description = "Струны для электрогитары, никель, комплект.",
            Price = 850m, DiscountPercent = 0, StockQuantity = 40,
            ImageUrl = "/images/products/product-strings-eb.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 4, Name = "D'Addario EJ16 Phosphor Bronze (12-53)", Category = ProductCategory.Strings,
            Description = "Струны для акустической гитары, фосфорная бронза.",
            Price = 1100m, DiscountPercent = 15, StockQuantity = 25,
            ImageUrl = "/images/products/product-strings-da.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 5, Name = "Yamaha P-125a", Category = ProductCategory.Pianos,
            Description = "Цифровое пианино, 88 взвешенных клавиш, звук рояля CFIIIS.",
            Price = 98000m, DiscountPercent = 5, StockQuantity = 4,
            ImageUrl = "/images/products/product-piano-yamaha.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 6, Name = "Casio CTK-3500", Category = ProductCategory.Pianos,
            Description = "Синтезатор для обучения, 61 клавиша, режим пошагового обучения.",
            Price = 32000m, DiscountPercent = 0, StockQuantity = 10,
            ImageUrl = "/images/products/product-piano-casio.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 7, Name = "Pearl Roadshow RS525SC", Category = ProductCategory.Drums,
            Description = "Ударная установка 5 барабанов с тарелками и стойками.",
            Price = 65000m, DiscountPercent = 0, StockQuantity = 3,
            ImageUrl = "/images/products/product-drums-pearl.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 8, Name = "Vic Firth American Classic 5A", Category = ProductCategory.Drums,
            Description = "Барабанные палочки, гикори, самая популярная модель.",
            Price = 1500m, DiscountPercent = 0, StockQuantity = 50,
            ImageUrl = "/images/products/product-sticks.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 17, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 9, Name = "Pink Floyd — The Dark Side of the Moon (LP)", Category = ProductCategory.Vinyl,
            Description = "Классический виниловый альбом, 180 грамм.",
            Price = 3500m, DiscountPercent = 20, StockQuantity = 15,
            ImageUrl = "/images/products/product-vinyl-pf.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 18, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 10, Name = "Daft Punk — Discovery (LP)", Category = ProductCategory.Vinyl,
            Description = "Электронный классик на виниле, двойной альбом.",
            Price = 2800m, DiscountPercent = 0, StockQuantity = 12,
            ImageUrl = "/images/products/product-vinyl-dp.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 19, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 11, Name = "Marshall MG15G", Category = ProductCategory.Amplifiers,
            Description = "Комбоусилитель для электрогитары, 15 Вт, реверб.",
            Price = 28000m, DiscountPercent = 10, StockQuantity = 6,
            ImageUrl = "/images/products/product-amp-marshall.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
        },
        new Product
        {
            Id = 12, Name = "Fender Champion 40", Category = ProductCategory.Amplifiers,
            Description = "Гитарный комбоусилитель, 40 Вт, 12 эффектов.",
            Price = 32000m, DiscountPercent = 0, StockQuantity = 7,
            ImageUrl = "/images/products/product-amp-fender.svg", IsActive = true,
            CreatedAt = new DateTime(2026, 1, 21, 0, 0, 0, DateTimeKind.Utc),
        },
    };
}
