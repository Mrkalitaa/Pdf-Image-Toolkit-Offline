# Privacy Policy for Pdf Image Toolkit: offline

**Effective Date:** November 2026  
**Last Updated:** November 2026  
**Developer / Publisher:** MrKalita  
**Product:** Pdf Image Toolkit: offline  

---

## 1. Overview and Core Privacy Commitment
**Pdf Image Toolkit: offline** ("the Application") is developed by **MrKalita**. The Application is engineered as a 100% offline, local-first Windows desktop utility. We believe your private documents, financial statements, contracts, personal photos, and business records belong exclusively to you. 

Our fundamental architecture ensures that **your files never leave your personal computer**.

---

## 2. Zero Data Collection & Storage
- **No Personal Data Collection**: We do not collect, store, log, transmit, or share any personally identifiable information (PII) such as your name, email address, phone number, IP address, device identifiers, or physical location.
- **No Document or Image Content Transmission**: The Application does not upload or stream document contents, page structures, image files, or extracted text to any external server, cloud platform, or third party. All document parsing, rendering, rasterization, and compression take place exclusively in local memory (`RAM`) and private local temporary directories.
- **No Analytics, Tracking, or Telemetry**: There are zero tracking scripts, zero third-party analytics frameworks (e.g. Google Analytics, App Center), zero telemetry beacons, and zero advertising SDKs embedded in the application code.
- **No Account Required**: The Application requires zero user registration, account logins, or external profile authentications.

---

## 3. Local File System & Temporary Processing
- **Explicit File Access**: The Application only accesses files explicitly chosen by you through standard Windows file picker dialogs or via drag-and-drop actions.
- **Non-Destructive Processing**: The Application never overwrites your original files. All processed outputs are saved to your chosen output folder with non-destructive suffixes (e.g. `document_compressed.pdf`, `document_compressed (1).pdf`).
- **Temporary File Disposal**: Any temporary cache files created during rasterization or multi-step conversions are stored in your secure local Windows `%TEMP%` directory and are automatically wiped upon operation completion and application exit.
- **Local Error Logging**: Diagnostics or crash reports (if any) are written exclusively to a local text file on your hard drive (`%LocalAppData%\PdfImageToolkit\Logs\app.log`) for your private troubleshooting. No log files are transmitted over the internet.

---

## 4. In-App Purchases & Licensing (Microsoft Store)
The Application offers an optional lifetime license upgrade ("Pro").
- All transactions, billing, credit card processing, and receipt validation are handled exclusively by **Microsoft Corporation** through the official Microsoft Store client and Windows Store APIs (`Windows.Services.Store.StoreContext`).
- The developer never receives, processes, or stores your credit card details, billing address, or payment credentials.
- Purchase entitlements are verified locally via Windows system licensing services.

---

## 5. Security & Third-Party Libraries
The Application uses strictly vetted, open-source permissive libraries (MIT and Apache 2.0 licenses):
- **PDFsharp (MIT License)** for local PDF manipulation.
- **Windows.Data.Pdf (Built into Microsoft Windows)** for hardware-accelerated local PDF rendering.
- **Magick.NET (Apache-2.0 License)** for local image processing.

None of these libraries initiate external network requests or communicate with third-party endpoints.

---

## 6. Children's Privacy (COPPA & GDPR-K Compliance)
The Application is a neutral productivity tool rated for all ages (ESRB Everyone / PEGI 3 / IARC 3+). Because we do not collect any personal data whatsoever from any user, we do not knowingly collect, solicit, or maintain personal information from children under the age of 13 (or under 16 in the European Union).

---

## 7. User Rights (GDPR & CCPA Compliance)
Because the Application does not collect, transmit, or store any personal data or document content on external servers or databases, there is no personal data to access, rectify, port, or erase. Users retain 100% ownership and control over their local files at all times.

---

## 8. Changes to This Privacy Policy
We may periodically update this Privacy Policy. Any changes will be posted immediately with an updated "Last Updated" date at:  
https://github.com/Mrkalitaa/Pdf-Image-Toolkit-Offline/blob/main/PRIVACY_POLICY.md

---

## 9. Contact Information & Support
If you have any questions, concerns, or requests regarding this Privacy Policy or the Application, please contact us:
- **Developer / Publisher:** MrKalita
- **Support Email:** [johnkalita2014@outlook.com](mailto:johnkalita2014@outlook.com)
- **GitHub Issues:** [https://github.com/Mrkalitaa/Pdf-Image-Toolkit-Offline/issues](https://github.com/Mrkalitaa/Pdf-Image-Toolkit-Offline/issues)
- **Microsoft Store Listing:** [https://apps.microsoft.com/detail/9N40BWNB2JLQ](https://apps.microsoft.com/detail/9N40BWNB2JLQ)
