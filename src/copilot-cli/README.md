# MCP Copilot CLI

> Interactive MCP server registration CLI powered by GitHub Copilot SDK

## ?? Features

- ?? **AI-Powered Suggestions**: GitHub Copilot suggests optimal MCP server names
- ? **Intelligent Validation**: Real-time validation with AI assistance
- ?? **Auto-Generation**: Automatically generates metadata, OpenAPI specs, and documentation
- ?? **Beautiful UI**: Colorful and intuitive command-line interface
- ?? **Git Integration**: Automatic branch creation and commit

## ?? Prerequisites

- Node.js 18+ or later
- npm or yarn
- Git
- GitHub Copilot subscription (for AI features)

## ?? Installation

```bash
cd src/copilot-cli
npm install
```

## ?? Usage

### Development Mode

```bash
npm run dev
```

### Build and Run

```bash
npm run build
npm start
```

### Global Installation (Optional)

```bash
npm run build
npm link
```

Then you can run globally:

```bash
mcp-register
```

## ?? How It Works

1. **Description Input**: Describe your MCP server
2. **AI Suggestion**: Copilot suggests an optimal server name
3. **Metadata Collection**: Interactive prompts for all required fields
4. **File Generation**: Automatic creation of:
   - `metadata.json`
   - `openapi.json`
   - `README.md`
5. **Git Operations**: Branch creation and commit

## ?? Comparison with .NET CLI

| Feature | .NET CLI | Copilot CLI (TypeScript) |
|---------|----------|--------------------------|
| AI Suggestions | ? | ? GitHub Copilot SDK |
| Platform | Windows/.NET | Cross-platform (Node.js) |
| Validation | Basic | AI-powered |
| User Experience | Console input | Interactive prompts |

## ?? Authentication

The Copilot CLI uses GitHub Copilot SDK authentication. Make sure you:

1. Have an active GitHub Copilot subscription
2. Are authenticated with GitHub CLI (`gh auth login`)

## ?? Example Session

```
===========================================
  MCP Server Registration Assistant
  ?? Powered by GitHub Copilot SDK
===========================================

?? Initializing GitHub Copilot...
? Copilot ready!

Let's register your MCP server! ??

? Brief description of what this MCP server does: Provides real-time weather data for multiple cities

?? Generating name suggestion based on description...
   Suggested: weather-data-service

? MCP Server Name (lowercase, use hyphens): weather-data-service
?? Validating name...
   ? Name looks good!

? Version: 1.0.0
? Company/Organization: Contoso
? Owner (name of person or team): Weather Team
...

?? Generating files...
? Files generated successfully!

Files created at: apis/weather-data-service/
  - openapi.json
  - metadata.json
  - README.md

? Would you like to commit these changes? Yes

?? Creating branch and committing...
? Changes committed successfully!
  Branch: mcp-registration/weather-data-service

? Registration process completed!
```

## ??? Development

### Build

```bash
npm run build
```

### Clean

```bash
npm run clean
```

### Type Check

```bash
npx tsc --noEmit
```

## ?? License

MIT

## ?? Contributing

This is part of the MCP Server Registry project. See the main README for contribution guidelines.
