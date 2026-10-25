# AGENTS.md — AI Agent Guidance & Repository Rules

This document provides mandatory guidance for autonomous AI coding agents (Antigravity, Claude Code, GitHub Copilot, Cursor, Windsurf, Devin) operating in `akazad13/cloud-work`.

---

## 1. System Identity & Architecture

**Product**: `Cloud Work` — Suite of practical cloud-native and serverless projects demonstrating C# / .NET integrations with Amazon Web Services (AWS).
- **Runtime**: .NET Core / .NET 8 / 10, C#.
- **Cloud Provider**: Amazon Web Services (AWS).
- **Core SDKs**: Official AWS SDK for .NET (`AWSSDK.S3`, `AWSSDK.DynamoDBv2`, `AWSSDK.Extensions.NETCore.Setup`, `Amazon.Lambda.Core`).

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

---

## 3. Commands Cheat Sheet

```bash
# Build all projects in a specific folder
cd AWSS3Connect/AWSS3Connect
dotnet build

cd ../../DynamoDBAndS3/DynamoDBAndS3
dotnet build

# Run local web application
dotnet run

# Package Lambda function
dotnet lambda package
```

---

## 4. Verification Gates for AI Agents

Before submitting code changes or finishing a turn:
1. Verify all modified projects build cleanly with `dotnet build`.
2. Confirm that no AWS credentials or account IDs were exposed in code or config files.
