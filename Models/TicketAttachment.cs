using System;
using System.Collections.Generic;

namespace WIL_Project.Models;

public partial class TicketAttachment
{
    public int TicketAttachmentsId { get; set; }

    public string? Attachments1 { get; set; }

    public string? Attachments2 { get; set; }

    public string? Attachments3 { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
