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


        private static void SendResponse(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("/*Replace this with your name*/", "/*Replace this with email*/"));
            message.To.Add(new MailboxAddress("/*Replace this with email*/", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("/*Replace this with email*/", "/*Replace this with App password*/");

                client.Send(message);
                client.Disconnect(true);
            }
        }
        [HttpPost]
        public IActionResult SendEmailReply([FromBody] EmailRequest request)
        {
            SendResponse(request.Email, request.Subject, request.Body);
            return Ok(new { Message = "Email sent!" });
        }
        public IActionResult YourTickets()
        {
            return View(); 
        }

        [HttpGet]
        public IActionResult CreateTicket()
        {
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
                    TicketCreationDate = DateTime.UtcNow,
                    TicketStatus = "Open"
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