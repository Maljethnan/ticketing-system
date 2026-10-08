# Development Environment Rules

## Remote-Only Policy
- This project is developed **entirely** in GitHub Codespaces.
- All commands, builds, tests, and installations must run in the **remote terminal**.
- Do NOT suggest installing .NET SDK, Node.js, Python, or any tool on the local machine.
- Use remote workspace paths only (e.g., `/home/codespace/...`).
- If a dependency is missing, install it inside the Codespace container.

## Project Context
- Project: Technical Issue Tracking System (Ticketing System)
- Stack: .NET 9, SQL Server, Windows Servers (private network)
- Repository: https://github.com/Maljethnan/ticketing-system.git

## AI Agent Behavior
- When suggesting fixes or new features, assume all execution happens in the remote container.
- Never reference `C:\` or local Windows paths unless discussing deployment targets.
- Always use `dotnet` commands through the remote terminal.
- If the user asks about local setup, redirect them to Codespaces.
