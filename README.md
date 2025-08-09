# YC.Wallet - Enterprise Solana Desktop Wallet

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Material%20Design-green.svg)](https://materialdesigninxaml.net/)
[![Solana](https://img.shields.io/badge/Solana-C%23%20SDK-orange.svg)](https://solana.com/)
[![License](https://img.shields.io/badge/License-Private-red.svg)](LICENSE)

> The first enterprise-grade Solana desktop wallet built for C# developers, featuring multi-language support, Google OAuth login, SPL token management, and enterprise-level security.

## 📖 Documentation

- [中文文档 / Chinese Documentation](README.cn.md)
- [中文用户手册 / Chinese User Manual](document/用户使用说明书.md)
- [English User Manual](document/User-Manual-EN.md)

## 📸 Interface Preview

### Login & Registration
<div align="center">
<img src="Images/Login.png" alt="Login Interface" width="400"/>
<img src="Images/Register.png" alt="Registration Interface" width="400"/>
</div>

### Wallet Management
<div align="center">
<img src="Images/WalletManage.png" alt="Wallet Management" width="800"/>
</div>

### Token Management
<div align="center">
<img src="Images/TokenDefManage.png" alt="Token Management" width="800"/>
</div>

### Transaction Management
<div align="center">
<img src="Images/TransferManage.png" alt="Transaction Management" width="800"/>
</div>

## 🌟 Project Highlights

### 🚀 Technical Advantages
- **First C# Desktop Wallet** - Fills the desktop gap in Solana ecosystem
- **Enterprise Security** - Suitable for enterprise users and institutions
- **Multi-language Support** - Complete internationalization solution
- **Google OAuth** - Innovative user experience design

### 💼 Business Value
- **Desktop Solution** - Complements mobile wallet ecosystem
- **Developer Tools** - Provides Solana development framework for C# developers
- **Enterprise Application** - Suitable for enterprise users and institutions

## 📋 Features

### 🔐 Wallet Management
- ✅ Create new wallets
- ✅ Import existing wallets
- ✅ Export wallet private keys
- ✅ Wallet list management
- ✅ Wallet account management

### 💰 Transaction Features
- ✅ SPL token transfers
- ✅ Transaction record queries
- ✅ Transaction status tracking
- ✅ Transaction fee calculation
- ✅ Batch transaction support

### 🪙 Token Management
- ✅ SPL token support
- ✅ Token issuance and management
- ✅ Token type definitions
- ✅ Token information management
- ✅ Token list display

### 👤 User System
- ✅ Google OAuth login
- ✅ Multi-language interface (Chinese/English)
- ✅ User registration and verification
- ✅ Account information management

### 🛡️ Security Features
- ✅ Local SQLite database storage
- ✅ Private key security management
- ✅ Transaction signature verification
- ✅ Enterprise-level security protection

## 🛠️ Technology Stack

### Core Framework
- **.NET 8.0** - Latest .NET framework
- **WPF** - Windows Presentation Foundation
- **MVVM Architecture** - Using Prism framework

### UI Components
- **MaterialDesignThemes.Wpf** - Material Design styling
- **MahApps.Metro** - Modern Metro styling
- **HandyControl** - Enhanced WPF controls

### Blockchain Integration
- **Solana C# SDK** - Solana blockchain interaction
- **Nethereum** - Ethereum blockchain support

### Data Processing
- **FreeSql** - ORM database operations
- **SQLite** - Local database storage
- **Newtonsoft.Json** - JSON serialization

### Third-party Services
- **Google.Apis.Auth** - Google OAuth authentication
- **Mapster** - Object mapping tool

## 📦 Installation and Usage

### System Requirements
- Windows 10/11
- .NET 8.0 Runtime
- At least 2GB available memory

### Quick Start

1. **Download and Install**
   ```bash
   # Download the latest version from Release page
   https://github.com/linbin524/YC.Wallet/releases
   ```

2. **First Run**
   - Launch the application
   - Select language (Chinese/English)
   - Login with Google account or register new account

3. **Create Wallet**
   - Click "Create Wallet"
   - Set wallet password
   - Backup mnemonic phrase (Important!)

4. **Start Using**
   - Import SOL or SPL tokens
   - Perform transfer transactions
   - Manage tokens and accounts

## 🏗️ Project Structure

```
YC.Wallet/
├── YC.WalletApp/              # Main application
│   ├── Views/                 # View layer
│   ├── ViewModels/            # ViewModel layer
│   ├── Controls/              # Custom controls
│   ├── Extension/             # Extension features
│   └── Assets/                # Resource files
├── YC.ApplicationService/      # Application service layer
├── YC.Model/                  # Data model layer
├── YC.Common/                 # Common utilities layer
└── Sdk/                       # SDK integration layer
    ├── YC.NethereumService/   # Ethereum service
    └── YC.SolanaSdkService/   # Solana service
```

## 🔧 Development Environment

### Requirements
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Git

### Local Development

1. **Clone Project**
   ```bash
   git clone https://github.com/linbin524/YC.Wallet.git
   cd YC.Wallet
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build Project**
   ```bash
   dotnet build YC.WalletApp.sln
   ```

4. **Run Application**
   ```bash
   dotnet run --project YC.WalletApp
   ```

## 📚 Documentation

### Core Modules
- [Wallet Management Module](docs/Wallet-Management.md)
- [Transaction Processing Module](docs/Transaction-Processing.md)
- [Token Management Module](docs/Token-Management.md)
- [Multi-language System](docs/Multi-Language.md)

### API Documentation
- [Solana SDK Integration](docs/Solana-Integration.md)
- [Google OAuth Integration](docs/Google-OAuth.md)
- [Database Operations](docs/Database-Operations.md)

### Best Practices
- [MVVM Architecture Guide](docs/MVVM-Architecture.md)
- [Security Development Guidelines](docs/Security-Guidelines.md)
- [Performance Optimization Tips](docs/Performance-Optimization.md)

## 🤝 Contributing

We welcome all forms of contributions!

### Ways to Contribute
1. **Report Issues** - Report bugs or suggest new features in GitHub Issues
2. **Submit Code** - Fork the project and submit Pull Requests
3. **Improve Documentation** - Help improve documentation and tutorials
4. **Community Support** - Answer questions from other users

### Development Standards
- Follow C# coding conventions
- Use MVVM architecture pattern
- Add appropriate unit tests
- Update related documentation

## 📄 License

This project is private and all rights reserved.

## 🏆 Project Highlights

### Technical Innovation
- **First C# Desktop Wallet** - Fills the gap in Solana ecosystem
- **Enterprise Architecture** - MVVM + Prism framework
- **Multi-language Support** - Complete internationalization solution
- **Google OAuth** - Innovative user experience

### Business Value
- **Desktop Solution** - Complements mobile wallet ecosystem
- **Developer Tools** - Provides Solana development framework for C# developers
- **Enterprise Application** - Suitable for enterprise users and institutions

### Ecosystem Contribution
- **Open Source Code** - Available for community learning and use
- **Technical Documentation** - Detailed development documentation and best practices
- **Developer Tools** - Provides Solana development framework for C# developers

## 📞 Contact Us

- **Project URL**: https://github.com/linbin524/YC.Wallet
- **Issue Feedback**: https://github.com/linbin524/YC.Wallet/issues
- **Email Contact**: contact@ycwallet.com
- **Discord**: your-discord-username
- **Twitter**: @YCWallet

## 🙏 Acknowledgments

Thanks to the following open source projects and technical communities:

- [Solana Labs](https://solana.com/) - Excellent blockchain platform
- [Material Design](https://material.io/) - Excellent design language
- [Prism Framework](https://prismlibrary.com/) - MVVM architecture support
- [.NET Community](https://dotnet.microsoft.com/) - Powerful development platform

---

⭐ If this project helps you, please give us a Star!
