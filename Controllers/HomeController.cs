using Microsoft.AspNetCore.Mvc;
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
                new Ticket { TicketId = 2, TicketSubject = "Sample Subject 2", TicketCreationDate = DateTime.Now, TicketStatus = "Closed" }
            };
            return tickets;
        }

        public IActionResult YourTickets()
        {
            return View(); 
        }

        public IActionResult CreateTicket()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}