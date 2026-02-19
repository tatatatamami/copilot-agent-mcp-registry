import { CopilotClient } from "@github/copilot-sdk";
import chalk from "chalk";
import inquirer from "inquirer";
import type { McpMetadata } from "./models.js";

export class CopilotAssistant {
  private client: CopilotClient;
  private session: any;

  constructor(client: CopilotClient) {
    this.client = client;
  }

  async initialize() {
    console.log(chalk.gray("?? Initializing GitHub Copilot..."));
    this.session = await this.client.createSession({ model: "gpt-4.1" });
    console.log(chalk.green("? Copilot ready!\n"));
  }

  async collectMetadataInteractive(): Promise<McpMetadata> {
    if (!this.session) await this.initialize();

    console.log(chalk.cyan("Let's register your MCP server! ??\n"));

    // 1. Description first (to generate name suggestion)
    const { description } = await inquirer.prompt([
      {
        type: "input",
        name: "description",
        message: "Brief description of what this MCP server does:",
        validate: (input) => input.length > 0 || "Description is required",
      },
    ]);

    // 2. Copilotに名前を提案させる
    console.log(chalk.gray("\n?? Generating name suggestion based on description..."));
    const suggestedName = await this.suggestMcpName(description);
    console.log(chalk.green(`   Suggested: ${chalk.bold(suggestedName)}`));

    const { name } = await inquirer.prompt([
      {
        type: "input",
        name: "name",
        message: "MCP Server Name (lowercase, use hyphens):",
        default: suggestedName,
        validate: (input) => {
          if (!/^[a-z0-9-]+$/.test(input)) {
            return "Name must be lowercase with hyphens only";
          }
          if (input.length < 3 || input.length > 50) {
            return "Name must be between 3 and 50 characters";
          }
          return true;
        },
      },
    ]);

    // 3. Copilotにバリデーションさせる
    console.log(chalk.gray("?? Validating name..."));
    const validation = await this.validateField("name", name);
    if (!validation.isValid) {
      console.log(chalk.yellow(`   ??  ${validation.message}`));
      if (validation.suggestion) {
        console.log(chalk.gray(`   Suggestion: ${validation.suggestion}`));
      }
    } else {
      console.log(chalk.green("   ? Name looks good!"));
    }

    // 4. 他のフィールドを収集
    const answers = await inquirer.prompt([
      {
        type: "input",
        name: "version",
        message: "Version:",
        default: "1.0.0",
        validate: (input) =>
          /^\d+\.\d+\.\d+$/.test(input) || "Version must follow semantic versioning (e.g., 1.0.0)",
      },
      {
        type: "input",
        name: "company",
        message: "Company/Organization:",
        validate: (input) => input.length > 0 || "Company is required",
      },
      {
        type: "input",
        name: "owner",
        message: "Owner (name of person or team):",
        validate: (input) => input.length > 0 || "Owner is required",
      },
      {
        type: "input",
        name: "contactEmail",
        message: "Contact Email (optional):",
        validate: (input) => {
          if (!input) return true;
          return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(input) || "Invalid email format";
        },
      },
      {
        type: "list",
        name: "status",
        message: "Status:",
        choices: ["active", "deprecated", "planned"],
        default: "active",
      },
      {
        type: "list",
        name: "lifecycle",
        message: "Lifecycle stage:",
        choices: [
          "design",
          "development",
          "testing",
          "preview",
          "production",
          "deprecated",
          "retired",
        ],
        default: "development",
      },
      {
        type: "list",
        name: "authMethod",
        message: "Authentication Method:",
        choices: ["none", "api-key", "oauth2", "entra-id"],
        default: "api-key",
      },
      {
        type: "input",
        name: "endpointUrl",
        message: "Endpoint URL (optional):",
        validate: (input) => {
          if (!input) return true;
          try {
            new URL(input);
            return true;
          } catch {
            return "Invalid URL format";
          }
        },
      },
      {
        type: "input",
        name: "documentationUrl",
        message: "Documentation URL (optional):",
        validate: (input) => {
          if (!input) return true;
          try {
            new URL(input);
            return true;
          } catch {
            return "Invalid URL format";
          }
        },
      },
      {
        type: "input",
        name: "tags",
        message: "Tags (comma-separated, optional):",
      },
    ]);

    return {
      name,
      description,
      ...answers,
      tags: answers.tags ? answers.tags.split(",").map((t: string) => t.trim()).filter(Boolean) : [],
    };
  }

  private async suggestMcpName(description: string): Promise<string> {
    try {
      const response = await this.session.sendAndWait({
        prompt: `Generate a MCP server name based on this description: "${description}"

Requirements:
- lowercase only
- use hyphens to separate words
- maximum 50 characters
- descriptive and memorable
- follow naming conventions for APIs

Reply with ONLY the suggested name, nothing else. No explanations.`,
      });

      const suggested = response.text.trim().toLowerCase().replace(/[^a-z0-9-]/g, "-");
      return suggested.substring(0, 50);
    } catch (error) {
      console.log(chalk.yellow("   (Copilot unavailable, using default naming)"));
      return description
        .toLowerCase()
        .replace(/[^a-z0-9\s-]/g, "")
        .replace(/\s+/g, "-")
        .substring(0, 50);
    }
  }

  private async validateField(
    fieldName: string,
    value: string
  ): Promise<{ isValid: boolean; message?: string; suggestion?: string }> {
    try {
      const response = await this.session.sendAndWait({
        prompt: `Validate this MCP server field:
Field: ${fieldName}
Value: '${value}'

Check if it follows best practices:
- Naming conventions
- Clarity and readability
- Common patterns

Respond in JSON format:
{
  "isValid": true/false,
  "message": "explanation if invalid",
  "suggestion": "corrected version if applicable"
}`,
      });

      return JSON.parse(response.text);
    } catch {
      return { isValid: true };
    }
  }

  async askYesNo(question: string, defaultValue = true): Promise<boolean> {
    const { answer } = await inquirer.prompt([
      {
        type: "confirm",
        name: "answer",
        message: question,
        default: defaultValue,
      },
    ]);
    return answer;
  }

  async askForEnhancedReadme(): Promise<boolean> {
    return this.askYesNo("?? Would you like Copilot to generate an enhanced README?");
  }
}
