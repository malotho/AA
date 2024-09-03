CREATE TABLE ProcessedRecords (
    SourceDbName TEXT PRIMARY KEY,
    LastProcessedId INTEGER,
    LastProcessedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);