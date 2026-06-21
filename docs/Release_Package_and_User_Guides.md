# II. Release Package & User Guides

## 1. Deliverable Package

| No. | Deliverable Item | Description | Version |
|---|---|---|---|
| 1 | Project Schedule/Tracking | Jira/Trello board for tracking progress and milestones | v1.0 |
| 2 | Project Backlog | List of user stories and functional requirements (FE-01 to FE-15) | v1.0 |
| 3 | Source Codes | EventHub system source code (.NET 9, C# 13, MVC/Razor Pages, Blazor) | v1.0 |
| 4 | Database Script(s) | EF Core Migrations and initial Seed Data scripts | v1.0 |
| 5 | Final Report Document | Final summary report, architecture, and technical decisions | v1.0 |
| 6 | Test Cases Document | List of test cases (Unit tests and Manual tests) | v1.0 |
| 7 | Defects List | List of recorded bugs and their status | v1.0 |
| 8 | Issues List | List of technical issues (Technical Debt/Impediments) | v1.0 |
| 9 | Slide | Final project presentation slides | v1.0 |

## 2. Installation Guides

### 2.1 System Requirements

**Hardware:**
- CPU: Dual-core 2.0 GHz or higher
- RAM: Minimum 4GB (8GB recommended)
- Storage: Minimum 500MB free space

**Software:**
- OS: Windows 10/11, macOS, Linux
- Runtime: .NET 9.0 SDK
- Database: SQL Server 2022 or PostgreSQL 15+
- Message/Cache: Redis Server
- Browser: Latest version of Chrome, Firefox, Edge, Safari

### 2.2 Installation Instruction

1. **Clone the repository:**
   ```bash
   git clone <repository_url>
   cd EventHub
   ```

2. **Database Configuration:**
   - Ensure SQL Server / PostgreSQL is running.
   - Update the `ConnectionStrings` in `appsettings.json` and `appsettings.Development.json` within the MVC/API project.

3. **Run EF Core Migrations:**
   ```bash
   dotnet ef database update --project DAL --startup-project MVC
   ```

4. **Start Redis:**
   - Ensure Redis Server is running on the default port `6379`.

5. **Build and Run the system:**
   ```bash
   dotnet build
   dotnet run --project MVC
   ```
   The system will be available at `http://localhost:5000` or `https://localhost:5001`.

## 3. User Manual

### 3.1 Overview

**UniEvent Hub** is an online event management and ticket booking platform. The system supports multiple roles with core functional groups:
- **Attendees:** Search for events (FE-03), book tickets directly (FE-04), track history, and interact with Live Q&A (FE-15).
- **Organizers:** Manage events (FE-02), set venue capacity limits (FE-05), and monitor the Live Dashboard (FE-10).
- **Admins:** Manage RBAC (FE-01) and Admin Control Panel (FE-09).

### 3.2 Workflow 1: Event Management (Organizer)

**Purpose:**
Allows Organizers to create new events, set seating limits, and publish events to the system.

**Detailed Instructions:**
1. **Login:** Access the system with an Organizer account.
2. **Open Dashboard:** Select **Event Management** on the navigation bar.
3. **Create Event:** Click **Create New Event**.
4. **Enter Information:**
   - Fill in Event Name, Description, and Time.
   - Select Venue (the system automatically checks capacity limits according to FE-05).
   - Add Category and Tags.
5. **Publish:** Click **Submit**. The event transitions to the "Published" state.

### 3.3 Workflow 2: Search and Book Tickets (Attendee)

**Purpose:**
Allows attendees to find suitable events and book tickets in real-time with anti-duplication processing (Concurrency).

**Detailed Instructions:**
1. **Search Event:** Use the search bar and filters on the homepage (FE-03) to find events by name, tag, or time.
2. **View Details:** Click on the event card to view detailed information and the number of available tickets.
3. **Proceed to Book:**
   - Select the number of tickets to buy.
   - Click **Book Now**.
   - *Note:* The system applies Live Ticket Booking (FE-04), transactions may be rejected if the tickets were just purchased by someone else (DbUpdateConcurrencyException will be handled safely).
4. **Confirmation:** Check email to receive confirmation notifications and automated reminders (FE-06).
