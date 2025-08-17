using CsvHelper;
using CsvHelper.Configuration;
using CsvImportApi.Models;
using CsvImportApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


namespace CsvImportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvImportController : ControllerBase
    {
        private readonly AppDbContext _context; // контекст
        private readonly CsvValidator _validator; // валидатор данных

        public CsvImportController(AppDbContext context, CsvValidator validator) // конструктор
        {
            _context = context;
            _validator = validator;
        }

        // Метод 1. Сохранение файлов из csv файла в бд
        [HttpPost]
        [Route("csv")]
        public async Task<IActionResult> UploadCSV(IFormFile file) 
        {
            if (file == null || file.Length == 0) { return BadRequest("Файл не загружен"); }

            var records = new List<Values>();

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    PrepareHeaderForMatch = args => args.Header.Trim(),
                    HeaderValidated = null,
                    MissingFieldFound = null
                }))
                {
                    csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[]
                 {
                    "yyyy-MM-dd",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-ddTHH-mm-ss.ffffZ"
                };

                    records = csv.GetRecords<Values>().ToList();

                    // Приведение дат к UTC
                    foreach (var record in records)
                    {
                        if (record.Date.Kind == DateTimeKind.Unspecified)
                        {
                            record.Date = DateTime.SpecifyKind(record.Date, DateTimeKind.Utc);
                        }
                        else
                        {
                            record.Date = record.Date.ToUniversalTime();
                        }
                        record.FileName = file.FileName;
                    }
                }
              

                if (!_validator.IsValideFile(records, out var error)) { return BadRequest(error); } // проверка данных файла через валидатор

                try
                {

                    // Удаление данных файла с таким же именем
                    var oldValues = await _context.Values.Where(v => v.FileName == file.FileName).ToListAsync();
                    var oldResult = await _context.Results.FirstOrDefaultAsync(r => r.FileName == file.FileName);

                    if (oldValues.Any())
                        _context.Values.RemoveRange(oldValues);
                    if (oldResult != null)
                        _context.Results.Remove(oldResult);

                    await _context.SaveChangesAsync();

                    // Сохраняем в Values
                    await _context.Values.AddRangeAsync(records);
                    await _context.SaveChangesAsync();

                    //  Интегральные результаты для Results
                    var result = new Result
                    {
                        FileName = file.FileName,
                        DeltaDateSeconds = (records.Max(r => r.Date) - records.Min(r => r.Date)).TotalSeconds,
                        MinDate = records.Min(r => r.Date),
                        ExecutionTime = records.Average(r => r.ExecutionTime),
                        AvgValue = records.Average(r => r.Value),
                        MedianValue = GetMedian(records.Select(r => r.Value).ToList()),
                        MaxValue = records.Max(r => r.Value),
                        MinValue = records.Min(r => r.Value)
                    };

                    // Сохранение в Results
                    await _context.Results.AddAsync(result);
                    await _context.SaveChangesAsync();

                    return Ok("Файл успешно обработан");
                }
                catch (DbUpdateException ex)
                {
                    return StatusCode(500, $"Database error: {ex.InnerException?.Message}");
                }
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Метод 2. Получение списков записей с фильтрами
        [HttpGet]
        [Route("results")]
        public async Task<IActionResult> GetDataWithFilters(
            // Переменные параметы
            string? fileName,
            DateTime? startDateFrom,
            DateTime? startDateTo,
            double? avgValueFrom,
            double? avgValueTo,
            double? avgExecutionTimeFrom,
            double? avgExecutionTimeTo)
        {
            var query = _context.Results.AsQueryable();

            // Проверка, что фильтр есть в запросе
            if (!string.IsNullOrWhiteSpace(fileName)) 
                query = query.Where(r => r.FileName == fileName);

            if (startDateFrom.HasValue)
                    query = query.Where(r => r.MinDate >= startDateFrom.Value);
           
            if (startDateTo.HasValue)
                query = query.Where(r => r.MinDate <= startDateTo.Value);

            if (avgValueFrom.HasValue)
                query = query.Where( r => r.AvgValue >= avgValueFrom.Value);

            if (avgValueTo.HasValue)
                query = query.Where(r => r.AvgValue <= avgValueTo.Value);

            if (avgExecutionTimeFrom.HasValue)
                query = query.Where(r => r.ExecutionTime >= avgExecutionTimeFrom.Value);

            if (avgExecutionTimeTo.HasValue)
                query = query.Where(r => r.ExecutionTime <= avgExecutionTimeTo.Value);

            var results = await query.ToListAsync(); // отправка запроса
            return Ok(results);
        }


        // Метод 3. Получение спиcка последних 10 значений  отсортированных
        // по начальному времени запуска Date по имени заданного файла.
        [HttpGet]
        [Route("values/latest/{fileName}")]
        public async Task<IActionResult> GetTenLatestDatas(string fileName)
        {
            var query = _context.Values.AsQueryable();

            query = query.Where(r => r.FileName == fileName);

            var values = await query
               .OrderByDescending(v => v.Date)
               .Take(10)
               .ToListAsync();

            return Ok(values);
        }


        // Нахождение медианы значений
        private double GetMedian(List<double> values) 
        {
            var sorted = values.OrderBy(n => n).ToList(); 
            int count = sorted.Count;

            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }
            else
            {
                return sorted[count / 2];
            }
        }
        
    }
}
