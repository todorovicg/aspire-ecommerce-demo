using Ecommerce.Catalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Catalog.Api.Infrastructure;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength).IsRequired();
        builder.Property(p => p.Category).HasMaxLength(Product.CategoryMaxLength).IsRequired();
        builder.Property(p => p.Price).HasPrecision(10, 2);
        builder.Property(p => p.AvailableStock).IsRequired();
        builder.Property(p => p.ImageKey).HasMaxLength(Product.ImageKeyMaxLength).IsRequired();

        builder.HasIndex(p => p.Name);

        builder.OwnsMany(p => p.Translations, translations =>
        {
            translations.ToTable("product_translations");
            translations.WithOwner().HasForeignKey("ProductId");
            translations.HasKey("ProductId", nameof(ProductTranslation.Language));
            translations.Property(t => t.Language).HasMaxLength(ProductTranslation.LanguageMaxLength).IsRequired();
            translations.Property(t => t.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
            translations.Property(t => t.Description).HasMaxLength(Product.DescriptionMaxLength).IsRequired();
            translations.Property(t => t.Category).HasMaxLength(Product.CategoryMaxLength).IsRequired();
        });
        builder.Navigation(p => p.Translations).HasField("_translations").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
