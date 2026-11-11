# Privacy Policy for Pdf Image Toolkit: offline

**Effective Date:** January 1, 2026  
**Last Updated:** November 11, 2026

## 1. Overview and Core Privacy Commitment
**Pdf Image Toolkit: offline** ("the Application", "we", "us", or "our") is designed from the ground up as a 100% offline, local-first Windows desktop utility. We believe your private documents, financial statements, contracts, personal photos, and business records belong exclusively to you. 

Our fundamental architecture ensures that **your files never leave your personal computer**.

---

## 2. Zero Data Collection & Storage
- **No Personal Data Collection**: We do not collect, store, log, or transmit names, email addresses, phone numbers, IP addresses, hardware identifiers, or any personally identifiable information (PII).
- **No Document or Image Content Transmission**: The application does not upload or stream document contents, page structures, image files, or extracted text to any external server, cloud platform, or third party. All parsing, rendering, rasterization, and compression take place exclusively in local memory (`RAM`) and private local temporary directories.
- **No Analytics or Telemetry**: There are zero tracking scripts, zero analytics frameworks (such as Google Analytics or App Center), zero telemetry beacons, and zero advertising SDKs in the application code.
- **No Account Required**: The application does not require user registration, profile creation, or external authentication.

---

## 3. Local File System & Temporary Processing
- **Local File Access**: The application only accesses files explicitly chosen by you through standard Windows file picker dialogs or via drag-and-drop actions.
- **Non-Destructive Processing**: The application never overwrites your original files. All processed outputs are saved to your chosen output folder with non-destructive suffixes (e.g. `document_compressed.pdf`, `document_compressed (1).pdf`).
- **Temporary File Disposal**: Any temporary cache files created during rasterization or multi-step conversions are stored in your secure local `ApplicationData` directory and are automatically wiped upon operation completion and application startup.
- **Local Error Logging**: If an unexpected crash occurs, diagnostics are written exclusively to a local text file on your hard drive (`%LocalAppData%\PdfImageToolkit\Logs\app.log`) for your private troubleshooting. No log files are transmitted over the internet.

---

## 4. In-App Purchases & Payments
The Application offers an optional lifetime license upgrade ("Pro").
- All transactions, billing, credit card processing, and receipt validation are handled exclusively by **Microsoft Corporation** through the official Microsoft Store client and Windows Store APIs (`Windows.Services.Store.StoreContext`).
- We never receive, process, or store your credit card details, billing address, or payment credentials.
- Purchase status is verified locally via Windows system entitlements.

---

## 5. Security & Third-Party Libraries
The Application uses strictly vetted, open-source permissive libraries (MIT and Apache 2.0 licenses):
- **PDFsharp (MIT License)** for local PDF manipulation.
- **Windows.Data.Pdf (Built into Microsoft Windows)** for hardware-accelerated local PDF rendering.
- **Magick.NET (Apache-2.0 License)** for local image processing.

None of these libraries initiate network requests or communicate with third-party endpoints.

---

## 6. Children's Privacy
The Application is a neutral document and image productivity tool rated for all ages (Everyone / 3+). Because we do not collect any personal data from any user, we do not knowingly collect or solicit data from children under 13.

---

## 7. Contact Information
If you have any questions or require assistance regarding this Privacy Policy, you may contact:
- **Project Repository**: https://github.com/Mrkalitaa/Pdf-Image-Toolkit-Offline/issues
- **Developer Profile**: https://github.com/Mrkalitaa
