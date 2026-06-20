# Project Memory

This folder stores concise, durable project knowledge for AI agents.

Use Markdown and JSON files here for human-reviewable memory. Use the local
SQLite database only as a generated search index that can be rebuilt from git
tracked files.

Treat this folder as the durable, portable product specification layer. Keep
non-trivial feature algorithms, business logic, workflow contracts, data rules,
architecture migration history, verification guarantees, and implementation
maps here so another agent can rebuild the same behavior on a different
language, framework, platform, or UI stack. Treat `tools/summary/` as compact
chat handoff state, not as the canonical product specification.

Use it for verified findings that should survive chat resets:

- architecture notes
- architecture migration history
- debugging findings
- important decisions
- known pitfalls
- local workflows
- feature workflow contracts
- business-rule, data-model, and integration specifications
- dependency maps
- reusable agent experience that may improve `gi`

Do not store secrets or credentials here.

## Reusable Experience For GI

When this project reveals a reusable workflow, failure pattern, token-saving
tactic, or agent-instruction improvement, write a concise recommendation for the
shared instruction kit.

Prefer the shared intake folder when available:

```text
$env:GENERAL_INSTRUCTIONS_HOME\updates\
```

If the shared library is unavailable, use a local intake folder:

```text
tools/project-memory/instruction-updates/
```

Recommendations should include:

- observed problem or repeated friction
- reusable rule, pattern, template, checklist, or migration idea
- evidence paths or commands
- expected benefit for token economy, startup retrieval, safety, or workflow
- privacy review notes

Do not include secrets, credentials, private user data, production data, or
unnecessary project-specific details.

## SQLite Index

If the project benefits from searchable agent memory, use a local SQLite
database as an agent index/experience store, not as the application database.

Recommended path:

```text
tools/project-memory/project_memory.sqlite
```

Rebuild it from git tracked repository content:

```powershell
python .\tools\project-memory\build_project_memory_index.py rebuild
```

Check index size:

```powershell
python .\tools\project-memory\build_project_memory_index.py stats
```

Search indexed content:

```powershell
python .\tools\project-memory\build_project_memory_index.py search "gi config"
```

The SQLite file is local/generated and ignored by git when it is large or
rebuildable. Commit this README, durable Markdown notes, preference JSON files,
schema notes, Markdown exports, and indexing scripts instead.

Use the database for verified facts, searchable file/symbol indexes, debugging
findings, useful commands, recurring failures, and durable notes with evidence
paths. Do not store secrets, credentials, private user data, or production data.

Do not dump the database into chat. Query it by symbol, path, topic, error, or
feature name with small limits.

## Two Memory Layers

- Markdown is the human-reviewable layer. Keep summaries, decisions,
  architecture notes, specifications, contracts, and curated exports concise.
- SQLite is the searchable agent-memory layer for detailed findings,
  file/symbol indexes, references, commands, failures, and evidence-backed notes.

Use structured memory and semantic retrieval for different jobs. SQLite or
another structured store is for deterministic facts and graphs: paths, symbols,
GUIDs, generated identifiers, asset links, reverse dependencies, commands,
failures, and evidence-backed notes. Vector retrieval is a complementary
semantic layer for conceptual search over curated notes, summaries,
architecture docs, specifications, and selected chunks. Verify current source
files before editing because any index can be stale.

Do not blindly migrate all Markdown into SQLite. When Markdown memory becomes
too large to read cheaply, introduce or rebuild the SQLite memory/index and keep
Markdown as the concise reviewable export.

## RAG System Structure

Treat RAG as a layered project-memory system, not as a synonym for vector
search. Keep source corpus rules, exclusions, manifest metadata, chunk
metadata, structured memory, retrieval adapters, context packets, observability,
evals, and writeback as separate responsibilities.

The project-local RAG manifest is:

```text
tools/project-memory/rag-system.json
```

Use SQLite FTS for exact paths, commands, symbols, versions, and error text
before adding semantic retrieval. Before enabling vector retrieval, export
semantic-ready chunks and record embedding metadata including model reference,
provider, dimensions, collection name, collection version, chunking rule, and
indexed time. Do not mix embeddings from different models or dimensions in one
collection version.

The generated semantic chunk export is rebuildable and ignored:

```powershell
python .\tools\project-memory\build_project_memory_index.py export-chunks
```

For a local semantic MVP, build Chroma from exported chunks while keeping Chroma
behind the retrieval adapter contract:

```powershell
python .\tools\project-memory\build_project_memory_index.py rebuild
python .\tools\project-memory\build_project_memory_index.py export-chunks
uv run --with chromadb python .\tools\project-memory\build_chroma_index.py rebuild
```

Keep generated semantic corpora, embedding caches, and vector indexes ignored
when rebuildable. Commit only reviewable config, docs, schemas, evals, and
scripts.

Use `gi sql` / `gi sqlite` and `gi vector` only as diagnostic commands. They
report counts, readiness, staleness, and recommendations for the project-memory
retrieval layers; they do not deploy external services, install heavy
dependencies, upload data, or index private sources by default.

Use `gi tools rebuild` or `gi rag rebuild` for a full configured
project-memory/RAG tooling rebuild after explicit confirmation. Node forms such
as `gi tools rebuild sql`, `gi rag rebuild chunks`, `gi tools rebuild vector`,
`gi rag rebuild manifest`, and `gi tools rebuild evals` rebuild or check only
that node. Use only commands documented in `rag-system.json`, local runbooks, or
helper scripts.

Prefer machine-checkable retrieval evals that verify readable configured RAG
files, generated-ignore rules, count consistency between enabled SQLite,
semantic corpus, and vector layers, exact keyword retrieval for paths, commands,
symbols, versions, and error text, semantic retrieval for conceptual guidance,
hybrid retrieval through keyword or vector evidence, and expected source paths
in top results. Do not make a model's free-form answer wording the primary eval
target.

## Suggested Files

- `pending-tasks.md`: active project-wide plans and multi-step work.
- `STUDY_PLAN.md`: roadmap for understanding the project.
- `git-preferences.json`: commit-message language preferences.
- `system-preferences.json`: agent user-facing working language preferences.
- `rag-system.json`: RAG source, exclusion, retrieval, context-packet, and
  writeback configuration.
- `semantic-retrieval-evals.md`: small eval set for semantic and hybrid
  retrieval quality.
- `build_chroma_index.py`: optional local Chroma adapter when semantic
  retrieval is enabled.
- `architecture-migrations.md`: durable history of major architecture moves.
- `specs/integration-contracts/connected-projects.md`: connected-projects
  register when this project depends on or regularly interacts with external
  repositories, cloned examples, sibling workspaces, service projects,
  libraries, documentation sites, upstream tools, or dashboards.
- `NOTES.md`: reviewable export of durable notes from local agent memory.
- `architecture.md`: verified architecture notes.
- `decisions.md`: durable decisions and rationale.
- `known-issues.md`: recurring bugs, caveats, and workarounds.

## Connected Projects

If this project adds, researches, vendors, or regularly interacts with an
external project, keep a connected-projects register, preferably:

```text
tools/project-memory/specs/integration-contracts/connected-projects.md
```

For each connected project, record its purpose, role in MultiSnap, approved
local folder path when it is a workspace dependency, canonical URLs, service
IDs or contract endpoints, owner/source of truth, update cadence, version or
branch policy, exchanged data and generated artifacts, safe setup/sync/build
commands, privacy and license boundaries, current status, caveats, and why the
dependency still exists. Read the register before touching integrations or
nested repositories. The register is not permission to inspect arbitrary
external folders; project-local scope and explicit user requests still govern
filesystem access.

## Task Planning

For analysis, refactoring, migration, or multi-step implementation tasks, keep a
concise checklist in `pending-tasks.md` or a dedicated task plan in this folder.

Include:

- goal
- planned changes
- execution order
- risks or dependencies
- verification steps

Update progress as meaningful steps complete. Keep plans task-relevant and avoid
full diffs, large logs, generated outputs, secrets, credentials, or private
production data.

## Rule

If a future agent would waste time rediscovering the same fact, write it down.
