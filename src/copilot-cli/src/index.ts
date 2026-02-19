#!/usr/bin/env node
import { CopilotClient } from "@github/copilot-sdk";
import chalk from "chalk";
import { CopilotAssistant } from "./copilot-assistant.js";
import { FileGenerator } from "./file-generator.js";
import { GitHandler } from "./git-handler.js";

async function main() {
  console.log(chalk.bold.blue("==========================================="));
  console.log(chalk.bold.blue("  MCP Server Registration Assistant"));
  console.log(chalk.bold.green("  ?? Powered by GitHub Copilot SDK"));
  console.log(chalk.bold.blue("===========================================\n"));

  try {
    // 1. Initialize Copilot Client
    const copilotClient = new CopilotClient();
    const assistant = new CopilotAssistant(copilotClient);
    await assistant.initialize();

    // 2. Collect metadata interactively with Copilot assistance
    const metadata = await assistant.collectMetadataInteractive();

    // 3. Generate files
    console.log(chalk.yellow("\n?? Generating files..."));
    const generator = new FileGenerator();
    await generator.generateFiles(metadata);

    console.log(chalk.green("? Files generated successfully!"));
    console.log(chalk.gray(`\nFiles created at: ${chalk.bold(`apis/${metadata.name}/`)}`));
    console.log(chalk.gray("  - openapi.json"));
    console.log(chalk.gray("  - metadata.json"));
    console.log(chalk.gray("  - README.md"));

    // 4. Git operations
    const shouldCommit = await assistant.askYesNo("\nWould you like to commit these changes?");

    if (shouldCommit) {
      console.log(chalk.yellow("\n?? Creating branch and committing..."));
      const git = new GitHandler();
      await git.createBranchAndCommit(metadata);

      const currentBranch = await git.getCurrentBranch();
      console.log(chalk.green("\n? Changes committed successfully!"));
      console.log(chalk.gray(`  Branch: ${chalk.bold(currentBranch)}`));

      console.log(chalk.yellow("\n?? Next steps:"));
      console.log(chalk.gray(`  1. Push the branch:`));
      console.log(chalk.cyan(`     git push -u origin ${currentBranch}`));
      console.log(chalk.gray(`  2. Create a Pull Request on GitHub`));
      console.log(chalk.gray(`  3. After PR approval and merge, GitHub Actions will register to Azure API Center`));
    } else {
      console.log(chalk.yellow("\n??  Changes not committed."));
      console.log(chalk.gray("You can commit manually later:"));
      console.log(chalk.cyan(`  git add apis/${metadata.name}`));
      console.log(chalk.cyan(`  git commit -m "Register MCP server: ${metadata.name}"`));
    }

    console.log(chalk.green("\n? Registration process completed!\n"));
  } catch (error: any) {
    console.error(chalk.red("\n? Error:"), error.message);
    if (error.stack) {
      console.error(chalk.gray(error.stack));
    }
    process.exit(1);
  }
}

main();
