using System;
using System.Collections.Generic;

namespace WIL_Project.Models;

public partial class Admin
{
    public string AdminId { get; set; } = null!;

    public string? AdminEmail { get; set; }

    public string? AdminName { get; set; }
}
