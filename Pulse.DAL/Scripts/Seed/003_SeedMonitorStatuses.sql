IF NOT EXISTS (SELECT 1 FROM MonitorStatuses WHERE Name = 'Enabled')
BEGIN
    INSERT INTO MonitorStatuses (Name) VALUES ('Enabled');
END;

IF NOT EXISTS (SELECT 1 FROM MonitorStatuses WHERE Name = 'Disabled')
BEGIN
    INSERT INTO MonitorStatuses (Name) VALUES ('Disabled');
END;

IF NOT EXISTS (SELECT 1 FROM MonitorStatuses WHERE Name = 'Error')
BEGIN
    INSERT INTO MonitorStatuses (Name) VALUES ('Error');
END;
