using Microsoft.AspNetCore.Mvc; 
using POE_MVC_part1.Models;     

namespace POE_MVC_part1.Controllers
{
    public class Users : Controller
    {
        [HttpGet] 
        public IActionResult Lecture()
        {
            // Creating an instance 
            ConnectDatabase create_table = new ConnectDatabase(); 
            create_table.GenerateClaimTable();
            // Retrieve the Data  from session
            int? empNum = HttpContext.Session.GetInt32("EmpoyeeNum"); 
            string employeeName = HttpContext.Session.GetString("EmployeeName");

            // Checking if user session is invalid or expired
            if (empNum == null || string.IsNullOrEmpty(employeeName)) 
            {
                return RedirectToAction("LogIn", "Home"); 
            }
            // Creating a new Claims model object to pass to the view
            var model = new Claims 
            {
                employeenum = empNum.Value, 
                name = employeeName         
            };

            return View(model); 
        }


        [HttpPost] 
        public IActionResult Lecture(Claims Submitclaim) 
        {
            // Checking if the submitted form passes validation rules defined in the model
            if (ModelState.IsValid) 
            {
                // Checking if the user has uploaded a file
                if (Submitclaim.IsFileUploaded)
                { // Creating a memory stream to hold uploaded file data
                    using (var memoryStream = new MemoryStream()) 
                    {
                        // Copying the uploaded file data into the memory stream
                        Submitclaim.UploadFile.CopyTo(memoryStream);
                        // Converting the stream to a byte array
                        byte[] fileData = memoryStream.ToArray();  

                        try
                        {
                            ConnectDatabase create_table = new ConnectDatabase(); 
                            create_table.Store_IntoClaimTable(                 
                                Submitclaim.name,                              
                                Submitclaim.module_name,                       
                                Submitclaim.number_of_sssions,               
                                Submitclaim.hourly_rate,                       
                                Submitclaim.startdate,                         
                                Submitclaim.end_date,                          
                                fileData,                                      
                                Submitclaim.employeenum                        
                            );
                            // success message 
                            ViewBag.Message = "Your claim has been uploaded successfully!";
                        }
                        catch (Exception ex) 
                        {
                            ViewBag.Message = $"Error: {ex.Message}"; 
                        }
                    }
                }
                else // If no file was uploaded
                {
                    ViewBag.Message = "Please upload a document."; 
                }
            }
            else
            {// Collecting all validation error messages
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(); 
                ViewBag.Errors = errors;
                ViewBag.Message = "Please correct the errors in the form."; 
            }

            return View(Submitclaim); 
        }


        public IActionResult Academic_Manager() 
        {
            return View(); 
        }

        public IActionResult Program_Coordinator() 
        {
            return View(); 
        }

        public IActionResult ViewHistory() 
        {
            ConnectDatabase db = new ConnectDatabase(); 

            int? empNum = HttpContext.Session.GetInt32("EmpoyeeNum"); // Geting the employee number from session

            if (empNum == null) // If session expired or user not logged in
            {
                return RedirectToAction("LogIn", "Home"); 
            }

            var claims = db.GetClaimHistoryForDownload(empNum.Value); // Fetching claim history

            return View(claims ?? new List<Claims>()); 
        }


        [HttpGet] 
        public IActionResult ViewandPreApprove()
        {
            ConnectDatabase db = new ConnectDatabase(); 
            var claims = db.ViewandPreApproveClaims();   
            return View(claims);
        }

        [HttpPost] 
        public IActionResult ViewandPreApprove(int claimId, string status)
        {
            ConnectDatabase db = new ConnectDatabase(); 
            bool success = false; 

            try
            {
                // Updating claim status in database
                success = db.UpdateClaimStatus(claimId, status); 
            }
            catch (Exception ex)
            {
                // Catching exceptions and store message in TempData to show after redirecting
                TempData["Message"] = $"Error updating claim status: {ex.Message}";
                return RedirectToAction("ViewandPreApprove");
            }

            // Displaying success or error message after redirect
            TempData["Message"] = success
                ? "Claim status updated successfully!"
                : "Error updating claim status.";

            return RedirectToAction("ViewandPreApprove"); 
        }

        [HttpGet] 
        public IActionResult finalApproval()
        {
            ConnectDatabase db = new ConnectDatabase(); 
            var claims = db.ViewandPreApproveClaims(); 
            return View(claims);
        }

        [HttpPost] 
        public IActionResult finalApproval(int claimId, string status)
        {
            ConnectDatabase db = new ConnectDatabase(); 
            bool success = false;

            try
            {
                // Calling database method to update claim status for final approval
                success = db.UpdateClaimStatusForFinalApproval(claimId, status);
            }
            catch (Exception ex) // Catching exceptions if database update fails
            {
                TempData["Message"] = $"Error updating final approval status: {ex.Message}";
                return RedirectToAction("FinalApproval"); 
            }

            // Storing the message in TempData to display after redirect
            TempData["Message"] = success
                ? "Claim final approval status updated successfully!"
                : "Error updating final approval status.";

            return RedirectToAction("FinalApproval"); 
        }


        public IActionResult DownloadClaim(int id) 
        {
            var db = new ConnectDatabase();
            // Fetching the claim from database by ID
            var claim = db.GetClaimById(id);

            //Checking If claim or file data does not exist
            if (claim == null || claim.DocumentData == null) 
                return NotFound("Claim file not found.");
            // Generating file name for download
            string fileName = $"Claim_{claim.Claim_Id}.pdf";
            // Sending file to client as download
            return File(claim.DocumentData, "application/pdf", fileName); 
        }
    }
}
