#!/bin/bash

# =================================================================
# Database Migration Script
# =================================================================
# This script handles database migrations for the application
# Usage: ./scripts/db-migrate.sh [command] [options]
# =================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to show usage
show_help() {
    echo "Database Migration Script"
    echo ""
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "Commands:"
    echo "  add <name>           Add a new migration with the specified name"
    echo "  update              Update database to latest migration"
    echo "  rollback <migration> Rollback to specified migration"
    echo "  status              Show migration status"
    echo "  reset               Reset database (WARNING: This will delete all data!)"
    echo "  seed                Seed database with initial data"
    echo "  drop                Drop the database (WARNING: This will delete all data!)"
    echo ""
    echo "Options:"
    echo "  -e, --environment   Specify environment (Development, Staging, Production)"
    echo "  -c, --context       Specify DbContext name (default: ApplicationDbContext)"
    echo "  -h, --help          Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 add CreateUserTable"
    echo "  $0 update"
    echo "  $0 status"
    echo "  $0 rollback InitialCreate"
    echo "  $0 reset -e Development"
}

# Default values
ENVIRONMENT="Development"
CONTEXT="ApplicationDbContext"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -c|--context)
            CONTEXT="$2"
            shift 2
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            if [[ -z $COMMAND ]]; then
                COMMAND="$1"
            else
                ARGS="$ARGS $1"
            fi
            shift
            ;;
    esac
done

# Set environment variable
export ASPNETCORE_ENVIRONMENT="$ENVIRONMENT"

print_info "Environment: $ENVIRONMENT"
print_info "DbContext: $CONTEXT"

# Function to check if dotnet-ef is installed
check_ef_tools() {
    if ! command -v dotnet-ef &> /dev/null; then
        print_error "dotnet-ef tool is not installed."
        print_info "Installing dotnet-ef tool globally..."
        dotnet tool install --global dotnet-ef
        print_success "dotnet-ef tool installed successfully."
    fi
}

# Function to add a new migration
add_migration() {
    local migration_name=$1

    if [[ -z "$migration_name" ]]; then
        print_error "Migration name is required for 'add' command."
        print_info "Usage: $0 add <migration_name>"
        exit 1
    fi

    print_info "Adding migration: $migration_name"
    dotnet ef migrations add "$migration_name" --context "$CONTEXT" --output-dir src/Data/Migrations

    if [[ $? -eq 0 ]]; then
        print_success "Migration '$migration_name' added successfully."
    else
        print_error "Failed to add migration '$migration_name'."
        exit 1
    fi
}

# Function to update database
update_database() {
    print_info "Updating database to latest migration..."
    dotnet ef database update --context "$CONTEXT"

    if [[ $? -eq 0 ]]; then
        print_success "Database updated successfully."
    else
        print_error "Failed to update database."
        exit 1
    fi
}

# Function to rollback to specific migration
rollback_migration() {
    local migration_name=$1

    if [[ -z "$migration_name" ]]; then
        print_error "Migration name is required for 'rollback' command."
        print_info "Usage: $0 rollback <migration_name>"
        exit 1
    fi

    print_warning "Rolling back to migration: $migration_name"
    print_warning "This action may result in data loss!"

    read -p "Are you sure you want to continue? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        dotnet ef database update "$migration_name" --context "$CONTEXT"

        if [[ $? -eq 0 ]]; then
            print_success "Database rolled back to '$migration_name' successfully."
        else
            print_error "Failed to rollback database to '$migration_name'."
            exit 1
        fi
    else
        print_info "Rollback cancelled."
    fi
}

# Function to show migration status
show_status() {
    print_info "Showing migration history and status..."
    echo ""
    echo "=== Applied Migrations ==="
    dotnet ef migrations list --context "$CONTEXT"
    echo ""
    echo "=== Pending Migrations ==="
    # This would show pending migrations if any exist
    dotnet ef migrations list --context "$CONTEXT" --no-connect 2>/dev/null || echo "Unable to check pending migrations without database connection"
}

# Function to reset database
reset_database() {
    print_warning "This will completely reset the database and all data will be lost!"

    read -p "Are you sure you want to reset the database? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_info "Dropping database..."
        dotnet ef database drop --context "$CONTEXT" --force

        print_info "Updating database to latest migration..."
        dotnet ef database update --context "$CONTEXT"

        if [[ $? -eq 0 ]]; then
            print_success "Database reset successfully."
        else
            print_error "Failed to reset database."
            exit 1
        fi
    else
        print_info "Database reset cancelled."
    fi
}

# Function to seed database
seed_database() {
    print_info "Seeding database with initial data..."

    # Run the application with a special flag to seed data
    # This would typically be handled by a seeding endpoint or startup logic
    dotnet run --no-build -- --seed-data

    if [[ $? -eq 0 ]]; then
        print_success "Database seeded successfully."
    else
        print_error "Failed to seed database."
        exit 1
    fi
}

# Function to drop database
drop_database() {
    print_warning "This will completely drop the database and all data will be lost!"

    read -p "Are you sure you want to drop the database? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_info "Dropping database..."
        dotnet ef database drop --context "$CONTEXT" --force

        if [[ $? -eq 0 ]]; then
            print_success "Database dropped successfully."
        else
            print_error "Failed to drop database."
            exit 1
        fi
    else
        print_info "Database drop cancelled."
    fi
}

# Main script logic
case $COMMAND in
    add)
        check_ef_tools
        add_migration $ARGS
        ;;
    update)
        check_ef_tools
        update_database
        ;;
    rollback)
        check_ef_tools
        rollback_migration $ARGS
        ;;
    status)
        check_ef_tools
        show_status
        ;;
    reset)
        check_ef_tools
        reset_database
        ;;
    seed)
        seed_database
        ;;
    drop)
        check_ef_tools
        drop_database
        ;;
    "")
        print_error "No command specified."
        show_help
        exit 1
        ;;
    *)
        print_error "Unknown command: $COMMAND"
        show_help
        exit 1
        ;;
esac