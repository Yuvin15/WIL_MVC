using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WIL_Project.Models;

namespace WIL_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CobraContext _context;
        public HomeController(ILogger<HomeController> logger, CobraContext context)
        {
            _logger = logger;
            _context = context;
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
            List<Ticket> tickets = GetTicketsFromDatabase(); // Assuming you have a method to fetch data from the database
            return View(tickets);
        }
        private List<Ticket> GetTicketsFromDatabase()
        {
            // Implement logic to fetch data from your database here
            // Example:
            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" },
                new Ticket { TicketId = 1, TicketSubject = "Sample Subject 1", TicketCreationDate = DateTime.Now, TicketStatus = "Open" },
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" }
            };
            return tickets;
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
        public IActionResult CreateTicket(string subject, string description)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("", "Subject and description are required.");
                return View(); // Return to the form view with an error message
            }

            // Assuming you have a 'Ticket' model with 'Subject' and 'Description' properties
            var ticket = new Ticket
            {
                TicketSubject = subject,
                TicketBody = description,
                TicketCreationDate = DateTime.Now // Assuming you have a DateCreated property
                                           // Add other properties as necessary
            };

            _context.Tickets.Add(ticket);  // Assuming your DbSet name is 'Tickets'
            _context.SaveChanges();

            return RedirectToAction("Success"); // Redirect to a 'Success' view or any other action after successful submission
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}