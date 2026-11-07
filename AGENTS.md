# AGENTS.md — AI Agent Guidance & Repository Rules

This document provides mandatory guidance for autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot, Cursor, Windsurf, Devin) operating in `akazad13/cloud-work`.

---

## 1. System Identity & Architecture

**Product**: `Cloud Work` — Suite of practical cloud-native and serverless projects demonstrating C# / .NET integrations with Amazon Web Services (AWS).
- **Runtime**: .NET Core / .NET 8 / 10, C#.
- **Cloud Provider**: Amazon Web Services (AWS).
- **Core SDKs**: Official AWS SDK for .NET (`AWSSDK.S3`, `AWSSDK.DynamoDBv2`, `AWSSDK.Extensions.NETCore.Setup`, `Amazon.Lambda.Core`).
- **Architecture Decisions**: Architectural context, invariants, and accepted patterns are cataloged in [`docs/adr/`](docs/adr/README.md). Consult them before proposing structural changes.

### Project Layout:
- `AWSS3Connect/`: ASP.NET Core MVC application showing S3 bucket CRUD, object streaming, and presigned URLs.
- `DynamoDBAndS3/`: ASP.NET Core Web API integrating S3 file uploads with DynamoDB metadata cataloging.
- `ServerlessAppForCreatingGIF/`: AWS Lambda serverless function compiling image batches into animated GIFs.
- `ServerlessApplicationsWithAWS/`: Foundational serverless patterns, API Gateway triggers, and Lambda routing.
- `ServerlessAppsForImgProcessing/`: S3-triggered event pipeline resizing and optimizing uploaded image assets.
- `TakeItToTheCloud/`: Multi-tier enterprise ASP.NET Core application blueprint structured for cloud lift-and-shift.

---

## 2. Inviolable Architectural Invariants

1. **Zero Secret Hardcoding**:
   - Never commit AWS access keys (`AKIA...`), secret access keys, or AWS session tokens.
   - All AWS clients must rely on `AWSSDK.Extensions.NETCore.Setup` default credential provider chains (Environment Variables, AWS CLI profile, or IAM Roles / Task Execution Roles).
2. **Proper Stream & Memory Disposal**:
   - S3 `GetObjectResponse.ResponseStream` and file upload memory streams must be wrapped in `using` declarations to prevent socket exhaustion and memory leaks.
3. **Async / Await Purity**:
   - Use async AWS SDK calls (`PutObjectAsync`, `GetItemAsync`, `ScanAsync`) exclusively. Avoid synchronous `.Result` or `.Wait()`.
4. **Cross-Platform Image Processing Compliance**:
   - Standardize on `SixLabors.ImageSharp` (v3.x) for all image mutations and GIF generation.
   - Never introduce `System.Drawing` or `System.Drawing.Common`, as they cause `PlatformNotSupportedException` on non-Windows OSes (including AWS Lambda's Amazon Linux runtimes). See [ADR-0002](docs/adr/0002-aws-serverless-architecture-and-dependency-modernization.md).
5. **Decoupled Event-Driven Compute**:
   - Offload heavy image and video manipulation to asynchronous S3 event triggers invoking AWS Lambda workers instead of synchronous HTTP request handlers.
6. **Recursive Execution Loop Prevention**:
   - Lambda functions writing output back to S3 must target a separate destination bucket or key prefix that is excluded from the function's event trigger subscription.
7. **Architecture Decision Record (ADR) Compliance**:
   - Any significant architectural modification, framework upgrade, or cloud dependency change must be captured in `docs/adr/` following the format specified in [ADR-0001](docs/adr/0001-record-architecture-decisions.md) and indexed in `docs/adr/README.md`.

---

## 3. Testing Paradigm & Agentic Verification

All testing across this repository follows the standards codified in [ADR-0003](docs/adr/0003-testing-paradigm-and-agentic-verification.md) and the `test-driven-development` skill:

### Core Testing Invariants:
1. **Zero Live Cloud Credentials**:
   - Automated tests must run 100% offline and locally without requiring AWS credentials (`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`), AWS profiles, or network connectivity.
   - Use in-memory test doubles (`IAmazonS3`, `IAmazonDynamoDB`, `IAmazonRekognition`, `IAWSS3Helper`) via constructor injection.
2. **State over Interaction Assertions**:
   - Assert against output artifacts, converted image streams, DynamoDB attribute dictionaries, and returned status models.
   - Avoid brittle mock call count verifications (`Times.Once()`) unless testing fire-and-forget side effects with no observable state change.
3. **DAMP (Descriptive And Meaningful Phrases) Test Design**:
   - Favor clear, readable, self-contained Arrange-Act-Assert (AAA) tests over heavy DRY abstractions that conceal intent.
   - Use strongly-typed Lambda event fixtures (`Amazon.Lambda.S3Events.S3Event`, `APIGatewayProxyRequest`, `EventNotification`) representing realistic cloud events.

### Agentic TDD Workflow:
1. **RED**: Write a failing xUnit test reproducing a bug (Prove-It pattern) or specifying new behavior before touching implementation code.
2. **GREEN**: Write minimal production code required to satisfy the test.
3. **REFACTOR**: Clean up, optimize, and streamline while keeping tests green.
4. **VERIFY**: Execute `dotnet test` across relevant projects to validate correctness.

---

## 4. Commands Cheat Sheet

```bash
# Build all projects in a specific folder
cd AWSS3Connect/AWSS3Connect
dotnet build

cd ../../DynamoDBAndS3/DynamoDBAndS3
dotnet build

# Run tests across solution or specific project
dotnet test

# Run local web application
dotnet run

# Package Lambda function
dotnet lambda package
```

---

## 5. Verification Gates for AI Agents

Before submitting code changes or finishing a turn:
1. Verify all modified projects build cleanly with `dotnet build`.
2. Run automated test suites with `dotnet test` and ensure all tests pass.
3. Confirm that no AWS credentials or account IDs were exposed in code or config files.
4. If new architectural patterns, major dependencies, or cloud services were introduced, ensure a corresponding ADR is created in `docs/adr/` and indexed in `docs/adr/README.md`.
