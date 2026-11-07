# ADR-0003: Testing Paradigm and Agentic Verification for Cloud & Serverless Solutions

## Status
Accepted

## Date
2026-11-07

## Context
The `cloud-work` repository contains cloud-native .NET solutions integrating with Amazon Web Services (AWS) services including Amazon S3, Amazon DynamoDB, AWS Lambda, Amazon Rekognition, and Amazon SQS. 

Serverless and cloud-native systems present distinct testing challenges:
1. **External Infrastructure Coupling**: Relying on live AWS environments or persistent cloud resources for regular test runs introduces network latency, flakiness, billing costs, deployment overhead, and complex teardown/cleanup requirements.
2. **Credential Management Hazards**: Requiring live cloud credentials (`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, STS session tokens) in automated test environments exposes repositories to credential leakage risks, especially across CI runners and multi-agent development sessions.
3. **Agentic Test-Driven Development (TDD) Demands**: Autonomous coding agents (such as Antigravity, Claude Code, GitHub Copilot) rely on fast (< 2 seconds), deterministic, local feedback loops using the **Red-Green-Refactor** pattern. Unreliable external dependencies break automated reasoning loops and halt agent autonomy.
4. **Mock Brittleness & Interaction Anti-Patterns**: Over-reliance on strict interaction mocks (e.g., verifying exact method invocation counts, parameter matchers, or call sequencing) couples tests tightly to internal implementation details. Innocent internal refactoring often causes false-positive test failures even when functional behavior remains correct.

A standardized, deterministic, and credential-free testing paradigm is required to enable rapid, safe development for both human developers and autonomous AI agents.

## Decision
We establish a unified testing paradigm and verification standard for all cloud and serverless components across `cloud-work`:

### 1. In-Memory Test Doubles for Cloud SDKs
- All service interactions with AWS (e.g., `IAmazonS3`, `IAmazonDynamoDB`, `IAmazonRekognition`) and custom utility abstractions (e.g., `IAWSS3Helper`) must be injected via constructor dependency injection.
- Unit and component tests must use lightweight in-memory test doubles (stubs, fakes, or mocks via Moq/NSubstitute) that replicate AWS service contracts in-process.
- Real network calls to AWS endpoints (`s3.amazonaws.com`, `dynamodb.*.amazonaws.com`) are prohibited during automated test execution.

### 2. Event-Driven Lambda Payload Fixtures
- AWS Lambda functions triggered by cloud events (such as S3 `ObjectCreated` events or API Gateway HTTP requests) must be tested against realistic, self-contained event payload fixtures.
- Fixtures represent valid JSON event models deserialized into AWS Lambda event types (e.g., `Amazon.Lambda.S3Events.S3Event`, `APIGatewayProxyRequest`, or domain event envelopes like `EventNotification`).
- Fixtures must be isolated per test or constructed using fluent test builder patterns, avoiding shared mutable state across test executions.

### 3. Core Testing Invariants
All tests authored in this repository must satisfy three inviolable invariants:

1. **Zero Live Cloud Credentials**:
   - Tests must run cleanly in completely offline, air-gapped, or zero-configuration environments without requiring AWS credentials, profiles, or environment variables.
   - Any test that attempts to resolve credentials from IAM metadata endpoints, AWS STS, or environment secrets without an explicit local fake will fail verification.
2. **State over Interaction Assertions**:
   - Assertions must prioritize the **resulting state, output payload, and domain behavior** over brittle verification of SDK call invocations.
   - For example: assert that the generated thumbnail image has expected dimensions (e.g., 200x200) and format, that the DynamoDB entity dictionary contains required partition/sort key attributes, or that the returned HTTP payload matches the expected schema.
   - Restrict interaction verifications (`Times.Once()`) exclusively to fire-and-forget side effects where no state change or return value is observable.
3. **DAMP (Descriptive And Meaningful Phrases) Test Design**:
   - Prioritize test clarity, readability, and diagnostic value over extreme DRY (Don't Repeat Yourself) deduplication.
   - Each test must clearly communicate its scenario via descriptive naming conventions (`Method_Scenario_ExpectedResult`) and explicit **Arrange-Act-Assert (AAA)** structure.
   - Test data builders and fixtures should make relevant inputs and expected outcomes immediately visible without navigating complex multi-layer inheritance hierarchies.

### 4. Agentic TDD Verification Workflow
Autonomous AI agents modifying or extending functionality must follow the Test-Driven Development (TDD) cycle:
1. **RED**: Author a failing unit test capturing the desired capability, bug reproduction (Prove-It pattern), or edge case using in-memory test doubles.
2. **GREEN**: Implement minimal production code to satisfy the test.
3. **REFACTOR**: Clean up architecture, streamline code, and ensure adherence to performance and readability guidelines while keeping tests green.
4. **VERIFY**: Run `dotnet test` locally before committing or pushing changes.

## Alternatives Considered

### LocalStack / Docker Testcontainers
- **Pros**: High fidelity emulation of AWS services running in local Docker containers.
- **Cons**: Substantial startup latency (10–30+ seconds), heavy CPU/memory overhead, requires a running Docker daemon, and introduces platform-specific virtualization issues on developer workstations and restricted CI runners.
- **Rejected**: Too heavyweight for rapid unit testing and sub-second agentic TDD loops. Reserved as an optional consideration for full integration smoke suites.

### Live Cloud Sandbox Testing (AWS Account per Developer)
- **Pros**: Direct validation against real AWS service behavior and IAM policy evaluation.
- **Cons**: Requires active cloud credentials, generates AWS billing costs, vulnerable to network timeouts, rate limits, and concurrency race conditions during parallel builds.
- **Rejected**: Incompatible with the Zero Live Cloud Credentials invariant and sub-second agentic verification requirements.

### Strict Mock-Heavy Interaction Testing
- **Pros**: Simple to set up using standard mocking libraries.
- **Cons**: Tightly couples test cases to internal call sequences and method parameter ordering. Refactorings that preserve behavior but adjust SDK call sequences fail tests, creating developer friction and maintenance overhead.
- **Rejected**: Deprecated in favor of state-based assertions and domain output verification.

## Consequences

### Positive
- **Lightning-Fast Execution**: Tests execute locally in milliseconds, delivering immediate feedback to developers and autonomous AI agents.
- **Security & Zero Leakage**: Elimination of AWS credential dependencies in test suites completely prevents accidental secret leaks.
- **High Refactoring Resilience**: State-based assertions allow internal refactoring and optimization of AWS SDK calls without breaking behavioral test suites.
- **Autonomous Agent Compatibility**: Unlocks seamless automated TDD loops for coding agents operating without cloud infrastructure access.

### Negative / Trade-offs
- In-memory test doubles do not detect remote AWS API changes, subtle IAM policy restrictions, or service-specific throttling limits. These risks are mitigated via targeted pre-production integration smoke tests.
