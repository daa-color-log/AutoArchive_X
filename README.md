# AutoArchiveX

> [!TIP]
> ### 📥 **[Download AutoArchive_X.exe (Direct Link)](https://github.com/daa-color-log/AutoArchive_X/raw/main/AutoArchive_X.exe)**
> You can download the compiled executable directly and run it on Windows instantly.

AutoArchiveX is a premium, high-performance WPF-based desktop application designed for automated media archiving, file organization, and metadata imprinting. It offers a professional dashboard for handling large batches of images and files seamlessly.

---

## Key Features

- **Directory Mappings & Volume Monitoring**: Instantly map source directories to backup drives. Built-in real-time volume space checks notify users with crimson red warnings when disk usage exceeds 85%.
- **Automatic Organization & Rules**: Auto-split files into target subfolders based on EXIF shooting dates or current operational timestamps. Customize static root naming templates.
- **Bulk Renaming Engine**: Rename hundreds of files in seconds using flexible tokens such as `{yyyy}`, `{MM}`, `{dd}`, `{HH}`, `{mm}`, `{ss}`, and sequential `{index}`.
- **Metadata Watermarking & Tiled Matrix Patterns**:
  - Automatically extract camera manufacturer, body model, lens specs, focal lengths, aperture stops, shutter speeds, and ISO fields from raw EXIF tags.
  - Apply custom signature strings with drop shadows.
  - Dynamically scale typography based on portrait/landscape dimensions.
  - Apply translucent, 30-degree tilted **Adobe Stock / Stock Photo style dense diagonal watermark grids** covering all edges and corners of the image.
- **WPF Live Previews**: Simulated WYSIWYG viewports representing landscape (4:3) and portrait (3:4) orientations in real-time.
- **Quick Preset Palette**: Select from an aesthetic palette of preset color chips or input a custom `#HEX` value for the watermark brush.
- **Borderless Fullscreen Mode**: Switch to a fully borderless dashboard with a single click. Seamlessly exit the fullscreen mode at any time by pressing the `ESC` key.

---

## File Structure

The project repository consists of the following core files:

| File Name | Role & Description |
| :--- | :--- |
| **`Program.cs`** | The main application entrance. Initializes the WPF thread context and launches the dashboard. |
| **`MainWindow.cs`** | The core UI engine. Dynamically generates layouts without external XAML overhead. Controls localized localization dictionary switching (English, Japanese, Korean) and wires all user interactions. |
| **`AppLogic.cs`** | The logical engine of the application. Handles local JSON configuration serialization, file backup operations, custom-pattern bulk file renaming, GDI+ EXIF watermark drawing, and disk monitoring sweeps. |
| **`app_icon.ico`** | The desktop icon asset representing the AutoArchiveX application context. |
| **`build.bat`** | Windows command script to compile the application instantly using the local C# compiler (`csc.exe`) under `.NET Framework 4.0/4.5`. |
| **`.gitignore`** | Configuration to prevent pushing transient build binaries (`*.exe`, `*.pdb`), test workspaces, and local sensitive JSON data containing webhook URLs. |

---

## How to Build & Run

### Prerequisites
- Windows OS
- .NET Framework 4.0 or 4.5 Runtime (standard in all modern Windows environments).

### Compilation
Simply double-click or run the build script in your terminal:
```cmd
.\build.bat
```

### Execution
Run the compiled executable to launch the suite:
```cmd
AutoArchive_X.exe
```
