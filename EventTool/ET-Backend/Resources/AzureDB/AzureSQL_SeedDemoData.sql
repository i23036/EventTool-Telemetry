USE [db-eventtool];
GO

-- Organisation einfügen
IF NOT EXISTS (SELECT 1 FROM Organizations WHERE Domain = 'demo.org')
BEGIN
    INSERT INTO Organizations (Name, Domain, Description)
    VALUES ('DemoOrg', 'demo.org', 'Dies ist eine Demo-Organisation');
END
GO

-- User einfügen
IF NOT EXISTS (SELECT 1 FROM Users WHERE Lastname = 'Mustermann')
BEGIN
    INSERT INTO Users (Firstname, Lastname, Password)
    VALUES ('Max', 'Mustermann', 'demo');
END
GO

-- Trigger einfügen
IF NOT EXISTS (SELECT 1 FROM Triggers WHERE Attribut = 'E-Mail bestätigt')
BEGIN
    INSERT INTO Triggers (Attribut)
    VALUES ('E-Mail bestätigt');
END
GO

-- Prozess einfügen
IF NOT EXISTS (SELECT 1 FROM Processes WHERE Name = 'Onboarding')
BEGIN
    INSERT INTO Processes (Name, OrganizationId)
    SELECT 'Onboarding', Id FROM Organizations WHERE Domain = 'demo.org';
END
GO

-- ProcessStep einfügen
IF NOT EXISTS (SELECT 1 FROM ProcessSteps WHERE Name = 'Willkommen')
BEGIN
    INSERT INTO ProcessSteps (Name, OrganizationId, TriggerId)
    SELECT 
        'Willkommen',
        o.Id,
        t.Id
    FROM Organizations o
    CROSS JOIN Triggers t
    WHERE o.Domain = 'demo.org' AND t.Attribut = 'E-Mail bestätigt';
END
GO

-- Account einfügen
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Email = 'admin@demo.org')
BEGIN
    INSERT INTO Accounts (Email, IsVerified, UserId)
    SELECT 
        'admin@demo.org',
        1,
        u.Id
    FROM Users u
    WHERE u.Lastname = 'Mustermann';
END
GO

-- Mitgliedschaft zur Organisation hinzufügen
IF NOT EXISTS (
    SELECT 1 
    FROM OrganizationMembers 
    WHERE AccountId = (SELECT Id FROM Accounts WHERE Email = 'admin@demo.org')
      AND OrganizationId = (SELECT Id FROM Organizations WHERE Domain = 'demo.org')
)
BEGIN
    INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role)
    SELECT 
        a.Id,
        o.Id,
        3 -- Admin
    FROM Accounts a
    CROSS JOIN Organizations o
    WHERE a.Email = 'admin@demo.org' AND o.Domain = 'demo.org';
END
GO

-- Event einfügen
IF NOT EXISTS (SELECT 1 FROM Events WHERE Name = 'Kickoff Meeting')
BEGIN
    INSERT INTO Events (
        Name, Description, OrganizationId, ProcessId,
        StartDate, EndDate, StartTime, EndTime, Location,
        MinParticipants, MaxParticipants,
        RegistrationStart, RegistrationEnd, IsBlueprint
    )
    SELECT 
        'Kickoff Meeting', 'Erstes Demo-Event',
        o.Id,
        p.Id,
        '2025-06-01', '2025-06-01',
        '10:00:00', '12:00:00',
        'Konferenzraum A',
        5, 20,
        '2025-05-15', '2025-05-31',
        0
    FROM Organizations o
    JOIN Processes p ON p.OrganizationId = o.Id AND p.Name = 'Onboarding'
    WHERE o.Domain = 'demo.org';
END
GO

-- EventMember hinzufügen
IF NOT EXISTS (
    SELECT 1
    FROM EventMembers
    WHERE AccountId = (SELECT Id FROM Accounts WHERE Email = 'admin@demo.org')
      AND EventId = (SELECT Id FROM Events WHERE Name = 'Kickoff Meeting')
)
BEGIN
    INSERT INTO EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
    SELECT 
        a.Id,
        e.Id,
        1,
        1
    FROM Accounts a
    CROSS JOIN Events e
    WHERE a.Email = 'admin@demo.org' AND e.Name = 'Kickoff Meeting';
END
GO
