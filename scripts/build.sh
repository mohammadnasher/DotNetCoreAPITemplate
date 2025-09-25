#!/bin/bash

# =================================================================
# Build Script with Version Tagging and Docker Support
# =================================================================
# This script handles building, testing, versioning, and Docker operations
# Usage: ./scripts/build.sh [command] [options]
# =================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
PROJECT_NAME="DotNetCoreAPITemplate"
DOCKER_IMAGE_NAME="dotnet-api-template"
DOCKER_REGISTRY=""  # Set your registry here
DEFAULT_VERSION_FILE="$PROJECT_ROOT/version.txt"

# Default values
ENVIRONMENT="Release"
SKIP_TESTS=false
SKIP_DOCKER=false
VERSION=""
TAG_VERSION=false
PUSH_DOCKER=false
FORCE_VERSION=false

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

print_step() {
    echo -e "${PURPLE}[STEP]${NC} $1"
}

# Function to show usage
show_help() {
    echo "Build Script with Version Tagging and Docker Support"
    echo ""
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "Commands:"
    echo "  build               Build the application (default)"
    echo "  test                Run tests only"
    echo "  package             Create NuGet packages"
    echo "  docker-build        Build Docker image"
    echo "  docker-push         Push Docker image to registry"
    echo "  release             Full release process (build, test, version, docker)"
    echo "  version             Update version and create git tag"
    echo "  clean               Clean build artifacts"
    echo ""
    echo "Options:"
    echo "  -v, --version       Specify version (e.g., 1.2.3)"
    echo "  -e, --environment   Build environment (Debug, Release) [default: Release]"
    echo "  -t, --tag           Create and push git tag for the version"
    echo "  -p, --push          Push Docker image to registry"
    echo "  -f, --force         Force version update even if same version exists"
    echo "  -s, --skip-tests    Skip running tests"
    echo "  -d, --skip-docker   Skip Docker operations"
    echo "  -r, --registry      Docker registry URL"
    echo "  -h, --help          Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 build"
    echo "  $0 release -v 1.2.3 -t -p"
    echo "  $0 docker-build -v 1.2.3"
    echo "  $0 test"
    echo "  $0 version -v 1.2.3 -t"
}

# Function to get current version
get_current_version() {
    if [[ -f "$DEFAULT_VERSION_FILE" ]]; then
        cat "$DEFAULT_VERSION_FILE"
    else
        echo "1.0.0"
    fi
}

# Function to update version
update_version() {
    local new_version=$1

    if [[ -z "$new_version" ]]; then
        print_error "Version is required"
        return 1
    fi

    # Validate version format (semver)
    if [[ ! $new_version =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9]+)?$ ]]; then
        print_error "Invalid version format. Use semantic versioning (e.g., 1.2.3 or 1.2.3-beta)"
        return 1
    fi

    local current_version=$(get_current_version)

    if [[ "$current_version" == "$new_version" && "$FORCE_VERSION" == false ]]; then
        print_warning "Version $new_version is the same as current version. Use --force to override."
        return 1
    fi

    print_step "Updating version from $current_version to $new_version"

    # Update version.txt
    echo "$new_version" > "$DEFAULT_VERSION_FILE"

    # Update csproj file
    if [[ -f "$PROJECT_ROOT/$PROJECT_NAME.csproj" ]]; then
        sed -i.bak "s|<Version>.*</Version>|<Version>$new_version</Version>|g" "$PROJECT_ROOT/$PROJECT_NAME.csproj"
        sed -i.bak "s|<AssemblyVersion>.*</AssemblyVersion>|<AssemblyVersion>$new_version.0</AssemblyVersion>|g" "$PROJECT_ROOT/$PROJECT_NAME.csproj"
        sed -i.bak "s|<FileVersion>.*</FileVersion>|<FileVersion>$new_version.0</FileVersion>|g" "$PROJECT_ROOT/$PROJECT_NAME.csproj"
        rm -f "$PROJECT_ROOT/$PROJECT_NAME.csproj.bak"
    fi

    print_success "Version updated to $new_version"

    # Create git tag if requested
    if [[ "$TAG_VERSION" == true ]]; then
        create_git_tag "$new_version"
    fi
}

# Function to create git tag
create_git_tag() {
    local version=$1
    local tag_name="v$version"

    print_step "Creating git tag: $tag_name"

    # Check if tag already exists
    if git tag -l | grep -q "^$tag_name$"; then
        print_warning "Tag $tag_name already exists"
        if [[ "$FORCE_VERSION" == true ]]; then
            print_info "Deleting existing tag $tag_name"
            git tag -d "$tag_name" || true
            git push origin ":refs/tags/$tag_name" || true
        else
            return 1
        fi
    fi

    # Create annotated tag
    git add .
    git commit -m "chore: bump version to $version" || print_info "No changes to commit"
    git tag -a "$tag_name" -m "Release version $version"

    # Push tag to remote
    git push origin "$tag_name"
    git push origin main

    print_success "Created and pushed git tag: $tag_name"
}

# Function to clean build artifacts
clean_build() {
    print_step "Cleaning build artifacts"

    cd "$PROJECT_ROOT"

    # Clean .NET artifacts
    dotnet clean
    rm -rf bin obj
    rm -rf **/bin **/obj

    # Clean Docker artifacts
    docker system prune -f || print_warning "Docker cleanup failed (Docker may not be running)"

    print_success "Build artifacts cleaned"
}

# Function to restore packages
restore_packages() {
    print_step "Restoring NuGet packages"

    cd "$PROJECT_ROOT"
    dotnet restore

    print_success "Packages restored"
}

# Function to build application
build_application() {
    print_step "Building application ($ENVIRONMENT configuration)"

    cd "$PROJECT_ROOT"
    dotnet build --configuration "$ENVIRONMENT" --no-restore

    print_success "Application built successfully"
}

# Function to run tests
run_tests() {
    print_step "Running tests"

    cd "$PROJECT_ROOT"

    # Check if test projects exist
    if find . -name "*Test*.csproj" -o -name "*Tests*.csproj" | grep -q .; then
        dotnet test --configuration "$ENVIRONMENT" --no-build --verbosity normal
        print_success "All tests passed"
    else
        print_warning "No test projects found"
    fi
}

# Function to publish application
publish_application() {
    print_step "Publishing application"

    cd "$PROJECT_ROOT"
    dotnet publish --configuration "$ENVIRONMENT" --no-build --output "./publish"

    print_success "Application published to ./publish"
}

# Function to create NuGet packages
create_packages() {
    print_step "Creating NuGet packages"

    cd "$PROJECT_ROOT"
    dotnet pack --configuration "$ENVIRONMENT" --no-build --output "./packages"

    print_success "NuGet packages created in ./packages"
}

# Function to build Docker image
build_docker_image() {
    local version=${1:-$(get_current_version)}

    print_step "Building Docker image: $DOCKER_IMAGE_NAME:$version"

    cd "$PROJECT_ROOT"

    # Build the Docker image
    docker build -t "$DOCKER_IMAGE_NAME:$version" \
                 -t "$DOCKER_IMAGE_NAME:latest" \
                 --build-arg VERSION="$version" \
                 --build-arg BUILD_DATE="$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
                 --build-arg VCS_REF="$(git rev-parse HEAD)" \
                 .

    # If registry is specified, also tag with registry
    if [[ -n "$DOCKER_REGISTRY" ]]; then
        docker tag "$DOCKER_IMAGE_NAME:$version" "$DOCKER_REGISTRY/$DOCKER_IMAGE_NAME:$version"
        docker tag "$DOCKER_IMAGE_NAME:latest" "$DOCKER_REGISTRY/$DOCKER_IMAGE_NAME:latest"
    fi

    print_success "Docker image built: $DOCKER_IMAGE_NAME:$version"
}

# Function to push Docker image
push_docker_image() {
    local version=${1:-$(get_current_version)}

    if [[ -z "$DOCKER_REGISTRY" ]]; then
        print_error "Docker registry not specified. Use -r or set DOCKER_REGISTRY environment variable"
        return 1
    fi

    print_step "Pushing Docker image to $DOCKER_REGISTRY"

    docker push "$DOCKER_REGISTRY/$DOCKER_IMAGE_NAME:$version"
    docker push "$DOCKER_REGISTRY/$DOCKER_IMAGE_NAME:latest"

    print_success "Docker image pushed: $DOCKER_REGISTRY/$DOCKER_IMAGE_NAME:$version"
}

# Function for full release process
full_release() {
    local version=${1:-$(get_current_version)}

    print_info "Starting full release process for version: $version"

    # Update version if specified
    if [[ -n "$VERSION" ]]; then
        update_version "$VERSION"
        version="$VERSION"
    fi

    # Clean and restore
    clean_build
    restore_packages

    # Build
    build_application

    # Run tests if not skipped
    if [[ "$SKIP_TESTS" == false ]]; then
        run_tests
    fi

    # Publish
    publish_application

    # Create packages
    create_packages

    # Docker operations if not skipped
    if [[ "$SKIP_DOCKER" == false ]]; then
        build_docker_image "$version"

        if [[ "$PUSH_DOCKER" == true ]]; then
            push_docker_image "$version"
        fi
    fi

    print_success "Full release completed successfully!"
}

# Parse command line arguments
COMMAND=""
while [[ $# -gt 0 ]]; do
    case $1 in
        -v|--version)
            VERSION="$2"
            shift 2
            ;;
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -t|--tag)
            TAG_VERSION=true
            shift
            ;;
        -p|--push)
            PUSH_DOCKER=true
            shift
            ;;
        -f|--force)
            FORCE_VERSION=true
            shift
            ;;
        -s|--skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        -d|--skip-docker)
            SKIP_DOCKER=true
            shift
            ;;
        -r|--registry)
            DOCKER_REGISTRY="$2"
            shift 2
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            if [[ -z "$COMMAND" ]]; then
                COMMAND="$1"
            else
                print_error "Unknown option: $1"
                show_help
                exit 1
            fi
            shift
            ;;
    esac
done

# Default command
if [[ -z "$COMMAND" ]]; then
    COMMAND="build"
fi

# Set registry from environment if not specified
if [[ -z "$DOCKER_REGISTRY" && -n "$DOCKER_REGISTRY_URL" ]]; then
    DOCKER_REGISTRY="$DOCKER_REGISTRY_URL"
fi

# Print configuration
print_info "Configuration:"
print_info "  Command: $COMMAND"
print_info "  Environment: $ENVIRONMENT"
print_info "  Version: ${VERSION:-$(get_current_version)}"
print_info "  Docker Registry: ${DOCKER_REGISTRY:-"Not set"}"
print_info "  Skip Tests: $SKIP_TESTS"
print_info "  Skip Docker: $SKIP_DOCKER"
print_info "  Tag Version: $TAG_VERSION"
print_info "  Push Docker: $PUSH_DOCKER"

# Execute command
case $COMMAND in
    build)
        clean_build
        restore_packages
        build_application
        if [[ "$SKIP_TESTS" == false ]]; then
            run_tests
        fi
        ;;
    test)
        restore_packages
        build_application
        run_tests
        ;;
    package)
        restore_packages
        build_application
        create_packages
        ;;
    docker-build)
        if [[ -n "$VERSION" ]]; then
            update_version "$VERSION"
        fi
        build_docker_image "${VERSION:-$(get_current_version)}"
        ;;
    docker-push)
        push_docker_image "${VERSION:-$(get_current_version)}"
        ;;
    release)
        full_release
        ;;
    version)
        if [[ -n "$VERSION" ]]; then
            update_version "$VERSION"
        else
            print_error "Version is required for version command"
            exit 1
        fi
        ;;
    clean)
        clean_build
        ;;
    *)
        print_error "Unknown command: $COMMAND"
        show_help
        exit 1
        ;;
esac

print_success "Build script completed successfully!"