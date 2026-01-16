using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;
using AutoFixture;
namespace DeliveryMonitoring.Helpers
{
    public class GetSampleOrder
    {
        private static readonly Fixture _fixture = new Fixture();

        public static T Create<T>()
        {
            return _fixture.Create<T>();
        }

        public static List<T> CreateList<T>(int count = 1)
        {
            return _fixture.CreateMany<T>(count).ToList();
        }
        public static List<OrderDetail> CreateSampleOrder()
        {
            var orders = CreateList<OrderDetail>(4);
            orders.ForEach(o => o.Alert = null);
            return orders;
        }
    }
}
