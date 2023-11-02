using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mail;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using WIL_Project.Models;
using Microsoft.Extensions.Logging;  
using System.IO;   
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Data;

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
        
        /*[Authorize(Roles = "Staff")]*/
        public IActionResult AllTickets()
        {
            if (!User.IsInRole("Staff"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            // This displays all tickets in the db
            List<Ticket> tickets = _obraContext.Tickets.ToList();
            return View(tickets);
        }
        // THe below allows the Staff member send the email to the specfic ticket 
        private static void SendResponse(string toEmail, string subject, string body, string displayName)
        {
            Console.WriteLine($"{toEmail} {displayName}"); // Testing do not take out!!
            using (var message = new MimeMessage())
            {
                message.From.Add(new MailboxAddress("Staff", displayName));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain")
                {
                    Text = body
                };
                // Make use of the Smtp Client to send the email using the above details
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(displayName, ""); // This will not work after presentation
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
        }
        // This gets the data needed to send the email from above from the javascript/text areas in the view
        [HttpPost]
        public IActionResult SendEmailReply([FromBody] EmailReplyModel request)
        {
            int ticketId = request.TicketId;
            try
            {
                string displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;

                SendResponse(request.Email, request.Subject, request.Body, displayName);
                Console.WriteLine("Ticket ID:", ticketId); // Testing!!!!
                var ticket = _obraContext.Tickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket == null)
                {
                    return NotFound(new { Message = $"Ticket with ID {ticketId} not found." });
                }
                 // Closes Ticket after the dtaff response to it
                ticket.TicketStatus = "Closed";

                var newResponse = new TicketResponse
                {
                    ResponseSubject = request.Subject,
                    ResponseBody = request.Body,
                    TicketId = ticketId
                };
                _obraContext.TicketResponses.Add(newResponse); // Saves to DB
                _obraContext.SaveChanges();

                return Ok(new { Message = "Email sent!" }); // Returns this to the view if successful
            } // Below is to test the errors we got.
            catch (MailKit.Net.Smtp.SmtpCommandException ex)
            {
                return BadRequest(new { Message = $"SMTP Error: {ex.Message}", StatusCode = ex.StatusCode, Response = ex.Source });
            }
            catch (MailKit.Net.Smtp.SmtpProtocolException ex)
            {
                return BadRequest(new { Message = $"SMTP Protocol Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"General Error: {ex.Message}" });
            }
        }

/*
        [Authorize(Roles = "Student")]*/
        public IActionResult YourTickets()
        {
            if (!User.IsInRole("Student"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            try
            {
                string displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
                // Gets the list to display in view of students tickets logged in
                List<Ticket> yourTickets = _obraContext.Tickets
                                               .Where(t => t.UserTicket == displayName)
                                               .ToList();
                // Returns the data to the view
                return View(yourTickets);
            }
            catch (Exception ex) // Error handling for testing purposes
            {
                Console.WriteLine(ex.Message);

                return View();
            }

        }

        [HttpGet]
        /*[Authorize(Roles = "Student")]*/
        public IActionResult CreateTicket()
        {
            if (!User.IsInRole("Student"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateTicket(List<IFormFile> attachments, string subject, string body)
        {
            string displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
            // Below gets the details to put to the database and then saves it
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
                
                _obraContext.Tickets.Add(newTicket);
                _obraContext.SaveChanges();
                // Does not work!!! :(
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
                // After creation return to this view
                return RedirectToAction("YourTickets", "Home");
            }
            catch (Exception ex) // Exception for testing purposes
            {
                Console.WriteLine($"Main Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return View();
            }
        }
        // This should get the byte from the image/attachment
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

        public IActionResult AccessDenied() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
