﻿using Microsoft.AspNetCore.Mvc;
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
using System.Security.Claims;

namespace WIL_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        CobraContext _obraContext = new CobraContext();

        Ticket newTicket = new Ticket();
        TicketAttachment newAttachment = new TicketAttachment();
        TicketResponse newResponse = new TicketResponse();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index() { return View(); }

        public IActionResult Privacy() { return View(); }
        private void SeedTickets()
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
        }

        public IActionResult AllTickets()
        {
            /*SeedTickets();*/
            /*List<Ticket> tickets = GetTicketsFromDatabase();*/
            List<Ticket> tickets = _obraContext.Tickets.ToList();
            return View(tickets);
        }

        private List<Ticket> GetTicketsFromDatabase() 
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
                      .Include(t => t.UserTicket)
                      .Include(t => t.TicketAttachments)
                      .Include(t => t.TicketResponses)
                      .ToList();
        }

        private static void SendResponse(string toEmail, string subject, string body, string displayName)
        {
            
            using (var message = new MimeMessage())
            {

                message.From.Add(new MailboxAddress("Staff", displayName));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(displayName, "hggv lrox zewq lyot");

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
        }
        [HttpPost]
        public IActionResult SendEmailReply([FromBody] EmailReplyModel request, int ticketId)
        {
            try
            {
                string displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
                
                SendResponse(request.Email, request.Subject, request.Body, displayName);

                var newResponse = new TicketResponse
                {
                    ResponseSubject = request.Subject,
                    ResponseBody = request.Body,
                    TicketId = ticketId
                };
                _obraContext.TicketResponses.Add(newResponse);
                _obraContext.SaveChanges();
                return Ok(new { Message = "Email sent!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Error: {ex.Message}" });
            }
        }
        

        public IActionResult YourTickets()
        {
            try
            {
                string displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
                List<Ticket> yourTickets = _obraContext.Tickets
                                               .Where(t => t.UserTicket == displayName)
                                               .ToList();

                return View(yourTickets);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return View();
            }

            /*try
            {
                // Fetch all users from the database
                var users = _obraContext.Tickets.ToList();

                // Check if there are any users
                if (!users.Any())
                {
                    Console.WriteLine("No users found in the database.");
                    
                }

                // Display each user's details in the console
                foreach (var user in users)
                {
                    Console.WriteLine($"UserID: {user.UserTicket}, Email: {user.TicketId}, Full Name: {user.TicketStatus ?? "N/A"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }*/
        }

        [HttpGet]
        public IActionResult CreateTicket()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTicket(List<IFormFile> attachments, string subject, string body)
        {
            string displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;

            try
            {
                var newTicket = new Ticket
                {
                    TicketSubject = subject,
                    TicketBody = body,
                    TicketCreationDate = DateTime.UtcNow,
                    TicketStatus = "Open",
                    UserTicket = displayName
                };
                /*_logger.LogInformation(_obraContext.Model.ToDebugString());*/
                Console.WriteLine(newTicket.TicketCreationDate);
                _obraContext.Tickets.Add(newTicket);
                _obraContext.SaveChanges();

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
                    _obraContext.SaveChanges();
                }

                return RedirectToAction("YourTickets", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Main Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
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