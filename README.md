<div align="center">

# ☁️ Cloud Work

### Cloud-native architectures, AWS integration (Lambda, S3, DynamoDB), and serverless .NET / C# applications

[![CI Status](https://img.shields.io/github/actions/workflow/status/akazad13/cloud-work/ci.yml?branch=main&style=flat-square&logo=githubactions&logoColor=white)](https://github.com/akazad13/cloud-work/actions)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%2F%20Core-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![AWS](https://img.shields.io/badge/AWS-Cloud-FF9900?style=flat-square&logo=amazonaws&logoColor=white)](https://aws.amazon.com/)
[![AWS Lambda](https://img.shields.io/badge/Serverless-Lambda-FF9900?style=flat-square&logo=awslambda&logoColor=white)](https://aws.amazon.com/lambda/)
[![Amazon S3](https://img.shields.io/badge/Storage-S3-569A31?style=flat-square&logo=amazons3&logoColor=white)](https://aws.amazon.com/s3/)
[![Amazon DynamoDB](https://img.shields.io/badge/NoSQL-DynamoDB-4053D6?style=flat-square&logo=amazondynamodb&logoColor=white)](https://aws.amazon.com/dynamodb/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](LICENSE)

[Overview](#-overview) •
[Architecture](#-architecture) •
[Projects Inventory](#-projects-inventory) •
[Tech Stack](#-tech-stack) •
[Quick Start](#-quick-start) •
[License](#-license)

</div>

---

## 📌 Overview

**Cloud Work** is a curated suite of practical cloud-native and serverless projects demonstrating how to build, integrate, and deploy scalable C# and .NET applications on Amazon Web Services (AWS). It covers object storage (S3), serverless compute (Lambda), NoSQL data persistence (DynamoDB), and event-driven image and video processing pipelines.

### Key Highlights
- ⚡ **Serverless Compute**: Event-driven AWS Lambda functions in .NET handling asset transformations and async workloads.
- 🗄️ **Object & Document Storage**: Integration with Amazon S3 bucket lifecycles and DynamoDB single-digit millisecond NoSQL access.
- 🖼️ **Media Pipelines**: Automated serverless image processing and GIF generation triggered directly by S3 upload events.
- 🏢 **Cloud Migration Blueprint**: Full-stack ASP.NET Core cloud portal (`TakeItToTheCloud`) structured for cloud deployment.

---

## 🏛 Architecture

```mermaid
flowchart LR
    Client["Client / User"] --> S3["Amazon S3<br/>Asset Bucket"]
    S3 -.->|S3 Event Trigger| Lambda["AWS Lambda<br/>(.NET Serverless Worker)"]
    Lambda --> DDB[("Amazon DynamoDB<br/>Metadata & Status")]
    Lambda --> OutS3["Amazon S3<br/>Processed Output"]
```

> [!NOTE]
> All projects utilize the official AWS SDK for .NET (`AWSSDK.S3`, `AWSSDK.DynamoDBv2`, `Amazon.Lambda.Core`).

---

## 📂 Projects Inventory

| Project | Focus | Key Services |
| :--- | :--- | :--- |
| **`AWSS3Connect`** | Object storage operations: bucket lifecycle, file upload, download, and streaming | Amazon S3, .NET SDK |
| **`DynamoDBAndS3`** | Relational metadata storage paired with blob management | Amazon S3, DynamoDB |
| **`ServerlessAppForCreatingGIF`** | Event-driven Lambda function generating animated GIFs from uploaded media | AWS Lambda, Amazon S3 |
| **`ServerlessAppsForImgProcessing`** | Automated image transformation pipeline (resizing, optimization) on S3 triggers | AWS Lambda, Amazon S3 |
| **`ServerlessApplicationsWithAWS`** | Serverless REST API architectures and CloudWatch integration | API Gateway, Lambda |
| **`TakeItToTheCloud`** | Full-stack ASP.NET Core cloud portal ready for container and cloud hosting | ASP.NET Core, Bootstrap |

---

## 🛠 Tech Stack

| Domain | Technologies |
| :--- | :--- |
| **Language & Frameworks** | C#, .NET 8 / .NET Core, ASP.NET Core |
| **Cloud Provider** | Amazon Web Services (AWS) |
| **Compute & Serverless** | AWS Lambda, API Gateway |
| **Storage & Databases** | Amazon S3, Amazon DynamoDB |
| **SDKs & Libraries** | AWS SDK for .NET, AWSSDK.S3, AWSSDK.DynamoDBv2, Amazon.Lambda.Core |

---

## 🚀 Quick Start

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/)
- [AWS CLI](https://aws.amazon.com/cli/) configured with valid credentials (`aws configure`)
- [Amazon.Lambda.Tools](https://github.com/aws/aws-extensions-for-dotnet-cli) (`dotnet tool install -g Amazon.Lambda.Tools`)

### 1. Clone & Browse Projects
```bash
git clone https://github.com/akazad13/cloud-work.git
cd cloud-work
```

### 2. Run a Sample Console App
```bash
cd AWSS3Connect
dotnet restore
dotnet run
```

### 3. Deploy a Serverless Function
```bash
cd ../ServerlessAppsForImgProcessing
dotnet lambda deploy-function ProcessImageFunction
```

---

## 👤 Author

**Md. Abul Kalam**
- GitHub: [@akazad13](https://github.com/akazad13)
- LinkedIn: [Md. Abul Kalam](https://linkedin.com/in/akazad13)

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
