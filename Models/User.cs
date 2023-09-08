using System;
using System.Collections.Generic;

namespace WIL_Project.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserEmail { get; set; }

    public string? UserFullName { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
