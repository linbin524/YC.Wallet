# YC.Wallet User Manual

> Enterprise-grade Solana desktop wallet - Built for C# developers, featuring multi-language support, Google OAuth login, SPL token management, and enterprise-level security

## 📋 Table of Contents

1. [System Requirements](#system-requirements)
2. [Download & Installation](#download--installation)
3. [First Launch](#first-launch)
4. [User Registration & Login](#user-registration--login)
5. [Wallet Management](#wallet-management)
6. [Token Management](#token-management)
7. [Transaction Operations](#transaction-operations)
8. [Settings & Preferences](#settings--preferences)
9. [Frequently Asked Questions](#frequently-asked-questions)
10. [Security Reminders](#security-reminders)

---

## 🖥️ System Requirements

### Minimum Requirements
- **Operating System**: Windows 10 (64-bit) or Windows 11
- **Memory**: At least 2GB available RAM
- **Storage**: At least 500MB available space
- **Network**: Stable internet connection

### Recommended Requirements
- **Operating System**: Windows 11 (64-bit)
- **Memory**: 4GB or more
- **Storage**: 1GB available space
- **Network**: High-speed stable internet connection

### Required Software
- **.NET 8.0 Runtime** - Will be automatically prompted to download if not installed

---

## 📥 Download & Installation

### Step 1: Download the Program
1. Visit the project release page: `https://github.com/your-username/YC.Wallet/releases`
2. Find the latest version (usually marked as "Latest")
3. Download the corresponding installation package:
   - `YC.Wallet-Setup.exe` - Windows installer (recommended)
   - `YC.Wallet.zip` - Portable version

### Step 2: Install the Program
#### Method 1: Using Installer (Recommended)
1. Double-click the downloaded `YC.Wallet-Setup.exe`
2. If a security warning appears, click "Yes" or "Run"
3. Follow the installation wizard prompts:
   - Choose installation location (recommended to use default)
   - Select start menu folder
   - Choose whether to create desktop shortcut
4. Click "Install" to start installation
5. Click "Finish" when installation is complete

#### Method 2: Using Portable Version
1. Extract the downloaded `YC.Wallet.zip` file
2. Move the extracted folder to your desired location
3. Double-click `YC.WalletApp.exe` in the folder to start the program

### Step 3: Verify Installation
1. Find the "YC.Wallet" icon on desktop or start menu
2. Double-click to start the program
3. If the program starts normally, installation is successful

---

## 🚀 First Launch

### Starting the Program
1. Double-click the "YC.Wallet" icon on desktop
2. Or find "YC.Wallet" in start menu and click
3. The program will automatically check system environment after startup

### Environment Check
The program will automatically check:
- ✅ Whether .NET 8.0 Runtime is installed
- ✅ Whether network connection is normal
- ✅ Whether system permissions are sufficient

If .NET 8.0 Runtime is missing, the program will prompt to download and install.

### Language Selection
On first launch, the program will display a language selection interface:
- **中文** - Select Chinese interface
- **English** - Select English interface

Click "OK" to continue after selection.

---

## 👤 User Registration & Login

### Register New Account

#### Step 1: Enter Registration Page
1. Click "Register" button on login interface
2. Or click "Create New Account" link

#### Step 2: Fill Registration Information
1. **Username**: Enter your desired username (at least 6 characters)
2. **Email Address**: Enter your valid email address
3. **Password**: Set password (8-20 characters, including uppercase, lowercase, numbers, special characters)
4. **Confirm Password**: Enter password again to confirm
5. Click "Register" button

#### Step 3: Verify Email
1. Check your email for verification email
2. Click the verification link in the email
3. Or enter the verification code from the email

### Using Google Login

#### Step 1: Select Google Login
1. Click "Login with Google" button on login interface
2. The program will open browser for Google authorization

#### Step 2: Authorize Login
1. Select your Google account in browser
2. Click "Allow" to authorize YC.Wallet to access your account information
3. After successful authorization, it will automatically return to program and login

### Login to Account

#### Method 1: Using Username and Password
1. Enter username on login interface
2. Enter password
3. Click "Login" button

#### Method 2: Using Google Account
1. Click "Login with Google" button
2. Select your Google account
3. Complete authorization to login automatically

---

## 🔐 Wallet Management

### Create New Wallet

#### Step 1: Enter Wallet Management
1. After login, click "Wallet Management" tab on main interface
2. Or click "Wallet" option in left menu

#### Step 2: Create Wallet
1. Click "Create New Wallet" button
2. Enter wallet name (e.g., "My Main Wallet")
3. Set wallet password (remember this password)
4. Click "Create" button

#### Step 3: Backup Mnemonic Phrase (Important!)
1. The program will display 12 mnemonic words
2. **Please copy and safely store these mnemonic words**
3. Mnemonic phrase is the only way to recover wallet
4. Click "I Have Backed Up" to confirm

#### Step 4: Verify Mnemonic Phrase
1. The program will ask you to enter mnemonic phrase for verification
2. Enter the mnemonic words you just copied in order
3. Click "Verify" button

### Import Existing Wallet

#### Method 1: Import Using Mnemonic Phrase
1. Click "Import Wallet" in wallet management interface
2. Select "Import Using Mnemonic Phrase"
3. Enter your 12 mnemonic words
4. Set wallet name and password
5. Click "Import" button

#### Method 2: Import Using Private Key
1. Select "Import Using Private Key"
2. Enter your private key (be careful with security, don't leak to others)
3. Set wallet name and password
4. Click "Import" button

### Manage Wallets

#### View Wallet Information
1. Click the wallet you want to view in wallet list
2. View wallet address, balance and other information
3. Can copy wallet address for receiving tokens

#### Export Wallet
1. Select the wallet to export
2. Click "Export Wallet" button
3. Enter wallet password to confirm
4. Select export format (mnemonic phrase or private key)
5. Safely save the exported information

#### Delete Wallet
1. Select the wallet to delete
2. Click "Delete Wallet" button
3. Enter wallet password to confirm
4. Confirm deletion operation

---

## 🪙 Token Management

### View Token Balance

#### Step 1: Select Wallet
1. Select the wallet to view in wallet management interface
2. Click "View Details" or directly click the wallet

#### Step 2: View Token List
1. View all tokens in wallet details page
2. Including SOL (native token) and SPL tokens
3. Display token name, balance, value and other information

### Add Tokens

#### Step 1: Enter Token Management
1. Click "Token Management" in wallet details page
2. Or click "Add Token" button

#### Step 2: Search Tokens
1. Enter token name or contract address in search box
2. Program will display matching token list
3. Select the token to add

#### Step 3: Confirm Addition
1. View token information (name, symbol, contract address, etc.)
2. Click "Add" button
3. Token will appear in your token list

### Custom Tokens

#### Step 1: Manual Addition
1. Click "Manually Add Token"
2. Enter token contract address
3. Enter token name and symbol
4. Click "Add" button

#### Step 2: Verify Token
1. Program will verify token contract address
2. If verification succeeds, token will be added to list
3. If verification fails, please check if address is correct

---

## 💰 Transaction Operations

### Send Tokens

#### Step 1: Select Token to Send
1. Select the token to send in wallet details page
2. Click "Send" button
3. Or click "Send" icon next to the token

#### Step 2: Fill Transaction Information
1. **Recipient Address**: Enter or paste recipient's wallet address
2. **Send Amount**: Enter the amount of tokens to send
3. **Memo** (Optional): Add transaction memo information

#### Step 3: Confirm Transaction
1. Carefully check transaction information:
   - Whether recipient address is correct
   - Whether send amount is correct
   - Whether transaction fee is reasonable
2. Click "Confirm Send" button

#### Step 4: Enter Password
1. Enter wallet password to confirm transaction
2. Program will display transaction progress
3. Wait for transaction confirmation

### Receive Tokens

#### Step 1: Get Receiving Address
1. Click "Receive" in wallet details page
2. Or click "Copy" button next to wallet address
3. Copy wallet address

#### Step 2: Share Address
1. Send wallet address to sender
2. Can share via email, message, etc.
3. Ensure address is complete and correct

#### Step 3: Wait for Arrival
1. After sender completes transaction, tokens will arrive automatically
2. Can view transaction status in transaction records
3. Can use after confirming arrival

### View Transaction Records

#### Step 1: Enter Transaction Records
1. Click "Transaction Records" in wallet details page
2. Or click "History" tab

#### Step 2: View Transaction List
1. View all transaction records
2. Including sent, received, failed and other statuses
3. Display transaction time, amount, status and other information

#### Step 3: View Transaction Details
1. Click any transaction record to view details
2. Including transaction hash, confirmations, fees and other information
3. Can copy transaction hash for query

---

## ⚙️ Settings & Preferences

### Language Settings

#### Change Interface Language
1. Click settings icon in top-right corner of main interface
2. Select "Language Settings"
3. Select Chinese or English
4. Click "OK" to save settings

### Security Settings

#### Change Password
1. Select "Security Settings" in settings
2. Click "Change Password"
3. Enter current password
4. Enter new password and confirm
5. Click "Save" button

#### Enable Two-Factor Authentication
1. Select "Two-Factor Authentication" in security settings
2. Follow prompts to set up Google Authenticator
3. Scan QR code or enter key
4. Enter verification code to confirm

### Network Settings

#### Select Network
1. Select "Network Settings" in settings
2. Select the network to use:
   - **Mainnet** - Official network, using real tokens
   - **Testnet** - Test network, using test tokens
3. Click "Save" button

### Display Settings

#### Currency Unit
1. Select "Display Settings" in settings
2. Select currency unit (USD, CNY, etc.)
3. Select decimal places
4. Click "Save" button

#### Theme Settings
1. Select interface theme:
   - **Light Theme** - White background
   - **Dark Theme** - Black background
   - **Auto** - Follow system settings
2. Click "Save" button

---

## ❓ Frequently Asked Questions

### Installation Issues

#### Q: What if the program cannot start?
A:
1. Check if .NET 8.0 Runtime is installed
2. Run program as administrator
3. Check if antivirus software is blocking the program
4. Re-download and install the program

#### Q: Insufficient permissions during installation?
A:
1. Right-click installer, select "Run as administrator"
2. Or close antivirus software and reinstall
3. Check Windows User Account Control settings

### Login Issues

#### Q: What if I forgot my password?
A:
1. Click "Forgot Password" on login interface
2. Enter the email address used during registration
3. Check email and click reset link
4. Set new password

#### Q: What if Google login fails?
A:
1. Check if network connection is normal
2. Ensure browser allows pop-up windows
3. Clear browser cache and retry
4. Try logging in with username and password

### Wallet Issues

#### Q: Wallet shows 0 balance?
A:
1. Check if network connection is normal
2. Confirm if wallet address is correct
3. Wait a few minutes and refresh balance
4. Check if correct network is selected

#### Q: Transaction not confirmed for a long time?
A:
1. Check network congestion
2. Confirm if transaction fee is sufficient
3. Wait longer (usually minutes to hours)
4. If not confirmed for a long time, try resending

### Token Issues

#### Q: Can't see a certain token?
A:
1. Add the token in token management
2. Check if token contract address is correct
3. Confirm if token actually exists
4. Refresh token list

#### Q: Token balance display error?
A:
1. Click "Refresh Balance" button
2. Wait a few minutes and refresh again
3. Check network connection
4. Restart program and retry

---

## 🔒 Security Reminders

### Password Security
- ✅ Use strong passwords (including uppercase, lowercase, numbers, special characters)
- ✅ Don't use birthdays, phone numbers and other easily guessed passwords
- ✅ Change passwords regularly
- ❌ Don't tell passwords to anyone
- ❌ Don't use same password on multiple websites

### Mnemonic Phrase Security
- ✅ Copy mnemonic phrase on paper and store safely
- ✅ Can make multiple backups
- ✅ Store in fireproof, waterproof, secure location
- ❌ Don't store mnemonic phrase on computer or phone
- ❌ Don't tell mnemonic phrase to anyone
- ❌ Don't send mnemonic phrase anywhere

### Private Key Security
- ✅ Private key is the only credential to access wallet
- ✅ Please keep safe, don't leak to anyone
- ❌ Don't store private key on computer or phone
- ❌ Don't send private key to anyone
- ❌ Don't enter private key on insecure websites

### Network Security
- ✅ Use secure network connections
- ✅ Avoid important operations on public WiFi
- ✅ Regularly update system and security software
- ❌ Don't enter wallet information on untrusted websites
- ❌ Don't click suspicious links or download unknown files

### Transaction Security
- ✅ Carefully verify recipient address
- ✅ Confirm transaction amount and fees
- ✅ Use small test transactions for verification
- ❌ Don't believe false information like "free giveaways"
- ❌ Don't make transactions on untrusted platforms

---

## 📞 Technical Support

### Get Help
- **Official Documentation**: https://docs.ycwallet.com
- **GitHub Issues**: https://github.com/your-username/YC.Wallet/issues
- **Email Support**: support@ycwallet.com
- **Discord Community**: your-discord-server

### Feedback & Suggestions
If you encounter problems during use or have improvement suggestions, welcome to contact us through:
1. Submit issues on GitHub Issues
2. Send email to support@ycwallet.com
3. Discuss in Discord community

---

## 📝 Update Log

### Version 1.0.0 (2025-01-XX)
- ✅ Initial release
- ✅ Support wallet creation and import
- ✅ Support SOL and SPL token transfers
- ✅ Support Google OAuth login
- ✅ Support Chinese and English interfaces
- ✅ Support multi-wallet management

---

**Thank you for using YC.Wallet! If you find this wallet helpful, please give us a Star! ⭐**
