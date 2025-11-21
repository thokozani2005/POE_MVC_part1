using System.Data.SqlClient;
using System.Security.Claims;
using POE_MVC_part1.Controllers;

namespace POE_MVC_part1.Models
{
    public class ConnectDatabase
    {


        string connection = @"Server=(localdb)\Contract_Monthly_Claim_System;Database=ContractMonthlyClaimSystem;";


        //a Method that creates the tables

        public void GenerateTable()
        {
            try
            {
                using (SqlConnection connect_server = new SqlConnection(connection))
                {
                    //opening the connection
                    connect_server.Open();

                    //creating a query that will help us create the user Table

                    string query = @"CREATE TABLE Users (
                                    EmpoyeeNum INT IDENTITY (100,1) NOT NULL PRIMARY KEY,
                                    EmployeeName VARCHAR(50) NOT NULL,
                                    EmployeeSurname VARCHAR(50) NOT NULL,
                                    CellNumber VARCHAR(20) NOT NULL,
                                    Email VARCHAR(30) NOT NULL,
                                    Password VARCHAR (50) NOT NULL,
                                    Role VARCHAR(50) NOT NULL);";

                    using (SqlCommand generateTable = new SqlCommand(query, connect_server))
                    {
                        //running the query
                        generateTable.ExecuteNonQuery();

                        //success message
                        Console.WriteLine("We are connected");
                    }
                    connect_server.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }


        public void Store_into_Table(string name, string lastname, string phoneNumber, string email, string password, string role)
        {
            try
            {
                using (SqlConnection connect_server = new SqlConnection(connection))
                {
                    //opening the connection
                    connect_server.Open();

                    //creating a query that will help us create the user Table

                    string query = @"INSERT INTO Users (EmployeeName, EmployeeSurname, CellNumber, Email, Password, Role) VALUES 
                                   ('" + name + "','" + lastname + "','" + phoneNumber + "','" + email + "','" + password + "','" + role + "')";

                    using (SqlCommand generateTable = new SqlCommand(query, connect_server))
                    {
                        //running the query
                        generateTable.ExecuteNonQuery();

                        //success message
                        Console.WriteLine("info stored");
                    }
                    connect_server.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }



        //a method that gets the user info
        public bool GetUserInfo(string email, string password, string role, out string name, out int empNum, out string lastname)
        {
            name = "";
            empNum = 0;
            lastname = "";
            bool userFound = false;

            try
            {
                using (SqlConnection connect_server = new SqlConnection(connection))
                {
                    connect_server.Open();

                    string query = @"SELECT EmployeeName, EmpoyeeNum, EmployeeSurname 
                             FROM Users 
                             WHERE Email = @Email AND Password = @Password AND Role = @Role";

                    using (SqlCommand command = new SqlCommand(query, connect_server))
                    {
                        // I used parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Role", role);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //storing these to  be able to display them when our user logs in
                                name = reader["EmployeeName"].ToString();
                                lastname = reader["EmployeeSurname"].ToString();
                                empNum = Convert.ToInt32(reader["EmpoyeeNum"]);
                                userFound = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return userFound;
        }




        //a method that creates the claim tabble

        public void GenerateClaimTable()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string sql_query = @"CREATE TABLE Claims (
    Claim_Id INT IDENTITY(200,1) NOT NULL PRIMARY KEY,
    EmpoyeeNum INT FOREIGN KEY REFERENCES Users(EmpoyeeNum),
    Lecture_Name VARCHAR(50) NOT NULL,
    Module_Name VARCHAR(50) NOT NULL,
    Number_ofSessions INT NOT NULL,
    Hourly_rate DECIMAL(10,2) NOT NULL,
    Total_Amount DECIMAL(10,2) NOT NULL,
    Claim_Start_Date DATE NOT NULL,
    Claim_End_Date DATE NOT NULL,
    Document_Data VARBINARY(MAX),
    Claim_Status VARCHAR(20) DEFAULT 'Pending'
);
";

                    using (SqlCommand command = new SqlCommand(sql_query, connect))
                    {

                        command.ExecuteNonQuery();
                        Console.WriteLine("Your table is Created and stored!");


                    }

                    connect.Close();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }


        //Creating a method that stores into the Claims Table

        public void Store_IntoClaimTable(
    string LectureName,
    string moduleName,
    int numOFses,
    double hourlyrate,
    string startDate,
    string endDate,
    byte[] Data,
    int EmployeeNum)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    // Calculating the total amount
                    double totalAmount = numOFses * hourlyrate;

                    //store into the query and using parameters to avoid sql injection
                    string sql_query = @"
                INSERT INTO Claims 
                (Lecture_Name, Module_Name, Number_ofSessions, Hourly_rate, Total_Amount, 
                 Claim_Start_Date, Claim_End_Date, Document_Data, EmpoyeeNum, Claim_Status)
                VALUES 
                (@LectureName, @ModuleName, @NumSessions, @HourlyRate, @TotalAmount, 
                 @StartDate, @EndDate, @DocumentData, @EmpoyeeNum, @ClaimStatus)";

                    using (SqlCommand command = new SqlCommand(sql_query, connect))
                    {
                        // Adding parameters safely
                        command.Parameters.AddWithValue("@LectureName", LectureName);
                        command.Parameters.AddWithValue("@ModuleName", moduleName);
                        command.Parameters.AddWithValue("@NumSessions", numOFses);
                        command.Parameters.AddWithValue("@HourlyRate", hourlyrate);
                        command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);
                        command.Parameters.AddWithValue("@DocumentData", Data);
                        command.Parameters.AddWithValue("@EmpoyeeNum", EmployeeNum);
                        command.Parameters.AddWithValue("@ClaimStatus", "Pending"); //setting the status to be default by submission


                        command.ExecuteNonQuery();

                        Console.WriteLine("Claim stored successfully!");
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error while storing claim: " + error.Message);
            }
        }
        //created a generic method that gets the claims and stores them into a list

        public List<Claims> GetClaimHistoryForDownload(int employeeNum)
        {
            List<Claims> claimsList = new List<Claims>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();
                    //store into the query and using parameters to avoid sql injection
                    string query = @"SELECT Claim_Id, EmpoyeeNum, Module_Name, Number_ofSessions, Hourly_rate, 
                                    Total_Amount, Claim_Start_Date, Claim_End_Date, 
                                    Claim_Status, Document_Data
                             FROM Claims
                             WHERE EmpoyeeNum = @EmpNum";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EmpNum", employeeNum);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Claims claim = new Claims
                                {
                                    //reading/retrieving from the claims table are converting them to the suitable datatypes
                                    Claim_Id = Convert.ToInt32(reader["Claim_Id"]),
                                    employeenum = Convert.ToInt32(reader["EmpoyeeNum"]),
                                    module_name = reader["Module_Name"].ToString(),
                                    number_of_sssions = Convert.ToInt32(reader["Number_ofSessions"]),
                                    hourly_rate = Convert.ToDouble(reader["Hourly_rate"]),
                                    TotalAmount = Convert.ToDouble(reader["Total_Amount"]),
                                    startdate = Convert.ToDateTime(reader["Claim_Start_Date"]).ToString("yyyy-MM-dd"),
                                    end_date = Convert.ToDateTime(reader["Claim_End_Date"]).ToString("yyyy-MM-dd"),
                                    ClaimStatus = reader["Claim_Status"].ToString(),
                                    DocumentData = reader["Document_Data"] as byte[]
                                };

                                claimsList.Add(claim);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching claim history: " + ex.ToString());
            }

            return claimsList;
        }

        //This method gets the Claim b the claim Id
        public Claims GetClaimById(int claimId)
        {
            Claims claim = null;

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT Claim_Id, Module_Name, Number_ofSessions, Hourly_rate, 
                            Total_Amount, Claim_Start_Date, Claim_End_Date, 
                            Claim_Status, Document_Data
                     FROM Claims
                     WHERE Claim_Id = @ClaimId";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ClaimId", claimId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                claim = new Claims
                                {
                                    //reading/retrieving from the claims table are converting them to the suitable datatypes
                                    Claim_Id = Convert.ToInt32(reader["Claim_Id"]),
                                    module_name = reader["Module_Name"].ToString(),
                                    number_of_sssions = Convert.ToInt32(reader["Number_ofSessions"]),
                                    hourly_rate = Convert.ToDouble(reader["Hourly_rate"]),
                                    TotalAmount = Convert.ToDouble(reader["Total_Amount"]),
                                    startdate = Convert.ToDateTime(reader["Claim_Start_Date"]).ToString("yyyy-MM-dd"),
                                    end_date = Convert.ToDateTime(reader["Claim_End_Date"]).ToString("yyyy-MM-dd"),
                                    ClaimStatus = reader["Claim_Status"].ToString(),
                                    DocumentData = reader["Document_Data"] as byte[]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching claim by ID: " + ex.Message);
            }

            return claim;
        }


        public List<Claims> ViewandPreApproveClaims()
        {
            // List to store claims
            List<Claims> claims = new List<Claims>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    // Query to fetch all claims
                    string query = @"SELECT * FROM Claims";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Populating the claim list with the data from the reader
                                claims.Add(new Claims
                                {
                                    Claim_Id = Convert.ToInt32(reader["Claim_Id"]),
                                    name = reader["Lecture_Name"].ToString(),
                                    employeenum = Convert.ToInt32(reader["EmpoyeeNum"]),
                                    module_name = reader["Module_Name"].ToString(),
                                    number_of_sssions = Convert.ToInt32(reader["Number_ofSessions"]),
                                    hourly_rate = Convert.ToDouble(reader["Hourly_rate"]),
                                    TotalAmount = reader.IsDBNull(reader.GetOrdinal("Total_Amount")) ? 0 : Convert.ToDouble(reader["Total_Amount"]),//converting my decimal to double
                                    startdate = Convert.ToDateTime(reader["Claim_Start_Date"]).ToString("yyyy-MM-dd"),
                                    end_date = Convert.ToDateTime(reader["Claim_End_Date"]).ToString("yyyy-MM-dd"),
                                    ClaimStatus = reader["Claim_Status"].ToString(),
                                    DocumentData = reader["Document_Data"] as byte[]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching claims: " + ex.Message);
            }

            return claims;
        }



        //a returning method that updates the claim status for the pre approval
        public bool UpdateClaimStatus(int claimId, string newStatus)
        {
            bool updated = false;
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();
                    string query = "UPDATE Claims SET Claim_Status = @status WHERE Claim_Id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", claimId);

                        int rows = cmd.ExecuteNonQuery();
                        updated = rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error updating claim status: " + ex.Message);

            }
            return updated;
        }

        //This method updates the table for the Final approval, it changes the status of the Claim
        public bool UpdateClaimStatusForFinalApproval(int claimId, string newStatus)
        {
            bool updated = false;
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();
                    string query = "UPDATE Claims SET Claim_Status = @status WHERE Claim_Id = @id";

                    using (SqlCommand command = new SqlCommand(query, connect))
                    {
                        command.Parameters.AddWithValue("@status", newStatus);
                        command.Parameters.AddWithValue("@id", claimId);

                        int rows = command.ExecuteNonQuery();
                        updated = rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating final approval status: " + ex.Message);
            }
            return updated;
        }


        //creating a method that retrieves all the Lectures

        public List<GetUserInfo> GetAllLecturers()
        {
            List<GetUserInfo> lecturers = new List<GetUserInfo>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT EmployeeName, EmployeeSurname, EmpoyeeNum, 
                             Email, Role 
                             FROM Users 
                             WHERE Role = 'Lecturer'";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lecturers.Add(new GetUserInfo
                                {
                                    Name = reader["EmployeeName"].ToString(),
                                    LastName = reader["EmployeeSurname"].ToString(),
                                    EmployeeNum = reader["EmpoyeeNum"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Role = reader["Role"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching lecturers: " + ex.Message);
            }

            return lecturers;
        }


        public List<GetUserInfo> GetAllLecturers(string searchTerm = "")
        {
            List<GetUserInfo> lecturers = new List<GetUserInfo>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT EmpoyeeNum, EmployeeName, EmployeeSurname, Email 
                             FROM Users
                             WHERE Role = 'Lecturer'
                             AND (EmployeeName LIKE @search OR EmployeeSurname LIKE @search OR Email LIKE @search)";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + searchTerm + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lecturers.Add(new GetUserInfo
                                {
                                    EmployeeNum = reader["EmpoyeeNum"].ToString(),
                                    Name = reader["EmployeeName"].ToString(),
                                    LastName = reader["EmployeeSurname"].ToString(),
                                    Email = reader["Email"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return lecturers;
        }


        public GetUserInfo GetLecturerById(string empNum)
        {
            GetUserInfo lecturer = null;

            using (SqlConnection connect = new SqlConnection(connection))
            {
                connect.Open();

                string query = @"SELECT EmpoyeeNum, EmployeeName, EmployeeSurname, Email
                         FROM Users WHERE EmpoyeeNum = @num";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@num", empNum);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lecturer = new GetUserInfo
                            {
                                EmployeeNum = reader["EmpoyeeNum"].ToString(),
                                Name = reader["EmployeeName"].ToString(),
                                LastName = reader["EmployeeSurname"].ToString(),
                                Email = reader["Email"].ToString()
                            };
                        }
                    }
                }
            }

            return lecturer;
        }


        public void UpdateLecturer(GetUserInfo user)
        {
            using (SqlConnection connect = new SqlConnection(connection))
            {
                connect.Open();

                string query = @"UPDATE Users 
                         SET EmployeeName = @name,
                             EmployeeSurname = @surname,
                             Email = @Email
                         WHERE EmpoyeeNum = @num";

                using (SqlCommand cmd = new SqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@name", user.Name);
                    cmd.Parameters.AddWithValue("@surname", user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@num", user.EmployeeNum);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // amethod that retrieves all the claims

        public List<Claims> GetApprovedClaims()
        {
            List<Claims> approvedClaims = new List<Claims>();

            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();

                    string query = @"SELECT Claim_Id, EmpoyeeNum, Lecture_Name, Module_Name, Number_ofSessions, 
                                    Hourly_rate, Total_Amount, Claim_Start_Date, Claim_End_Date, Claim_Status
                             FROM Claims
                             WHERE Claim_Status = 'Approved'";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                approvedClaims.Add(new Claims
                                {
                                    Claim_Id = Convert.ToInt32(reader["Claim_Id"]),
                                    employeenum = Convert.ToInt32(reader["EmpoyeeNum"]),
                                    name = reader["Lecture_Name"].ToString(),
                                    module_name = reader["Module_Name"].ToString(),
                                    number_of_sssions = Convert.ToInt32(reader["Number_ofSessions"]),
                                    hourly_rate = Convert.ToDouble(reader["Hourly_rate"]),
                                    TotalAmount = Convert.ToDouble(reader["Total_Amount"]),
                                    startdate = Convert.ToDateTime(reader["Claim_Start_Date"]).ToString("yyyy-MM-dd"),
                                    end_date = Convert.ToDateTime(reader["Claim_End_Date"]).ToString("yyyy-MM-dd"),
                                    ClaimStatus = reader["Claim_Status"].ToString()

                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching approved claims: " + ex.Message);
            }

            return approvedClaims;
        }



        // New method for MarkAsProcessed view, includes PaymentStatus
        public List<Claims> GetApprovedClaimsWithPaymentStatus()
        {
            List<Claims> approvedClaims = new List<Claims>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();
                    string query = @"SELECT Claim_Id, EmpoyeeNum, Lecture_Name, Module_Name, Number_ofSessions, 
                             Hourly_rate, Total_Amount, Claim_Start_Date, Claim_End_Date, Claim_Status, PaymentStatus
                             FROM Claims
                             WHERE Claim_Status = 'Approved'";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                approvedClaims.Add(new Claims
                                {
                                    Claim_Id = Convert.ToInt32(reader["Claim_Id"]),
                                    employeenum = Convert.ToInt32(reader["EmpoyeeNum"]),
                                    name = reader["Lecture_Name"].ToString(),
                                    module_name = reader["Module_Name"].ToString(),
                                    number_of_sssions = Convert.ToInt32(reader["Number_ofSessions"]),
                                    hourly_rate = Convert.ToDouble(reader["Hourly_rate"]),
                                    TotalAmount = Convert.ToDouble(reader["Total_Amount"]),
                                    startdate = Convert.ToDateTime(reader["Claim_Start_Date"]).ToString("yyyy-MM-dd"),
                                    end_date = Convert.ToDateTime(reader["Claim_End_Date"]).ToString("yyyy-MM-dd"),
                                    ClaimStatus = reader["Claim_Status"].ToString(),
                                    PaymentStatus = reader["PaymentStatus"] == DBNull.Value ? "" : reader["PaymentStatus"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching approved claims with payment status: " + ex.Message);
            }
            return approvedClaims;
        }


        // a method that updates the payment status

        public bool UpdatePaymentStatus(int claimId, string status)
        {
            bool updated = false;
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();
                    string query = "UPDATE Claims SET PaymentStatus = @status WHERE Claim_Id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@id", claimId);

                        int rows = cmd.ExecuteNonQuery();
                        updated = rows > 0; // If rows > 0, the update was successful
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating payment status: " + ex.Message);
            }
            return updated;
        }


        public List<Claims> GetProcessedClaimsByLecturer(string empNum)
        {
            List<Claims> claims = new List<Claims>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection))
                {
                    connect.Open();
                    string query = @"SELECT Claim_Id, EmpoyeeNum, Lecture_Name, Module_Name, 
                    Number_ofSessions, Hourly_rate, Total_Amount, Claim_Start_Date, Claim_End_Date
                    FROM Claims
                    WHERE EmpoyeeNum = @empNum AND PaymentStatus = 'Processed'";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@empNum", empNum);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                claims.Add(new Claims
                                {
                                    Claim_Id = Convert.ToInt32(reader["Claim_Id"]),
                                    employeenum = Convert.ToInt32(reader["EmpoyeeNum"]),
                                    name = reader["Lecture_Name"].ToString(),
                                    module_name = reader["Module_Name"].ToString(),
                                    number_of_sssions = Convert.ToInt32(reader["Number_ofSessions"]),
                                    hourly_rate = Convert.ToDouble(reader["Hourly_rate"]),
                                    TotalAmount = Convert.ToDouble(reader["Total_Amount"]),
                                    startdate = Convert.ToDateTime(reader["Claim_Start_Date"]).ToString("yyyy-MM-dd"),
                                    end_date = Convert.ToDateTime(reader["Claim_End_Date"]).ToString("yyyy-MM-dd")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching processed claims: " + ex.Message);
            }
            return claims;
        }

    }
}
