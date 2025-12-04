# Contributing to Google Flights MCP Server

Thank you for your interest in contributing to the Google Flights MCP Server! We welcome contributions from the community.

## How to Contribute

### Reporting Issues

If you find a bug or have a feature request:
1. Check if the issue already exists in the [Issues](https://github.com/tdav/Google-Flights-MCP-Server/issues) section
2. If not, create a new issue with a clear description
3. Include steps to reproduce (for bugs) or use cases (for features)

### Submitting Changes

1. Fork the repository
2. Create a new branch for your feature or bugfix:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. Make your changes
4. Test your changes thoroughly
5. Commit with clear, descriptive messages:
   ```bash
   git commit -m "Add feature: description of the feature"
   ```
6. Push to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```
7. Open a Pull Request

### Development Setup

1. Ensure you have .NET 8.0 SDK installed
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR-USERNAME/Google-Flights-MCP-Server.git
   cd Google-Flights-MCP-Server
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the tests (when available):
   ```bash
   dotnet test
   ```

### Code Style

- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Add appropriate error handling

### Testing

- Test your changes locally before submitting
- Ensure the project builds without errors or warnings
- Test API endpoints using the Swagger UI or curl

### Pull Request Guidelines

- Provide a clear description of the changes
- Reference any related issues
- Keep PRs focused on a single feature or fix
- Update documentation if needed
- Ensure code builds and tests pass

## Getting Help

If you need help or have questions:
- Open a discussion in the [Discussions](https://github.com/tdav/Google-Flights-MCP-Server/discussions) section
- Comment on the relevant issue
- Reach out to the maintainers

## Code of Conduct

Please be respectful and constructive in all interactions. We aim to maintain a welcoming and inclusive community.

Thank you for contributing!
