#  JiraLite Task Management API

A **Jira-like task management backend system** built with ASP.NET Core, designed to manage projects, tasks, workflows, and team collaboration efficiently.

---

##  Overview

JiraLite API is a backend system that simulates core features of tools like Jira and Trello.  
It provides a scalable and secure REST API for managing projects, tasks, users, and workflows.

The system is built with a focus on:
- Clean architecture
- Secure authentication
- Real-world business logic
- Scalable API design

---

##  Key Objectives

- Build a real-world backend system
- Implement authentication and authorization
- Design workflow-based task management
- Support multi-user collaboration
- Track system activity and history
- Provide analytics through dashboards

---

##  Features

---

###  Authentication & Authorization

- JWT-based authentication
- ASP.NET Core Identity integration
- Role-based authorization:
  - Admin
  - User
- Secure endpoint protection
- Token-based access control

---

###  Project Management

- Create new projects
- Update project details
- Delete projects
- Set project deadlines
- Assign users to projects
- Restrict access based on membership

---

###  Project Collaboration

- Add users to projects
- Remove users from projects
- Manage project members
- Enforce role-based permissions

---

###  Task Management

- Create tasks within projects
- Assign tasks to users
- Define task priority levels
- Set deadlines for tasks
- Track task status:
  - ToDo
  - InProgress
  - Done

---

### Task Workflow System

- Users can request tasks
- Admin can:
  - Approve requests 
  - Reject requests 
- Automatic assignment after approval
- Prevent duplicate or invalid requests

---

###  Comments System

- Add comments to tasks
- Support multi-user discussion
- Track user activity per task
- Link comments to tasks and projects

---

###  Attachments

- Upload files to tasks
- Manage task-related documents
- Store file metadata

---

###  Dashboard

#### Admin Dashboard:
- Total projects count
- Total tasks count
- Number of users
- Pending requests
- Task status distribution

#### User Dashboard:
- Assigned tasks
- User activity
- Pending requests
- Personal task statistics

---

###  Activity Logging System

Tracks all major system actions:

- Task creation
- Task updates
- Request approval/rejection
- Comments
- Role assignments

Features:
- Filter logs by task
- Filter logs by user
- Pagination support
- Audit tracking

---

##  Technologies Used

---

### Backend
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

---

### Authentication
- ASP.NET Identity
- JWT (JSON Web Tokens)

---

### API Design
- RESTful API architecture
- Model validation
- Exception handling

---

### Additional Concepts
- Role-based authorization
- Pagination & filtering
- Foreign key relationships
- Dependency Injection

---

##  Architecture

The system follows a structured architecture:

- Controllers → Handle HTTP requests
- Services/Logic → Business logic
- Data Layer → Entity Framework Core
- Models → Domain entities
- DTOs → Data transfer objects

---

##  API Endpoints

---

###  Authentication

- `POST /api/acc/register`
- `POST /api/acc/login`

---

###  Users

- `GET /api/acc/GetAll`
- `DELETE /api/acc/{userId}`
- `POST /api/acc/assign-role`

---

###  Projects

- `POST /api/project`
- `GET /api/project`
- `GET /api/project/{id}`
- `PUT /api/project/{id}`
- `PATCH /api/project/{id}`
- `DELETE /api/project/{id}`

---

###  Project Users

- `POST /api/projectuser/AddUser/{userId}`
- `DELETE /api/projectuser/remove/{projectId}/{userId}`

---

###  Tasks

- `POST /api/tasks`
- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `PATCH /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

---

###  Task Requests

- `POST /api/taskrequests`
- `PATCH /api/taskrequests/{id}`

---

###  Comments

- `POST /api/comments`
- `GET /api/comments/{taskId}`

---

###  Dashboard

- `GET /api/dashboard/admin`
- `GET /api/dashboard/user`

---

###  Activity Logs

- `GET /api/activitylogs`
- `GET /api/activitylogs/my`

---

##  Authentication Flow

1. User logs in using:
2. The API returns a JWT token.
3. Include the token in requests:
---

## 🗄️ Database

- SQL Server
- Code-first approach using Entity Framework Core
- Migrations supported

---

## 📊 Example Workflow

### ✅ Scenario

1. Admin creates project
2. Admin assigns users to project
3. Admin creates tasks
4. Users request tasks
5. Admin approves requests
6. Tasks get assigned automatically
7. Users work and add comments
8. Activity logs track all actions

---

## ⚠️ Validation Rules

- Deadlines must be in the future
- Duplicate task requests are prevented
- Users must belong to a project
- Only Admin can approve or reject task requests
- Role-based access control enforced

---

## 🔒 Security Features

- JWT token validation
- Role-based authorization
- Secure API endpoints
- Input validation
- Error handling

---

## 🧪 Testing

Tested using:
- Swagger UI
- Manual API testing
- Edge case validation

---

## 💡 Project Highlights

- Real-world workflow system
- Complex business logic implementation
- Secure authentication and authorization
- Activity tracking and audit logging
- Scalable and maintainable API design

---

## 👨‍💻 Author

Ahmed Saad  
GitHub: https://github.com/ahmeedsaad53

---

## 🚀 Future Improvements

- Email notifications
- Real-time updates (SignalR)
- API versioning
- Unit and integration testing
- Microservices-based architecture

---

## ⭐ Conclusion

This project demonstrates:

- Backend engineering skills
- API design expertise
- Security implementation
- Real-world problem solving
- System architecture thinking

---
