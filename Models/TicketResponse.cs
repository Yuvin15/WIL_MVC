using System;
using System.Collections.Generic;

namespace WIL_Project.Models;

public partial class TicketResponse
{
    public int ResponseId { get; set; }

    public string? ResponseSubject { get; set; }

    public string? ResponseBody { get; set; }

    public int? TicketId { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
