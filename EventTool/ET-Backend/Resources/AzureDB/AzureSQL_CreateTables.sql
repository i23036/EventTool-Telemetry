USE [db-eventtool];
GO

-- Organization
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Organizations' AND xtype='U')
BEGIN
    CREATE TABLE Organizations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Domain NVARCHAR(255) NOT NULL UNIQUE,
        Description NVARCHAR(MAX),
        OrgaPicAsBase64 NVARCHAR(MAX) NULL
    );
END
GO

-- Users
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Firstname NVARCHAR(255) NOT NULL,
        Lastname NVARCHAR(255) NOT NULL,
        Password NVARCHAR(255) NOT NULL
    );
END
GO

-- Accounts
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Accounts' AND xtype='U')
BEGIN
    CREATE TABLE Accounts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        IsVerified BIT DEFAULT 0,
        UserId INT NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
END
GO

-- Processes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Processes' AND xtype='U')
BEGIN
    CREATE TABLE Processes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        OrganizationId INT NOT NULL,
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
    );
END
GO

-- Triggers
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Triggers' AND xtype='U')
BEGIN
    CREATE TABLE Triggers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Attribut NVARCHAR(255) NOT NULL
    );
END
GO

-- ProcessSteps
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProcessSteps' AND xtype='U')
BEGIN
    CREATE TABLE ProcessSteps (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        OrganizationId INT NOT NULL,
        TriggerId INT,
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
        FOREIGN KEY (TriggerId) REFERENCES Triggers(Id)
    );
END
GO

-- Events
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Events' AND xtype='U')
BEGIN
    CREATE TABLE Events (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        OrganizationId INT NOT NULL,
        ProcessId INT NULL,
        StartDate DATE,
        EndDate DATE,
        StartTime TIME,
        EndTime TIME,
        Location NVARCHAR(255),
        MinParticipants INT,
        MaxParticipants INT,
        RegistrationStart DATE,
        RegistrationEnd DATE,
        IsBlueprint BIT DEFAULT 0,
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
        FOREIGN KEY (ProcessId) REFERENCES Processes(Id)
    );
END
GO

-- EventMembers
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EventMembers' AND xtype='U')
BEGIN
    CREATE TABLE EventMembers (
        AccountId INT NOT NULL,
        EventId INT NOT NULL,
        IsOrganizer BIT DEFAULT 0,
        IsContactPerson BIT DEFAULT 0,
        PRIMARY KEY (AccountId, EventId),
        FOREIGN KEY (AccountId) REFERENCES Accounts(Id),
        FOREIGN KEY (EventId) REFERENCES Events(Id)
    );
END
GO

-- OrganizationMembers
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrganizationMembers' AND xtype='U')
BEGIN
    CREATE TABLE OrganizationMembers (
        AccountId INT NOT NULL,
        OrganizationId INT NOT NULL,
        Role INT NOT NULL,
        PRIMARY KEY (AccountId, OrganizationId),
        FOREIGN KEY (AccountId) REFERENCES Accounts(Id),
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
    );
END
GO
