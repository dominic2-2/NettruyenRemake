using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class Comic
{
    public int ComicId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Author { get; set; }

    public int StatusId { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

    public virtual ICollection<ComicComment> ComicComments { get; set; } = new List<ComicComment>();

    public virtual ComicStat? ComicStat { get; set; }

    public virtual ICollection<Follow> Follows { get; set; } = new List<Follow>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();

    public virtual ComicStatus Status { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
