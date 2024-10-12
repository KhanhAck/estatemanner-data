using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
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

        private readonly ReeSoftDataProvider _reeSoftDataProvider;

        public List<RemapsProvinceModel> Provinces { get; private set; } = [];
        public List<RemapsDistrictModel> Districts { get; private set; } = [];
        public List<RemapsWardModel> Wards { get; private set; } = [];

        public WeatherForecastController(
            IWebHostEnvironment environment,
            ILogger<WeatherForecastController> logger,
            IHttpClientFactory httpClientFactory,
            ReeSoftDataProvider reeSoftDataProvider)
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

            _reeSoftDataProvider = reeSoftDataProvider;
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

            //// provinces
            //var provinces = _reeSoftDataProvider.Provinces;

            //foreach (var provinceRecord in Provinces)
            //{
            //    try
            //    {
            //        var reesoftMatchingRecord =
            //            provinces.SingleOrDefault(_ => _.Name.EndsWith(provinceRecord.Name,
            //                StringComparison.OrdinalIgnoreCase));

            //        if (reesoftMatchingRecord is not null)
            //        {
            //            provinceRecord.ReesoftProvinceId = reesoftMatchingRecord.Id;

            //            var associatedDistricts = Districts.Where(_ => _.ParentCode == provinceRecord.Code)
            //                                               .ToList();
            //            associatedDistricts.ForEach(_ => _.ReesoftProvinceId = reesoftMatchingRecord.Id);
            //        }
            //        else
            //        {
            //            continue;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        continue;
            //    }
            //}

            //// districts
            //var districts = _reeSoftDataProvider.Districts;
            //var groupedByProvince = Districts.GroupBy(_ => _.ReesoftProvinceId);

            //foreach (var group in groupedByProvince)
            //{
            //    if (group.Key is null)
            //    {
            //        continue;
            //    }

            //    var reesoftDistricts = districts.Where(x => x.ProvinceId == group.Key!.Value).ToList();

            //    foreach (var districtRecord in group)
            //    {
            //        try
            //        {
            //            ReeSoftDistrictModel? reesoftMatchingRecord = null;

            //            var districtRecordSlug = districtRecord.Name.ToSlug()
            //                .Replace("01", "1")
            //                .Replace("03", "3")
            //                .Replace("04", "4")
            //                .Replace("05", "5")
            //                .Replace("06", "6")
            //                .Replace("07", "7")
            //                .Replace("08", "8");
            //            var reesoftMatchingRecords = reesoftDistricts.Where(_ => _.Name.ToSlug().EndsWith(districtRecordSlug,
            //                    StringComparison.OrdinalIgnoreCase)).ToList();

            //            if (reesoftMatchingRecords.Count > 1)
            //            {
            //                var nameWithTypeSlug = districtRecord.NameWithType.ToSlug();
            //                reesoftMatchingRecord = reesoftMatchingRecords.SingleOrDefault(_ => _.Name.ToSlug().EndsWith(nameWithTypeSlug,
            //                    StringComparison.OrdinalIgnoreCase));
            //            }
            //            else
            //            {
            //                reesoftMatchingRecord = reesoftMatchingRecords.SingleOrDefault();
            //            }

            //            if (reesoftMatchingRecord is null)
            //            {
            //                continue;
            //            }

            //            districtRecord.ReesoftDistrictId = reesoftMatchingRecord.Id;

            //            var associatedWards = Wards.Where(_ => _.ParentCode == districtRecord.Code)
            //                                       .ToList();

            //            associatedWards.ForEach(_ =>
            //            {
            //                _.ReesoftDistrictId = districtRecord.ReesoftDistrictId;
            //                _.ReesoftProvinceId = districtRecord.ReesoftProvinceId;
            //            });
            //        }
            //        catch (Exception ex)
            //        {
            //            continue;
            //        }
            //    }
            //}

            // wards
            var wards = _reeSoftDataProvider.Wards;
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
                        var wardRecordSlug = wardRecord.Name.ToSlug()
                            .Replace("01", "1")
                            .Replace("02", "2")
                            .Replace("03", "3")
                            .Replace("04", "4")
                            .Replace("05", "5")
                            .Replace("06", "6")
                            .Replace("07", "7")
                            .Replace("08", "8")
                            .Replace("09", "9");

                        //var wardRecordSlug = wardRecord.Name.ToSlug();
                        var matchingRecords = reesoftWards.Where(_ => _.Name.ToSlug().EndsWith(wardRecordSlug,
                            StringComparison.OrdinalIgnoreCase)).ToList();

                        if (matchingRecords.Count > 1)
                        {
                            var nameWithType = wardRecord.NameWithType;
                            matchingRecords = matchingRecords.Where(_ => _.Name.EndsWith(nameWithType,
                                StringComparison.OrdinalIgnoreCase)).ToList();
                        }

                        if (matchingRecords.Count == 0)
                        {
                            continue;
                        }

                        wardRecord.ReesoftWardIds = matchingRecords.Select(x => x.Id).ToArray();
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            // sync
            await Task.WhenAll(
                           //System.IO.File.WriteAllTextAsync(_provincesJsonFile.PhysicalPath!,
                           //                       JsonSerializer.Serialize(Provinces, _jsonSerializerOptions)),
                           //System.IO.File.WriteAllTextAsync(_districtsJsonFile.PhysicalPath!,
                           //                       JsonSerializer.Serialize(Districts, _jsonSerializerOptions)),
                           System.IO.File.WriteAllTextAsync(_wardsJsonFile.PhysicalPath!,
                                                  JsonSerializer.Serialize(Wards, _jsonSerializerOptions))
                          );

            return Ok();
        }
    }
}