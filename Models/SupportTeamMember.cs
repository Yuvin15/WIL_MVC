using System;
using System.Collections.Generic;

namespace WIL_Project.Models;

public partial class SupportTeamMember
{
    public int SupportMemberId { get; set; }

    public string? SupportMemberEmail { get; set; }

    public string? SupportMemberName { get; set; }
}
