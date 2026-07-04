# Agent Instructions

## Project

MultiSnap is a Windows-first screenshot capture and annotation desktop app
inspired by fast Monosnap-style workflows. The primary runtime surface is a
.NET 8 Windows desktop app with a WPF UI, WinForms tray integration, global
hotkeys, and Windows screen capture services.

## Loading Contract

- Start with this file.
- Read only the modules needed for the current request.
- If the request contains a GI chat command such as `gi ...`, `ги ...`, or a
  known mojibake form, treat it as a concrete task even when the message is
  short. First read `COMMANDS.md` when present, then read every runtime module
  routed to that command before acting.
- For state-changing GI commands that start, stop, restart, rebuild, deploy,
  test, install, reset, update, commit, push, or manage task-manager state, do
  not execute from memory, old chat examples, or a command name alone. If the
  command's routed module is unavailable, stop and report the missing path.
- For `gi restart`, `gi reboot`, `gi docker`, `ги рестарт`, `ги ребут`,
  `ги докер`, and equivalent aliases,
  `patterns/AGENTS_RUNTIME/09-project-operation-commands.md` is mandatory
  context before any process inspection, Docker build, stop, start, or success
  report.
- Keep behavior compatible with the previous monolithic `AGENTS.md`; the
  runtime modules change retrieval shape, not accepted safety, scope, command,
  verification, or git rules.

## Runtime Module Routing

- Purpose, RAG startup, project memory, handoff summaries, and shared-rule
  propagation: `patterns/AGENTS_RUNTIME/01-purpose.md`
- Repository map: `patterns/AGENTS_RUNTIME/02-repository-map.md`
- Rule precedence and scope arbitration:
  `patterns/AGENTS_RUNTIME/03-rule-precedence.md`
- Authoring reusable rules, configuration boundaries, code quality,
  project info/stack inventory, and batch verification:
  `patterns/AGENTS_RUNTIME/04-content-and-authoring.md`
- Windows shell and networking policy:
  `patterns/AGENTS_RUNTIME/05-windows-command-policy.md`
- Token economy, verification command lookup, `gi info`, `gi stack`,
  `gi refactor`, feature contracts, and large-output handling:
  `patterns/AGENTS_RUNTIME/06-tool-usage-and-token-economy.md`
- Startup, restore, project goal, bug evidence, repository cleanup,
  filesystem boundaries, and first-message handling:
  `patterns/AGENTS_RUNTIME/07-startup-and-scope.md`
- Config-service, service guide/contract lookup, task manager commands,
  manager-backed and local sprint commands, and web-service port registration:
  `patterns/AGENTS_RUNTIME/08-config-service-and-task-manager.md`
- Dev/prod publication, FTP deploy, restart/reboot, first test, full test,
  default reset, installer packaging, SQL/vector inspection, and project/RAG
  rebuild commands: `patterns/AGENTS_RUNTIME/09-project-operation-commands.md`
- Nested repositories, private local app data, product-plan intent signals,
  and missing required entities:
  `patterns/AGENTS_RUNTIME/10-private-scope-and-missing-context.md`
- Project, commit, task, and response language preferences:
  `patterns/AGENTS_RUNTIME/11-language-preferences.md`
- UI focus and frontend verification:
  `patterns/AGENTS_RUNTIME/12-ui-and-focus.md`
- Progress-update style: `patterns/AGENTS_RUNTIME/13-progress-updates.md`
- Update intake and `updates/` handling:
  `patterns/AGENTS_RUNTIME/14-update-intake.md`
- Verification policy: `patterns/AGENTS_RUNTIME/15-verification.md`
- Git policy: `patterns/AGENTS_RUNTIME/16-git-policy.md`

## Restore Context

If the user only sends a short greeting, thanks, acknowledgement, or
status-neutral message, do not run startup restore or read project files. Reply
briefly and ask what they want to do next.

Start here:

```powershell
.\tools\agent-start.ps1
```

If the startup script is unavailable, read only the smallest useful slices of:

- `AGENTS.md`
- latest file in `tools/summary/`
- `tools/AGENT_WORKING_AGREEMENTS.md`
- `tools/AGENT_RUNBOOK.md`
- relevant notes in `tools/project-memory/`

Use the RAG startup flow: retrieve only task-relevant context, search memory by
specific terms, and query SQLite memory only with small `LIMIT`s. For `gi start`,
`gi restore`, or title-only first messages, restore only enough orientation for
the next turn; do not read full summaries, runbooks, memory notes, logs, or diffs
unless a concrete task needs them.

Treat RAG as a layered system, not as a synonym for vector search. Use SQLite
FTS for exact paths, commands, symbols, versions, and error text before adding
semantic retrieval. Keep Chroma, Qdrant, pgvector, and similar stores behind a
retrieval adapter contract so prompts, `gi` startup, and memory writeback do not
depend on one vector database. Keep generated semantic corpora, embedding
caches, and vector indexes ignored when rebuildable.

Structured memory and semantic retrieval serve different jobs. Structured
stores such as SQLite hold deterministic facts and graphs: paths, symbols,
GUIDs, generated identifiers, asset links, reverse dependencies, commands,
failures, and evidence-backed notes. Vector retrieval is only a complementary
semantic layer over curated notes, summaries, architecture docs, and selected
chunks. Always verify the current source files before editing because any
memory or retrieval index can be stale.

The copied instruction kit is a token-economy and RAG-startup layer for this
project. Use it to restore only the needed context from local instructions,
handoff summaries, targeted searches, and project memory instead of reading the
whole repository or printing broad outputs.

Treat `cached input` as a symptom, not the main optimization target. Keep total
live context small by starting new sessions for unrelated tasks, using compact
handoff summaries instead of long investigation history, and splitting multi-step
R&D when later steps do not need the full previous reasoning trace.

Keep `gi start`, `gi restore`, and title-only startup messages scoped to
compact orientation for the next turn. Mention remembered plans, stale task
notes, old refactoring phases, or local commits ahead of a remote only when
relevant; do not offer to continue, run, finish, or push remembered work unless
the user explicitly asks for that action.

For `gi summary` / handoff summaries, write thematic thread state rather than a
short chronological retelling. Preserve user intent, important decisions, code
or architecture changes, business/product logic, verification evidence,
blockers, and next useful context. Break complex threads into topic sections and
brief thesis bullets, with links only when they help a future agent understand
or verify the context. For architecture or research conversations, especially
when the user evaluates an external project, article, pattern, or tool as a
possible integration target, preserve the user's integration intent and map
external concepts to current project components. Omit routine successful
command bookkeeping such as staging counts, branch names, push targets, commit
hashes, and git directives when git logs or command history can recover them.
If detailed protocol is needed, keep it separate as `Thread Timeline`.

When answering where a previous thread stopped, treat handoff summaries as
evidence, not the sole authority. Reconcile them with the latest visible thread
conclusion, screenshots, direct quotes, or other user-provided evidence. Prefer
the last explicit architectural/product decision, open question, or agreed next
direction over incidental caveats or old next-step bullets.

## Durable Memory

Durable project knowledge lives in:

```text
tools/project-memory/
```

Important findings should be written there or in a handoff summary, not only
left in chat.

Treat `tools/project-memory/` as the durable, portable product specification
layer. Record non-trivial feature algorithms, business logic, workflow
contracts, data rules, architecture migration history, verification guarantees,
and current implementation maps there so another agent can rebuild the same
behavior on a different stack. Treat `tools/summary/` as compact chat handoff
state, not as the canonical product specification.

For analysis, refactoring, migration, or multi-step implementation tasks, create
or update a concise checklist in `tools/project-memory/pending-tasks.md` or a
dedicated task plan in `tools/project-memory/` before editing code. Keep plans
task-relevant and update progress as meaningful steps complete.

Classify each meaningful batch before editing as refactor, development,
verification, operation, migration, configuration cleanup, or a named mix. Do
not hide behavior changes, public-contract changes, service operations, or data
migrations inside a "refactor" label.

For non-trivial feature, business-rule, data-model, integration, or architecture
work, update the relevant project-memory specification in the same scoped
change. Keep specs split by meaning, such as feature specs, business-rule docs,
data-model docs, integration contracts, and architecture migrations.

When this project reveals a reusable improvement to agent instructions,
workflows, templates, or checklists, write a dated recommendation to the shared
instruction library's `updates/` folder if it is available. If it is not
available, use a local intake folder such as `tools/instruction-updates/` or
`tools/project-memory/instruction-updates/`. Treat recommendations as intake,
not accepted rules.

Treat recommendation source projects and owners as provenance only. Reading a
recommendation in this repository's `updates/` folder is allowed during
`general-instructions` maintenance, but evidence paths, project names,
task-manager notes, product plans, or owner labels in that recommendation are
not permission to read, search, edit, or inspect the source project. Ask the
user or that project's owner for an explicit concrete path and action before
crossing the repository boundary.

Use this project as an experience source for `gi`: capture reusable workflows,
failure patterns, token-saving tactics, and agent-instruction improvements that
could help other projects. Keep recommendations concise, evidence-backed, and
free of secrets, private user data, production data, and unnecessary
project-specific details.

## Common Commands

Install dependencies:

```powershell
dotnet restore .\dotnet\MultiSnap.sln
```

Run:

```powershell
dotnet run --project .\dotnet\MultiSnap\MultiSnap.csproj
```

Test:

```powershell
dotnet build .\dotnet\MultiSnap\MultiSnap.csproj -p:Platform=x64
```

Build:

```powershell
dotnet publish .\dotnet\MultiSnap\MultiSnap.csproj -c Release -r win-x64 --self-contained false
```

Inspect logs:

```powershell
Get-Content .\*.log -Tail 120
```

## Working Areas

- Source: `dotnet/`
- Tests: add tests near the changed .NET source or under `dotnet/` when introduced.
- Tools: `tools/`
- Summaries: `tools/summary/`
- Project memory: `tools/project-memory/`

## Rules

- Do not revert user changes unless explicitly requested.
- Treat dirty worktrees as normal.
- Keep changes scoped to the current task.
- Ask before destructive operations, broad refactors, or unrelated scope
  expansion.
- Treat this project root as the filesystem boundary for normal work. Do not
  read, search, edit, create, delete, move, or inspect files in another project
  or arbitrary external folder unless the user gives an explicit concrete path
  and action. Use APIs, connectors, or task-manager endpoints for cross-project
  communication.
- Before filesystem writes, verify the active project root and target identity
  from local instructions, README, manifests, git remote, service id, or project
  memory. If the task appears to target a different product, repository, or
  absolute path outside this root, stop and warn the user unless the current
  message explicitly authorizes that exact external path and action.
- Treat `gi config`, `gi конфиг`, `ги конфиг`, `gi config service`,
  `ги конфиг сервис`, `ги конфиг сервис url=<url>`, and
  `ги конфиг сервис урл=<url>` as requests to get or set the bootstrap config
  for the config/discovery service. Read a project-local override only if local
  instructions define one, then read GI main config from
  `D:\AI\general-instructions\config\gi-main.json` or
  `GENERAL_INSTRUCTIONS_HOME`. Use its `configServiceUrl` to query the config
  service. Resolve local app and task-manager runtime URLs by service id through
  config-service; project task-manager config should keep only the selected
  manager name/id and non-secret project preferences. For the `url=<url>` form,
  validate a full `http://` or `https://` URL with no secrets, update the shared
  `configServiceUrl` pointer, and tell local services to use that URL for
  registration and discovery. Do not scan sibling project folders, guess ports,
  copy URLs from old task-manager memory, or use stale task-manager records as a
  runtime fallback.
- Treat `gi config service on`, `gi config service off`,
  `ги конфиг сервис on`, and `ги конфиг сервис off` as requests to set the
  current application's project-local config-service self-registration flag.
  `on` means the app should publish or refresh its own service record during
  startup; `off` means it must not. Do not reinterpret this as starting or
  stopping config-service itself. When setting `on`, first confirm a
  config-service URL is already configured in the same local config area or
  documented GI bootstrap config; if no URL is configured, tell the user to set
  `gi config service url=<url>` before enabling self-registration. Ask one
  short question if no local config location is documented.
- Treat `gi manager`, `gi tm`, `gi manager test`, `ги менеджер`, and
  `ги манагер` as task-manager inspection commands. Read the enabled manager id
  or `service_id` from project-local task-manager config, resolve it through
  config-service, read `endpoints.guide` when present, then read
  `endpoints.contract`, and use `endpoints.api` only for documented manager
  operations. Ignore legacy `base_url` values unless a local migration rule
  explicitly converts them into config-service records.
- Treat `gi active task`, `gi next task`, `gi get task`, and equivalent
  active-task wording as requests to obtain executable work from the configured
  task manager. Use only documented active-task, next-task, start/progress,
  blocker, completion, and readback operations, and only lifecycle identifiers
  returned by the manager.
- Treat `gi start sprint`, `gi sprint start`, and equivalent active-sprint
  wording as more specific than plain `gi start`. Restore only the context
  needed for task-manager work, resolve the configured manager through
  config-service, read the guide and contract, request the active Sprint/Cycle
  or next task through the documented operation, move work through documented
  lifecycle states, and submit completion through the manager contract. Do not
  fall back to generic startup restore, local task notes, raw intake, guessed
  endpoints, or filesystem task edits.
- Treat `gi local sprint`, `gi sprint local`, and equivalent explicitly local
  sprint wording as requests to run a local sprint checklist without a
  configured task manager or config-service. Use sprint content from the current
  message, current chat context, or a project-local planning file named by local
  instructions. If no sprint content is available, ask one short question for
  the local sprint goal and task list. Do not create raw manager intake, edit
  task-manager internals, resolve config-service, or claim that a visible
  Sprint/Cycle was created, started, completed, or synchronized.
- Treat `gi add sprint`, `gi create sprint`, `gi добавить спринт`, and
  equivalent add-sprint wording as requests to create a visible executable
  Sprint/Cycle through the configured task manager. Resolve the manager through
  config-service, normalize the title, goal, and tasks into the documented
  executable shape, read the created object back when supported, and do not
  downgrade the request to raw intake, Work Items, local checklists, or one-task
  substitutes.
- Treat task-manager sync commands as routine integration steps once the user
  has supplied sprint/task content or selected the workflow. Follow
  config-service discovery, service guide, strict contract, documented payload,
  lifecycle identifiers, readback, and blocker reporting. Do not replace
  manager API work with `project-memory`, `pending-tasks.md`, raw intake
  receipts, guessed commands, local checklists, Work Items, or "tell me the
  exact command" fallback.
- Agent-facing HTTP services should expose a compact guide endpoint plus a
  strict contract endpoint, preferably `GET /agent/guide` and
  `GET /agent/contract`, or adapter-specific equivalents such as
  `GET /agent-intake/guide` and `GET /agent-intake/contract`. Read the guide
  first when present, then the contract, and stop on guide/contract mismatches
  about endpoints, ownership, lifecycle identifiers, permissions, or supported
  object types.
- For web-facing applications that expose a port, HTTP API, web UI,
  task-manager service, or local daemon endpoint, require a live config-service
  lookup before the process binds or reserves any port. On every startup, read
  the configured config-service URL, verify the config service is reachable, and
  query the app's own `service_id` startup/service record to learn the port and
  neighboring service endpoints. If the service record exists, bind only the
  recorded port. If the service record is missing and the project-local
  self-registration flag is `on`, read the config-service guide and contract,
  list current records, choose a local free port that is absent from
  config-service, bind it, verify local health, and create or update the record
  only through the documented config-service operation. If self-registration is
  `off`, config-service is unavailable, or registration is undocumented, stop
  with a clear blocker. Do not invent registration payloads, write directly to
  config-service storage, reuse stale local runtime config, guess ports, scan
  blindly, or bind fallback ports. MultiSnap is a desktop app, so normal startup
  must not query or publish to config-service unless local instructions
  explicitly add a discoverable web/API runtime.
- Treat `gi ftp`, `ги фтп`, `gi ftp push`, `ги фтп пуш`, `gi upload ftp`,
  `gi deploy ftp`, and `gi залей на фтп` as requests to upload the current
  project's configured build output to FTP, FTPS, or SFTP. Treat
  `gi ftp config`, `gi ftp конфиг`, and `ги фтп конфиг` as requests to create,
  inspect, or update the project-local FTP/SFTP config without uploading. Treat
  `gi ftp folder`, `gi ftp папка`, and `ги фтп папка` as requests to inspect,
  choose, or update the remote upload folder (`remotePath`) without uploading.
  Treat `gi ftp service`, `gi ftp сервис`, and `ги фтп сервис` as requests to
  manually register, inspect, or select an FTP/FTPS/SFTP service record in
  config-service without uploading. Read project-local deploy instructions and
  `tools/deploy/ftp.local.json` first; when a project needs FTP and local config
  does not name a target service, query config-service for FTP-capable services.
  If exactly one matching service exists, use it after verifying its contract;
  if several exist, ask the user to choose with the same numbered Markdown
  checkbox style used by language selection. Keep secrets out of config-service:
  store only discovery metadata and secret references such as environment
  variable names. Keep project-specific deploy settings in the separate
  project-local config file rather than shared instructions or chat history.
  Prefer `tools/deploy/ftp.local.example.json` only as a redacted shape. Do not
  commit hostnames, usernames, passwords, tokens, private keys, or private
  remote paths unless project policy explicitly marks them non-secret.
- For FTP/FTPS upload stalls, hangs, repeated timeouts, or failed stream opens,
  inspect project-local FTP config, selected service contracts, and current
  user-provided details for an authorized SSH-based SFTP route to the same
  remote folder. If SSH host, port, user, remote folder, and credential
  reference are available, switch to SFTP over SSH before more FTP/FTPS upload
  variants and report that fallback. If required SFTP details are missing,
  report the exact missing fields or ask one short question. Do not disable FTPS
  certificate validation as a routine fallback unless explicitly authorized.
- Treat `gi reboot`, `ги ребут`, `gi restart`, and `ги рестарт` as requests to
  start or restart all documented applications in the current project using
  project-local run instructions. Before starting anything, identify the full
  app set from local run instructions, manifests, service records, desktop
  packaging metadata, or project memory. If local instructions define a
  preferred command that launches the full app set, use it; otherwise restart
  each running documented app and start each missing documented app in the
  background so focus does not jump away. After launch, wait briefly and verify
  each documented startup signal: expected processes, visible desktop windows
  when applicable, web/API health or discovery endpoints when applicable,
  worker readiness signals, and relevant startup or crash logs when documented.
  The final report must account for each app by name or role with
  started/restarted/skipped status and verification evidence. Do not report
  reboot success from a PID alone, from a web health check alone, or while any
  expected app is unlaunched or unverified.
- Treat `gi docker`, `ги докер`, and equivalent Docker restart wording as a
  request to restart the current project's documented Docker or Docker Compose
  runtime. Read project-local Docker/run instructions, compose files, Dockerfile
  or Containerfile, scripts, manifests, service records, and health-check
  contracts before touching containers. If no Docker/Compose config or
  documented Docker run contract exists, report that Docker is not configured
  for the project and stop. If Docker CLI, Docker Compose, or Docker engine is
  unavailable, report the blocker. Rebuild before restart when the image is
  missing, build inputs changed, the local contract requires rebuild, or image
  freshness cannot be proven. Scope operations to the current project only; do
  not prune Docker state, remove volumes/images, or stop unrelated containers.
  Verify container status, health checks, mapped URLs, and relevant recent logs
  before reporting status.
- Treat `gi first test`, `gi первый тест`, and `ги первый тест` as first-launch
  verification requests. Read project-local run, cleanup, cache reset, and test
  instructions before clearing anything. Reset only documented project-owned
  app cache, generated state, temporary first-run profiles, and rebuildable
  local app settings. Do not delete user documents, production data, secrets,
  credentials, external service data, shared system caches, sibling projects,
  or arbitrary user-home folders. If exact reset paths, keys, scripts, or
  commands are missing, ask one concise clarification question instead of
  guessing. After reset, start the app, run the documented first-launch checks,
  and report what was cleared, what passed, and what was left untouched.
- Treat `gi rebuild` and `ги ребилд` as requests to rebuild only the current
  project/application output, such as an executable, package, or documented
  artifact. Read project-local build or rebuild instructions, manifests,
  scripts, and packaging metadata before running the documented command. Do not
  treat it as dependency restore, tests-only verification, a RAG rebuild, or a
  combined project-plus-RAG rebuild. If no project rebuild contract exists, ask
  one short clarification question instead of inventing a command.
- Treat `gi tools rebuild`, `gi rag rebuild`, `ги тулс ребилд`, and
  `ги раг ребилд` as full GI/project-memory/RAG rebuild requests. Treat node
  forms such as `gi tools rebuild sql`, `gi rag rebuild chunks`,
  `gi tools rebuild vector`, `gi rag rebuild manifest`, and
  `gi tools rebuild evals` as scoped node rebuilds. A full rebuild requires
  explicit confirmation immediately before execution after listing source
  groups, privacy exclusions, generated paths that may be replaced, node
  commands, status checks, and required services. Use only documented
  project-local commands from `tools/project-memory/rag-system.json`, runbooks,
  or helper scripts. Do not commit generated SQLite databases, semantic
  corpora, vector indexes, logs, secrets, telemetry, or private runtime data.
- Treat `gi sql`, `gi sqlite`, and `gi vector` as diagnostic commands for
  project-memory retrieval readiness. They report counts, readiness, staleness,
  and recommendations; they do not deploy external services, install heavy
  dependencies, upload data, or index private sources by default.
- Treat `gi install`, `gi инсталл`, `ги инсталл`, and obvious typo variants
  such as `gi иснтлл` as requests to build the current project and produce an
  installer. Use Inno Setup by default when no installer tool is named. If the
  user writes a program after `gi install` / `gi инсталл`, use that program as
  the preferred packaging tool. Read project-local build and packaging
  instructions, scripts, manifests, and installer configs first; ask a short
  clarification question if the build or installer contract is missing instead
  of inventing one. Before packaging, resolve the application version from
  project-local metadata such as manifests, package files, assembly attributes,
  release files, or installer configs. Keep the production build, installer
  metadata, and installer filename aligned with that version when local tooling
  supports it. Do not use shared-instruction version numbers such as `VERSION.md`
  as the application version for another project.
- Treat restore, dependency install, build, and test as preliminary checks for
  `gi install`, not as completion. Complete `gi install` only after the
  project-local packaging command runs and a current installer artifact is
  produced or explicitly verified. If only verification checks ran, say so
  plainly and do not describe the project as installed or restored. On success,
  report the installer artifact path, version, and checks.
- Keep configuration boundaries explicit. Do not hard-code deployment,
  runtime, machine, service, credential, path, feature-flag, model, limit, or
  operational-policy values when they belong in project-local config,
  environment variables, service discovery records, or secret references. Avoid
  embedding machine-specific absolute paths in committed source, shared
  instructions, or examples; validate configured absolute paths at startup or
  I/O boundaries.
- Treat nested checkouts, vendored repositories, cloned examples, and
  third-party source trees as separate scope. Do not inspect them as part of the
  main project unless the user explicitly asks, the task is about that nested
  tree, or local instructions identify it as an active workspace component.
- Treat user-home application data and personal telemetry as private external
  sources. Do not read `.codex`, `.cursor`, IDE logs, browser profiles, shell
  history, application SQLite databases, or local app logs outside the project
  root unless the user gives an explicit path and action. For analyzer tasks,
  prefer mock or sample data, or ask for permission to inspect a specific file.
- Treat product plans, `apps.txt`, summaries, and task-manager notes as intent
  signals only. They are not permission to read private local data sources.
- If a required file, skill, config, script, endpoint, task, or other entity is
  missing or not found, first reread the relevant local instructions, runbook,
  project memory, and accepted instruction-kit artifacts for the current scope.
  If the entity is still missing, ask the user a short clarification question.
  Do not use another project folder or the shared instruction library as a
  runtime fallback unless the user explicitly gives that path and action.
- Prefer one language command with three ordered choices when the user wants
  language preferences for project work. Treat `gi language`, `gi язык`,
  `ги язык`, `gi project language`, `gi проект язык`, `ги проект язык`,
  `gi язык проекта`, and `ги язык проекта` as requests to configure, in order:
  project working environment languages, commit-message languages, and task
  languages in `tools/project-memory/system-preferences.json` and
  `tools/project-memory/git-preferences.json`.
- Apply the configured project working-environment language order to plans,
  checklists, progress updates, final answers, clarifying questions, and
  user-facing explanations. Do not use it to rewrite existing task text, code,
  commands, logs, quoted text, or a response language the user explicitly
  requested for a specific message.
- Apply the configured task language order to agent-created task titles, task
  descriptions, and task-manager updates.
- For each `gi язык` choice, preserve the user's selected order. The first
  selected language in each choice is primary for that surface.
- If `gi язык` or an equivalent unified project-language command is sent
  without explicit languages, run a three-step chat flow instead of asking for
  one free-form line. At each step, show the same numbered checklist of
  available languages with the current selection checked, render each option as
  a plain inline checkbox marker on one physical line such as
  `[x] 1. English`, name the current surface, and tell the user they may reply
  with numbers or language names. Do not use Markdown task-list syntax such as
  `- [x] 1. English` or ordered-task syntax such as `1. [x] English`. If a
  language surface has no current ordered selection, default it to
  `English`, then `Russian`, while preserving any explicit existing selection.
- When the user replies to that flow with a numeric-only answer such as `1 2`,
  interpret the numbers against the most recent language checklist and apply the
  resulting ordered languages to the current step. Do not ask which languages the
  numbers mean when the checklist was just shown.
- Do not commit secrets, credentials, local databases, logs, or generated caches.
- Treat API keys, access tokens, service-account keys, webhook secrets, signing
  secrets, and similar credentials as secret boundaries. Keep them out of source
  code, committed config, client bundles, public frontend environment variables,
  logs, traces, generated artifacts, chat responses, and project memory. Prefer
  per-person or per-service credentials, separate dev/staging/prod secrets,
  managed production secret stores, scoped permissions, usage monitoring,
  rotation, and network restrictions where supported.
- Do not print full `git diff` output by default. Prefer `git diff --stat` and
  targeted queries for relevant files or patterns.
- Preserve text encodings when editing files. On Windows, do not rewrite source
  files with PowerShell pipelines such as `Get-Content ... | Set-Content ...`
  unless both read and write encodings are explicit and known correct. Prefer
  `apply_patch`, editor-native saves, or language APIs that read and write the
  file with an explicit encoding such as UTF-8. If non-ASCII text appears as
  mojibake after a command, stop, restore the last clean file version, and
  reapply only the intended small patch.
- Do not remove `AGENTS.md`, `tools/`, `tools/project-memory/`, `skills/`,
  bootstrap scripts, update scripts, deploy scripts, or agent-facing
  instruction/config files merely because they look internal. Inspect their
  purpose first and treat them as possible RAG/startup infrastructure; delete
  them only when the user explicitly confirms they are temporary or unrelated
  to the project.
- Classify `*.sqlite`, `*.sqlite3`, and `*.db` before deleting or committing.
  Keep rebuildable generated agent-memory indexes ignored, but preserve useful
  README files, Markdown/JSON memory exports, schemas, and indexing scripts.
  Never commit databases containing secrets, private data, telemetry,
  task-manager state, absolute local paths, or agent conversation history.
- For non-trivial features, keep a durable project-local feature document with
  the feature idea, functional description, workflow contract, implementation
  plan, sprint breakdown, tasks, definitions of done, and verification. Before
  changing a feature with a recorded contract, read it and preserve its
  user-visible guarantees unless the user explicitly changes the agreement. If
  implementation changes the agreed workflow, update the contract in the same
  scoped change and report the behavior change. Use
  `tools/templates/FEATURE_WORKFLOW_CONTRACT.template.md` when a new contract
  is needed.
- For first-pass project study, read local instructions, README, manifests, and
  config entry points before building a file map. Use recursive scans only after
  a targeted search fails or the task clearly requires repository-wide
  inventory.
- Do not read large files in full by default, including large `index.html`,
  bundled JS/CSS, logs, lockfiles, generated files, and build artifacts. Prefer
  targeted searches, heads, tails, or small line ranges such as
  `Get-Content -TotalCount`, `Get-Content -Tail`, and `Select-String` on
  PowerShell.
- For verification, count or query HTML elements programmatically instead of
  printing the whole HTML document.
- Do not produce broad artifacts, such as zip archives, or run full check
  matrices unless the user explicitly asks for that scope.
- Final responses should summarize only the changes, checks, and current status;
  do not restate the full investigation context.
- Search for specific symbols, paths, errors, or patterns before doing broad
  repository scans.
- Do not print large logs. Prefer tails and targeted error searches.
- Keep progress updates phase-level, not command-level. Do not narrate after
  every command batch, report counters such as "ran 4 commands", or live-blog
  each intermediate hypothesis. Update when the phase changes, a meaningful
  finding changes the next step, a blocker appears, or work has been quiet long
  enough that the user needs reassurance.
- Do not duplicate tool-run counters that the chat UI may show automatically;
  system UI counters are not agent progress updates.
- Startup restore must be compact; do not dump large files, full runbooks, full
  SQLite contents, full logs, generated outputs, or full diffs.
- Treat short greetings, thanks, acknowledgements, and status-neutral messages
  as no-ops unless they include an explicit task, path, command, error, or
  project question. Do not run startup restore for those messages.
- Treat screenshots, logs, pasted errors, or other bug evidence as requests for
  analysis first. Explain the likely issue and ask what action the user wants
  before editing files, unless the user explicitly says to fix it, such as
  `fix`, `почини`, or `gi почини`.
- Treat `gi help`, `ги хелп`, `ги help`, `gi commands`, and `ги команды` as
  read-only requests for a compact local GI command index with short
  descriptions. Prefer `COMMANDS.md` when present, otherwise use `AGENTS.md`,
  the runbook, and working agreements. Do not run startup restore, resume old
  work, call services or task managers, mutate files, execute listed commands,
  or ask the user to choose a command unless they request help with a specific
  command.
- Treat `gi test plan`, `gi тест-план`, and equivalent verification-plan
  wording as requests to inspect current project-local test, smoke-check, API,
  UI, or CLI command contracts before recommending or running checks. Verify
  exact commands, CLI flags, ports, routes, health endpoints, request payload
  fields, and environment variables from current local instructions, runbooks,
  manifests, config entry points, or source code. Treat handoff summaries, task
  notes, screenshots, and old chat examples as status evidence, not
  authoritative command contracts.
- Before running a WorkNest sprint workflow, verify required endpoint methods
  and query parameters against the adapter contract. The current documented
  `next-task` contract is
  `GET /agent-intake/next-task?project=<project>&sprintId=<sprintId>`. If a
  task-manager endpoint returns an unexpected method, parameter, or routing
  error, re-read the adapter endpoint docs before trying any workaround. If the
  documented contract still does not match the running service, report a stale
  or misconfigured manager endpoint and stop before sending or completing work.
- Keep commit-message language preferences separate from the agent's
  user-facing working language unless the user uses the unified project-language
  command.
- Treat `gi commit language`, `gi коммит язык`, `ги коммит язык`, and older
  `gi язык коммита` forms as requests to configure commit-message languages in
  `tools/project-memory/git-preferences.json`.
- Treat `gi system language`, `gi систем язык`, and `ги систем язык` as
  requests to configure the agent's project working language in
  `tools/project-memory/system-preferences.json`.
- Follow `tools/project-memory/system-preferences.json` for progress updates,
  final answers, clarifying questions, user-facing explanations, and
  agent-created task artifacts. Do not use it to rewrite existing task text,
  code, commands, logs, quoted text, or a response language the user explicitly
  requested for a specific message.
- Launch applications in the background so focus does not jump away from the
  user's current window.
- After implementing a frontend, backend, API, or full-stack feature, restart
  the affected dev server or backend process when local run instructions provide
  a restart command or hot reload is uncertain. Then refresh the browser,
  client, or API caller before verification so checks do not use stale HTML,
  JavaScript, routes, schemas, or cached responses.
- Follow the copied `general-instructions` instruction kit for the full set of
  rules. In this project, use `AGENTS.md`, `tools/AGENT_WORKING_AGREEMENTS.md`,
  `tools/AGENT_RUNBOOK.md`, `tools/agent-start.ps1`, and project memory as the
  local authoritative sources.
- Use Context7 or similar external documentation retrieval only when configured
  or explicitly requested for current public library, framework, SDK, or API
  docs. Do not use it as project memory, service discovery, task management, or
  current local source truth. Prefer project-local instructions and
  guide/contract endpoints for project behavior, and prefer official OpenAI
  documentation workflows for OpenAI product questions. Do not send secrets,
  credentials, private source code, private business rules, user data,
  production data, telemetry, local paths, or project-memory contents to
  external doc services unless explicit private-source configuration exists and
  the user approves the exact scope. Pin exact library IDs and versions when
  known, and verify current local source before editing.
- Treat copied shared-library files such as `COMMANDS.md` and `patterns/*.md`
  as local instruction-kit runtime material after they are installed. During
  accepted instruction-kit updates, compare them with the configured upstream
  source and preserve project-local rules that are stricter or more specific.
- Use `patterns/SENIOR_AGENT_ENGINEERING_STANDARD.md` as the compact execution
  checklist for code-writing work: load relevant local context, preserve
  intended behavior, keep architecture and configuration boundaries clear, work
  in coherent verified batches, update durable project memory when behavior or
  architecture changes, and escalate high-risk actions through documented
  approval paths.
- Treat `init <source>`, `инит <source>`, `инициализируй <source>`, and
  `инит правила <source>` as shared-instruction bootstrap/startup requests when
  `<source>` points to the canonical shared-instruction Git repository, the
  current shared-instruction checkout/cache, `GENERAL_INSTRUCTIONS_HOME`, or
  another known `general-instructions` source. Read existing instruction files
  and follow GI bootstrap/startup rules. Do not reinterpret those forms as
  `git init`, folder creation, OpenCode setup, project creation, `npm init`, or
  `python -m venv` unless the user explicitly names that action.
- Run `gi обновить` quietly by default. Keep it scoped to accepted
  instruction-kit updates and migrations. Do not reinterpret it as a request to
  push pre-existing local commits, sync a feature branch, resume a remembered
  plan, or perform general Git maintenance. Commit or push only changes created
  by the update flow itself and only when the local update policy permits it.
- During `gi обновить`, inspect newly applied migrations for RAG-impacting
  changes. If they change RAG source rules, chunking, embedding metadata,
  SQLite/vector schemas, retrieval adapters, or project-memory index scripts,
  compare the migration IDs with `tools/project-memory/rag-system.json` rebuild
  state. Report stale nodes and ask before a full rebuild, or run/offer the
  smallest documented node rebuild for narrow migrations. Do not mark rebuild
  state current until rebuild and readback/status checks succeed.
- When local project rules conflict with shared instructions, the local
  `AGENTS.md`, runbook, and working agreements take precedence.

## Imported Claude Cowork project instructions
