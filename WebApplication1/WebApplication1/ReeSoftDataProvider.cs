using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1
{
    public class ReeSoftDataProvider
    {
        private readonly IFileInfo _wardsJsonFile;
        private readonly IFileInfo _districtsJsonFile;
        private readonly IFileInfo _provincesJsonFile;

        public List<ReeSoftProvinceModel> Provinces { get; private set; } = [];
        public List<ReeSoftDistrictModel> Districts { get; private set; } = [];
        public List<ReeSoftWardModel> Wards { get; private set; } = [];

        public ReeSoftDataProvider(IWebHostEnvironment environment)
        {
            var appDataFileProvider = new PhysicalFileProvider(Path.Combine(environment.ContentRootPath, "App_Data", "ReeSoft"));

            _provincesJsonFile = appDataFileProvider.GetFileInfo("provinces.json");
            var provincesTextFromJson = File.ReadAllText(_provincesJsonFile.PhysicalPath ?? "[]");

            if (!string.IsNullOrEmpty(provincesTextFromJson))
                Provinces = JsonSerializer.Deserialize<List<ReeSoftProvinceModel>>(provincesTextFromJson) ?? [];

            _districtsJsonFile = appDataFileProvider.GetFileInfo("districts.json");
            var districtsTextFromJson = File.ReadAllText(_districtsJsonFile.PhysicalPath ?? "[]");

            if (!string.IsNullOrEmpty(districtsTextFromJson))
                Districts = JsonSerializer.Deserialize<List<ReeSoftDistrictModel>>(districtsTextFromJson) ?? [];

            _wardsJsonFile = appDataFileProvider.GetFileInfo("wards.json");
            var wardsTextFromJson = File.ReadAllText(_wardsJsonFile.PhysicalPath ?? "[]");

            if (!string.IsNullOrEmpty(wardsTextFromJson))
                Wards = JsonSerializer.Deserialize<List<ReeSoftWardModel>>(wardsTextFromJson) ?? [];
        }
    }

    public class ReeSoftProvinceModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class ReeSoftDistrictModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("provinceid")]
        public int ProvinceId { get; set; }
    }

    public class ReeSoftWardModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("districtid")]
        public int? DistrictId { get; set; }
    }
}