# Database Schemas & System Workflow

## 1. Database Schema (PostgreSQL)

### Table: projects
| Column | Type | Description |
| :--- | :--- | :--- |
| id | UUID (PK) | ไอดีโปรเจกต์ |
| name | VARCHAR | ชื่อโปรเจกต์ |

### Table: context_files
| Column | Type | Description |
| :--- | :--- | :--- |
| project_id | UUID (FK) | เชื่อมกับ projects |
| file_path | TEXT | Path ใน GitHub |
| last_sha | VARCHAR | SHA ล่าสุด |

## 2. System Workflow
1. Client -> GET /api/context
2. Server -> ค้นหา Path ใน DB
3. Server -> Fetch จาก GitHub API
4. Response -> Raw text สำหรับ Prompt