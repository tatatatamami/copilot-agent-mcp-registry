# MCP Registration PR Creation Script
# Usage: .\scripts\create-pr.ps1

param(
    [string]$BranchName = (git branch --show-current),
    [string]$Title,
    [string]$Body = "MCP Server Registration - Ready for Azure API Center"
)

Write-Host "?? PR Creation Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check current branch
Write-Host "?? Current branch: $BranchName" -ForegroundColor Yellow

if ($BranchName -eq "main") {
    Write-Host "? Cannot create PR from main branch" -ForegroundColor Red
    exit 1
}

# Step 2: Ensure everything is committed
$status = git status --porcelain
if ($status) {
    Write-Host "??  Uncommitted changes detected:" -ForegroundColor Yellow
    Write-Host $status
    Write-Host ""
    $commit = Read-Host "Commit changes? (y/n)"
    if ($commit -eq "y") {
        git add .
        $message = Read-Host "Commit message"
        git commit -m $message
    }
}

# Step 3: Push to remote
Write-Host ""
Write-Host "?? Pushing to remote..." -ForegroundColor Yellow
git push -u origin $BranchName

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Push failed" -ForegroundColor Red
    exit 1
}

Write-Host "? Push successful" -ForegroundColor Green

# Step 4: Create PR
Write-Host ""
Write-Host "?? Creating Pull Request..." -ForegroundColor Yellow

# Extract MCP name from branch
if ($BranchName -match "mcp-registration/(.+)") {
    $mcpName = $Matches[1]
    $defaultTitle = "Register MCP Server: $mcpName"
} else {
    $defaultTitle = "Update: $BranchName"
}

if (-not $Title) {
    $Title = $defaultTitle
}

# Create PR with gh CLI
gh pr create --title $Title --body $Body --base main --head $BranchName

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "? PR Created Successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Show PR URL
    $prUrl = gh pr view --json url -q .url
    Write-Host "?? PR URL: $prUrl" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "??  PR creation failed or PR already exists" -ForegroundColor Yellow
    Write-Host "   Opening PR list in browser..." -ForegroundColor Yellow
    gh pr list --web
}

Write-Host ""
Write-Host "?? Done!" -ForegroundColor Green
