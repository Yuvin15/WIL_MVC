using System;
using System.Collections.Generic;

namespace WIL_Project.Models;

public partial class SupportTeamMember
{
    public string SupportMemberId { get; set; } = null!;

    public string? SupportMemberEmail { get; set; }

    public string? SupportMemberName { get; set; }
}
