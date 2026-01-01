# NotesApp

## New Aspire and BlazorServer Hosted Application

### A tool to Create and Manage Notes using a MongoDb to store documents. It includes unit, integration, and architecture tests with the integration tests using a docker container for the tests it creates a MongoDb database to ensure clean isolated data for the tests

****

[![Build and Test](https://github.com/mpaulosky/NotesApp/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mpaulosky/NotesApp/actions/workflows/build-and-test.yml)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/mpaulosky/NotesApp?logo=github)
![GitHub last commit (branch)](https://img.shields.io/github/last-commit/mpaulosky/NotesApp/main?label=last%20commit%20main&logo=github)

****

[![codecov](https://codecov.io/gh/mpaulosky/NotesApp/branch/main/graph/badge.svg)](https://codecov.io/gh/mpaulosky/NotesApp)
[![License](https://img.shields.io/github/license/mpaulosky/NotesApp?logo=github)](https://github.com/mpaulosky/NotesApp/blob/main/LICENSE.txt)

****

[![Issues Open](https://img.shields.io/github/issues/mpaulosky/NotesApp?logo=github)](https://github.com/mpaulosky/NotesApp/issues)
[![Issues Closed](https://img.shields.io/github/issues-closed/mpaulosky/NotesApp?logo=github)](https://github.com/mpaulosky/NotesApp/issues?q=is%3Aissue+is%3Aclosed)
[![Pull Requests Open](https://img.shields.io/github/issues-pr/mpaulosky/NotesApp?logo=github)](https://github.com/mpaulosky/NotesApp/pulls)
[![Pull Requests Closed](https://img.shields.io/github/issues-pr-closed/mpaulosky/NotesApp?logo=github)](https://github.com/mpaulosky/NotesApp/pulls?q=is%3Apr+is%3Aclosed)

****

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
- [Architecture](#-architecture)
- [Running the Application](#️-running-the-application)
- [Testing](#-testing)
- [Documentation](#-documentation)
- [Contributing](#-contributing)
- [Security](#-security)
- [License](#-license)

## ✨ Features

- **Modern Blazor UI** - Interactive server-side rendering with Blazor Web App
- **Secure Authentication** - Enterprise-grade authentication with Auth0
- **AI Integration** - OpenAI integration for enhanced note-taking capabilities
- **Cloud-Ready** - Built with .NET Aspire for cloud-native deployment
- **MongoDB Database** - NoSQL database with support for both local and MongoDB Atlas
- **CQRS Pattern** - Clean architecture with Command Query Responsibility Segregation using MediatR
- **Vertical Slice Architecture** - Features organized by business capability
- **Comprehensive Testing** - Unit tests, integration tests, and architecture tests
- **Code Coverage** - Integrated with Codecov for coverage tracking
- **CI/CD** - Automated build and test pipeline with GitHub Actions

## 🛠 Technology Stack

### Core Technologies

- **.NET 10.0** - Latest .NET platform with C# 13
- **Blazor Web App** - Interactive server-side rendering UI framework
- **MongoDB** - NoSQL database (supports local and MongoDB Atlas)
- **MongoDB Entity Framework Core** - EF Core provider for MongoDB
- **Auth0** - Enterprise authentication and authorization
- **OpenAI SDK** - AI integration capabilities
- **.NET Aspire** - Cloud-ready application orchestration

### Architecture & Patterns

- **Vertical Slice Architecture** - Features organized by business capability
- **CQRS Pattern** - Separation of read and write operations using MediatR
- **Dependency Injection** - Built-in ASP.NET Core DI container

### Testing & Quality

- **xUnit** - Unit and integration test framework
- **FluentAssertions** - Fluent assertion library
- **NSubstitute** - Mocking library
- **bUnit** - Blazor component testing
- **NetArchTest.Rules** - Architecture constraint testing
- **Coverlet** - Code coverage collection
- **GitHub Actions** - CI/CD automation

### UI & Styling

- **Tailwind CSS** - Utility-first CSS framework
- **Blazored.LocalStorage** - Local storage for Blazor

See [REFERENCES.md](docs/REFERENCES.md) for complete technology references and links.

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (for Tailwind CSS compilation)
- [MongoDB](https://www.mongodb.com/try/download/community) (local) or [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) account
- [Auth0](https://auth0.com/) account for authentication
- [Visual Studio 2022](https://visualstudio.microsoft.com/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/mpaulosky/NotesApp.git
   cd NotesApp
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Install Node.js packages** (for Tailwind CSS)

   ```bash
   cd src/Notes.Web
   npm install
   cd ../..
   ```

### Configuration

#### 1. Auth0 Setup

Configure Auth0 authentication in user secrets:

```bash
cd src/Notes.Web
dotnet user-secrets set "Auth0:Domain" "your-tenant.us.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "your-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"
dotnet user-secrets set "Auth0:Audience" "https://api.yourapp.com"
```

See [SECURITY.md](docs/SECURITY.md) for detailed Auth0 configuration instructions.

#### 2. MongoDB Setup

**Option A: Local MongoDB** (default)

The application will use a local MongoDB container when run with .NET Aspire.

**Option B: MongoDB Atlas** (cloud)

```bash
cd src/Notes.AppHost
dotnet user-secrets set "ConnectionStrings:mongodb" "mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority"
```

See [MONGODB_ATLAS_SETUP.md](docs/MONGODB_ATLAS_SETUP.md) for detailed MongoDB Atlas setup.

#### 3. OpenAI Setup (Optional)

For AI features:

```bash
cd src/Notes.Web
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
dotnet user-secrets set "OpenAI:ChatModel" "gpt-4o-mini"
dotnet user-secrets set "OpenAI:EmbeddingModel" "text-embedding-3-small"
```

## 🏗 Architecture

This project follows **Vertical Slice Architecture** with **CQRS pattern**:

```text
src/
├── Notes.AppHost/              # .NET Aspire orchestration
├── Notes.ServiceDefaults/      # Shared service configurations
├── Notes.Web/                  # Main Blazor application
│   ├── Components/             # Blazor components
│   │   ├── Account/           # Authentication components
│   │   ├── Layout/            # Layout components
│   │   └── Pages/             # Page components
│   ├── Data/                  # MongoDB models and context
│   ├── Features/              # Vertical slices (CQRS)
│   │   └── Notes/             # Notes feature
│   │       ├── CreateNote/    # Command + Handler
│   │       ├── UpdateNote/    # Command + Handler
│   │       ├── DeleteNote/    # Command + Handler
│   │       ├── GetNoteDetails/# Query + Handler
│   │       └── ListNotes/     # Query + Handler
│   └── Services/              # Application services
└── Shared/                    # Shared models and utilities

Tests/
├── Notes.Tests.Architecture/       # Architecture constraint tests
├── Notes.Web.Tests.Integration/    # Integration tests
└── Notes.Web.Tests.Unit/           # Unit tests
```

Each feature slice contains:

- Command/Query record
- Response DTO record
- Handler implementation
- All contained in a single consolidated file

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for detailed architecture guidelines.

## ▶️ Running the Application

### Using .NET Aspire (Recommended)

```bash
dotnet run --project src/Notes.AppHost/Notes.AppHost.csproj
```

This will start the application with the Aspire dashboard for monitoring and management.

### Direct Run

```bash
cd src/Notes.Web
dotnet run
```

Navigate to `https://localhost:5001` (or the port shown in the console).

## 🧪 Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Projects

```bash
# Unit tests
dotnet test Tests/Notes.Web.Tests.Unit

# Integration tests
dotnet test Tests/Notes.Web.Tests.Integration

# Architecture tests
dotnet test Tests/Notes.Tests.Architecture
```

### Code Coverage

Code coverage is automatically collected and uploaded to Codecov during CI/CD. To generate coverage locally:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverage reports are uploaded to [Codecov](https://codecov.io/gh/mpaulosky/NotesApp).

## 📚 Documentation

Additional documentation is available in the `docs/` folder:

- **[CONTRIBUTING.md](docs/CONTRIBUTING.md)** - Contribution guidelines, code style, and development workflow
- **[SECURITY.md](docs/SECURITY.md)** - Security policy, Auth0 configuration, and vulnerability reporting
- **[REFERENCES.md](docs/REFERENCES.md)** - Complete technology stack and reference links
- **[MONGODB_ATLAS_SETUP.md](docs/MONGODB_ATLAS_SETUP.md)** - MongoDB Atlas cloud setup guide

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](docs/CONTRIBUTING.md) before submitting pull requests.

### Quick Start for Contributors

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Follow the coding standards in [CONTRIBUTING.md](docs/CONTRIBUTING.md)
4. Write tests for your changes
5. Ensure all tests pass (`dotnet test`)
6. Commit with clear messages
7. Push to your fork and submit a pull request

### Code of Conduct

We have adopted a code of conduct from the Contributor Covenant. Contributors are expected to adhere to this code. Please report unwanted behavior to [matthew.paulosky@outlook.com](mailto:matthew.paulosky@outlook.com).

## 🔒 Security

We take security seriously. Please review our [Security Policy](docs/SECURITY.md) for:

- Security features and authentication setup
- Vulnerability reporting process
- Best practices for contributors
- Production deployment recommendations

**To report a security vulnerability**, please email [matthew.paulosky@outlook.com](mailto:matthew.paulosky@outlook.com) with the subject line `[SECURITY] NotesApp Vulnerability Report`. **Do NOT open a public GitHub issue.**

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

---

**Maintainer:** [Matthew Paulosky](https://github.com/mpaulosky)

**Repository:** [https://github.com/mpaulosky/NotesApp](https://github.com/mpaulosky/NotesApp)
