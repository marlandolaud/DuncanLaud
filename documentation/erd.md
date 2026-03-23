# Entity-Relationship Diagram

```mermaid
erDiagram
    GROUP {
        UNIQUEIDENTIFIER Id PK "UUID v7, client-generated"
        NVARCHAR(100) Name "Required display name"
        DATETIME2 CreatedAtUtc "Server-set on creation"
    }
    PERSON {
        UNIQUEIDENTIFIER Id PK "UUID v7"
        UNIQUEIDENTIFIER GroupId FK "Cascade delete on group removal"
        NVARCHAR(100) FirstName "Required, 2-100 chars"
        NVARCHAR(100) LastName "Required, 2-100 chars"
        NVARCHAR(100) PreferredName "Optional, 2-100 chars"
        DATE BirthDate "Required, past date, year >= 1900"
        NVARCHAR(2048) ImageThumbnail "Optional HTTPS URL (CF or MinIO)"
        DATETIME2 CreatedAtUtc "Server-set"
    }

    GROUP ||--o{ PERSON : "has members"
```

## Indexes

| Table | Index Name | Columns | Purpose |
|---|---|---|---|
| Person | PK (clustered) | Id | Primary key lookup |
| Person | IX_Person_GroupId | GroupId | All member lookups by group |
| Person | IX_Person_GroupId_BirthDate | GroupId, BirthDate | Birthday window queries |
| Group | PK (clustered) | Id | Primary key lookup |
