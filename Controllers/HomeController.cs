using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mail;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using WIL_Project.Models;
using Microsoft.Extensions.Logging;  // ensure this is imported
using System.IO;   // for MemoryStream
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Data.SqlClient;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace WIL_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        CobraContext _obraContext = new CobraContext();

        Ticket newTicket = new Ticket();
        TicketAttachment newAttachment = new TicketAttachment();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index(){ return View(); }

        public IActionResult Privacy(){ return View(); }
        /*private void SeedTickets()
        {
            if (!_obraContext.Tickets.Any())
            {
                var tickets = new List<Ticket>
                {
                    new Ticket { TicketSubject = "Sample Subject 1", TicketBody = "Testing for now", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                };

                _obraContext.Tickets.AddRange(tickets);
                _obraContext.SaveChanges();
            }
        }*/

        public IActionResult AllTickets()
        {
            /*SeedTickets();*/

            List<Ticket> tickets = GetTicketsFromDatabase();
            return View(tickets);
        }

        private List<Ticket> GetTicketsFromDatabase() // Change this to getting stuff from db
        {
            /*List<Ticket> tickets = new List<Ticket>
            {
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 3, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 4, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 5, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 6, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 7, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 8, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 9, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 10, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 11, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 12, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 13, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 14, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 15, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 16, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 17, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" }
            };*/

            return _obraContext.Tickets
                      .Include(t => t.User)              
                      .Include(t => t.TicketAttachments) 
                      .Include(t => t.TicketResponses)   
                      .ToList();
        }

        private static async Task<string> GetAccessToken(string clientId, string clientSecret, string redirectUri, string code)
        {
            using (var httpClient = new HttpClient())
            {
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/common/oauth2/v2.0/token");

                tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["scope"] = "https://graph.microsoft.com/.default",
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri,
                    ["grant_type"] = "authorization_code",
                    ["client_secret"] = clientSecret
                });

                var tokenResponse = await httpClient.SendAsync(tokenRequest);
                var tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();

                var tokenData = JsonConvert.DeserializeObject<dynamic>(tokenResponseBody);
                return tokenData.access_token;
            }
        }


        private async Task SendResponse(string toEmail, string subject, string body, string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var email = new
                {
                    message = new
                    {
                        subject = subject,
                        body = new
                        {
                            contentType = "Text",
                            content = body
                        },
                            toRecipients = new[]
                            {
                                new
                            {
                                emailAddress = new
                            {
                                address = toEmail
                            }
                        }
                  }
            }
        };
                var serializedEmail = JsonConvert.SerializeObject(email);
                var content = new StringContent(serializedEmail, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://graph.microsoft.com/v1.0/me/sendMail", content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to send email: {response.StatusCode}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailReply([FromBody] EmailReplyModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Subject) || string.IsNullOrEmpty(model.Body))
            {
                return BadRequest("Invalid email details provided.");
            }

            // Assuming you have stored your access token somewhere after the OAuth flow
            /*string accessToken = await GetAccessToken("eeda81d9-5269-402b-b580-a6575b95258c", "3qc8Q~HUp2EDiBcWomq5HkrPg6ccx53KMDG4Cbcm", redirectUri, code); // Implement the GetAccessToken method to fetch the token
*/
            /*await SendResponse(model.Email, model.Subject, model.Body, accessToken);
*/
            return Ok(new { message = "Email sent successfully!" });
        }

        public IActionResult YourTickets()
        {
            return View(); 
        }
        
        [HttpGet]
        public IActionResult CreateTicket()
        {
            /*try
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = @"Server = tcp:wil.database.windows.net,1433; Initial Catalog = Cobra; Persist Security Info = False; User ID = admin2; Password = Cobra123; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30";
                con.Open();
                string query = "ALTER TABLE Ticket ALTER COLUMN TicketCreationDate DATETIME;";
                //SQL command to input into a database
                SqlDataAdapter da = new SqlDataAdapter(query, con);                
                da.SelectCommand.ExecuteNonQuery();
                Console.WriteLine(da.ToString());
                con.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }*/

            return View();
        }

        [HttpPost]
        public IActionResult CreateTicket(List<IFormFile> attachments, string subject, string body)
        {
            try
            {
                var newTicket = new Ticket
                {
                    TicketSubject = subject,
                    TicketBody = body,
                    TicketCreationDate = DateTime.Now,
                    TicketStatus = "Open"
                };
                
                /*_logger.LogInformation(_obraContext.Model.ToDebugString());*/
                /*Console.WriteLine(newTicket.TicketCreationDate);*/
                
                if (attachments != null && attachments.Any())
                {
                    if (attachments.Count > 0)
                    {
                        newTicket.TicketAttatchment1 = GetBytesFromFormFile(attachments[0]);
                    }
                    if (attachments.Count > 1)
                    {
                        newTicket.TicketAttatchment2 = GetBytesFromFormFile(attachments[1]);
                    }
                    if (attachments.Count > 2)
                    {
                        newTicket.TicketAttatchment3 = GetBytesFromFormFile(attachments[2]);
                    }

                    _obraContext.Tickets.Add(newTicket);
                    _obraContext.SaveChanges();
                }

                return RedirectToAction("YourTickets", "Home");
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        private byte[] GetBytesFromFormFile(IFormFile formFile)
        {
            using var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public IActionResult GetAttachmentImage(int ticketId, int attachmentNumber)
        {
            var ticket = _obraContext.Tickets.Find(ticketId);
            byte[] fileBytes = null;

            switch (attachmentNumber)
            {
                case 1: fileBytes = ticket.TicketAttatchment1; break;
                case 2: fileBytes = ticket.TicketAttatchment2; break;
                case 3: fileBytes = ticket.TicketAttatchment3; break;
            }

            if (fileBytes != null)
            {
                return File(fileBytes, "image/jpeg");
            }
            return NotFound();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}