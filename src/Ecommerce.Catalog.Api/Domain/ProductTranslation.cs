namespace Ecommerce.Catalog.Api.Domain;

public sealed class ProductTranslation
{
    public const int LanguageMaxLength = 8;

    // EF Core materializes entities through this constructor
    private ProductTranslation()
    {
    }

    private ProductTranslation(string language, string name, string description, string category)
    {
        Language = language;
        Name = name;
        Description = description;
        Category = category;
    }

    public string Language { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public static ProductTranslation Create(string language, string name, string description, string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        return new ProductTranslation(language.Trim().ToLowerInvariant(), name.Trim(), description.Trim(), category.Trim());
    }
}
