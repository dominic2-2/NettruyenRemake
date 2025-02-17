using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class ComicStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Comic> Comics { get; set; } = new List<Comic>();
}
