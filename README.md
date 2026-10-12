# Boarding House Management System

## Project Description
The **Boarding House Management System** is a full-stack Web Application designed to streamline operations for dormitory and boarding house owners while simplifying the room inquiry and tenancy management process for students and boarders. The system automates room reservations, lease tracking, payment processing, and maintenance issue reporting.

---

## Objectives
* **Automate Reservations & Room Assignment:** Eliminate manual ledger booking by providing real-time room availability, online reservation requests, and landlord approval workflows.
* **Streamline Financial Management:** Digitally track rent payments, payment statuses, and transaction reference numbers.
* **Improve Maintenance Handling:** Allow tenants to report facility maintenance issues directly through the portal and enable admins to track resolution progress[cite: 9].
* **Centralize User Roles:** Maintain structured access control for Students/Tenants, Administrators, and Landlords[cite: 9].

---

## Key Features
* **User Authentication & Role Management:** Secure sign-up/login with role-based dashboard access for Students, Admins, and Landlords[cite: 9].
* **Room Directory & Availability:** View rooms by category, rate, capacity, and current occupancy status[cite: 9].
* **Online Room Reservation:** Students/Tenants can submit reservation requests for open rooms[cite: 9].
* **Lease & Room Assignment:** Landlords/Admins approve reservations and generate lease agreements for active move-ins[cite: 9].
* **Payment Tracking System:** Log rent payments with payment method selection and transaction reference verification[cite: 9].
* **Maintenance Request Portal:** Submit and monitor repair requests with status tracking (`Pending`, `In Progress`, `Resolved`)[cite: 9].

---

## Technology Stack
* **Backend:** ASP.NET Core Web API (.NET 8/9) / C#
* **OR Mapper:** Entity Framework Core (Pomelo MySQL)
* **Database:** MySQL Server
* **Frontend:** Razor Components / Blazor / HTML5, CSS3, JavaScript
* **Migration Tooling:** EF Core CLI (`dotnet ef`)

---

## Project Structure
```text
BoardingHouseManagementSystem/
├── Controllers/              # API Endpoints (Reservations, Payments, Maintenance)
├── Data/
│   ├── AppDbContext.cs       # Entity Framework Context & Configuration
│   ├── Migrations/           # Database Migrations
│   └── schema.sql            # Raw SQL schema backup
├── Models/                   # Data Entities (User, Room, Lease, Payment, MaintenanceRequest)
├── Properties/
│   └── launchSettings.json   # Server execution & port configurations
├── appsettings.json          # Database connection strings & application config
└── Program.cs                # Dependency injection & pipeline setup
