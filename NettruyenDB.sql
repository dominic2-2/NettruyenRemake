USE master;
GO

CREATE DATABASE NettruyenDB;
GO

USE NettruyenDB;
GO

CREATE TABLE roles (
    role_id INT IDENTITY(1,1) PRIMARY KEY,
    role_name NVARCHAR(50) NOT NULL UNIQUE
);
INSERT INTO roles (role_name) VALUES ('user'), ('admin');

CREATE TABLE users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    email NVARCHAR(100) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    avatar VARBINARY(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    role_id INT NOT NULL DEFAULT 1,
    FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

CREATE TABLE comic_status (
    status_id INT IDENTITY(1,1) PRIMARY KEY,
    status_name NVARCHAR(50) NOT NULL UNIQUE
);
INSERT INTO comic_status (status_name) VALUES ('ongoing'), ('completed');

CREATE TABLE comics (
    comic_id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    author NVARCHAR(100),
    status_id INT NOT NULL DEFAULT 1,
    thumbnail_url NVARCHAR(255),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (status_id) REFERENCES comic_status(status_id)
);

CREATE TABLE comic_stats (
    comic_id INT PRIMARY KEY,
    view_count INT DEFAULT 0,
    follow_count INT DEFAULT 0,
    comment_count INT DEFAULT 0,
    rating_count INT DEFAULT 0,
    last_updated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (comic_id) REFERENCES comics(comic_id) ON DELETE CASCADE
);
GO

CREATE TRIGGER trg_InsertComicStats
ON comics
AFTER INSERT
AS
BEGIN
    INSERT INTO comic_stats (comic_id)
    SELECT comic_id FROM inserted;
END;
GO

CREATE TABLE follows (
    follow_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    comic_id INT NOT NULL,
    followed_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (comic_id) REFERENCES comics(comic_id) ON DELETE CASCADE
);

CREATE TABLE chapters (
    chapter_id INT IDENTITY(1,1) PRIMARY KEY,
    comic_id INT NOT NULL,
    chapter_number INT NOT NULL,
    title NVARCHAR(255),
    content NVARCHAR(MAX), -- Luu nhieu url anh duoi dang JSON (scraping)
    backup_content NVARCHAR(MAX), -- Nhu tren (cloud)
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (comic_id) REFERENCES comics(comic_id) ON DELETE CASCADE
);

CREATE TABLE categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE comic_category (
    comic_id INT NOT NULL,
    category_id INT NOT NULL,
    PRIMARY KEY (comic_id, category_id),
    FOREIGN KEY (comic_id) REFERENCES comics(comic_id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES categories(category_id) ON DELETE CASCADE
);

CREATE TABLE comic_comments (
    comment_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    comic_id INT NOT NULL,
    content NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (comic_id) REFERENCES comics(comic_id) ON DELETE CASCADE
);

CREATE TABLE chapter_comments (
    comment_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    chapter_id INT NOT NULL,
    content NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (chapter_id) REFERENCES chapters(chapter_id) ON DELETE CASCADE
);

CREATE TABLE ratings (
    rating_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    comic_id INT NOT NULL,
    rating_value INT CHECK (rating_value BETWEEN 1 AND 5),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (comic_id) REFERENCES comics(comic_id) ON DELETE CASCADE
);

CREATE TABLE reading_history (
    history_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    comic_id INT NOT NULL,
    chapter_id INT NOT NULL,
    last_read_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (comic_id) REFERENCES comics(comic_id) ON DELETE NO ACTION,
    FOREIGN KEY (chapter_id) REFERENCES chapters(chapter_id) ON DELETE CASCADE
);

