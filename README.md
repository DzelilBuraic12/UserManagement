UserManagement is a user management web application with an ASP.NET Core Web API backend and an Angular frontend.
​

Tech stack
Backend: ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT authentication

Frontend: Angular, TypeScript, SCSS

Other: REST API, AutoMapper, FluentValidation (adjust to what you actually use)

Project structure
UserManagement/
  backend/     # ASP.NET Core API
  frontend/    # Angular application

cd backend

configure connection string in appsettings.Development.json
dotnet restore
dotnet ef database update   # if you use EF migrations
dotnet run

cd frontend
npm install
ng serve -o

Features
User registration and login with JWT-based authentication.

Create, update, delete and list users.

Role-based access (e.g. admin vs regular user – describe your roles/permissions here).
