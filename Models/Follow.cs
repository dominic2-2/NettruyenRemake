using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class Follow
{
    public int FollowId { get; set; }

    public int UserId { get; set; }

    public int ComicId { get; set; }

    public DateTime? FollowedAt { get; set; }

    public virtual Comic Comic { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
