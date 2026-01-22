# DevContext Nexus API v2

**Centralized Context Management System** designed for AI and Development workflows. This API acts as a "Source of Truth" for your project context files, documentation, and requirements, stored securely in **Cloudinary** and indexed via **SQL Server**.

🚀 **Live API:** [http://context-nexus.runasp.net](http://context-nexus.runasp.net)

---

## ✨ Features

- **Cloud Storage**: Persistent storage of `.txt`, `.md`, and binary files using Cloudinary.
- **Relational Metadata**: Managed by MS SQL Server for fast querying and system overviews.
- **Project Isolation**: Organize contexts by Project Names.
- **CLI Power**: Comprehensive PowerShell scripts for seamless local-to-cloud sync.
- **Modern API UI**: Interactive documentation powered by **Scalar** (.NET 9 Native).

## 🔒 Authentication

All requests require an API Key passed in the header:

- **Header Name:** `x-api-key`
- **Value:** `YOUR_SECRET_KEY`

---

## 🛠️ API Reference

### 1. Projects & Overview
- `GET /api/Context/projects`: List all managed projects.
- `GET /api/Context/overview`: Get system-wide stats (total projects, file counts).
- `POST /api/Context/projects`: Initialize a new project.

### 2. Context Operations
- `GET /api/Context/{projectName}/files`: List all files in a project with their Cloudinary URLs.
- `POST /api/Context/upload`: Upload local files (Multipart/Form-Data).
- `POST /api/Context/content`: Upload raw text/markdown content directly (JSON).
- `GET /api/Context/{projectName}/{*filePath}`: Get the public URL of a specific context file.
- `DELETE /api/Context/{projectName}/{*filePath}`: Remove a file from cloud and database.

### 📚 Interactive Documentation
Access the full API spec and test endpoints directly at:
👉 **[http://context-nexus.runasp.net/scalar/v1](http://context-nexus.runasp.net/scalar/v1)**

---

## 💻 CLI Tools (ScriptV2)

The `scripts/ScriptV2` folder contains powerful tools to interact with the API from your terminal.

### Quick Commands (via `nexus-profile.ps1`)
Dot-source the profile in your PowerShell `$PROFILE` to enable global commands:
```powershell
. "C:/path/to/project/scripts/ScriptV2/nexus-profile.ps1"
```

| Command | Action |
| :--- | :--- |
| `nx-upload-file` | Upload a local file to the cloud. |
| `nx-download-context` | Fetch and save a remote file locally. |
| `nx-list-files` | See what's currently in your project. |
| `nx-overview` | Get a birds-eye view of all projects. |

---

## 🚀 Technical Stack
- **Framework**: .NET 9.0 (ASP.NET Core)
- **Database**: Microsoft SQL Server
- **Object Storage**: Cloudinary
- **Hosting**: IIS (MonsterASP)
- **Middleware**: API Key Auth, Global Error Handling

---

## 📝 Change Log (From v1)
- ✅ Switched from GitHub storage to **Cloudinary** (No more GitHub API rate limits).
- ✅ Migrated from PostgreSQL to **SQL Server**.
- ✅ Native **.NET 9 OpenAPI/Scalar** implementation.
- ✅ Enhanced support for **Nested Folders** in cloud storage.
