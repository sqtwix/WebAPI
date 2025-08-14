namespace CsvImportApi.Models
{
    public class Result
    {
        public int Id { get; set; } // первичный ключ
        public double DeltaDateSeconds { get; set; }
        public DateTime MinDate { get; set; }
        public double ExecutionTime { get; set; }
        public double AvgValue {get; set;}
        public double MedianValue {get; set;}
        public double MaxValue { get; set;} 
        public double minValue { get; set;}
    }
}
