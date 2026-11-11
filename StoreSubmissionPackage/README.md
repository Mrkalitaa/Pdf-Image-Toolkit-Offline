# Microsoft Partner Center Complete Submission Package

**Application Title:** Pdf Image Toolkit: offline  
**Package Identity Name:** `MrKalita.PdfImageToolkitOffline`  
**Publisher:** `CN=8B1A0A49-E80C-49BD-9839-D7293676F3BC`  
**Publisher Display Name:** `MrKalita`  
**Package Family Name (PFN):** `MrKalita.PdfImageToolkitOffline_0v7m79yeqhw60`  
**Store ID:** `9N40BWNB2JLQ`  
**Store URL:** https://apps.microsoft.com/detail/9N40BWNB2JLQ  
**Version:** 1.0.0 (x64 Production)  
**Package Folder:** `c:\Users\Mr-Kalita\Downloads\Projects\PdfImageToolkit\StoreSubmissionPackage\`

---

## 📁 Package Directory Structure

```
StoreSubmissionPackage/
├── README.md                                          # Master index of this submission package
├── 01_Binaries_and_Packages/
│   ├── Upload_Ready_MSIX/
│   │   ├── PdfImageToolkit.App_1.0.0.0_x64.msix       # 78 MB Release MSIX package ready for Store upload
│   │   ├── PdfImageToolkit.App_1.0.0.0_x64.cer        # Signing certificate
│   │   └── Install_MSIX_Locally.bat                   # 1-click script to install and test MSIX on Windows
│   └── Testing_Standalone_EXE/
│       ├── PdfImageToolkit.App.exe                    # Portable standalone executable (no install required)
│       ├── Run_Standalone_App.bat                     # 1-click test launcher
│       └── [Complete runtime assemblies & assets]
├── 02_Logos_and_Icons/
│   ├── StoreLogo_300x300.png                          # Required 300x300 1:1 App Store Tile Logo
│   ├── Square150x150Logo.png                          # Required 150x150 Medium App Tile Logo
│   ├── Square71x71Logo.png                            # Required 71x71 Small Tile Logo
│   ├── Square44x44Logo.png                            # 44x44 App List / Taskbar Icon
│   ├── StoreLogo_50x50.png                            # 50x50 Standard Store Badge
│   ├── Wide310x150Logo.png                            # 310x150 Wide Tile Branding
│   └── SplashScreen_620x300.png                       # 620x300 Application Splash Screen
├── 03_Store_Screenshots/
│   ├── Screenshot_01_Home_Dashboard.png               # 1920x1080 - Main Dashboard & Offline Banner
│   ├── Screenshot_02_PDF_Merge_and_Split.png          # 1920x1080 - PDF Merge & Page Extraction
│   ├── Screenshot_03_PDF_Compression.png              # 1920x1080 - Dual-Mode PDF Compression & Savings
│   ├── Screenshot_04_Image_Compress_and_Resize.png    # 1920x1080 - Batch Image Quality & Aspect Resizing
│   ├── Screenshot_05_Image_Format_Conversion.png      # 1920x1080 - JPG, PNG, WebP, Apple HEIC Conversion
│   └── Screenshot_06_Settings_and_Privacy.png         # 1920x1080 - Privacy First, Mica Theme & Defaults
├── 04_Listing_Metadata_and_Copy/
│   ├── PARTNER_CENTER_COPY_PASTE_SHEET.txt            # Zero-friction copy-paste sheet for all form fields
│   ├── PRIVACY_POLICY.md                              # Complete privacy policy ready to host on web/GitHub
│   ├── TERMS_AND_CONDITIONS.md                        # Commercial End User License Agreement (EULA)
│   └── SUPPORT_AND_CONTACT.md                         # Customer support channels and FAQs
└── 05_Submission_Guide_and_Checklist/
    ├── STEP_BY_STEP_SUBMISSION_GUIDE.md               # Visual screen-by-screen submission walkthrough
    └── INFORMATION_YOU_MUST_PROVIDE.md                # Checklist of personal/account items you provide
```

---

## ⚡ Quick Start: 3 Steps to Publish

1. **Test the App Right Now**:
   - Double-click **`01_Binaries_and_Packages\Testing_Standalone_EXE\Run_Standalone_App.bat`** for instant portable execution.
   - Or run **`01_Binaries_and_Packages\Upload_Ready_MSIX\Install_MSIX_Locally.bat`** to test the installed Windows package.
2. **Log into Partner Center**:
   - Go to [Microsoft Partner Center Dashboard](https://partner.microsoft.com/dashboard/apps-and-games/overview).
   - Follow **`05_Submission_Guide_and_Checklist\STEP_BY_STEP_SUBMISSION_GUIDE.md`**.
3. **Copy-Paste and Upload**:
   - Copy all fields from **`04_Listing_Metadata_and_Copy\PARTNER_CENTER_COPY_PASTE_SHEET.txt`**.
   - Upload logos from **`02_Logos_and_Icons\`**.
   - Upload screenshots from **`03_Store_Screenshots\`**.
   - Upload the MSIX package from **`01_Binaries_and_Packages\Upload_Ready_MSIX\`**.
   - Click **Submit to the Store**!
