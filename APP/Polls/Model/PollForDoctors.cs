using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

[BsonIgnoreExtraElements]
public class PollForDoctors
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _Id { get; set; }

    [BsonElement("firstName")]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [BsonElement("lastName")]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [BsonElement("role")]
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [BsonElement("specialization")]
    [JsonPropertyName("specialization")]
    public string Specialization { get; set; }

    [BsonElement("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [BsonElement("score")]
    [JsonPropertyName("score")]
    public List<Dictionary<string, string>> Score { get; set; }
}