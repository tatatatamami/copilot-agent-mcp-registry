# Contributing to MCP Registry

Thank you for your interest in contributing to the MCP Server Registry! This document provides guidelines for contributing to the project.

## Types of Contributions

### 1. Registering an MCP Server

The primary way to contribute is by registering your MCP server. Follow these steps:

1. **Use the CLI Tool**: Run the registration assistant
   ```bash
   dotnet run --project src/McpRegistration.Cli
   ```

2. **Follow the Process**: See [docs/RUNBOOK.md](docs/RUNBOOK.md) for detailed steps

3. **Create a Pull Request**: Use the PR template provided

4. **Address Review Feedback**: Respond to reviewer comments

### 2. Improving the CLI Tool

To contribute code improvements:

1. **Fork the Repository**
   ```bash
   git fork https://github.com/tatatatamami/copilot-agent-mcp-registry.git
   ```

2. **Create a Feature Branch**
   ```bash
   git checkout -b feature/my-improvement
   ```

3. **Make Your Changes**
   - Follow C# coding conventions
   - Add XML documentation comments
   - Update relevant documentation

4. **Test Your Changes**
   ```bash
   dotnet build
   dotnet test # if tests exist
   ```

5. **Commit with Clear Messages**
   ```bash
   git commit -m "feat: add validation for custom properties"
   ```

6. **Submit Pull Request**
   - Describe what and why
   - Reference any related issues
   - Ensure CI checks pass

### 3. Documentation Improvements

Documentation contributions are always welcome:

- Fix typos or clarify instructions
- Add examples or use cases
- Improve troubleshooting guides
- Translate documentation (future)

### 4. Reporting Issues

If you encounter problems:

1. **Search Existing Issues**: Check if it's already reported
2. **Create New Issue**: Use appropriate template
3. **Provide Details**:
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (.NET version, OS)
   - Error messages and logs

## Coding Standards

### C# Code Style

- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and small
- Use async/await for I/O operations

### Example

```csharp
/// <summary>
/// Validates the MCP metadata against defined rules
/// </summary>
/// <param name="metadata">The metadata to validate</param>
/// <returns>List of validation errors, empty if valid</returns>
public List<string> ValidateMetadata(McpMetadata metadata)
{
    // Implementation
}
```

### File Organization

- Place models in `Models/` directory
- Place services in `Services/` directory
- Keep related functionality together
- One class per file

## Pull Request Guidelines

### Before Submitting

- [ ] Code builds without errors
- [ ] All tests pass (if applicable)
- [ ] Documentation updated
- [ ] Commit messages are clear
- [ ] Branch is up-to-date with main

### PR Description

Include:
- **What**: Summary of changes
- **Why**: Reason for changes
- **How**: Implementation approach
- **Testing**: How you tested it

### Review Process

1. Automated checks run (build, tests)
2. Code review by maintainers
3. Address feedback
4. Approval and merge

## Commit Message Format

Use conventional commits:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

### Examples

```
feat(cli): add support for custom metadata properties

Added ability to specify custom properties during registration.
Users can now add organization-specific metadata fields.

Closes #42
```

```
docs(readme): update Azure setup instructions

Clarified steps for creating service principal.
Added troubleshooting section for common issues.
```

## Code Review Checklist

When reviewing PRs, check:

- [ ] Code follows project conventions
- [ ] Changes are focused and minimal
- [ ] Documentation is updated
- [ ] No sensitive data in commits
- [ ] Error handling is appropriate
- [ ] Code is readable and maintainable

## Testing Guidelines

### Manual Testing

Before submitting:

1. Run the CLI tool with various inputs
2. Test validation with invalid data
3. Verify generated files are correct
4. Check Git operations work properly

### Automated Testing (Future)

- Unit tests for validation logic
- Integration tests for file generation
- End-to-end tests for full workflow

## Documentation Standards

### Markdown

- Use clear headings (H1, H2, H3)
- Include code examples with syntax highlighting
- Add tables for structured data
- Use lists for step-by-step instructions
- Include links to related documents

### Code Comments

- Explain "why" not "what"
- Document non-obvious behavior
- Keep comments up-to-date with code
- Use XML docs for public APIs

## Getting Help

### Resources

- [README.md](README.md) - Main documentation
- [docs/RUNBOOK.md](docs/RUNBOOK.md) - Operations guide
- [docs/DEMO_SCENARIO.md](docs/DEMO_SCENARIO.md) - Demo walkthrough
- [docs/AZURE_SETUP.md](docs/AZURE_SETUP.md) - Azure configuration

### Support Channels

- **GitHub Issues**: For bugs and feature requests
- **Pull Requests**: For code discussions
- **Discussions**: For questions and ideas (if enabled)

## Recognition

Contributors will be:
- Listed in PR history
- Mentioned in release notes (major contributions)
- Credited in documentation (significant docs improvements)

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT License).

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Welcome diverse perspectives
- Focus on constructive feedback
- Be patient with newcomers
- Assume good intentions

### Unacceptable Behavior

- Harassment or discriminatory language
- Personal attacks
- Publishing private information
- Trolling or insulting comments
- Spam or off-topic content

## Questions?

If you have questions about contributing:

1. Check existing documentation
2. Search closed issues
3. Create a new issue with the "question" label
4. Contact the maintainers

---

Thank you for contributing to the MCP Registry! Your efforts help build a better ecosystem for everyone.

**Last Updated**: 2026-02-18  
**Maintained by**: Platform Team
