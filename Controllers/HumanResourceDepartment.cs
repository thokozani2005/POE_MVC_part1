using Microsoft.AspNetCore.Mvc;
using POE_MVC_part1.Models;
using QuestPDF.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Infrastructure;

namespace POE_MVC_part1.Controllers
{
    public class HumanResourceDepartment : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageLectures(string search = "")
        {
            ConnectDatabase db = new ConnectDatabase();
            var lecturers = db.GetAllLecturers(search);
            return View(lecturers);
        }

        public IActionResult EditLecturer(string empNum)
        {
            ConnectDatabase db = new ConnectDatabase();
            var lecturer = db.GetLecturerById(empNum); // fetch from DB
            return View(lecturer); // passes to EditLecturer.cshtml
        }


        [HttpPost]
        public IActionResult EditLecturer(GetUserInfo model)
        {
            ConnectDatabase db = new ConnectDatabase();
            db.UpdateLecturer(model); // updates the database

            return RedirectToAction("ManageLectures"); // back to table view
        }

        public IActionResult ViewApprovedClaims()
        {
               ConnectDatabase db = new ConnectDatabase();
          var approvedClaims = db.GetApprovedClaims();
         return View(approvedClaims);
            
        }

        public IActionResult MarkAsProcessed()
        {
            ConnectDatabase db = new ConnectDatabase();
            var approvedClaims = db.GetApprovedClaimsWithPaymentStatus();
            return View(approvedClaims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePayment(int claimId, string status)
        {
            ConnectDatabase db = new ConnectDatabase();
            bool updated = db.UpdatePaymentStatus(claimId, status);

            if (!updated)
            {
                TempData["Error"] = "Payment status could not be updated!";
            }

            // Fetch updated claims to reflect changes immediately in the table
            var approvedClaims = db.GetApprovedClaimsWithPaymentStatus();

            return View("MarkAsProcessed", approvedClaims);
        }
        
        public IActionResult GenerateInvoice(string empNum)
        {
            ConnectDatabase db = new ConnectDatabase();

            var claims = db.GetProcessedClaimsByLecturer(empNum);

            if (claims == null || !claims.Any())
            {
                TempData["Error"] = "No processed claims available to generate invoice!";
                return RedirectToAction("MarkAsProcessed");
            }

            var grandTotal = claims.Sum(c => c.TotalAmount);
            string lecturerName = claims.FirstOrDefault()?.name ?? "Unknown";

            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                    // HEADER
                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeColumn()
                                .Column(col =>
                                {
                                    col.Item().Text($"Invoice").Bold().FontSize(24).FontColor(QuestPDF.Helpers.Colors.Blue.Medium); // <- Use a shade
                                    col.Item().Text($"Lecturer: {lecturerName}").Bold().FontSize(14);
                                    col.Item().Text($"Claim Period: {claims.First().startdate} - {claims.First().end_date}").FontSize(12);
                                });

                            row.ConstantColumn(100)
                                .AlignRight()
                                .Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(12);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Table(table =>
                        {
                            // Columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200); // Module
                                columns.ConstantColumn(80);  // Sessions
                                columns.ConstantColumn(100); // Hourly Rate
                                columns.RelativeColumn();    // Total
                            });

                            // HEADER ROW
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Module").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Sessions").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Hourly Rate").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total Amount").SemiBold();
                            });

                            // DATA ROWS
                            foreach (var claim in claims)
                            {
                                table.Cell().Padding(5).Text(claim.module_name);
                                table.Cell().Padding(5).Text(claim.number_of_sssions.ToString());
                                table.Cell().Padding(5).Text(claim.hourly_rate.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().Padding(5).Text(claim.TotalAmount.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                            }

                            // GRAND TOTAL
                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(3).AlignRight().Padding(5).Text("Grand Total").Bold();
                                footer.Cell().Padding(5).Text(grandTotal.ToString("C", new System.Globalization.CultureInfo("en-ZA"))).Bold();
                            });
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated by ResConnect © {DateTime.Now:yyyy}");
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Invoice_{lecturerName}_{DateTime.Now:yyyyMMdd}.pdf");
        }



    }
}
