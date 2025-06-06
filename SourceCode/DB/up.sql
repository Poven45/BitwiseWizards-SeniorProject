

CREATE TABLE Users (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Profile_Name NVARCHAR(50) NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    ProfilePicture VARBINARY(MAX),
    Bio NVARCHAR(500),
    Is_Admin BIT DEFAULT 0,
    Is_Verified BIT DEFAULT 0,
    EncryptedAPIKey VARBINARY(MAX) NULL 
);

CREATE TABLE Followers (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FollowerUserID INT NOT NULL,
    FollowingUserID INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Stock (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TickerSymbol VARCHAR(10) NOT NULL UNIQUE,
    StockPrice DECIMAL(13,2) NOT NULL,
    LastUpdated DATETIME DEFAULT GETDATE()
);

CREATE TABLE Trade (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    TickerSymbol VARCHAR(10) NOT NULL,
    TradeType VARCHAR(20) NOT NULL,  
    Quantity DECIMAL(10,2) NOT NULL,
    EntryPrice DECIMAL(13,2) NOT NULL,
    CurrentPrice DECIMAL(13,2) NOT NULL,
    LastUpdated DATETIME DEFAULT GETDATE()
);

CREATE TABLE Posts (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Title NVARCHAR(100),
    Content NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    Tag NVARCHAR(50),  /* Remove this column */
    PrivacySetting NVARCHAR(20) DEFAULT 'Public'
);

CREATE TABLE Comments (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL,
    UserID INT NOT NULL,
    Content NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Likes (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    PostID INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Tags (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TagName NVARCHAR(100) UNIQUE
);

CREATE TABLE PostTags (
    PostID INT,
    TagID INT
    PRIMARY KEY (PostID, TagID)
);



ALTER TABLE Followers 
ADD CONSTRAINT FK_Followers_Follower FOREIGN KEY (FollowerUserID) REFERENCES Users(ID) ON DELETE CASCADE,
    CONSTRAINT FK_Followers_Following FOREIGN KEY (FollowingUserID) REFERENCES Users(ID) ON DELETE NO ACTION;

ALTER TABLE Trade 
ADD CONSTRAINT FK_Trade_User FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE,
    CONSTRAINT FK_Trade_Stock FOREIGN KEY (TickerSymbol) REFERENCES Stock(TickerSymbol);

ALTER TABLE Posts 
ADD CONSTRAINT FK_Posts_User FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE;

ALTER TABLE Comments 
ADD CONSTRAINT FK_Comments_Post FOREIGN KEY (PostID) REFERENCES Posts(ID) ON DELETE CASCADE, 
    CONSTRAINT FK_Comments_User FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE NO ACTION;

ALTER TABLE Likes 
ADD CONSTRAINT FK_Likes_User FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE,
    CONSTRAINT FK_Likes_Post FOREIGN KEY (PostID) REFERENCES Posts(ID) ON DELETE NO ACTION;

ALTER TABLE PostTags
ADD CONSTRAINT FK_PostTags_Post FOREIGN KEY (PostID) REFERENCES Posts(ID) ON DELETE CASCADE,
    CONSTRAINT FK_PostTags_Tag FOREIGN KEY (TagID) REFERENCES Tags(ID) ON DELETE CASCADE;

