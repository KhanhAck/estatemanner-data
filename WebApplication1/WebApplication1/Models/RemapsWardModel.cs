using System.Text.Json.Serialization;

namespace WebApplication1;

public class RemapsWardModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name_with_type")]
    public string NameWithType { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("path_with_type")]
    public string PathWithType { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("parent_code")]
    public string ParentCode { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public int[] ReesoftWardIds { get; set; } = [];
    public int? ReesoftDistrictId { get; set; }
    public int? ReesoftProvinceId { get; set; }
}

public class RemapsWardResModel
{
    public List<RemapsWardModel> Data { get; set; } = [];
}