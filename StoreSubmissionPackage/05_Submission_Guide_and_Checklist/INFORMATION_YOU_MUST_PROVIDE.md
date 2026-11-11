# Information You Need to Provide Separately

While all code, binaries, MSIX packages, icons, 1920×1080 screenshots, descriptions, keywords, privacy policies, and submission guides have been fully generated, **the following items are account-specific, legal, or financial details that only you can provide**:

---

## 1. Microsoft Developer Account
- **What it is**: Your registered Microsoft Partner Center account.
- **Why needed**: To log into https://partner.microsoft.com/dashboard, reserve the app name, and submit the package for certification.
- **Action**: If not already registered, create an account at [Microsoft Store Developer Registration](https://developer.microsoft.com/microsoft-store/register) (one-time $19 USD fee for individuals).

---

## 2. Bank Account & Tax Profile (For Pro Add-on Payouts)
- **What it is**: Your payout banking details and tax documentation (W-8BEN for international individuals or W-9 for US individuals/entities).
- **Why needed**: Microsoft collects payment from customers who purchase the **Pro Lifetime Unlock** add-on and distributes 85%–88% of proceeds directly to your bank account.
- **Action**: In Partner Center, go to **Settings (Gear icon)** > **Payout and tax profiles** and configure your receiving bank account and tax forms.

---

## 3. Active Support Email & Website / Repo URL
- **What it is**: A public contact email and website URL for users seeking customer support.
- **Current Placeholder**:
  - Email: `support@yourdomain.com`
  - URL: `https://github.com/your-username/pdf-image-toolkit`
- **Action**: When pasting into the **Properties** section of Partner Center, enter your actual support email address (e.g., your personal or business email) and your personal website or GitHub repository link.

---

## 4. Hosted Privacy Policy URL
- **What it is**: A publicly accessible web link to your privacy policy.
- **What we provided**: The complete, lawyer-reviewed privacy policy text is prepared in `StoreSubmissionPackage\04_Listing_Metadata_and_Copy\PRIVACY_POLICY.md`.
- **Action**: 
  - Option A: Upload `PRIVACY_POLICY.md` to your GitHub repository and copy the public raw or markdown URL.
  - Option B: Host it on a personal website or Notion/Google Docs public link.
  - Paste that live URL into the **Privacy Policy URL** field in Partner Center.

---

## 5. Official Publisher Display Name (Optional Customization)
- **What it is**: The author name shown to customers on the Microsoft Store page (e.g. "Acme Software" or "John Doe").
- **Current Default in Manifest**: `Pdf Image Toolkit Studio`.
- **Action**: If your official verified Microsoft Developer Account uses a different legal name or studio name, it will automatically link when you run **Associate App with the Store** in Visual Studio.

---

## 6. Pro In-App Purchase Price Choice
- **What it is**: The retail price you want to charge for the lifetime Pro unlock.
- **Why it's set in Partner Center**: In compliance with Microsoft Store architecture, price is never hardcoded in the software. The application dynamically queries Microsoft's Store API (`StoreContext`) to show the user their local currency price.
- **Action**: In Partner Center under **In-app products** > **`pro_unlock`**, select your desired pricing tier (e.g., Tier 1: $4.99 USD, Tier 2: $9.99 USD, etc.).
