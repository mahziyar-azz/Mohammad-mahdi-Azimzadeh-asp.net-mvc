namespace Azimzadeh_MVC_project.Models
{
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
