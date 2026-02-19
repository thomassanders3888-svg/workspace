---
name: code-mentor
description: AI coding assistant for rapid development - generates, refactors, and reviews code across languages
homepage: https://github.com/openclaw/code-mentor
metadata:
  openclaw:
    emoji: 💻
    requires:
      env: []
      bins: [node, git, dotnet, python3]
---

# Code Mentor

Multi-language AI coding assistant. Generates production-ready code, performs security reviews, and helps debug complex issues.

## Capabilities

| Task | Description |
|------|-------------|
| **Generate** | Create functions, classes, modules from description |
| **Refactor** | Optimize existing code for performance/readability |
| **Review** | Security audit, best practices check |
| **Debug** | Analyze stack traces, identify root causes |
| **Test** | Generate unit tests, integration tests |
| **Document** | Add comments, generate READMEs, API docs |

## Supported Languages

- **Web:** HTML/CSS/JS, TypeScript, React, Vue, Angular
- **Backend:** Node.js, Python, C#/.NET, Go, Rust
- **Data:** SQL, MongoDB aggregation, Jupyter
- **Infra:** Docker, Kubernetes, Terraform, Bash
- **Game:** Unity C#, Unreal, Godot, WebGL

## Usage

### Quick Generation
```
Generate a React component for user authentication with form validation
```

### Refactoring
```
Refactor this to use async/await and add error handling
[paste code]
```

### Code Review
```
Security review this authentication handler
[paste code]
```

### Test Generation
```
Generate Jest tests for this function
[paste code]
```

### Debug
```
Fix this error: TypeError: Cannot read property 'map' of undefined
[paste code and stack trace]
```

## Output Standards

- **Complete files:** Never partial snippets unless specified
- **Error handling:** Always include try/catch or error returns
- **Types:** TypeScript when possible, JSDoc otherwise  
- **Comments:** Explain "why" not "what"
- **Tests:** Include basic happy path + error case

## Project Patterns

### Single File Site
```html
<!-- index.html -->
<!DOCTYPE html>
<html>
<head><title>Tool Name</title></head>
<body>
  <!-- minimalist UI -->
  <script>
    // embedded functionality
  </script>
</body>
</html>
```

### .NET Service
```csharp
// Program.cs - Entry point
// Service.cs - Business logic
// Models/ - Data classes
// Tests/ - xUnit tests
```

### Node.js API
```javascript
// server.js - Express setup
// routes/ - API endpoints
// models/ - Data layer
// middleware/ - Auth, validation
```

## Best Practices

### Performance
- Lazy loading for heavy components
- Memoization for expensive calculations
- Database indexing for queries
- Caching layers where appropriate

### Security
- Never commit secrets (use .env)
- Input validation on all boundaries
- SQL parameterized queries
- XSS/CSRF protections for web

### Maintainability  
- Single Responsibility Principle
- Early returns, flat code structure
- Descriptive naming > comments
- Configuration over hardcoding

## Examples

### Generate Landing Page
```
Create a SaaS landing page with:
- Hero section with email capture
- Features grid (3 items)
- Pricing cards (3 tiers)
- Dark theme, responsive
```

### Generate Game Script
```
Unity C# script for:
- First person camera controller
- WASD movement
- Jump physics
- Mouse look
```

### Database Query
```
SQL query for:
- Get top 10 users by revenue
- Include total orders count
- Join users, orders, order_items tables
```

## File Organization

```
sites/[project]/
  index.html      # Main entry
  style.css       # If not embedded
  app.js          # If complex
  assets/         # Images, fonts

projects/[name]/
  src/            # Source code
  tests/          # Test files
  docs/           # Documentation
  README.md
  .gitignore
```

## Quick Commands

| Command | Action |
|---------|--------|
| `code new [name]` | Scaffold new project |
| `code review [file]` | Full code review |
| `code optimize [file]` | Performance pass |
| `code document [file]` | Add inline docs |
| `code test [file]` | Generate tests |

## Deployment Ready Checklist

- [ ] All dependencies listed
- [ ] Environment variables documented
- [ ] Build script works (`npm run build`, `dotnet build`)
- [ ] Tests pass
- [ ] No console errors
- [ ] Mobile responsive (web)
- [ ] SEO meta tags (web)
- [ ] Analytics added (optional)

## Notes

- Prefer working code over perfect code
- Include error handling even in examples
- Generate git commands when creating projects
- Commit frequently, push to remote
