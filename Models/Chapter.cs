using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class Chapter
{
    public int ChapterId { get; set; }

    public int ComicId { get; set; }

    public int ChapterNumber { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? BackupContent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ChapterComment> ChapterComments { get; set; } = new List<ChapterComment>();

    public virtual Comic Comic { get; set; } = null!;

    public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
}
