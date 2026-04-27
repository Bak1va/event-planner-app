# Event Planner App

A full-stack event planning application built with a **.NET 9 backend** and **Angular frontend**, providing users with a comprehensive solution to create, manage, and track events.

## Project Overview

Event Planner App is a modern web application that enables users to efficiently plan and manage events. The application features user authentication, event creation and management, and a responsive user interface for seamless event planning experience.

## Key Functionalities

### Authentication & User Management
- User registration and login with secure authentication
- JWT token-based authentication
- User profile management
- Role-based access control

### Event Management
- Create, read, update, and delete events
- Event details including title, description, date, and time
- Event listing and filtering
- Event card view for quick overview
- Event data validation

### User Features
- Browse all users in the system
- View user profiles
- User account management

### Data Validation
- Comprehensive input validation for users and events
- Business logic validation layer
- Error handling and meaningful error messages

## Technology Stack

### Backend
- **.NET 9** - Modern C# framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **SQL Database** - Data persistence

### Frontend
- **Angular** - Modern TypeScript framework
- **TypeScript** - Type-safe development
- **ESLint** - Code quality and linting

### Testing
- **xUnit** - Unit and integration testing framework
- **Unit Tests** - Service and controller layer testing
- **Integration Tests** - End-to-end API testing

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration

## Getting Started

### Prerequisites
- .NET 9 SDK or later
- Node.js 18+ and npm
- Docker and Docker Compose (optional, for containerized setup)
- SQL Server or compatible database

### Backend Configuration

#### 1. Database Setup
Update the connection string in `backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EventPlannerDb;User Id=sa;Password=YourPassword;"
  }
}
```

#### 2. Install Dependencies
```bash
cd backend
dotnet restore
```

#### 3. Apply Database Migrations
```bash
dotnet ef database update
```

#### 4. Run the Backend
```bash
dotnet run
```
The backend API will be available at `https://localhost:5000` (or the port specified in `launchSettings.json`)

### Frontend Configuration

#### 1. Install Dependencies
```bash
cd frontend
npm install
```

#### 2. Configure API Endpoint
Update the API base URL in `frontend/src/services/event.service.ts` if needed:
```typescript
private apiUrl = 'http://localhost:5000/api';
```

#### 3. Run the Frontend
```bash
npm start
```
The frontend application will be available at `http://localhost:4200`

### Docker Setup (Alternative)

Run the entire application using Docker Compose without installing dependencies locally.

#### Prerequisites for Docker Setup
- Docker Desktop installed and running
- Docker Compose CLI

#### Quick Start with Docker

1. **Clone/Navigate to the project directory:**
```bash
cd d:\Facultate\event-planner-app
```

2. **Build and start all services:**
```bash
docker-compose up --build
```

This will:
- Build the backend .NET 9 Docker image
- Build the frontend Angular Docker image
- Create and start the database container
- Start the backend service on port 5000
- Start the frontend service on port 3000
- Initialize the database with migrations

3. **Access the application:**
- Frontend: `http://localhost:3000`
- Backend API: `http://localhost:5000/api`
- API Documentation: `http://localhost:5000/swagger`

#### Docker Compose Commands

**Start services in the background:**
```bash
docker-compose up -d
```

**View running containers:**
```bash
docker-compose ps
```

**View logs from all services:**
```bash
docker-compose logs
```

**View logs from a specific service:**
```bash
docker-compose logs backend
docker-compose logs frontend
```

**Stop running services:**
```bash
docker-compose stop
```

**Start stopped services:**
```bash
docker-compose start
```

**Restart services:**
```bash
docker-compose restart
```

**Stop and remove all containers, networks:**
```bash
docker-compose down
```

**Stop and remove containers, networks, and volumes:**
```bash
docker-compose down -v
```

#### Docker Environment Configuration

The `docker-compose.yml` file contains environment variables for:
- Database connection strings
- API port settings
- Frontend port settings

Modify these in `docker-compose.yml` if you need to change default ports or database settings.

#### Rebuilding After Code Changes

If you make changes to the code:

**For backend changes:**
```bash
docker-compose up --build backend
```

**For frontend changes:**
```bash
docker-compose up --build frontend
```

**For both:**
```bash
docker-compose up --build
```

#### Troubleshooting Docker

**Containers not starting:**
```bash
docker-compose logs
```

**Port already in use:**
- Change ports in `docker-compose.yml`
- Or kill process using the port

**Database connection issues:**
- Ensure database container is running: `docker-compose ps`
- Check database container logs: `docker-compose logs database`

**Complete reset:**
```bash
docker-compose down -v
docker-compose up --build
```

## Project Structure

```
event-planner-app/
├── backend/                 # .NET 9 API
│   ├── Controllers/         # API endpoints
│   ├── Services/            # Business logic
│   ├── Models/              # Domain models
│   ├── DTOs/                # Data transfer objects
│   ├── Data/                # Database context
│   └── appsettings.json     # Configuration
├── Backend.Tests/           # Unit and integration tests
├── frontend/                # Angular application
│   ├── src/
│   │   ├── components/      # Angular components
│   │   ├── services/        # API services
│   │   └── DTOs/            # Frontend DTOs
│   └── package.json         # Dependencies
└── docker-compose.yml       # Docker orchestration
```

## Testing

### Run Backend Tests
```bash
cd backend
dotnet test ../Backend.Tests/Backend.Tests.csproj
```

### Run Frontend Tests
```bash
cd frontend
npm test
```

### Test Coverage
- **Unit Tests** - Service and controller logic validation
- **Integration Tests** - API endpoint testing with database

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login user

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Events
- `GET /api/events` - Get all events
- `GET /api/events/{id}` - Get event by ID
- `POST /api/events` - Create new event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

## Configuration Files

### Backend Configuration
- **appsettings.json** - Database connection strings and application settings
- **launchSettings.json** - Launch profiles and port configuration

### Frontend Configuration
- **angular.json** - Angular CLI configuration
- **tsconfig.json** - TypeScript configuration
- **eslint.config.js** - Code linting rules

## Troubleshooting

### Backend Issues
- Ensure .NET 9 SDK is installed: `dotnet --version`
- Check database connection string in `appsettings.json`
- Verify SQL Server is running

### Frontend Issues
- Ensure Node.js 18+ is installed: `node --version`
- Clear node_modules and reinstall: `rm -r node_modules && npm install`
- Check that the backend API is running and accessible

### Docker Issues
- Ensure Docker daemon is running
- Check port availability (3000, 5000, etc.)
- View container logs: `docker-compose logs`

## License

This project is part of the educational curriculum.

## Support

For issues or questions, please open an issue in the repository.