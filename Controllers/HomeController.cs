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


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AllTickets()
        {
            List<Ticket> tickets = GetTicketsFromDatabase(); 
            return View(tickets);
        }
        private List<Ticket> GetTicketsFromDatabase() // Change this to getting stuff from db
        {
            // Implement logic to fetch data from your database here
            // Example:
            List<Ticket> tickets = new List<Ticket>
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
                new Ticket { TicketId = 14, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" }
            };
            return tickets;
        }

        private static void sendResponse()
        {
            // This is the email they send back when responding to the ticket
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
                    TicketCreationDate = DateTime.Now
                };
                _obraContext.Tickets.Add(newTicket);
                _obraContext.SaveChanges();

                if (attachments != null && attachments.Any())
                {
                    var newAttachment = new TicketAttachment();

                    if (attachments.Count > 0)
                    {
                        newAttachment.Attachments1 = GetBytesFromFormFile(attachments[0]);
                    }
                    if (attachments.Count > 1)
                    {
                        newAttachment.Attachments2 = GetBytesFromFormFile(attachments[1]);
                    }
                    if (attachments.Count > 2)
                    {
                        newAttachment.Attachments3 = GetBytesFromFormFile(attachments[2]);
                    }

                    _obraContext.TicketAttachments.Add(newAttachment);
                    _obraContext.SaveChanges();
                }

                TempData["SuccessMessage"] = "Ticket submitted successfully!";
                return RedirectToAction("YourTickets", "Home");

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error while submitting ticket. Please try again.";
                return View(); 
            }

        }


        private byte[] GetBytesFromFormFile(IFormFile formFile)
        {
            using var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}