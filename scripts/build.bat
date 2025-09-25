@echo off
REM =================================================================
REM Build Script with Version Tagging and Docker Support (Windows)
REM =================================================================
REM This script handles building, testing, versioning, and Docker operations
REM Usage: scripts\build.bat [command] [options]
REM =================================================================

setlocal EnableDelayedExpansion

REM Configuration
set "SCRIPT_DIR=%~dp0"
set "PROJECT_ROOT=%SCRIPT_DIR%.."
set "PROJECT_NAME=DotNetCoreAPITemplate"
set "DOCKER_IMAGE_NAME=dotnet-api-template"
set "DOCKER_REGISTRY="
set "DEFAULT_VERSION_FILE=%PROJECT_ROOT%\version.txt"

REM Default values
set "ENVIRONMENT=Release"
set "SKIP_TESTS=false"
set "SKIP_DOCKER=false"
set "VERSION="
set "TAG_VERSION=false"
set "PUSH_DOCKER=false"
set "FORCE_VERSION=false"
set "COMMAND="

REM Parse command line arguments
:parse_args
if "%~1"=="" goto :end_parse
if /i "%~1"=="-v" (
    set "VERSION=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--version" (
    set "VERSION=%~2"
    shift
    shift
    goto :parse_args
)
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
if /i "%~1"=="-t" (
    set "TAG_VERSION=true"
    shift
    goto :parse_args
)
if /i "%~1"=="--tag" (
    set "TAG_VERSION=true"
    shift
    goto :parse_args
)
if /i "%~1"=="-p" (
    set "PUSH_DOCKER=true"
    shift
    goto :parse_args
)
if /i "%~1"=="--push" (
    set "PUSH_DOCKER=true"
    shift
    goto :parse_args
)
if /i "%~1"=="-f" (
    set "FORCE_VERSION=true"
    shift
    goto :parse_args
)
if /i "%~1"=="--force" (
    set "FORCE_VERSION=true"
    shift
    goto :parse_args
)
if /i "%~1"=="-s" (
    set "SKIP_TESTS=true"
    shift
    goto :parse_args
)
if /i "%~1"=="--skip-tests" (
    set "SKIP_TESTS=true"
    shift
    goto :parse_args
)
if /i "%~1"=="-d" (
    set "SKIP_DOCKER=true"
    shift
    goto :parse_args
)
if /i "%~1"=="--skip-docker" (
    set "SKIP_DOCKER=true"
    shift
    goto :parse_args
)
if /i "%~1"=="-r" (
    set "DOCKER_REGISTRY=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--registry" (
    set "DOCKER_REGISTRY=%~2"
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
echo [ERROR] Unknown option: %~1
goto :show_help

:end_parse

REM Default command
if "!COMMAND!"=="" set "COMMAND=build"

REM Set registry from environment if not specified
if "!DOCKER_REGISTRY!"=="" if not "!DOCKER_REGISTRY_URL!"=="" set "DOCKER_REGISTRY=!DOCKER_REGISTRY_URL!"

REM Get current version
call :get_current_version CURRENT_VERSION
if "!VERSION!"=="" set "VERSION=!CURRENT_VERSION!"

REM Print configuration
echo [INFO] Configuration:
echo [INFO]   Command: !COMMAND!
echo [INFO]   Environment: !ENVIRONMENT!
echo [INFO]   Version: !VERSION!
if not "!DOCKER_REGISTRY!"=="" (
    echo [INFO]   Docker Registry: !DOCKER_REGISTRY!
) else (
    echo [INFO]   Docker Registry: Not set
)
echo [INFO]   Skip Tests: !SKIP_TESTS!
echo [INFO]   Skip Docker: !SKIP_DOCKER!
echo [INFO]   Tag Version: !TAG_VERSION!
echo [INFO]   Push Docker: !PUSH_DOCKER!

REM Execute command
if /i "!COMMAND!"=="build" goto :cmd_build
if /i "!COMMAND!"=="test" goto :cmd_test
if /i "!COMMAND!"=="package" goto :cmd_package
if /i "!COMMAND!"=="docker-build" goto :cmd_docker_build
if /i "!COMMAND!"=="docker-push" goto :cmd_docker_push
if /i "!COMMAND!"=="release" goto :cmd_release
if /i "!COMMAND!"=="version" goto :cmd_version
if /i "!COMMAND!"=="clean" goto :cmd_clean
echo [ERROR] Unknown command: !COMMAND!
goto :show_help

:show_help
echo Build Script with Version Tagging and Docker Support (Windows)
echo.
echo Usage: %~nx0 [COMMAND] [OPTIONS]
echo.
echo Commands:
echo   build               Build the application (default)
echo   test                Run tests only
echo   package             Create NuGet packages
echo   docker-build        Build Docker image
echo   docker-push         Push Docker image to registry
echo   release             Full release process (build, test, version, docker)
echo   version             Update version and create git tag
echo   clean               Clean build artifacts
echo.
echo Options:
echo   -v, --version       Specify version (e.g., 1.2.3)
echo   -e, --environment   Build environment (Debug, Release) [default: Release]
echo   -t, --tag           Create and push git tag for the version
echo   -p, --push          Push Docker image to registry
echo   -f, --force         Force version update even if same version exists
echo   -s, --skip-tests    Skip running tests
echo   -d, --skip-docker   Skip Docker operations
echo   -r, --registry      Docker registry URL
echo   -h, --help          Show this help message
echo.
echo Examples:
echo   %~nx0 build
echo   %~nx0 release -v 1.2.3 -t -p
echo   %~nx0 docker-build -v 1.2.3
echo   %~nx0 test
echo   %~nx0 version -v 1.2.3 -t
goto :end

:get_current_version
if exist "!DEFAULT_VERSION_FILE!" (
    set /p %1=<"!DEFAULT_VERSION_FILE!"
) else (
    set "%1=1.0.0"
)
goto :eof

:update_version
set "new_version=%~1"
if "!new_version!"=="" (
    echo [ERROR] Version is required
    exit /b 1
)

call :get_current_version current_version
if "!current_version!"=="!new_version!" if "!FORCE_VERSION!"=="false" (
    echo [WARNING] Version !new_version! is the same as current version. Use --force to override.
    exit /b 1
)

echo [STEP] Updating version from !current_version! to !new_version!

REM Update version.txt
echo !new_version!> "!DEFAULT_VERSION_FILE!"

REM Update csproj file
if exist "!PROJECT_ROOT!\!PROJECT_NAME!.csproj" (
    powershell -Command "(Get-Content '!PROJECT_ROOT!\!PROJECT_NAME!.csproj') -replace '<Version>.*</Version>', '<Version>!new_version!</Version>' | Set-Content '!PROJECT_ROOT!\!PROJECT_NAME!.csproj'"
    powershell -Command "(Get-Content '!PROJECT_ROOT!\!PROJECT_NAME!.csproj') -replace '<AssemblyVersion>.*</AssemblyVersion>', '<AssemblyVersion>!new_version!.0</AssemblyVersion>' | Set-Content '!PROJECT_ROOT!\!PROJECT_NAME!.csproj'"
    powershell -Command "(Get-Content '!PROJECT_ROOT!\!PROJECT_NAME!.csproj') -replace '<FileVersion>.*</FileVersion>', '<FileVersion>!new_version!.0</FileVersion>' | Set-Content '!PROJECT_ROOT!\!PROJECT_NAME!.csproj'"
)

echo [SUCCESS] Version updated to !new_version!

if "!TAG_VERSION!"=="true" call :create_git_tag "!new_version!"
goto :eof

:create_git_tag
set "version=%~1"
set "tag_name=v!version!"

echo [STEP] Creating git tag: !tag_name!

REM Check if tag already exists
git tag -l | findstr /X "!tag_name!" >nul 2>&1
if !errorlevel! equ 0 (
    echo [WARNING] Tag !tag_name! already exists
    if "!FORCE_VERSION!"=="true" (
        echo [INFO] Deleting existing tag !tag_name!
        git tag -d "!tag_name!" 2>nul
        git push origin ":refs/tags/!tag_name!" 2>nul
    ) else (
        exit /b 1
    )
)

REM Create annotated tag
git add .
git commit -m "chore: bump version to !version!" 2>nul || echo [INFO] No changes to commit
git tag -a "!tag_name!" -m "Release version !version!"

REM Push tag to remote
git push origin "!tag_name!"
git push origin main

echo [SUCCESS] Created and pushed git tag: !tag_name!
goto :eof

:clean_build
echo [STEP] Cleaning build artifacts

cd /d "!PROJECT_ROOT!"

REM Clean .NET artifacts
dotnet clean
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

REM Clean Docker artifacts
docker system prune -f 2>nul || echo [WARNING] Docker cleanup failed (Docker may not be running)

echo [SUCCESS] Build artifacts cleaned
goto :eof

:restore_packages
echo [STEP] Restoring NuGet packages

cd /d "!PROJECT_ROOT!"
dotnet restore

echo [SUCCESS] Packages restored
goto :eof

:build_application
echo [STEP] Building application (!ENVIRONMENT! configuration)

cd /d "!PROJECT_ROOT!"
dotnet build --configuration "!ENVIRONMENT!" --no-restore

if !errorlevel! neq 0 (
    echo [ERROR] Build failed
    exit /b 1
)

echo [SUCCESS] Application built successfully
goto :eof

:run_tests
echo [STEP] Running tests

cd /d "!PROJECT_ROOT!"

REM Check if test projects exist
dir /s "*Test*.csproj" 2>nul >nul || dir /s "*Tests*.csproj" 2>nul >nul
if !errorlevel! equ 0 (
    dotnet test --configuration "!ENVIRONMENT!" --no-build --verbosity normal
    if !errorlevel! neq 0 (
        echo [ERROR] Tests failed
        exit /b 1
    )
    echo [SUCCESS] All tests passed
) else (
    echo [WARNING] No test projects found
)
goto :eof

:publish_application
echo [STEP] Publishing application

cd /d "!PROJECT_ROOT!"
dotnet publish --configuration "!ENVIRONMENT!" --no-build --output "./publish"

if !errorlevel! neq 0 (
    echo [ERROR] Publish failed
    exit /b 1
)

echo [SUCCESS] Application published to ./publish
goto :eof

:create_packages
echo [STEP] Creating NuGet packages

cd /d "!PROJECT_ROOT!"
dotnet pack --configuration "!ENVIRONMENT!" --no-build --output "./packages"

if !errorlevel! neq 0 (
    echo [ERROR] Package creation failed
    exit /b 1
)

echo [SUCCESS] NuGet packages created in ./packages
goto :eof

:build_docker_image
set "version=%~1"
if "!version!"=="" call :get_current_version version

echo [STEP] Building Docker image: !DOCKER_IMAGE_NAME!:!version!

cd /d "!PROJECT_ROOT!"

REM Build the Docker image
docker build -t "!DOCKER_IMAGE_NAME!:!version!" ^
             -t "!DOCKER_IMAGE_NAME!:latest" ^
             --build-arg VERSION="!version!" ^
             --build-arg BUILD_DATE="%date% %time%" ^
             .

if !errorlevel! neq 0 (
    echo [ERROR] Docker build failed
    exit /b 1
)

REM If registry is specified, also tag with registry
if not "!DOCKER_REGISTRY!"=="" (
    docker tag "!DOCKER_IMAGE_NAME!:!version!" "!DOCKER_REGISTRY!/!DOCKER_IMAGE_NAME!:!version!"
    docker tag "!DOCKER_IMAGE_NAME!:latest" "!DOCKER_REGISTRY!/!DOCKER_IMAGE_NAME!:latest"
)

echo [SUCCESS] Docker image built: !DOCKER_IMAGE_NAME!:!version!
goto :eof

:push_docker_image
set "version=%~1"
if "!version!"=="" call :get_current_version version

if "!DOCKER_REGISTRY!"=="" (
    echo [ERROR] Docker registry not specified. Use -r or set DOCKER_REGISTRY environment variable
    exit /b 1
)

echo [STEP] Pushing Docker image to !DOCKER_REGISTRY!

docker push "!DOCKER_REGISTRY!/!DOCKER_IMAGE_NAME!:!version!"
docker push "!DOCKER_REGISTRY!/!DOCKER_IMAGE_NAME!:latest"

if !errorlevel! neq 0 (
    echo [ERROR] Docker push failed
    exit /b 1
)

echo [SUCCESS] Docker image pushed: !DOCKER_REGISTRY!/!DOCKER_IMAGE_NAME!:!version!
goto :eof

:cmd_build
call :clean_build
call :restore_packages
call :build_application
if "!SKIP_TESTS!"=="false" call :run_tests
goto :success

:cmd_test
call :restore_packages
call :build_application
call :run_tests
goto :success

:cmd_package
call :restore_packages
call :build_application
call :create_packages
goto :success

:cmd_docker_build
if not "!VERSION!"=="" call :update_version "!VERSION!"
call :get_current_version current_version
call :build_docker_image "!current_version!"
goto :success

:cmd_docker_push
call :get_current_version current_version
call :push_docker_image "!current_version!"
goto :success

:cmd_release
echo [INFO] Starting full release process for version: !VERSION!

if not "!VERSION!"=="" (
    call :update_version "!VERSION!"
    set "VERSION=!VERSION!"
) else (
    call :get_current_version VERSION
)

call :clean_build
call :restore_packages
call :build_application
if "!SKIP_TESTS!"=="false" call :run_tests
call :publish_application
call :create_packages

if "!SKIP_DOCKER!"=="false" (
    call :build_docker_image "!VERSION!"
    if "!PUSH_DOCKER!"=="true" call :push_docker_image "!VERSION!"
)

echo [SUCCESS] Full release completed successfully!
goto :success

:cmd_version
if "!VERSION!"=="" (
    echo [ERROR] Version is required for version command
    exit /b 1
)
call :update_version "!VERSION!"
goto :success

:cmd_clean
call :clean_build
goto :success

:success
echo [SUCCESS] Build script completed successfully!
goto :end

:end
endlocal