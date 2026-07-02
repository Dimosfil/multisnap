# Agent Working Agreements

## Scope

- Keep changes small and tied to the current request.
- Ask before expanding into unrelated modules.
- If a task requires files outside the agreed working area, say so first.
- Treat the current project root as the filesystem boundary for normal work.
  Do not read, search, edit, create, delete, move, or inspect files in another
  project or arbitrary external folder unless the user gives an explicit
  concrete path and action. Use APIs, connectors, or task-manager endpoints for
  cross-project communication.
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

## User Changes

- Do not revert user changes unless explicitly requested.
- Treat dirty worktrees as normal.
- If user changes affect the task, work with them.
- Classify each meaningful batch before editing as refactor, development,
  verification, operation, migration, configuration cleanup, or a named mix. Do
  not hide behavior changes, public-contract changes, service operations, or
  data migrations inside a "refactor" label.

## Git

- Default: the agent edits and verifies; the user reviews and commits.
- Treat `gi коммит`, `gi пуш`, `gi коммит пуш`, and `gi только пуш` as explicit
  git finish requests. `gi коммит` commits scoped current changes only; `gi пуш`
  and `gi коммит пуш` commit scoped current changes and push the current branch;
  `gi только пуш` pushes existing local commits without creating a new commit.
  Inspect status, keep unrelated/user changes out, follow commit-message
  preferences, and stop on ambiguous scope, missing remote, conflicts, secrets,
  or push failures.
- Treat `gi пул`, `gi pull`, and `ги пул` as explicit requests to fetch and pull
  the current branch from its configured upstream. Inspect status, branch, and
  upstream first. Resolve only obvious, low-risk conflicts where intent is clear
  and user changes are preserved; if product judgment, unrelated changes,
  secrets, or uncertainty are involved, stop and ask the user with concise
  options.
- Exception: after a successful `gi обновить` / `gi обновись`, commit and push
  only the resulting instruction-kit update changes when this project is a git
  repository with a configured remote. If unrelated/user changes, no remote,
  push failure, or conflicts are present, stop and explain the blocker.
- Branch naming: `TODO`.
- Generated files policy: `TODO`.
- Never commit secrets, credentials, local databases, logs, or caches.
- Treat API keys, access tokens, service-account keys, webhook secrets, signing
  secrets, and similar credentials as secret boundaries. Keep them out of source
  code, committed config, client bundles, public frontend environment variables,
  logs, traces, generated artifacts, chat responses, and project memory. Prefer
  per-person or per-service credentials, separate dev/staging/prod secrets,
  managed production secret stores, scoped permissions, usage monitoring,
  rotation, and network restrictions where supported.
- Follow `tools/project-memory/git-preferences.json` for commit-message
  languages. English is primary; selected additional languages are included when
  the user explicitly asks the agent to commit.
- When the user asks in chat to change commit-message languages, update
  `tools/project-memory/git-preferences.json` directly and summarize the new
  setting.
- Do not infer additional commit-message languages from the user's UI language
  or message language. If the requested languages are ambiguous, ask which
  additional languages to enable.
- For ambiguous commit-language selection, ask with a concise numbered Markdown
  checklist showing `English` as always selected and current additional
  languages as checked. Explain that `English` is the required primary
  commit-message language and cannot be disabled. Ask the user to reply with
  language names or numbers.
- When reporting this change, mention the plain
  `tools/project-memory/git-preferences.json` path instead of malformed or
  placeholder markdown links.
- If the user explicitly wants to configure languages manually, they can run:

```powershell
.\tools\select-git-commit-languages.ps1
```

or:

```powershell
.\tools\agent-start.ps1 -ConfigureGitCommitLanguages
```

## Agent Language

- Follow `tools/project-memory/system-preferences.json` for the agent's
  user-facing working language in this project.
- Apply the configured system or project language to progress updates, final
  answers, clarifying questions, user-facing explanations, agent-created task
  titles, task descriptions, task-manager updates, plans, and checklists.
- Do not apply the system or project language to existing task text, code,
  commands, logs, quoted text, or a response language the user explicitly
  requested for a specific message.
- Treat `gi language`, `gi язык`, `ги язык`, `gi project language`,
  `gi проект язык`, `ги проект язык`, `gi язык проекта`, and `ги язык проекта`
  as requests to configure three ordered language sequences: project working
  environment, commit messages, and tasks.
- If the unified project-language command does not include explicit languages,
  ask in three numbered steps. For each step, show a concise numbered checklist
  with the available languages and the current selection using plain inline
  checkbox markers such as `[x] 1. English`, then accept the user's next answer
  as numbers or language names for that step. Do not use Markdown task-list
  syntax such as `- [x] 1. English` for chat selection prompts.
- If a language surface has no current ordered selection, default it to
  `English`, then `Russian`, while preserving any explicit existing selection.
- If the user replies with only numbers, such as `1 2`, map them to the most
  recent checklist and preserve that order. Do not ask what those numbers mean
  after showing the checklist.
- Treat `gi system language`, `gi систем язык`, and `ги систем язык` as
  requests to configure this preference.
- Keep this setting separate from commit-message languages. `gi commit
  language`, `gi коммит язык`, `ги коммит язык`, and older `gi язык коммита`
  forms configure `tools/project-memory/git-preferences.json`, not the agent's
  working language. The unified project-language command updates both
  preference files.
- If the user explicitly wants to configure the system language manually, they
  can run:

```powershell
.\tools\select-system-language.ps1
```

or:

```powershell
.\tools\agent-start.ps1 -ConfigureSystemLanguage
```

## Context Hygiene

- Do not print full `git diff` output by default. Prefer `git diff --stat` and
  targeted queries for relevant files or patterns.
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
- Launch applications in the background so focus does not jump away from the
  user's current window.
- Treat `gi reboot` and `gi restart` as requests to start or restart all
  documented applications in the current project using project-local run
  instructions. Identify the full app set before launch, use a preferred
  full-app start/restart command when one exists, otherwise account for each
  documented desktop app, web/API app, and worker separately. Verify each
  documented startup signal after launch and do not report success from a PID
  alone or from a web health check alone.
- Treat a short first message as a possible chat title: restore context, then
  ask what to do next instead of executing the title as a task.
- Treat short chat commands that start with `gi` as shared instruction-kit
  commands for the copied `general-instructions` kit in this project. `gi` is
  the only short prefix; do not rename it to `GAI` or another alias.
  Before acting on any state-changing `gi` / `ги` command, read `COMMANDS.md`
  when present and the routed `patterns/AGENTS_RUNTIME/` module for that
  command. If the routed module is missing, stop and report the missing path.
  If a `gi` command is missing a needed parameter, ask one short clarification
  question instead of guessing.
- Use the instruction kit as a token-economy and RAG-startup layer: restore only
  task-relevant context from local instructions, summaries, targeted searches,
  and project memory instead of broad repository reads or large outputs.
- Treat RAG as a layered system, not as a synonym for vector search. Use SQLite
  FTS for exact paths, commands, symbols, versions, and error text before adding
  semantic retrieval. Keep Chroma, Qdrant, pgvector, and similar stores behind a
  retrieval adapter contract, and keep generated semantic corpora, embedding
  caches, and vector indexes ignored when rebuildable.
- Use structured memory and semantic retrieval for different jobs. Structured
  stores hold deterministic facts and graphs such as paths, symbols, generated
  identifiers, dependencies, commands, failures, and evidence-backed notes.
  Vector retrieval is a complementary semantic layer over curated notes,
  summaries, architecture docs, and selected chunks. Verify current source files
  before editing because memory indexes can be stale.
- Keep `gi` command responses scoped to the shared instruction-kit command. Do
  not resume an older product task after a `gi` command unless the user
  explicitly asks.
- Run `gi` commands against this project root. Do not switch to another
  repository, the shared instruction library, or a path from an older task unless
  the user explicitly asks.
- Task-manager paths, raw intake metadata, summaries, or previous chat context
  are not permission to enter another project folder.
- `gi` means `general-instructions`, not `git`. Missing `.git` blocks only the
  automatic commit/push step after a successful GI update; it does not block
  checking or applying instruction-kit file updates.
- Treat `gi саммари` and `gi summary` as requests to write a handoff summary
  file under `tools/summary/`, not only as requests to summarize in chat.
- Write `gi summary` handoffs thematically, not as short chronological retells.
  Preserve user intent, important decisions, code or architecture changes,
  business/product logic, verification evidence, blockers, and next useful
  context. Omit routine successful command bookkeeping such as staging counts,
  branch names, push targets, commit hashes, and git directives when git logs or
  command history can recover them. If a thread protocol is needed, keep it in
  a separate `Thread Timeline` section.
- When the user asks where a previous thread stopped, reconcile handoff
  summaries with the latest visible thread conclusion, screenshots, quotes, or
  other user-provided evidence. Prefer the last explicit product/architecture
  decision, open question, or agreed next direction over incidental caveats or
  old next-step bullets.
- Treat `gi гит-обзор` and `gi git summary` as requests to summarize the latest
  git commit in the current project in chat. Include commit metadata, changed
  files, compact stats, inferred purpose, and notable risks or checks. Do not
  print a full diff, create a summary file, commit, or push for this command.
- Treat `gi тест-план` and `gi test plan` as requests to inspect local project
  test commands and produce a compact verification plan for the current feature,
  bug fix, or release check. Plan first; run checks only when the user asks or
  when the current task already requires verification.
- Treat `gi help`, `ги хелп`, `ги help`, `gi commands`, and `ги команды` as
  read-only requests for the local GI command index. Prefer `COMMANDS.md` when
  present, otherwise use project-local instructions. Do not run startup restore,
  resume old work, call services or task managers, mutate files, or execute the
  listed commands.
- Treat `gi first test` and equivalent first-launch wording as requests to
  reset only documented project-owned first-run state and verify the documented
  first-launch workflow. If reset paths, keys, or commands are undocumented,
  ask one concise clarification question instead of guessing.
- Treat `gi rebuild` as a project/application rebuild only. Treat
  `gi tools rebuild` and `gi rag rebuild` as GI/project-memory/RAG rebuilds,
  with node forms for `sql`, `chunks`, `vector`, `manifest`, and `evals`. Full
  GI/RAG rebuilds require explicit confirmation after listing source groups,
  exclusions, generated paths, node commands, status checks, and dependencies.
- Treat `gi sql`, `gi sqlite`, and `gi vector` as inspection-only diagnostics
  for project-memory retrieval readiness.
- For verification plans and smoke checks, confirm exact CLI flags, ports,
  routes, methods, JSON payload fields, health endpoints, and required
  environment variables from current local instructions, manifests, config, or
  source code. Summaries, task notes, screenshots, and old chat snippets are
  evidence, not authoritative command contracts.
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
  as the application version for another project. After success, report the
  build result, installer artifact path, version used, and checks performed.
- Treat restore, dependency install, build, and test as preliminary checks for
  `gi install`, not as completion. Complete `gi install` only after the
  packaging command runs and a current installer artifact is produced or
  explicitly verified. If only verification checks ran, say so plainly.
- Treat a first message that points to a shared instruction library as an
  instruction bootstrap, not as a request to add that library as a dependency.
- Treat `init <source>`, `инит <source>`, `инициализируй <source>`, and
  `инит правила <source>` as shared-instruction bootstrap/startup requests when
  `<source>` points to a known `general-instructions` source. Never reinterpret
  these forms as `git init`, folder creation, OpenCode setup, project creation,
  `npm init`, or `python -m venv` unless the user explicitly names that action.
- If the user asks to update from a shared instruction library and this project
  has no `tools/project-memory/instruction-kit.json`, treat that as first-time
  instruction bootstrap/init.
- Run `gi обновить` quietly by default: do not narrate step-by-step reasoning,
  repeated progress, command transcripts, broad file reads, or full diffs during
  normal successful updates. Apply the update, then report a compact summary
  with versions, migration counts/IDs, changed files, checks, commit/push
  result, and blockers if any.
- During `gi обновить`, inspect newly applied migrations for RAG-impacting
  changes and compare them with `tools/project-memory/rag-system.json` rebuild
  state. Report stale nodes and ask before a full rebuild, or run/offer the
  smallest documented node rebuild for narrow migrations. Do not mark rebuild
  state current until rebuild and readback/status checks succeed.
- For web applications, assume the user will inspect the UI manually. Do not
  open, browse, screenshot, or visually inspect the UI automatically unless the
  user explicitly asks for that.

## Editing

- Prefer patch-style edits for manual changes.
- Avoid unrelated formatting churn.
- Add comments only when they clarify non-obvious behavior.

## Task Planning

- For analysis, refactoring, migration, or multi-step implementation tasks,
  create or update a concise checklist in `tools/project-memory/pending-tasks.md`
  or a dedicated task plan in `tools/project-memory/` before editing code.
- Include the goal, planned changes, execution order, risks or dependencies, and
  verification steps.
- Update progress as meaningful steps complete.
- Keep plans concise. Do not store full diffs, large logs, generated outputs,
  secrets, credentials, or private production data.

## Shared Instruction Updates

- When this project reveals a reusable improvement to agent instructions,
  workflows, templates, or checklists, write a dated recommendation to the shared
  instruction library's `updates/` folder if it is available.
- If no shared instruction library is available, use a local intake folder such
  as `tools/instruction-updates/` or
  `tools/project-memory/instruction-updates/`.
- Treat recommendations as intake, not accepted rules.
- Treat recommendation source projects and owners as provenance only. Reading a
  recommendation in this repository's `updates/` folder is allowed during
  `general-instructions` maintenance, but evidence paths, project names,
  task-manager notes, product plans, or owner labels in that recommendation are
  not permission to read, search, edit, or inspect the source project. Ask the
  user or that project's owner for an explicit concrete path and action before
  crossing the repository boundary.
- Recommendations should explain the observed problem, reusable rule or
  workflow, evidence paths, affected files or commands, risks, and privacy
  review.
- Capture reusable workflows, failure patterns, token-saving tactics, and
  agent-instruction improvements that could improve `gi` for other projects.
- Do not add a shared instruction library as a project dependency, package,
  submodule, symlink, or runtime reference unless the user explicitly asks for
  that.

## Task Managers

- Treat task-manager configuration as project-local state.
- Store only the manager name or `service_id` plus non-secret project
  preferences in project memory.
- Resolve task-manager runtime URLs through GI config-service by service id;
  do not store, guess, or copy API endpoints from old notes or other projects.
- If a configured manager id is missing from config-service, stop with a concise
  blocker instead of falling back to port scans or stale task-manager memory.
- For web-facing services, require live config-service discovery before binding
  ports. If a service record is missing and project-local self-registration is
  `on`, self-register only through the documented config-service guide and
  contract after choosing a local free port that is absent from current service
  records and verifying local health. If self-registration is `off` or the
  registration contract is missing, stop with a blocker.
- Before posting plans or starting sprint work, verify the workflow-specific
  manager contract and capabilities, not only generic health.
- Treat task managers as work queues and lifecycle recorders, not as the actors
  doing implementation work. The agent takes, implements, verifies, and reports
  tasks through the manager.
- For single-task intake, require executable lifecycle identifiers, a clear
  rejection, or explicit intake-only documentation. Do not create replacement
  one-task plans to work around raw task receipts that cannot be advanced
  through lifecycle endpoints.
- Treat `gi active task`, `gi next task`, and equivalent wording as requests to
  obtain executable work through the configured manager's documented lifecycle
  API. Use only lifecycle identifiers returned by the manager, and report
  blockers when contract, auth, permissions, status updates, or readback are
  unavailable.
- Treat `gi start sprint`, `gi sprint start`, and equivalent active-sprint
  wording as more specific than plain `gi start`. Resolve the configured
  manager through config-service, read the guide and contract, request the
  active Sprint/Cycle or next task through documented operations, and complete
  work through documented lifecycle states. Do not fall back to generic startup
  restore, local task notes, raw intake, guessed endpoints, or filesystem task
  edits.
- Treat `gi local sprint`, `gi sprint local`, and equivalent explicitly local
  sprint wording as requests to run a local sprint checklist without a
  configured task manager or config-service. Use sprint content from the current
  message, current chat context, or a project-local planning file named by local
  instructions. If no sprint content is available, ask one short question for
  the local sprint goal and task list. Do not create raw manager intake, edit
  task-manager internals, resolve config-service, or claim that a visible
  Sprint/Cycle was created, started, completed, or synchronized.
- Treat `gi add sprint`, `gi create sprint`, and equivalent wording as requests
  to create a visible executable Sprint/Cycle through the configured manager.
  Resolve the manager through config-service, use only documented create/read
  operations, and do not downgrade the request to raw intake, local checklists,
  Work Items, or one-task substitutes.
- Treat task-manager sync commands as routine integration steps once the user
  has supplied sprint/task content or selected the workflow. Follow
  config-service discovery, service guide, strict contract, documented payload,
  lifecycle identifiers, readback, and blocker reporting. Do not replace
  manager API work with `project-memory`, `pending-tasks.md`, raw intake
  receipts, guessed commands, local checklists, Work Items, or "tell me the
  exact command" fallback.
- Treat `gi manager`, `gi tm`, and equivalent manager-test wording as requests
  to inspect the configured manager through config-service. Read
  `endpoints.guide` when present, then `endpoints.contract`, and use
  `endpoints.api` only for documented operations.
- Agent-facing HTTP services should publish an onboarding guide endpoint and a
  strict contract endpoint. Stop on guide/contract mismatches about endpoints,
  workflow ownership, lifecycle identifiers, permissions, or supported object
  types.

## Feature Workflow Contracts

- Use Context7 or similar external documentation retrieval only when configured
  or explicitly requested for current public library, framework, SDK, or API
  documentation. It is not project memory, service discovery, task management,
  or current local source truth. Prefer local instructions and service
  guide/contract endpoints for project behavior. Do not send secrets, private
  source, private business rules, user data, production data, telemetry, local
  paths, or project-memory contents to external doc services unless explicit
  private-source configuration exists and the user approves the exact scope.

- For non-trivial features, keep durable project-local feature documents that
  include idea, functional description, workflow contract, implementation plan,
  sprint breakdown, tasks, definitions of done, and verification.
- Before changing a feature with a recorded contract, read it and preserve its
  user-visible guarantees unless the user explicitly changes them.
- If implementation changes the agreed workflow, update the contract in the
  same scoped change and report the behavior change.
- Use `tools/templates/FEATURE_WORKFLOW_CONTRACT.template.md` for new
  contracts when no more specific project template exists.

## Repository Cleanup

- Do not delete `AGENTS.md`, `tools/`, `tools/project-memory/`, `skills/`,
  bootstrap scripts, update scripts, deploy scripts, or agent-facing
  instruction/config files merely because they look internal. Inspect their
  purpose first and delete only with explicit user confirmation.
- Classify `*.sqlite`, `*.sqlite3`, and `*.db` before deleting or committing.
  Never commit databases containing secrets, private data, telemetry,
  task-manager state, absolute local paths, or agent conversation history.

## Configuration Boundaries

- Keep deployment, runtime, machine, service, credential, path, feature-flag,
  model, limit, and operational-policy values in project-local config,
  environment variables, service discovery records, or secret references when
  they vary by environment.
- Avoid embedding machine-specific absolute paths in committed source,
  instructions, or examples; validate configured absolute paths at startup or
  I/O boundaries.
- Use `patterns/SENIOR_AGENT_ENGINEERING_STANDARD.md` as the compact execution
  checklist for code-writing work: load relevant local context, preserve
  intended behavior, keep architecture and configuration boundaries clear, work
  in coherent verified batches, update durable project memory when behavior or
  architecture changes, and escalate high-risk actions through documented
  approval paths.

## Verification

- Reread edited files after changes.
- Run the fastest relevant check first.
- Record checks run and failures in the handoff summary.

## Processes

- Ask before closing editors, apps, servers, or other visible processes.
- Launch GUI tools quietly in the background when possible.
