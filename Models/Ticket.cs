using System;
using System.Collections.Generic;

namespace WIL_Project.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string? TicketSubject { get; set; }

    public string? TicketBody { get; set; }

    public DateTime? TicketCreationDate { get; set; }

    public string? TicketStatus { get; set; }

    public int? TicketAttachmentsId { get; set; }

    public int? UserId { get; set; }

    public virtual TicketAttachment? TicketAttachments { get; set; }

    public virtual ICollection<TicketResponse> TicketResponses { get; set; } = new List<TicketResponse>();

    public virtual User? User { get; set; }
}
