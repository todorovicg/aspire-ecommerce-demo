using Ecommerce.Catalog.Api.Domain;

namespace Ecommerce.Catalog.Api.Infrastructure;

public static class CatalogSeedData
{
    public const string Serbian = "sr";

    public static IReadOnlyList<Product> CreateProducts() =>
    [
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire Ceramic Mug", "Glossy 350 ml ceramic mug with the Aspire logo", "Drinkware", 12.50m, 25, "mug")
            .WithTranslation(Serbian, "Aspire keramička šolja", "Sjajna keramička šolja od 350 ml sa Aspire logom", "Posuđe za piće"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000002"), "Aspire Travel Tumbler", "Insulated 450 ml tumbler that keeps coffee hot through a long deploy", "Drinkware", 24.00m, 15, "tumbler")
            .WithTranslation(Serbian, "Aspire putna čaša", "Termo čaša od 450 ml koja drži kafu toplom tokom dugog deploja", "Posuđe za piće"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000003"), "Aspire Water Bottle", "750 ml stainless steel bottle with a leak-proof cap", "Drinkware", 18.90m, 20, "bottle")
            .WithTranslation(Serbian, "Aspire flaša za vodu", "Flaša od nerđajućeg čelika od 750 ml sa čepom koji ne curi", "Posuđe za piće"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000004"), "Aspire Logo T-Shirt", "Soft cotton t-shirt with a small chest logo", "Apparel", 22.00m, 30, "tshirt")
            .WithTranslation(Serbian, "Aspire majica sa logom", "Mekana pamučna majica sa malim logom na grudima", "Odeća"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000005"), "Aspire Zip Hoodie", "Heavyweight zip hoodie with an embroidered logo", "Apparel", 49.00m, 12, "hoodie")
            .WithTranslation(Serbian, "Aspire duks sa rajsferšlusom", "Teški duks sa rajsferšlusom i izvezenim logom", "Odeća"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000006"), "Aspire Limited Edition Hoodie", "Numbered launch edition hoodie, sold out", "Apparel", 59.00m, 0, "hoodie-limited")
            .WithTranslation(Serbian, "Aspire duks limitirano izdanje", "Numerisano izdanje duksa sa lansiranja, rasprodato", "Odeća"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000007"), "Aspire Dad Cap", "Unstructured cotton cap with an adjustable strap", "Apparel", 19.50m, 18, "cap")
            .WithTranslation(Serbian, "Aspire kačket", "Pamučni kačket bez strukture sa podesivim kaišem", "Odeća"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000008"), "Aspire Sticker Pack", "Ten die-cut vinyl stickers for laptops and notebooks", "Accessories", 6.00m, 100, "stickers")
            .WithTranslation(Serbian, "Aspire paket nalepnica", "Deset vinil nalepnica za laptopove i sveske", "Dodaci"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000009"), "Aspire Laptop Sleeve", "Padded sleeve for 13 to 14 inch laptops", "Accessories", 34.00m, 10, "sleeve")
            .WithTranslation(Serbian, "Aspire futrola za laptop", "Podstavljena futrola za laptopove od 13 do 14 inča", "Dodaci"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000010"), "Aspire Launch Poster", "A2 poster from the launch event, only two left", "Accessories", 15.00m, 2, "poster")
            .WithTranslation(Serbian, "Aspire poster sa lansiranja", "A2 poster sa događaja lansiranja, ostala su samo dva", "Dodaci"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000011"), "Cloud-Native .NET Handbook", "Practical guide to building distributed .NET systems", "Books", 39.00m, 8, "book")
            .WithTranslation(Serbian, "Priručnik za cloud-native .NET", "Praktičan vodič za izgradnju distribuiranih .NET sistema", "Knjige"),
        Product.Create(Guid.Parse("a0000000-0000-4000-8000-000000000012"), "Distributed Tracing Field Guide", "Short guide to reading traces, spans and metrics", "Books", 29.00m, 6, "book-tracing")
            .WithTranslation(Serbian, "Terenski vodič za distribuirano praćenje", "Kratak vodič za čitanje trejsova, spanova i metrika", "Knjige"),
    ];
}
