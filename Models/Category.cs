using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Comic> Comics { get; set; } = new List<Comic>();
}
