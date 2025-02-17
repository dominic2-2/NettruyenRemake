using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class Rating
{
    public int RatingId { get; set; }

    public int UserId { get; set; }

    public int ComicId { get; set; }

    public int? RatingValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Comic Comic { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
