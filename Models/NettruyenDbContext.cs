using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NettruyenRemake.Models;

public partial class NettruyenDbContext : DbContext
{
    public NettruyenDbContext()
    {
    }

    public NettruyenDbContext(DbContextOptions<NettruyenDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Chapter> Chapters { get; set; }

    public virtual DbSet<ChapterComment> ChapterComments { get; set; }

    public virtual DbSet<Comic> Comics { get; set; }

    public virtual DbSet<ComicComment> ComicComments { get; set; }

    public virtual DbSet<ComicStat> ComicStats { get; set; }

    public virtual DbSet<ComicStatus> ComicStatuses { get; set; }

    public virtual DbSet<Follow> Follows { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<ReadingHistory> ReadingHistories { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DBConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__categori__D54EE9B421EFB9E4");

            entity.ToTable("categories");

            entity.HasIndex(e => e.CategoryName, "UQ__categori__5189E25597502E73").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
        });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(e => e.ChapterId).HasName("PK__chapters__745EFE876D8C779E");

            entity.ToTable("chapters");

            entity.Property(e => e.ChapterId).HasColumnName("chapter_id");
            entity.Property(e => e.BackupContent).HasColumnName("backup_content");
            entity.Property(e => e.ChapterNumber).HasColumnName("chapter_number");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Comic).WithMany(p => p.Chapters)
                .HasForeignKey(d => d.ComicId)
                .HasConstraintName("FK__chapters__comic___5812160E");
        });

        modelBuilder.Entity<ChapterComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__chapter___E7957687BD252489");

            entity.ToTable("chapter_comments");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.ChapterId).HasColumnName("chapter_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Chapter).WithMany(p => p.ChapterComments)
                .HasForeignKey(d => d.ChapterId)
                .HasConstraintName("FK__chapter_c__chapt__68487DD7");

            entity.HasOne(d => d.User).WithMany(p => p.ChapterComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__chapter_c__user___6754599E");
        });

        modelBuilder.Entity<Comic>(entity =>
        {
            entity.HasKey(e => e.ComicId).HasName("PK__comics__ABDBFE60FF467A05");

            entity.ToTable("comics", tb => tb.HasTrigger("trg_InsertComicStats"));

            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.Author)
                .HasMaxLength(100)
                .HasColumnName("author");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StatusId)
                .HasDefaultValue(1)
                .HasColumnName("status_id");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(255)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Status).WithMany(p => p.Comics)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__comics__status_i__46E78A0C");

            entity.HasMany(d => d.Categories).WithMany(p => p.Comics)
                .UsingEntity<Dictionary<string, object>>(
                    "ComicCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK__comic_cat__categ__5EBF139D"),
                    l => l.HasOne<Comic>().WithMany()
                        .HasForeignKey("ComicId")
                        .HasConstraintName("FK__comic_cat__comic__5DCAEF64"),
                    j =>
                    {
                        j.HasKey("ComicId", "CategoryId").HasName("PK__comic_ca__F68F10FB4EEDA496");
                        j.ToTable("comic_category");
                        j.IndexerProperty<int>("ComicId").HasColumnName("comic_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });
        });

        modelBuilder.Entity<ComicComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__comic_co__E7957687CD8E560C");

            entity.ToTable("comic_comments");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Comic).WithMany(p => p.ComicComments)
                .HasForeignKey(d => d.ComicId)
                .HasConstraintName("FK__comic_com__comic__6383C8BA");

            entity.HasOne(d => d.User).WithMany(p => p.ComicComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__comic_com__user___628FA481");
        });

        modelBuilder.Entity<ComicStat>(entity =>
        {
            entity.HasKey(e => e.ComicId).HasName("PK__comic_st__ABDBFE604578B1F1");

            entity.ToTable("comic_stats");

            entity.Property(e => e.ComicId)
                .ValueGeneratedNever()
                .HasColumnName("comic_id");
            entity.Property(e => e.CommentCount)
                .HasDefaultValue(0)
                .HasColumnName("comment_count");
            entity.Property(e => e.FollowCount)
                .HasDefaultValue(0)
                .HasColumnName("follow_count");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated");
            entity.Property(e => e.RatingCount)
                .HasDefaultValue(0)
                .HasColumnName("rating_count");
            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0)
                .HasColumnName("view_count");

            entity.HasOne(d => d.Comic).WithOne(p => p.ComicStat)
                .HasForeignKey<ComicStat>(d => d.ComicId)
                .HasConstraintName("FK__comic_sta__comic__4E88ABD4");
        });

        modelBuilder.Entity<ComicStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__comic_st__3683B531FAC5C254");

            entity.ToTable("comic_status");

            entity.HasIndex(e => e.StatusName, "UQ__comic_st__501B3753407643E2").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => e.FollowId).HasName("PK__follows__15A691442D150A99");

            entity.ToTable("follows");

            entity.Property(e => e.FollowId).HasColumnName("follow_id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.FollowedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("followed_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Comic).WithMany(p => p.Follows)
                .HasForeignKey(d => d.ComicId)
                .HasConstraintName("FK__follows__comic_i__5441852A");

            entity.HasOne(d => d.User).WithMany(p => p.Follows)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__follows__user_id__534D60F1");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__ratings__D35B278B2B187778");

            entity.ToTable("ratings");

            entity.Property(e => e.RatingId).HasColumnName("rating_id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.RatingValue).HasColumnName("rating_value");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Comic).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.ComicId)
                .HasConstraintName("FK__ratings__comic_i__6E01572D");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ratings__user_id__6D0D32F4");
        });

        modelBuilder.Entity<ReadingHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__reading___096AA2E973137E58");

            entity.ToTable("reading_history");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.ChapterId).HasColumnName("chapter_id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.LastReadAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_read_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Chapter).WithMany(p => p.ReadingHistories)
                .HasForeignKey(d => d.ChapterId)
                .HasConstraintName("FK__reading_h__chapt__73BA3083");

            entity.HasOne(d => d.Comic).WithMany(p => p.ReadingHistories)
                .HasForeignKey(d => d.ComicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reading_h__comic__72C60C4A");

            entity.HasOne(d => d.User).WithMany(p => p.ReadingHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__reading_h__user___71D1E811");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CC7012BDD9");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__783254B12AB7A719").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370F0C60C439");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164200BB995").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC5728565DA79").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Avatar).HasColumnName("avatar");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId)
                .HasDefaultValue(1)
                .HasColumnName("role_id");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__role_id__3E52440B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
