using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.APIs.Dtos
{
    public class OrderToReturnDto
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string Status { get; set; }
        public Address ShippingAddress { get; set; }
        public string DeliveryMethod { get; set; }
        public decimal DeliveryMethodCost { get; set; }
        public List<OrderItemDto> Items { get; set; } // Navigational Property [MANY]
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string PaymentIntentId { get; set; } 
    }
}
