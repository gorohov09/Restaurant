namespace Restaurant.Services.OrderAPI.Models.Dto
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string CouponCode { get; set; }
        public double OrderTotal { get; set; }
        public double DiscountTotal { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime OrderTime { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int CartTotalItems { get; set; }
        public bool PaymentStatus { get; set; }
    }
}
