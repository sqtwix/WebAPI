using CsvHelper.Expressions;
using CsvImportApi.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
namespace CsvImportApi.Services
{
    public class CsvValidator
    {
        public bool ValidateFile(List<Values> records, out string errorMessage)
        {
            // проверка на количество строк
            if (records.Count < 1 || records.Count > 10000)
            {
                errorMessage = "Количество строк должно быть от 1 до 10 000";
                return false;
            }

            foreach (var record in records) { 
                // Проверка на дату
                if (record.Date < new DateTime(2000,1,1) ||
                   record.Date >  DateTime.UtcNow)
                {
                    errorMessage = "Некорректная дата";
                    return false;
                }  

                // Проверка на время выполнения
                if (record.ExecutionTime < 0)
                {
                    errorMessage = "Ошибка, время выполения меньше 0";
                    return false;
                }

                // Проверка на значение
                if (record.Value< 0)
                {
                    errorMessage = "Ошибка, значение меньше 0";
                    return false;
                }

                // Проверка на значение
                if (record.Value < 0)
                {
                    errorMessage = "Ошибка, значение меньше 0";
                    return false;
                }
            }
            errorMessage = String.Empty;
            return true;
        }
    }
}
