using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class ReadingHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public int ComicId { get; set; }

    public int ChapterId { get; set; }

    public DateTime? LastReadAt { get; set; }

    public virtual Chapter Chapter { get; set; } = null!;

    public virtual Comic Comic { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
