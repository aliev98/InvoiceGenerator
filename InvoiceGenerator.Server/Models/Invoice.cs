using System.ComponentModel.DataAnnotations;

namespace InvoiceGenerator.Server.Models
{
    public class Invoice
    {
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientAddress { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}