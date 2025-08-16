namespace CsvImportApi.Models
{
    public class Values
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double ExecutionTime { get; set; }
        public double Value { get; set; }
        public string FileName { get; set; } // для поддержки перезаписи по имени файла
    }
}
