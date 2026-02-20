# kachaka-mcp


> MCP server for controlling Kachaka home delivery robots via AI models

## Overview

An MCP server that exposes the Kachaka home delivery robot's capabilities to AI models via the Model Context Protocol. It enables AI models like Claude, GPT-4, and local LLMs to monitor and control a Kachaka robot. Features include robot movement, shelf operations, sensor data access, and map management.

## Metadata

- **Version**: 1.0.0
- **Owner**: comoc
- **Company**: comoc
- **Status**: active
- **Lifecycle**: development

## Authentication

This MCP server uses **api-key** for authentication.

### API Key Authentication

To authenticate, include your API key in the request headers:

```
Authorization: Bearer YOUR_API_KEY
```

Contact the server owner to obtain an API key.

## Contact

- **Owner**: comoc


## Endpoint

Endpoint URL will be provided upon deployment.

## Documentation

For more details, see: https://github.com/comoc/kachaka-mcp

## Tags

`robot`, `kachaka`, `mcp`, `robotics`, `grpc`, `python`, `ai`, `test`

## Usage

To use this MCP server:

1. Ensure you have the appropriate authentication credentials
2. Connect to the endpoint using an MCP-compatible client
3. Follow the API specification defined in `openapi.json`

## Support

For support or questions, please contact comoc.
