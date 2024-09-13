namespace InvoiceGenerator.Server.Models
{
    public class InvoiceItem
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }
}
