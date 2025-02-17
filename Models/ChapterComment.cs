using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class ChapterComment
{
    public int CommentId { get; set; }

    public int UserId { get; set; }

    public int ChapterId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Chapter Chapter { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
