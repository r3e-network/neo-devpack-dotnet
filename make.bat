@echo off
REM Windows batch wrapper for Makefile
REM Usage: make.bat [target]

REM Check if make is available
where make >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: 'make' is not available on this system.
    echo Please install make or use the individual commands.
    echo.
    echo Alternative commands:
    echo   dotnet build              - Build the project
    echo   dotnet test               - Run tests
    echo   dotnet pack               - Create NuGet packages
    echo   dotnet clean              - Clean build artifacts
    echo.
    echo To install make on Windows:
    echo   - Install via chocolatey: choco install make
    echo   - Or use WSL/Git Bash
    exit /b 1
)

REM Forward all arguments to make
make %*