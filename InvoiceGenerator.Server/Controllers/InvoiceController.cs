using InvoiceGenerator.Server.Data;
using InvoiceGenerator.Server.Models;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InvoiceGenerator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceDbContext _context;
        public InvoiceController(InvoiceDbContext context) => _context = context;

        [HttpPost("preview-invoice-pdf")]
        public IActionResult PreviewInvoicePdf([FromBody] Invoice invoice)
        {
            if (invoice == null || invoice.Items == null || !invoice.Items.Any())
                return BadRequest("Invalid invoice data");

            byte[] pdfBytes = GenerateInvoicePdf(invoice);
            return File(pdfBytes, "application/pdf", "invoice.pdf");
        }

        [HttpPost("download-invoice")]
        public IActionResult DownInvoicePdf([FromBody] Invoice invoice)
        {
            if (invoice == null || invoice.Items == null || !invoice.Items.Any())
                return BadRequest("Invalid invoice data");

            byte[] pdfBytes = GenerateInvoicePdf(invoice);
            return File(pdfBytes, "application/pdf", "Invoice.pdf");
        }

        private byte[] GenerateInvoicePdf(Invoice invoice)
        {
            decimal total = invoice.Items.Sum(item => item.Total);
            invoice.TotalAmount = total;

            using (var memoryStream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                AddHeader(document);
                AddTitle(document);
                AddCompanyAndClientInfo(document, invoice.ClientName, invoice.ClientEmail, invoice.ClientAddress);
                AddItemsTable(document, invoice.Items.ToList());
                AddSummaryTable(document, invoice.TotalAmount);
                //AddFooter(document);

                document.Close();
                return memoryStream.ToArray();
            }
        }

        private void AddHeader(Document document)
        {
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 1 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);
            Paragraph companyAndInvoice = new Paragraph()
                .Add(new Text("TechTools").SetFontSize(24).SetBold())
                .Add(new Text("  |  Invoice").SetFontSize(18).SetBold())
                .SetTextAlignment(TextAlignment.LEFT)
                .SetPadding(5);

            headerTable.AddCell(new Cell().Add(companyAndInvoice)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT));

            document.Add(headerTable);
        }

        private void AddTitle(Document document)
        {
            Paragraph title = new Paragraph()
                .SetFont(PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);

            document.Add(title);
        }

        private void AddCompanyAndClientInfo(Document document, string clientName, string clientEmail, string clientAddress)
        {
            Table infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // Customer information
            infoTable.AddCell(new Cell()
                .Add(new Paragraph("Customer Information")
                .SetBold())
                .Add(new Paragraph($"{clientName}\n{clientEmail}\n{clientAddress}"))
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER));

            Table dateTable = new Table(UnitValue.CreatePercentArray(new float[] { 1 }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            dateTable.AddCell(new Cell()
                .Add(new Paragraph("Date:").SetBold())
                .Add(new Paragraph($"{DateTime.Now:MM/dd/yyyy}"))  
                .SetPaddingBottom(5)
                .SetBorder(Border.NO_BORDER));

            //dateTable.AddCell(new Cell()
            //    .Add(new Paragraph("Due Date:").SetBold())
            //    .Add(new Paragraph($"{DateTime.Now.AddDays(30):MM/dd/yyyy}"))  
            //    .SetBorder(Border.NO_BORDER));

            infoTable.AddCell(new Cell().Add(dateTable).SetPadding(5).SetBorder(Border.NO_BORDER));

            document.Add(infoTable);
        }

        private void AddItemsTable(Document document, List<InvoiceItem> items)
        {
            Table itemsTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1, 1 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("Description").SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("Quantity").SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            itemsTable.AddHeaderCell(new Cell().Add(new Paragraph("Unit Price").SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

            foreach (var item in items)
            {
                itemsTable.AddCell(item.Description);
                itemsTable.AddCell(item.Quantity.ToString());
                itemsTable.AddCell($"${item.UnitPrice.ToString("0.00")}");
            }

            document.Add(itemsTable);
        }

        private void AddSummaryTable(Document document, decimal totalAmount)
        {
            Table summaryTable = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            summaryTable.AddCell(new Cell().Add(new Paragraph("Subtotal").SetBold()));
            summaryTable.AddCell(new Cell().Add(new Paragraph($"${totalAmount.ToString("0.00")}")));

            summaryTable.AddCell(new Cell().Add(new Paragraph("Tax").SetBold()));
            summaryTable.AddCell(new Cell().Add(new Paragraph("$0.00")));

            summaryTable.AddCell(new Cell().Add(new Paragraph("Total Due").SetBold()));
            summaryTable.AddCell(new Cell().Add(new Paragraph($"${totalAmount.ToString("0.00")}")));

            document.Add(summaryTable);
        }

        //private void AddFooter(Document document)
        //{
        //    Paragraph footer = new Paragraph("Thank you for your business!")
        //        .SetFontSize(10)
        //        .SetTextAlignment(TextAlignment.CENTER)
        //        .SetMarginTop(20);

        //    document.Add(footer);
        //}
    }
}
