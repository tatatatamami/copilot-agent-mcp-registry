import simpleGit, { SimpleGit } from "simple-git";
import type { McpMetadata } from "./models.js";

export class GitHandler {
  private git: SimpleGit;

  constructor(baseDir: string = process.cwd()) {
    this.git = simpleGit(baseDir);
  }

  async createBranchAndCommit(metadata: McpMetadata): Promise<void> {
    const branchName = `mcp-registration/${metadata.name}`;

    // Check if branch already exists
    const branches = await this.git.branch();
    const branchExists = branches.all.includes(branchName);

    if (branchExists) {
      console.log(`Branch ${branchName} already exists. Using existing branch.`);
      await this.git.checkout(branchName);
    } else {
      // Create and checkout new branch
      await this.git.checkoutLocalBranch(branchName);
    }

    // Stage files
    await this.git.add(`apis/${metadata.name}/*`);

    // Commit
    const commitMessage = `Register MCP server: ${metadata.name}`;
    await this.git.commit(commitMessage);
  }

  async getCurrentBranch(): Promise<string> {
    const branch = await this.git.revparse(["--abbrev-ref", "HEAD"]);
    return branch.trim();
  }

  async hasUncommittedChanges(): Promise<boolean> {
    const status = await this.git.status();
    return status.files.length > 0;
  }
}
