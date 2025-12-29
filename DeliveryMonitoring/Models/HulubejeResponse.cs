namespace DeliveryMonitoring.Models
{
    public class HulubejeResponse <T> 
    {
        public bool IsSuccessful { get; set; }
        public T? Data { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public Dictionary<string, string>? AdditionalParameters { get; set; }
        public static HulubejeResponse<T> Success(T data)
        {
            return new HulubejeResponse<T>
            {
                IsSuccessful = true,
                Data = data
            };
        }

        public static HulubejeResponse<T> Fail(params string[] errors)
        {
            return new HulubejeResponse<T>
            {
                IsSuccessful = false,
                ErrorMessages = errors.ToList()
            };
        }

        public static HulubejeResponse<T> Fail(IEnumerable<string> errors)
        {
            return new HulubejeResponse<T>
            {
                IsSuccessful = false,
                ErrorMessages = errors.ToList()
            };
        }
    }
}
