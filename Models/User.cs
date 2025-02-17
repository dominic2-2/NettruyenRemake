using System;
using System.Collections.Generic;

namespace NettruyenRemake.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public byte[]? Avatar { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<ChapterComment> ChapterComments { get; set; } = new List<ChapterComment>();

    public virtual ICollection<ComicComment> ComicComments { get; set; } = new List<ComicComment>();

    public virtual ICollection<Follow> Follows { get; set; } = new List<Follow>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();

    public virtual Role Role { get; set; } = null!;
}
