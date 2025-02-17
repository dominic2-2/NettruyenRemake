using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class ComicStat
{
    public int ComicId { get; set; }

    public int? ViewCount { get; set; }

    public int? FollowCount { get; set; }

    public int? CommentCount { get; set; }

    public int? RatingCount { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Comic Comic { get; set; } = null!;
}
