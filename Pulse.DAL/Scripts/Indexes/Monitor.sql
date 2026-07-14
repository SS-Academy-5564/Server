CREATE NONCLUSTERED INDEX IDX_Monitors_StatusId_NextExecutionAt
    ON dbo.Monitors (StatusId, NextExecutionAt ASC);
