# NexusEnroll - University Course Enrollment System

NexusEnroll is a modernized, scalable University Course Enrollment System built as a proof-of-concept (POC) for the Software Architecture (SCS 2303) module[cite: 2]. 

Designed to replace a legacy monolith, this application implements a **Modular Monolith** architecture to simulate a **Microservices ecosystem**. It enforces strict domain decoupling and utilizes a Database-per-Service pattern via in-memory repositories.

## Architecture & Design Patterns

The core business logic completely decouples the Student, Faculty, and Administrator domains. To ensure a robust and maintainable system, the following Object-Oriented Design Patterns are heavily utilized:

*   **Strategy Pattern:** Isolates enrollment validation rules (Capacity, Prerequisites, Time Conflicts) to enforce the Open/Closed Principle.
*   **State Pattern:** Manages the strict lifecycle transitions of Faculty grade submissions (Pending -> Submitted -> Rejected).
*   **Observer Pattern:** Powers the decoupled Event Bus, asynchronously notifying Waitlists and Academic Advisors of critical enrollment changes.
*   **Factory Method:** Abstracts the creation of notification objects (such as Email Notifications), allowing the system to easily introduce new notification types (like SMS) without modifying existing client code[cite: 3].
*   **Facade Pattern:** Provides a unified, simplified interface (`EnrollmentFacade`) for the API Gateway to interact with complex underlying subsystems.

## Core Modules

1.  **Student Portal:** Course catalog browsing, real-time seat tracking, dynamic schedule validation, and course enrollment/dropping.
2.  **Faculty Portal:** Real-time class roster management and secure end-of-semester grade submissions.
3.  **Administrator Portal:** Administrative "Force-Add" enrollment overrides and live capacity analytics (>90% full alerts).
4.  **Live Event Bus:** Real-time notification log tracking decoupled system events.

## Getting Started

The system includes a C# .NET Minimal API backend serving a responsive, vanilla HTML/JS Single-Page Application (SPA) frontend. No external database configuration is required.

### Prerequisites
*   .NET 8 SDK installed on your local environment.

### Installation & Execution

1. Navigate to the project directory:
   ```bash
   cd NexusEnroll2
   dotnet run --project NexusEnroll.Api
