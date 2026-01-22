# 🚀 DevContext Nexus v2: Usage & Workflow

This guide explains how to use the DevContext Nexus API and its CLI tools to manage project documentation in the cloud.

---

## 1. Prerequisites

- **API Base URL:** `http://context-nexus.runasp.net`
- **Authentication:** All requests must include the `x-api-key` header.
  - **Header:** `x-api-key`
  - **Value:** (Refer to your `appsettings.json` or environment variables)

---

## 2. Core Workflow (API)

### Step 1: Initialize Project
Every context must belong to a project.
- **Endpoint:** `POST /api/Context/projects`
- **Body:** `{ "name": "ProjectA" }`

### Step 2: Upload Files/Content
You can upload either a physical file or raw text.
- **File Upload:** `POST /api/Context/upload` (Form-data with `file`, `projectName`, `filePath`)
- **Text Upload:** `POST /api/Context/content` (JSON with `projectName`, `filePath`, `content`)

### Step 3: Fetch Context
Retrieve the public Cloudinary URL of any stored file.
- **Endpoint:** `GET /api/Context/{projectName}/{filePath}`
- **Response:** `{ "url": "https://res.cloudinary.com/..." }`

---

## 3. CLI Power Tools (ScriptV2)

The most efficient way to interact with the API is through the provided PowerShell scripts in `scripts/ScriptV2`.

### Setup
We recommend dot-sourcing the `nexus-profile.ps1` in your Windows PowerShell `$PROFILE`:
```powershell
. "f:\GitHubProject\Dev Context Nexus\Backend\scripts\ScriptV2\nexus-profile.ps1"
```

### Common Commands
| Command | Usage |
| :--- | :--- |
| `nx-upload-file` | `nx-upload-file -ProjectName "App" -LocalFile "./readme.md" -RemotePath "docs/readme.md"` |
| `nx-upload-content` | `nx-upload-content -ProjectName "App" -FilePath "test.md" -Content "# Hello"` |
| `nx-list-files` | `nx-list-files -ProjectName "App"` |
| `nx-download-context` | `nx-download-context -ProjectName "App" -FilePath "docs/readme.md" -OutFile "local.md"` |
| `nx-overview` | `nx-overview` |
| `nx-delete-project`| `nx-delete-project -ProjectName "OldApp"` |

---

## 4. Bruno Configuration

If you use the Bruno API client:
1.  Open the Bruno collection in `DevContextNexus.API/Bruno`.
2.  Switch the environment to **MonsterEndpoint**.
3.  Set your `baseUrl` to `http://context-nexus.runasp.net`.
4.  Ensure `apiKey` is correctly set in the environment variables.

---

## 💡 Troubleshooting
- **404 Not Found**: Ensure the `projectName` matches exactly what was created in Step 1.
- **500 Internal Error**: Check the server logs at `/logs/stdout` if you have server access.
- **Cloudinary Delay**: Sometimes files take a few seconds to propagate through the CDN.
