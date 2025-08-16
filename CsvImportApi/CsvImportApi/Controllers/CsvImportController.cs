using CsvHelper;
using CsvImportApi.Models;
using CsvImportApi.Services;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace CsvImportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvImportController : ControllerBase
    {
        private readonly AppDbContext _context; // контекст
        private readonly CsvValidator _validator; // валидатор данных

        public CsvImportController (AppDbContext context, CsvValidator validator) // конструктор
        {
            _context = context;
            _validator = validator;
        }

        [HttpPost]
        [Route("csv")]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            if (file == null || file.Length == 0) { return BadRequest("Файл не загружен"); }

            var records = new List<Values>();

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    records = csv.GetRecords<Values>().ToList();
                    foreach (var r in records)
                    {
                        r.FileName = file.FileName;
                    }
                }

                if (!_validator.ValidateFile(records, out var error)) { return BadRequest(error); } // проверка данных файла через валидатор

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

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        private double GetMedian(List<double> values) // Нахождение медианы значений
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
