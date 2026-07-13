DECLARE @StatusEnabled UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MonitorStatuses WHERE Name = 'Enabled');
DECLARE @StatusDisabled UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MonitorStatuses WHERE Name = 'Disabled');
DECLARE @StatusError UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MonitorStatuses WHERE Name = 'Error');

DECLARE @MethodGet UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM HttpMethods WHERE Name = 'GET');
DECLARE @MethodPost UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM HttpMethods WHERE Name = 'POST');
DECLARE @MethodPut UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM HttpMethods WHERE Name = 'PUT');

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Billing Service API')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('Billing Service API', 'https://api.pulse.dev/billing/v1/health', @MethodGet, '$.status', '99.98%', DATEADD(minute, -2, SYSDATETIMEOFFSET()), @StatusEnabled, 60, 10);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'User Authentication Server')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('User Authentication Server', 'https://auth.pulse.dev/oauth2/token', @MethodPost, '$.active_sessions', '342', DATEADD(second, -45, SYSDATETIMEOFFSET()), @StatusEnabled, 60, 5);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Stripe Payment Gateway')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('Stripe Payment Gateway', 'https://api.stripe.com/v3/charges', @MethodGet, '$.total_processed', '$67,420', DATEADD(minute, -15, SYSDATETIMEOFFSET()), @StatusEnabled, 900, 15);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Database Backup Worker')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('Database Backup Worker', 'https://backup.pulse.dev/status', @MethodGet, '$.completed_count', '14', DATEADD(hour, -4, SYSDATETIMEOFFSET()), @StatusEnabled, 1800, 30);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Email Dispatcher Service')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('Email Dispatcher Service', 'https://mail.pulse.dev/queue/length', @MethodGet, '$.queue_size', '0', DATEADD(minute, -8, SYSDATETIMEOFFSET()), @StatusDisabled, 300, 10);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Legacy Inventory Sync API')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('Legacy Inventory Sync API', 'https://legacy.inventory.pulse.dev/sync', @MethodPut, '$.status', NULL, DATEADD(minute, -1, SYSDATETIMEOFFSET()), @StatusError, 60, 5);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'Notification Dispatch Gateway')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('Notification Dispatch Gateway', 'https://notify.pulse.dev/webhook', @MethodPost, '$.delivery_rate', '98.5%', DATEADD(minute, -10, SYSDATETIMEOFFSET()), @StatusEnabled, 600, 15);
END;

IF NOT EXISTS (SELECT 1 FROM Monitors WHERE Name = 'External Status Page API')
BEGIN
    INSERT INTO Monitors (Name, Url, HttpMethod, ResultPath, CurrentValue, LastCheckedAt, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
    VALUES ('External Status Page API', 'https://status.thirdparty.com/api', @MethodGet, '$.uptime', NULL, DATEADD(minute, -25, SYSDATETIMEOFFSET()), @StatusError, 900, 20);
END;
