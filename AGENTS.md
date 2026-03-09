# Flexor

## Purpose

Allows you to run scripts and other actions from bicep files during deployments.

## Scrinia Memory Guide

### Ephemeral memories (~name)
Use `~` prefix for in-session working state that shouldn't persist:
- `store(["scratch data"], "~scratch")` — dies when process exits
- Great for intermediate results, draft summaries, working context
- Promote to persistent with `copy("~scratch", "topic:final-name")`

### Topic organization
Use topic:subject naming to organize related memories:
- `store(["content"], "api:auth-flow")` — stored in api/ topic
- `store(["content"], "arch:decisions")` — stored in arch/ topic
- Topics are auto-discovered — no setup needed

### Chunked retrieval
For large memories, retrieve only what you need:
1. `chunk_count("my-memory")` — see how many chunks
2. `get_chunk("my-memory", 1)` — read just the first chunk
3. Process chunk by chunk to stay within context limits

### Incremental capture with append
Build up memories incrementally — each append adds a new independently retrievable chunk:
- `append("New finding here", "session-notes")` — adds as a new chunk
- Creates the memory if it doesn't exist yet
- Great for session journals, running logs, and incremental notes
- Each appended chunk is individually indexed for search

### Context compression
When you gather large amounts of information during research:
1. Summarize your findings into a concise document
2. `store([summary], "topic:finding-name")` — persist for future sessions
3. Later: `search("finding")` → `show("topic:finding-name")` to recall
This lets you carry knowledge across sessions without re-researching.

### Version history
When you overwrite an existing memory, the previous version is archived:
- Stored in `versions/` subdirectory with timestamp suffix
- No manual action needed — happens automatically on store/append

### Review conditions
Flag memories that may become stale:
- `store(["content"], "api:endpoints", reviewAfter="2026-06-01")` — date-based
- `store(["content"], "auth:flow", reviewWhen="when auth system changes")` — condition-based
- `list()` shows `[stale]` or `[review?]` markers

### Budget tracking
Monitor how much context you're consuming:
- `budget()` — shows per-memory chars/tokens loaded via show()/get_chunk()
- Helps decide when to use chunked retrieval vs. full show()

### Session-end reflection
Call `reflect()` at the end of a session for a checklist of knowledge to persist.

### Context preservation (~checkpoints)
Long conversations get compressed by your host platform. Use ephemeral checkpoints to survive:
- Before a large task or after a milestone, store your current state:
  `store(["Task: ...\nKey findings: ...\nNext steps: ..."], "~checkpoint")`
- After context compaction, restore your bearings:
  `list(scopes="ephemeral")` then `show("~checkpoint")`
- Update the checkpoint as you make progress

### Cross-project sharing
Export topics as portable .scrinia-bundle files:
1. `export(["api", "arch"])` — creates a .scrinia-bundle in .scrinia/exports/
2. Copy the bundle to another project
3. `import("path/to/bundle.scrinia-bundle")` — restores all topics
