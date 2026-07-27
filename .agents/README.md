# `.agents/`

Home for shared, tool-agnostic AI-agent assets for this repository
(reusable prompts, task templates, command snippets). Add files here as the
need arises.

## Source of truth

Project-wide agent instructions live in **[`../AGENTS.md`](../AGENTS.md)** — the
single source of truth. Every other agent config file in this repo is a thin
pointer to it, so guidance is written **once**:

| Tool           | File                              | How it points at `AGENTS.md`        |
| -------------- | --------------------------------- | ----------------------------------- |
| Claude Code    | `CLAUDE.md`                       | `@AGENTS.md` import stub            |
| GitHub Copilot | `.github/copilot-instructions.md` | pointer stub (Copilot reads `AGENTS.md` natively) |
| CodeRabbit     | `.coderabbit.yaml`                | `knowledge_base` → `AGENTS.md`      |

### Rules for agents and humans

- **Edit `AGENTS.md`**, never the pointer files.
- Keep `AGENTS.md` self-contained (plain Markdown, no tool-specific transitive
  imports) so every tool that reads it gets the complete guidance.
- To onboard a new tool, add a pointer to `AGENTS.md` here — do not copy content.
