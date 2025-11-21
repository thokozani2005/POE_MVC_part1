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
        private readonly ConnectDatabase _db;

        public IActionResult Index()
        {
            return View();
        }
        //a action result for managing lectures, it has a parameter than allows hr personnel to search for lecturers
        public IActionResult ManageLectures(string search = "")
        {
            ConnectDatabase db = new ConnectDatabase();
            var lecturers = db.GetAllLecturers(search);
            return View(lecturers);
        }
        //an Action result for editing the lecturer details
        public IActionResult EditLecturer(string empNum)
        {
            ConnectDatabase db = new ConnectDatabase();
            var lecturer = db.GetLecturerById(empNum);
            return View(lecturer); 
        }


        [HttpPost]
        //an Action result for editing the lecturer details, this one is a post it updates the details
        public IActionResult EditLecturer(GetUserInfo model)
        {
            ConnectDatabase db = new ConnectDatabase();
            db.UpdateLecturer(model); 

            return RedirectToAction("ManageLectures"); 
        }
        //an action result for all the approved claims, it retrieves the approved claims
        public IActionResult ViewApprovedClaims()
        {
               ConnectDatabase db = new ConnectDatabase();
          var approvedClaims = db.GetApprovedClaims();
         return View(approvedClaims);
            
        }
        //this action result allows the processing of all the approved payments
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

            // Fetching all the  updated claims to reflect changes immediately in the table
            var approvedClaims = db.GetApprovedClaimsWithPaymentStatus();

            return View("MarkAsProcessed", approvedClaims);
        }
        //This listener is for generating the invoce, I installed Quest helper to help me generate the invoice and convert it to a pdf
        public IActionResult GenerateInvoice(string empNum)
        {
            ConnectDatabase db = new ConnectDatabase();

            var claims = db.GetProcessedClaimsByLecturer(empNum);
            //we can only generate invoice if the claims are marked as processed
            if (claims == null || !claims.Any())
            {
                TempData["Error"] = "No processed claims available to generate invoice!";
                return RedirectToAction("MarkAsProcessed");
            }

            var grandTotal = claims.Sum(c => c.TotalAmount);
            var lecturer = db.GetLecturerById(empNum); // fetching the correct lecturer info
            string lecturerName = lecturer?.Name ?? "Unknown";



            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                    // This is the page HEADER
                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeColumn()
                                .Column(col =>
                                {
                                    col.Item().Text($"Invoice").Bold().FontSize(24).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
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
                                columns.ConstantColumn(200); 
                                columns.ConstantColumn(80);  
                                columns.ConstantColumn(100); 
                                columns.RelativeColumn();    
                            });

                            // HEADER ROW
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Module").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Sessions").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Hourly Rate").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total Amount").SemiBold();
                            });

                            // These are the DATA ROWS, I also used cultureInfo to convert the amount to SA Rands
                            foreach (var claim in claims)
                            {
                                table.Cell().Padding(5).Text(claim.module_name);
                                table.Cell().Padding(5).Text(claim.number_of_sssions.ToString());
                                table.Cell().Padding(5).Text(claim.hourly_rate.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().Padding(5).Text(claim.TotalAmount.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                            }

                            // Displaying the GRAND TOTAL
                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(3).AlignRight().Padding(5).Text("Grand Total").Bold();
                                footer.Cell().Padding(5).Text(grandTotal.ToString("C", new System.Globalization.CultureInfo("en-ZA"))).Bold();
                            });
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated by Contract Monthly Claim System © {DateTime.Now:yyyy}");
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Invoice_{lecturerName}_{DateTime.Now:yyyyMMdd}.pdf");
        }



    }
}
