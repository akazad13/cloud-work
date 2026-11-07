# ADR-0002: AWS Serverless Architecture and Dependency Modernization

## Status
Accepted

## Date
2026-11-07

## Context
The `cloud-work` repository contains a suite of .NET solutions integrating with Amazon Web Services (AWS):
1. **Serverless Image & Media Processing Pipelines** (`ServerlessAppsForImgProcessing`, `ServerlessAppForCreatingGIF`, `ServerlessApplicationsWithAWS`):
   - Automated thumbnail generation and image mutation on S3 uploads.
   - Computer vision and label detection using Amazon Rekognition.
   - Batch image compilation into multi-frame animated GIFs.
2. **Object and Metadata Persistence** (`AWSS3Connect`, `DynamoDBAndS3`):
   - S3 bucket lifecycle management, object streaming, multipart transfers, and presigned URLs.
   - High-throughput NoSQL data modeling in Amazon DynamoDB paired with S3 blob references.
3. **Enterprise Cloud Application Blueprint** (`TakeItToTheCloud`):
   - Multi-tier ASP.NET Core web application with Entity Framework Core, SQL Server, ASP.NET Core Identity, and AWS SQS message queueing / S3 asset storage.

### Key Architectural & Dependency Challenges:
- **Cross-Platform Image Processing in AWS Lambda**: Historical .NET image libraries (e.g., `System.Drawing.Common`) relied on Windows GDI+ (`libgdiplus`), which Microsoft deprecated and made unsupported on non-Windows operating systems in .NET 6+. Running functions using `System.Drawing.Common` in AWS Lambda's standard Amazon Linux execution environments results in runtime `PlatformNotSupportedException` failures.
- **Synchronous Bottlenecks**: Performing compute-intensive image mutations or AI label inference synchronously within HTTP request-response cycles causes high latency, thread starvation, client timeouts, and server scaling bottlenecks.
- **AWS SDK Modernization**: Older AWS SDK packages had inconsistent sub-dependencies and lacked modern dependency injection primitives.
- **Cold-Start Performance**: Serverless .NET Lambda functions require assembly trimming, ready-to-run images, and high-performance JSON serialization to minimize cold-start latency.

## Decision
We establish the following architectural patterns, dependency baselines, and runtime invariants:

1. **Decoupled Event-Driven Serverless Media Pipeline**:
   - Web applications and clients upload raw media assets directly to an Amazon S3 source bucket.
   - Amazon S3 emits an `ObjectCreated` event (via S3 bucket notifications or Amazon EventBridge) which invokes an AWS Lambda function asynchronously.
   - The Lambda function (`ServerlessAppsForImgProcessing`, `ServerlessAppForCreatingGIF`) streams the image into memory, performs transformations, and writes the output to a distinct destination S3 bucket.
   - Metadata, tags, and Amazon Rekognition label inferences are written to Amazon DynamoDB for low-latency querying.
2. **Standardize on SixLabors.ImageSharp for Media Processing**:
   - Adopt `SixLabors.ImageSharp` (v3.2.0) and `SixLabors.ImageSharp.Processing` across all image-processing Lambda functions.
   - Ban `System.Drawing` and `System.Drawing.Common` across the codebase to ensure 100% managed, cross-platform execution on Linux-based AWS Lambda runtimes and Linux container images.
3. **AWS SDK for .NET v4 Modernization**:
   - Modernize all AWS packages to version 4 (`AWSSDK.Core` 4.x, `AWSSDK.S3` 4.x, `AWSSDK.DynamoDBv2` 4.x, `AWSSDK.Rekognition` 4.x, `AWSSDK.SQS` 4.x).
   - Use `AWSSDK.Extensions.NETCore.Setup` (4.x) for idiomatic dependency injection in ASP.NET Core applications.
   - Strictly rely on default AWS credential provider chains (IAM instance profiles, task execution roles, environment variables, or AWS CLI profiles). Hardcoded access keys are prohibited.
4. **Optimized AWS Lambda Configuration**:
   - Standardize on `Amazon.Lambda.Core` (3.4.x), `Amazon.Lambda.S3Events` (3.1.x), and `Amazon.Lambda.Serialization.SystemTextJson` (2.4.x).
   - Configure `<PublishReadyToRun>true</PublishReadyToRun>` and `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` in serverless project files to reduce cold-start latency and support the AWS .NET Lambda Mock Test Tool.
5. **Dual Persistence Pattern (S3 + DynamoDB)**:
   - Binary blob data is stored exclusively in Amazon S3.
   - Structured metadata, file details, and processing status are cataloged in Amazon DynamoDB using composite partition and sort keys (`USER_<username>`, `MOVIE_<id>`), ensuring single-digit millisecond latency without relational connection pooling bottlenecks in serverless functions.
6. **Recursive Execution Loop Prevention**:
   - Lambda functions that write transformed media back to S3 must output to a dedicated destination bucket or a distinct key prefix filtered out of the event trigger subscription to prevent infinite invocation recursion loops.

## Alternatives Considered

### Synchronous Processing in Web API / MVC Endpoints
- **Pros**: Immediate response to the caller in a single request; simpler invocation topology.
- **Cons**: High request latency, risk of HTTP 504 timeouts on large files, substantial memory spikes under concurrent uploads, and expensive server provisioning.
- **Rejected**: Event-driven serverless decoupling is significantly more resilient and cost-effective.

### SkiaSharp / Native C++ Wrapper Libraries
- **Pros**: High-speed native rendering.
- **Cons**: Requires compiling and deploying native platform binaries (`libSkiaSharp.so`) in Lambda layers, complicating CI/CD packaging and multi-architecture deployments.
- **Rejected**: `SixLabors.ImageSharp` is fully managed C#, highly optimized, and runs natively across all operating systems without native shared library dependencies.

### Relational Database (SQL Server / PostgreSQL) for Serverless Processing Metadata
- **Pros**: Familiar ACID semantics and relational foreign keys.
- **Cons**: Serverless Lambda functions spinning up hundreds of concurrent executions can easily exhaust relational database connection pools, requiring costly proxy infrastructure (e.g., RDS Proxy).
- **Rejected**: Amazon DynamoDB provides native HTTP-based API access, automatic horizontal scaling, and IAM-authenticated security with zero connection pooling constraints.

## Consequences

### Positive
- Fully elastic media processing that scales from zero to high volume with zero idle compute cost.
- Robust cross-platform execution on Amazon Linux Lambda runtimes without native library compilation issues.
- Fast Lambda cold-start times through ReadyToRun ahead-of-time compilation and System.Text.Json serialization.
- Decoupled architecture preventing compute-heavy tasks from degrading web application responsiveness.

### Negative / Neutral
- Asynchronous architecture requires clients to poll, listen to WebSockets, or subscribe to SNS/EventBridge notifications for processing completion.
- Multi-bucket infrastructure must be managed (e.g., input bucket vs. thumbnail bucket) to avoid infinite event recursion loops.
