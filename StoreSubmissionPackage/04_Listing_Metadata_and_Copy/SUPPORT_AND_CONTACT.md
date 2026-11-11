# Customer Support & Troubleshooting Guide

**Application:** Pdf Image Toolkit: offline  
**Version:** 1.0.0 (x64)

---

## 1. Official Support Channels
- **Email Support:** support@yourdomain.com *(Replace with your active email)*
- **GitHub Issue Tracker:** https://github.com/your-username/pdf-image-toolkit/issues
- **Documentation & User Guide:** Included in the application distribution package (`README.md`).

---

## 2. Frequently Asked Questions (FAQ)

### Q: Why do you not require an account or cloud upload?
**A:** Most web converters upload your sensitive contracts, tax records, and personal photos to remote third-party servers. Pdf Image Toolkit: offline performs 100% of its PDF rendering, merging, splitting, compressing, and image conversions locally inside your computer's RAM and CPU. Files never leave your PC.

### Q: What is the difference between Free and Pro?
- **Free Tier:** 5 operations per calendar day (automatically resets at midnight), up to 10 files per batch, up to 50 MB per file.
- **Pro Tier:** One-time purchase for a lifetime unlock. Unlimited operations, unlimited batch processing, and unlimited file size.

### Q: What happens if I convert or compress a file? Will my original file be replaced?
**A:** Never. The application uses non-destructive auto-naming. If you compress `invoice.pdf`, it will save as `invoice_compressed.pdf`. If that file already exists, it creates `invoice_compressed (1).pdf`.

### Q: What is the difference between Light and Strong PDF compression?
- **Light Mode:** Re-encodes internal PDF data streams and strips unneeded metadata while preserving 100% selectable, searchable vector text and crisp fonts.
- **Strong Mode:** High-efficiency rasterization that re-renders pages at chosen DPI (96, 150, or 200 DPI) and JPEG quality. It achieves maximum size reduction (often 70% to 90% savings). Text in strong mode becomes an image.
- **Non-Expansion Guarantee:** If compression produces a file larger than the original, the application automatically retains the original file and informs you.

### Q: How do I open Apple HEIC photos on Windows?
**A:** The application has built-in Magick.NET support for HEIC. If your system requires additional codec rendering, install the free "HEIF Image Extensions" from the Microsoft Store.

---

## 3. Diagnostic Logs
If an unexpected crash or error occurs, the application writes diagnostic error logs strictly to your private local drive:
```
%LocalAppData%\PdfImageToolkit\Logs\app.log
```
No logs or diagnostics are ever transmitted over the network. When seeking support, you may inspect and attach this file to your support email.
