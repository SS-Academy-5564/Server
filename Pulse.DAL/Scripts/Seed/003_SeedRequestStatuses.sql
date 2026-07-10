INSERT INTO dbo.RequestStatus (Status)
SELECT v.Status
FROM (VALUES
    ('Success'),
    ('Failed'),
    ('Timeout'),
    ('NetworkError'),
    ('ExtractionError'),
    ('UnexpectedError')
) AS v(Status)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RequestStatus rs WHERE rs.Status = v.Status
);
