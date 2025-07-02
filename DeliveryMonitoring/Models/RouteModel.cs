namespace DeliveryMonitoring.Models
{
    public class RouteModel
    {
        public double? Distance { get; set; }
        public long? Eta { get; set; }
        public List<List<double>>? Coordinates { get; set; }
        public List<Instruction>? Instructions { get; set; }
    }
    public class Instruction
    {
        public double? Distance { get; set; }
        public double? Heading { get; set; }
        public int? Sign { get; set; }
        public List<int>? Interval { get; set; }
        public string? Text { get; set; }
        public int? Time { get; set; }
        public string? StreetName { get; set; }
    }
}
