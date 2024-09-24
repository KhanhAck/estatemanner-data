using System.Text.Json.Serialization;

namespace WebApplication1;

public class RemapsProvinceModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name_with_type")]
    public string NameWithType { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int? ReesoftProvinceId { get; set; }
}

public class RemapsProvinceResModel
{
    public List<RemapsProvinceModel> Data { get; set; } = [];
}