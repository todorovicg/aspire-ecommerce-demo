using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Ecommerce.Ordering.Api.Infrastructure;

// Driver-wide serialization rules, registered once before the first document is mapped
public static class MongoSerialization
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        // Stored as BSON dates so range filters on NextAttemptAt and LeaseUntil compare as instants
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
        ConventionRegistry.Register(
            "ecommerce",
            new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            },
            _ => true);
        _registered = true;
    }
}
