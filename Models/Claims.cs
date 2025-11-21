using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace POE_MVC_part1.Models
{
    public class Claims
    {
        [Required]
        public string name { get; set; }

        public int employeenum { get; set; }

        [Required]
        public string module_name { get; set; }

        [Required]
        public int number_of_sssions { get; set; }

        [Required]
        public double hourly_rate { get; set; }

        [Required]
        public string startdate { get; set; }

        [Required]
        public string end_date { get; set; }

        // Optional fields, will only be populated when file is uploaded
        public IFormFile UploadFile { get; set; }

        // Custom validation for file upload
        public bool IsFileUploaded => UploadFile != null && UploadFile.Length > 0;

        // Default status on submission
        public string status { get; set; } = "Pending";

       
        public int Claim_Id { get; set; }                
        public double TotalAmount { get; set; }        
        public byte[]? DocumentData { get; set; }
        public string? ClaimStatus { get; set; }

        public string? PaymentStatus { get; set; }
        public string? FileName { get; set; }

    }

}
