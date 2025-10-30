namespace DeliveryMonitoring.Models
{
    public class HulubejeResponse <T> 
    {
        public bool IsSuccessful { get; set; }
        public T? Data { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public Dictionary<string, string>? AdditionalParameters { get; set; }
    }
}
