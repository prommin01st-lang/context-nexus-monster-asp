```sql
-- Connect to your Supabase database and run this SQL:

-- View the corrupted record first
SELECT * FROM context_files
WHERE file_path LIKE 'F:%'
    OR file_path LIKE 'C:%';

-- Delete the corrupted record
DELETE FROM context_files
WHERE file_path = 'F:\Mobile Project\App01\Mobile Chat API-documentation.html';

-- Verify it's gone
SELECT *
FROM context_files
WHERE
    project_id = (
        SELECT id
        FROM projects
        WHERE
            name = 'App01'
    );
```