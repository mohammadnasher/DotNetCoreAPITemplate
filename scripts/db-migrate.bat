@echo off
REM =================================================================
REM Database Migration Script (Windows)
REM =================================================================
REM This script handles database migrations for the application
REM Usage: scripts\db-migrate.bat [command] [options]
REM =================================================================

setlocal EnableDelayedExpansion

REM Default values
set "ENVIRONMENT=Development"
set "CONTEXT=ApplicationDbContext"
set "COMMAND="
set "ARGS="

REM Parse command line arguments
:parse_args
if "%~1"=="" goto :end_parse
if /i "%~1"=="-e" (
    set "ENVIRONMENT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--environment" (
    set "ENVIRONMENT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="-c" (
    set "CONTEXT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--context" (
    set "CONTEXT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="-h" goto :show_help
if /i "%~1"=="--help" goto :show_help
if "!COMMAND!"=="" (
    set "COMMAND=%~1"
    shift
    goto :parse_args
)
set "ARGS=!ARGS! %~1"
shift
goto :parse_args

:end_parse

REM Set environment variable
set "ASPNETCORE_ENVIRONMENT=%ENVIRONMENT%"

echo [INFO] Environment: %ENVIRONMENT%
echo [INFO] DbContext: %CONTEXT%

REM Check command and execute
if /i "%COMMAND%"=="add" goto :add_migration
if /i "%COMMAND%"=="update" goto :update_database
if /i "%COMMAND%"=="rollback" goto :rollback_migration
if /i "%COMMAND%"=="status" goto :show_status
if /i "%COMMAND%"=="reset" goto :reset_database
if /i "%COMMAND%"=="seed" goto :seed_database
if /i "%COMMAND%"=="drop" goto :drop_database
if "%COMMAND%"=="" goto :no_command
goto :unknown_command

:show_help
echo Database Migration Script (Windows)
echo.
echo Usage: %~nx0 [COMMAND] [OPTIONS]
echo.
echo Commands:
echo   add ^<name^>           Add a new migration with the specified name
echo   update              Update database to latest migration
echo   rollback ^<migration^> Rollback to specified migration
echo   status              Show migration status
echo   reset               Reset database (WARNING: This will delete all data!)
echo   seed                Seed database with initial data
echo   drop                Drop the database (WARNING: This will delete all data!)
echo.
echo Options:
echo   -e, --environment   Specify environment (Development, Staging, Production)
echo   -c, --context       Specify DbContext name (default: ApplicationDbContext)
echo   -h, --help          Show this help message
echo.
echo Examples:
echo   %~nx0 add CreateUserTable
echo   %~nx0 update
echo   %~nx0 status
echo   %~nx0 rollback InitialCreate
echo   %~nx0 reset -e Development
goto :end

:check_ef_tools
dotnet tool list -g | findstr "dotnet-ef" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet-ef tool is not installed.
    echo [INFO] Installing dotnet-ef tool globally...
    dotnet tool install --global dotnet-ef
    if errorlevel 1 (
        echo [ERROR] Failed to install dotnet-ef tool.
        exit /b 1
    )
    echo [SUCCESS] dotnet-ef tool installed successfully.
)
goto :eof

:add_migration
call :check_ef_tools
if "%ARGS%"=="" (
    echo [ERROR] Migration name is required for 'add' command.
    echo [INFO] Usage: %~nx0 add ^<migration_name^>
    exit /b 1
)
echo [INFO] Adding migration: %ARGS%
dotnet ef migrations add %ARGS% --context "%CONTEXT%" --output-dir src/Data/Migrations
if errorlevel 1 (
    echo [ERROR] Failed to add migration '%ARGS%'.
    exit /b 1
)
echo [SUCCESS] Migration '%ARGS%' added successfully.
goto :end

:update_database
call :check_ef_tools
echo [INFO] Updating database to latest migration...
dotnet ef database update --context "%CONTEXT%"
if errorlevel 1 (
    echo [ERROR] Failed to update database.
    exit /b 1
)
echo [SUCCESS] Database updated successfully.
goto :end

:rollback_migration
call :check_ef_tools
if "%ARGS%"=="" (
    echo [ERROR] Migration name is required for 'rollback' command.
    echo [INFO] Usage: %~nx0 rollback ^<migration_name^>
    exit /b 1
)
echo [WARNING] Rolling back to migration: %ARGS%
echo [WARNING] This action may result in data loss!
set /p "confirm=Are you sure you want to continue? (y/N): "
if /i not "%confirm%"=="y" (
    echo [INFO] Rollback cancelled.
    goto :end
)
dotnet ef database update "%ARGS%" --context "%CONTEXT%"
if errorlevel 1 (
    echo [ERROR] Failed to rollback database to '%ARGS%'.
    exit /b 1
)
echo [SUCCESS] Database rolled back to '%ARGS%' successfully.
goto :end

:show_status
call :check_ef_tools
echo [INFO] Showing migration history and status...
echo.
echo === Applied Migrations ===
dotnet ef migrations list --context "%CONTEXT%"
echo.
echo === Pending Migrations ===
REM This would show pending migrations if any exist
dotnet ef migrations list --context "%CONTEXT%" --no-connect 2>nul || echo Unable to check pending migrations without database connection
goto :end

:reset_database
call :check_ef_tools
echo [WARNING] This will completely reset the database and all data will be lost!
set /p "confirm=Are you sure you want to reset the database? (y/N): "
if /i not "%confirm%"=="y" (
    echo [INFO] Database reset cancelled.
    goto :end
)
echo [INFO] Dropping database...
dotnet ef database drop --context "%CONTEXT%" --force
echo [INFO] Updating database to latest migration...
dotnet ef database update --context "%CONTEXT%"
if errorlevel 1 (
    echo [ERROR] Failed to reset database.
    exit /b 1
)
echo [SUCCESS] Database reset successfully.
goto :end

:seed_database
echo [INFO] Seeding database with initial data...
REM Run the application with a special flag to seed data
REM This would typically be handled by a seeding endpoint or startup logic
dotnet run --no-build -- --seed-data
if errorlevel 1 (
    echo [ERROR] Failed to seed database.
    exit /b 1
)
echo [SUCCESS] Database seeded successfully.
goto :end

:drop_database
call :check_ef_tools
echo [WARNING] This will completely drop the database and all data will be lost!
set /p "confirm=Are you sure you want to drop the database? (y/N): "
if /i not "%confirm%"=="y" (
    echo [INFO] Database drop cancelled.
    goto :end
)
echo [INFO] Dropping database...
dotnet ef database drop --context "%CONTEXT%" --force
if errorlevel 1 (
    echo [ERROR] Failed to drop database.
    exit /b 1
)
echo [SUCCESS] Database dropped successfully.
goto :end

:no_command
echo [ERROR] No command specified.
goto :show_help

:unknown_command
echo [ERROR] Unknown command: %COMMAND%
goto :show_help

:end
endlocal