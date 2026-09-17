namespace Ecommerce.Catalog.Api.Domain;

public sealed class Product
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int CategoryMaxLength = 100;
    public const int ImageKeyMaxLength = 50;

    private readonly List<ProductTranslation> _translations = [];

    // EF Core materializes entities through this constructor
    private Product()
    {
    }

    private Product(Guid id, string name, string description, string category, decimal price, int availableStock, string imageKey)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        AvailableStock = availableStock;
        ImageKey = imageKey;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public int AvailableStock { get; private set; }

    public string ImageKey { get; private set; } = string.Empty;

    public IReadOnlyList<ProductTranslation> Translations => _translations;

    public static Product Create(Guid id, string name, string description, string category, decimal price, int availableStock, string imageKey)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Product id must not be empty", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(imageKey);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        ArgumentOutOfRangeException.ThrowIfNegative(availableStock);

        return new Product(id, name.Trim(), description.Trim(), category.Trim(), price, availableStock, imageKey.Trim());
    }

    // Adds or replaces the translation for a language; the product's own text is English and the fallback
    public Product WithTranslation(string language, string name, string description, string category)
    {
        var translation = ProductTranslation.Create(language, name, description, category);
        _translations.RemoveAll(existing => existing.Language == translation.Language);
        _translations.Add(translation);
        return this;
    }

    public ProductText Localize(string language)
    {
        var translation = _translations.Find(existing => existing.Language == language);
        return translation is null
            ? new ProductText(Name, Description, Category)
            : new ProductText(translation.Name, translation.Description, translation.Category);
    }
}
