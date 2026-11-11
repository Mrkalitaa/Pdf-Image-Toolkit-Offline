# Step-by-Step Microsoft Partner Center Submission Guide

This guide walks you through submitting **Pdf Image Toolkit: offline** to the Microsoft Store via **Microsoft Partner Center** (https://partner.microsoft.com/dashboard).

---

## Prerequisites Before You Begin
1. An active **Microsoft Developer Account** (Individual or Company).
   - If you do not have one, register at [Partner Center Developer Registration](https://developer.microsoft.com/microsoft-store/register) (one-time fee: ~$19 USD for individuals).
2. The submission package files located in:
   `c:\Users\Mr-Kalita\Downloads\Projects\PdfImageToolkit\StoreSubmissionPackage\`

---

## Step 1: Reserve the Product Name
1. Open your browser and navigate to:
   👉 **https://partner.microsoft.com/dashboard/apps-and-games/overview**
2. In the top-right, click **New product** > **MSIX or PWA app**.
3. In the text box, enter the exact app name:
   ```
   Pdf Image Toolkit: offline
   ```
4. Click **Check availability**, then click **Reserve product name**.
5. Your application dashboard will be created.

---

## Step 2: Associate Package Identity with Your Partner Center Account
Partner Center generates a unique **Package Family Name** and **Publisher Identity** tied to your developer account. 

### Method A: 1-Click Association via Visual Studio (Recommended)
1. Open the solution file:
   [`Projects\PdfImageToolkit\PdfImageToolkit.sln`](file:///C:/Users/Mr-Kalita/Downloads/Projects/PdfImageToolkit/PdfImageToolkit.sln) in Visual Studio 2022.
2. In Solution Explorer, right-click the project **`PdfImageToolkit.App`**.
3. Choose **Package and Publish** > **Associate App with the Store...**
4. Sign in with your Microsoft Developer credentials when prompted.
5. Select the reserved name **`Pdf Image Toolkit: offline`** from your list and click **Associate**.
   - *This automatically updates `<Identity>` in `Package.appxmanifest` to match your official Microsoft Publisher ID.*
6. Build your final Store package:
   - Right-click **`PdfImageToolkit.App`** > **Package and Publish** > **Create App Packages...**
   - Choose **Microsoft Store using an existing app name**.
   - Select **Release (x64)**.
   - This produces an upload bundle (or `.msix` file).

### Method B: Direct Upload of Provided MSIX
- The MSIX in `StoreSubmissionPackage\01_Binaries_and_Packages\Upload_Ready_MSIX\PdfImageToolkit.App_1.0.0.0_x64.msix` is fully built and signed with test credentials.
- When you upload to Partner Center, Microsoft's certification ingestion system will verify the manifest against your reserved name and sign it with Microsoft's official trusted root certification upon publication.

---

## Step 3: Configure In-App Product (Pro Unlock Add-on)
In Partner Center, you must set up the `pro_unlock` add-on before or alongside submitting the app.

1. On your app dashboard in Partner Center, click **In-app products** in the left sidebar.
2. Click **Create a new in-app product**.
3. Select **Durable** (User owns it perpetually).
4. Set **Product ID** to:
   ```
   pro_unlock
   ```
   *(IMPORTANT: Must match this exact ID, as the app code queries this identifier).*
5. Click **Reserve product ID**.
6. Under **Properties**:
   - Set Title: `Pdf Image Toolkit: offline Pro (Lifetime Unlock)`
   - Set Description: `Unlock unlimited daily operations, unlimited batch processing, and unlimited file sizes.`
7. Under **Pricing and availability**:
   - Set your preferred price tier (e.g. Tier 1: $4.99 USD or Tier 2: $9.99 USD).
   - Set Markets: Worldwide.
8. Click **Submit to the Store** for the add-on.

---

## Step 4: Fill in Main Submission Details

From your app page, click **Start your submission**. Work through each section:

### 1. Pricing and Availability
- **Base Price**: Select **Free**.
- **Markets**: Select **All markets** (Worldwide).
- **Discoverability**: Public (make product discoverable in Store).
- Click **Save**.

### 2. Properties
- **Category**: `Productivity`
- **Subcategory**: `Document Management`
- **Secondary Category** (optional): `Photo & video` > `Editors`
- **Support Contact Info**: Enter your support email (e.g. `support@yourdomain.com`).
- **Privacy Policy URL**: 
  - Enter your hosted privacy policy URL (e.g., your GitHub repo link or website link).
  - *(The complete Markdown text is in `StoreSubmissionPackage\04_Listing_Metadata_and_Copy\PRIVACY_POLICY.md`)*.
- **Website**: Enter your GitHub repo or personal website URL.
- Click **Save**.

### 3. Age Ratings (IARC Questionnaire)
1. Enter your contact email.
2. Select application type: **Utility, Productivity, Tools, or Reference**.
3. Complete the questionnaire:
   - Violence: **No**
   - Sexual content / nudity: **No**
   - Profanity / crude humor: **No**
   - Drugs / Alcohol / Tobacco: **No**
   - Simulated gambling: **No**
   - Social networking / chat / user-to-user messaging: **No**
   - Personal location sharing: **No**
   - Digital purchases: **Yes** (select In-App Purchases).
4. The system will award an **Everyone / PEGI 3+** rating. Click **Save and generate certificate**.

### 4. Packages
1. Drag and drop the `.msix` file from:
   `StoreSubmissionPackage\01_Binaries_and_Packages\Upload_Ready_MSIX\PdfImageToolkit.App_1.0.0.0_x64.msix`
   *(or the `.msixupload` bundle generated from Visual Studio).*
2. Partner Center will validate package architecture (`x64`), version (`1.0.0.0`), and dependencies.
3. Click **Save**.

### 5. Store Listings (English - United States)
Click into the **English (United States)** listing. Open `StoreSubmissionPackage\04_Listing_Metadata_and_Copy\PARTNER_CENTER_COPY_PASTE_SHEET.txt` and copy each item:
- **Product Name**: `Pdf Image Toolkit: offline`
- **Short Description / Subtitle**:
  ```
  Offline PDF and image toolkit to merge, split, compress, resize, and convert with 100% privacy.
  ```
- **Description**: Copy the entire Description block from the sheet.
- **Product Features**: Add the 12 feature bullet points provided in the sheet.
- **Search Terms (Keywords)**: Add all 7 keywords:
  1. `pdf merge split`
  2. `compress pdf offline`
  3. `images to pdf`
  4. `pdf to jpg`
  5. `resize images batch`
  6. `heic to jpg converter`
  7. `webp converter offline`
- **What's New in This Version**: Copy the v1.0.0 release notes from the sheet.
- **Copyright & Trademark**: `Copyright (C) 2026. All rights reserved.`
- **License Terms**: `Standard Microsoft Public License terms apply.`

### 6. Store Logos & Icons Upload
In the Store listing assets section:
- **1:1 App Tile / Icon (300×300)**:
  Upload `StoreSubmissionPackage\02_Logos_and_Icons\StoreLogo_300x300.png`.
- **Small Tile Icon (71×71)**:
  Upload `StoreSubmissionPackage\02_Logos_and_Icons\Square71x71Logo.png`.
- **Medium Tile Icon (150×150)**:
  Upload `StoreSubmissionPackage\02_Logos_and_Icons\Square150x150Logo.png`.

### 7. Screenshots Upload
In the **Desktop Screenshots** section, upload the 6 high-resolution (1920×1080) PNG screenshots from:
`StoreSubmissionPackage\03_Store_Screenshots\`:
1. `Screenshot_01_Home_Dashboard.png`
2. `Screenshot_02_PDF_Merge_and_Split.png`
3. `Screenshot_03_PDF_Compression.png`
4. `Screenshot_04_Image_Compress_and_Resize.png`
5. `Screenshot_05_Image_Format_Conversion.png`
6. `Screenshot_06_Settings_and_Privacy.png`

Click **Save**.

### 8. Submission Options & Restricted Capability Declaration
Under **Submission Options** / **Declarations**:
- **Restricted Capabilities**:
  - The package requests `runFullTrust`.
  - Enter the provided justification from the copy-paste sheet:
    ```
    Pdf Image Toolkit: offline is a WinUI 3 desktop utility that performs local file processing (merging, splitting, compressing, and converting PDF documents and images). Full trust is strictly required for local disk I/O, file picking, directory management, and hardware-accelerated local rendering. No external network requests or system modifications are performed.
    ```
- **Certification Notes**:
  ```
  All features work 100% offline. No test accounts or credentials are required. To test Pro unlock, a durable in-app product 'pro_unlock' has been configured.
  ```

---

## Step 5: Submit for Certification
1. Review the submission summary to ensure all sections show green checkmarks.
2. Click **Submit to the Store**.
3. Microsoft's automated certification and manual review process takes approximately **24 to 48 hours**.
4. Once certified, the app goes live worldwide in the Microsoft Store!
