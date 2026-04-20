# 🚀 DEPLOYMENT GUIDE

## Development Setup

### Prerequisites
- Visual Studio 2019+ or VSCode with C# extension
- .NET Framework 4.8+
- Git for version control
- WebView2 Runtime (auto-installed with some OS versions)

### Installation

1. **Clone Repository**
   ```bash
   git clone <repo-url>
   cd VietnamFoodGuide
   ```

2. **Restore Dependencies**
   ```bash
   cd VietnamFoodGuide
   dotnet restore
   ```

3. **Build Solution**
   ```bash
   dotnet build -c Debug
   ```

4. **Run Application**
   ```bash
   dotnet run
   # Or F5 in Visual Studio
   ```

---

## Running the Application

### Visual Studio
1. Open `VietnamFoodGuide.sln`
2. Press **F5** to start debugging
3. Or **Ctrl + F5** for release without debugging

### Command Line
```bash
cd VietnamFoodGuide/VietnamFoodGuide
dotnet run --configuration Debug
```

### From Executable
```bash
VietnamFoodGuide\bin\Debug\net48\VietnamFoodGuide.exe
```

---

## Build Configurations

### Debug Build
```bash
dotnet build -c Debug
```
- Includes debug symbols
- Slower execution
- Easier debugging
- Larger file size

### Release Build
```bash
dotnet build -c Release
```
- Optimized for performance
- Smaller file size (~2-5x smaller)
- Faster execution
- No debug symbols

---

## Publishing

### Create Release Package

```bash
# Build and publish to folder
dotnet publish -c Release -f net48 -o .\publish

# Navigate to published folder
cd publish

# Run executable
VietnamFoodGuide.exe
```

### Executable Size Comparison
```
Debug:   ~5-10 MB
Release: ~2-3 MB
```

---

## Distribution

### Option 1: Standalone Executable
1. Build release
2. Copy `VietnamFoodGuide.exe` + dependencies
3. Include `Data/foods.json`
4. Include `Assets/` folder
5. Ensure WebView2 Runtime installed

### Option 2: Installer (NSIS/WiX)
```bash
# Create installer
# Use Visual Studio Installer Projects extension
# Or third-party tool like NSIS
```

### Option 3: Portable ZIP
```bash
# Create portable package
mkdir VietnamFoodGuide_Portable
cp -r publish/* VietnamFoodGuide_Portable/
zip -r VietnamFoodGuide_v1.0.0.zip VietnamFoodGuide_Portable/
```

---

## System Requirements

### Minimum
- **OS**: Windows 7+ (preferably Windows 10+)
- **Framework**: .NET Framework 4.8
- **RAM**: 256 MB
- **Disk**: 50 MB
- **Internet**: Required for TTS & Maps

### Recommended
- **OS**: Windows 10 or 11
- **Framework**: .NET Framework 4.8
- **RAM**: 512 MB+
- **Disk**: 100 MB
- **GPU**: Not required

---

## Configuration

### Application Settings

#### appsettings.json (Future)
```json
{
    "Logging": {
        "Level": "Information"
    },
    "Services": {
        "EnableOfflineMode": true,
        "CacheTimeout": 300
    }
}
```

#### Environment Variables
```bash
# Enable debug logging
VIETFOODGUIDE_DEBUG=true

# Override data path
VIETFOODGUIDE_DATA_PATH=C:\CustomData\
```

---

## Troubleshooting Deployment

### "WebView2 Runtime not found"
```bash
# Download and install from Microsoft
# https://developer.microsoft.com/microsoft-edge/webview2/

# Or add via NuGet
dotnet add package Microsoft.Web.WebView2
```

### "foods.json not found"
```bash
# Ensure file structure
VietnamFoodGuide/
├── VietnamFoodGuide.exe
├── Data/
│   └── foods.json
└── Assets/
    └── Images/
```

### ".NET Framework not installed"
```bash
# Install from Microsoft
# https://dotnet.microsoft.com/download/dotnet-framework

# Or target .NET 6+ instead
# Update .csproj: <TargetFramework>net6.0-windows</TargetFramework>
```

---

## Continuous Integration (CI/CD)

### GitHub Actions Workflow

**File: `.github/workflows/build.yml`**
```yaml
name: Build & Release

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2

      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 4.8

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build -c Release

      - name: Test
        run: dotnet test

      - name: Publish
        run: dotnet publish -c Release -f net48

      - name: Upload Artifacts
        uses: actions/upload-artifact@v2
        with:
          name: VietnamFoodGuide-Release
          path: publish/
```

---

## Version Management

### Semantic Versioning
```
MAJOR.MINOR.PATCH

1.0.0  - Initial release
1.0.1  - Bug fixes
1.1.0  - New features
2.0.0  - Breaking changes
```

### Update Version
```xml
<!-- VietnamFoodGuide.csproj -->
<PropertyGroup>
    <Version>1.0.0</Version>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
</PropertyGroup>
```

---

## Monitoring & Logging

### Enable Logging
```csharp
// In App.xaml.cs
#if DEBUG
    System.Diagnostics.Debug.WriteLine("App started in Debug mode");
#endif
```

### Log Viewer
- **Visual Studio**: Debug → Windows → Output
- **Debugger Terminal**: Application output displayed
- **Event Viewer**: Windows application logs (optional)

---

## Backup & Recovery

### Backup User Data
```bash
# Favorites are stored in:
# C:\Users\<username>\AppData\Roaming\VietnamFoodGuide\
```

### Backup Application
```bash
# Daily backup of publish folder
# git backup + cloud storage (GitHub, OneDrive)
```

---

## Performance Optimization

### Tips
1. **Use Release build** (5x faster than Debug)
2. **Cache food data** in memory
3. **Lazy load images** (WPF default)
4. **Minimize external calls** (TTS, Maps)
5. **Profile slow operations**

### Profiling
```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
// Operation
stopwatch.Stop();
Debug.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
```

---

## Security Deployment Checklist

- [ ] No hardcoded secrets in code
- [ ] API keys in secure config (not in repo)
- [ ] InputValidation in place
- [ ] Error messages don't expose system details
- [ ] HTTPS for external APIs
- [ ] Local storage is non-sensitive data only
- [ ] Dependencies are up-to-date
- [ ] Code reviewed before release

---

## Post-Deployment

### Monitor
- User feedback
- Error logs
- Performance metrics
- Usage statistics

### Maintain
- Regular updates for security patches
- Dependency updates (NuGet)
- Bug fixes
- Feature additions

### Support
- Documentation available
- FAQ on GitHub
- Issue tracker for bug reports
- Discussion forum for questions

---

## Rollback Procedure

If new version has critical issues:

```bash
1. Stop application
2. Restore previous version executable
3. Verify data integrity (favorites.json)
4. Restart application
5. Investigate issue in development
6. Release hotfix v1.0.x
```

---

## Upgrading Users

### In-App Update Check (Future)
```csharp
public class UpdateService
{
    public async Task<bool> CheckForUpdatesAsync()
    {
        // Check GitHub releases API
        // Compare current version with latest
        // Prompt user if update available
        return updateAvailable;
    }
}
```

---

## Conclusion

Deployment is straightforward:
1. Build Release
2. Copy to distribution folder
3. Include assets
4. Run executable

For production, consider:
- Installer (NSIS/WiX)
- Signing (Code signing certificate)
- Update mechanism
- Customer support process
