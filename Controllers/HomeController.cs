using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using WIL_Project.Models;

namespace WIL_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

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
            return View();
        }

        public IActionResult YourTickets()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTicket(IFormFile attachment)
        {
            using (var dbContext = new CobraContext())
            {
                Ticket newTicket = new Ticket
                {
                    TicketSubject = Request.Form["TicketSubject"],
                    TicketBody = Request.Form["TicketBody"],
                    TicketCreationDate = DateTime.Now
                };

                if (attachment != null && attachment.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        attachment.CopyTo(memoryStream);

                        TicketAttachment ticketAttachment = new TicketAttachment
                        {
                            Attachments1 = memoryStream.ToArray()
                        };

                    }
                }

                dbContext.Tickets.Add(newTicket);
                dbContext.SaveChanges();
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}