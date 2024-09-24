using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private readonly IFileProvider _appDataFileProvider;
        private readonly IFileInfo _wardsJsonFile;
        private readonly IFileInfo _districtsJsonFile;
        private readonly IFileInfo _provincesJsonFile;

        public List<RemapsProvinceModel> Provinces { get; private set; } = [];
        public List<RemapsDistrictModel> Districts { get; private set; } = [];
        public List<RemapsWardModel> Wards { get; private set; } = [];

        public WeatherForecastController(
            IWebHostEnvironment environment,
            ILogger<WeatherForecastController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            _appDataFileProvider = new PhysicalFileProvider(Path.Combine(environment.ContentRootPath, "App_Data"));

            _provincesJsonFile = _appDataFileProvider.GetFileInfo("provinces.json");

            var provincesTextFromJson = System.IO.File.ReadAllText(_provincesJsonFile.PhysicalPath ?? "");
            if (!string.IsNullOrEmpty(provincesTextFromJson))
                Provinces = JsonSerializer.Deserialize<List<RemapsProvinceModel>>(provincesTextFromJson) ?? [];

            _districtsJsonFile = _appDataFileProvider.GetFileInfo("districts.json");

            var districtsTextFromJson = System.IO.File.ReadAllText(_districtsJsonFile.PhysicalPath ?? "");
            if (!string.IsNullOrEmpty(districtsTextFromJson))
                Districts = JsonSerializer.Deserialize<List<RemapsDistrictModel>>(districtsTextFromJson) ?? [];

            _wardsJsonFile = _appDataFileProvider.GetFileInfo("wards.json");

            var wardsTextFromJson = System.IO.File.ReadAllText(_wardsJsonFile.PhysicalPath ?? "");
            if (!string.IsNullOrEmpty(wardsTextFromJson))
                Wards = JsonSerializer.Deserialize<List<RemapsWardModel>>(wardsTextFromJson) ?? [];
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            //var httpClient = _httpClientFactory.CreateClient("estatemanner");
            //var wards = new List<RemapsWardModel>();

            //foreach (var item in Districts)
            //{
            //    var res = await httpClient.GetFromJsonAsync<RemapsWardResModel>($"api/v1/cadastral/wards?district_code={item.Code}");
            //    wards.AddRange(res?.Data ?? []);
            //}

            //await System.IO.File.WriteAllTextAsync(_wardsJsonFile.PhysicalPath ?? "",
            //    JsonSerializer.Serialize(wards, _jsonSerializerOptions));

            var connString = "Server=localhost;Database=sagonap_preprod;User Id=sa;Password=sa#123;";
            using (var connection = new NpgsqlConnection(connString))
            {
                // provinces
                var provinces = await connection.QueryAsync<ProvinceModel>("SELECT * FROM public.province");

                foreach (var provinceRecord in Provinces)
                {
                    try
                    {
                        var reesoftMatchingRecord =
                            provinces.SingleOrDefault(_ => _.Slug.EndsWith(provinceRecord.Slug,
                                StringComparison.OrdinalIgnoreCase));

                        if (reesoftMatchingRecord is not null)
                        {
                            provinceRecord.ReesoftProvinceId = reesoftMatchingRecord.Id;

                            var associatedDistricts = Districts.Where(_ => _.ParentCode == provinceRecord.Code)
                                                               .ToList();
                            associatedDistricts.ForEach(_ => _.ReesoftProvinceId = reesoftMatchingRecord.Id);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }

                // districts
                var districts = await connection.QueryAsync<DistrictModel>("SELECT id, name, slug, province_id as ProvinceId FROM public.district");
                var groupedByProvince = Districts.GroupBy(_ => _.ReesoftProvinceId);

                foreach (var group in groupedByProvince)
                {
                    if (group.Key is null)
                    {
                        continue;
                    }

                    var reesoftDistricts = districts.Where(x => x.ProvinceId == group.Key!.Value).ToList();

                    foreach (var districtRecord in group)
                    {
                        try
                        {
                            DistrictModel? reesoftMatchingRecord = null;

                            var districtRecordSlug = districtRecord.Name.ToSlug()
                                .Replace("01", "1")
                                .Replace("03", "3")
                                .Replace("04", "4")
                                .Replace("05", "5")
                                .Replace("06", "6")
                                .Replace("07", "7")
                                .Replace("08", "8");
                            var reesoftMatchingRecords = reesoftDistricts.Where(_ => _.Name.ToSlug().EndsWith(districtRecordSlug,
                                    StringComparison.OrdinalIgnoreCase)).ToList();

                            if (reesoftMatchingRecords.Count > 1)
                            {
                                var nameWithTypeSlug = districtRecord.NameWithType.ToSlug();
                                reesoftMatchingRecord = reesoftMatchingRecords.SingleOrDefault(_ => _.Name.ToSlug().EndsWith(nameWithTypeSlug,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                            else
                            {
                                reesoftMatchingRecord = reesoftMatchingRecords.FirstOrDefault();
                            }

                            if (reesoftMatchingRecord is null)
                            {
                                continue;
                            }

                            districtRecord.ReesoftDistrictId = reesoftMatchingRecord.Id;

                            var associatedWards = Wards.Where(_ => _.ParentCode == districtRecord.Code)
                                                       .ToList();

                            associatedWards.ForEach(_ =>
                            {
                                _.ReesoftDistrictId = districtRecord.ReesoftDistrictId;
                                _.ReesoftProvinceId = districtRecord.ReesoftProvinceId;
                            });
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }

                // wards
                var wards = await connection.QueryAsync<WardModel>("SELECT id, name, slug, district_id as DistrictId FROM public.ward");
                var groupedByDistrict = Wards.GroupBy(_ => _.ReesoftDistrictId);

                foreach (var group in groupedByDistrict)
                {
                    if (group.Key is null)
                    {
                        continue;
                    }

                    var reesoftWards = wards.Where(_ => _.DistrictId == group.Key!.Value).ToList();

                    foreach (var wardRecord in group)
                    {
                        try
                        {
                            WardModel? matchingRecord = null;
                            var wardRecordSlug = wardRecord.Name.ToSlug();
                            var matchingRecords =
                                reesoftWards.Where(_ => _.Name.ToSlug().EndsWith(wardRecordSlug, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (matchingRecords.Count > 1)
                            {
                                var nameWithType = wardRecord.NameWithType;
                                matchingRecord = matchingRecords.SingleOrDefault(_ => _.Name.EndsWith(nameWithType,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                            else
                            {
                                matchingRecord = matchingRecords.FirstOrDefault();
                            }

                            if (matchingRecord is null)
                            {
                                continue;
                            }

                            wardRecord.ReesoftWardId = matchingRecord.Id;
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
            }

            // sync
            await Task.WhenAll(
                           System.IO.File.WriteAllTextAsync(_provincesJsonFile.PhysicalPath,
                                                  JsonSerializer.Serialize(Provinces, _jsonSerializerOptions)),
                           System.IO.File.WriteAllTextAsync(_districtsJsonFile.PhysicalPath,
                                                  JsonSerializer.Serialize(Districts, _jsonSerializerOptions)),
                           System.IO.File.WriteAllTextAsync(_wardsJsonFile.PhysicalPath,
                                                  JsonSerializer.Serialize(Wards, _jsonSerializerOptions))
                          );


            return Ok();
        }
    }

    public class ProvinceModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class DistrictModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
    }

    public class WardModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int DistrictId { get; set; }
    }
}