# Event Registration App

## Overview

This Event Registration App is a simple CRUD application developed using ASP.NET Core Blazor with .NET 8. The application allows users to view available events, manage events, and register for events. Authentication and role-based authorization ensure that only authorized users can perform certain actions, such as creating or deleting events.

## Features

- **Landing Page**: Displays a list of available events.
- **Event Management**: Admins can create, read, update, and delete events.
- **User Registration**: Users can register for events, with each registration generating a unique reference number.
- **Role-Based Authorization**: Admins have full control over event management, while regular users can only view and register for events.
- **Validation**: Implemented using Blazored.FluentValidation for robust form validation.

## Technology Stack

- **ASP.NET Core Blazor (Server-Side)**: Provides a rich, interactive UI.
- **SQLite**: A lightweight, file-based database.
- **Entity Framework Core (EFCore)**: Manages database access and ORM.
- **Bootstrap**: Ensures a responsive and modern UI design.
- **ASP.NET Identity**: Handles authentication and authorization.
- **Blazored.FluentValidation**: Manages validation logic within Blazor forms.

## Installation

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or any other IDE supporting .NET 8

### Steps

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   
## Database Structure
The database schema includes tables for managing users, roles, events, and registrations:
   ```mermaid
erDiagram
    IdentityUser {
        string Id PK
        string UserName
        string Email
        string PasswordHash
    }

    IdentityRole {
        string Id PK
        string Name
    }

    IdentityUserRole {
        string UserId FK
        string RoleId FK
    }

    EVENT {
        int EventId PK
        string EventName
        datetime EventDate
        int TotalSeats
        int AvailableSeats
    }

    REGISTRATION {
        int RegistrationId PK
        string UserId FK
        int EventId FK
        string ReferenceNumber
        datetime RegistrationDate
    }

    IdentityUser ||--o{ IdentityUserRole : "has"
    IdentityRole ||--o{ IdentityUserRole : "includes"
    IdentityUser ||--o{ REGISTRATION : "can register"
    EVENT ||--o{ REGISTRATION : "includes"
```

## Key Design Choices

### Entity Framework Core

- **Rationale**: EFCore is chosen due to the project's size and moderate need for SQL performance. Its ORM capabilities simplify database interactions.

### Async Patterns

- **Guidance**: Follows async/await patterns based on guidelines by David Fowler, ensuring responsive and scalable operations.

### Server Interactivity

- **Configuration**:
    ```csharp
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    app.MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode();
    ```

### Unique Reference Generation

- **Implementation**:
    ```csharp
    public string GenerateUniqueReference()
    {
        return Guid.NewGuid().ToString("N").ToUpper();
    }
    ```

### Validation

- **Blazored.FluentValidation**: Provides robust form validation.
- **Form Setup**:
    ```razor
    <EditForm Model="@registrationModel" OnValidSubmit="@HandleValidSubmit">
        <FluentValidationValidator />
        <DataAnnotationsValidator />
        <!-- Form Fields Here -->
    </EditForm>
    ```

### Authentication and Authorization

- **ASP.NET Identity**: Manages user authentication and role-based access control.
- **Role-Based Authorization**:
    ```csharp
    services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    });
    ```

## Usage

### Admin Actions

- **Create Event**: Accessible only by users with the "Admin" role.
- **Edit/Delete Event**: Only "Admin" users can modify or remove events.

### User Actions

- **View Events**: All users can view the list of available events.
- **Register for Events**: Users can register for events, provided seats are available.

## Resources

- [Role-Based Authorization in Blazor - C# Corner](https://www.c-sharpcorner.com/article/role-based-authorization-in-blazor/)
- [Async Patterns in ASP.NET Core - David Fowler](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios)

