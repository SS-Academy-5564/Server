DECLARE @StatusEnabled UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MonitorStatuses WHERE Name = 'Enabled');
DECLARE @StatusDisabled UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MonitorStatuses WHERE Name = 'Disabled');
DECLARE @StatusError UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MonitorStatuses WHERE Name = 'Error');

DECLARE @MethodGet UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM HttpMethods WHERE Name = 'GET');
DECLARE @MethodPost UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM HttpMethods WHERE Name = 'POST');
DECLARE @MethodPut UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM HttpMethods WHERE Name = 'PUT');

DECLARE @DefaultOrgId UNIQUEIDENTIFIER = 'B1000000-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'User name' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('User name', 'https://dummyjson.com/users/1', @MethodGet, 'firstName', NULL, DATEADD(minute, -2, SYSDATETIMEOFFSET()), @StatusEnabled, 60, 10, @DefaultOrgId);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'User age' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('User age', 'https://dummyjson.com/users/2', @MethodGet, 'age', NULL, DATEADD(second, -45, SYSDATETIMEOFFSET()), @StatusEnabled, 60, 5, @DefaultOrgId);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Random TODO title' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('Random TODO title', 'https://dummyjson.com/todos/random', @MethodGet, 'todo', NULL, DATEADD(minute, -15, SYSDATETIMEOFFSET()), @StatusEnabled, 900, 15, @DefaultOrgId);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Database Backup Worker' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('Database Backup Worker', 'https://dummyjson.com/http/200', @MethodGet, 'status', '200', DATEADD(hour, -4, SYSDATETIMEOFFSET()), @StatusEnabled, 1800, 30, @DefaultOrgId);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Random TODO ID' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('Random TODO ID', 'https://dummyjson.com/todos/random', @MethodGet, 'id', NULL, DATEADD(minute, -2, SYSDATETIMEOFFSET()), @StatusEnabled, 60, 30, @DefaultOrgId);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Email Dispatcher Service' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('Email Dispatcher Service', 'https://mail.pulse.dev/queue/length', @MethodGet, 'queue_size', '0', DATEADD(minute, -8, SYSDATETIMEOFFSET()), @StatusDisabled, 300, 10, @DefaultOrgId);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Legacy Inventory Sync API' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('Legacy Inventory Sync API', 'https://legacy.inventory.pulse.dev/sync', @MethodPut, 'status', NULL, DATEADD(minute, -1, SYSDATETIMEOFFSET()), @StatusError, 60, 5, @DefaultOrgId);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'External Status Page API' AND OrganizationId = @DefaultOrgId)
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds, OrganizationId)
    VALUES ('External Status Page API', 'https://status.thirdparty.com/api', @MethodGet, 'uptime', NULL, DATEADD(minute, -25, SYSDATETIMEOFFSET()), @StatusError, 900, 20, @DefaultOrgId);
END;
