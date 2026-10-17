// Fixed settings of the autopilot driver. Everything a reviewer needs to judge the run's
// behaviour lives here: models, limits, step types, paths and the file fence.

export const OPUS = 'claude-opus-5-5'

// Every v1 step finds findings or makes decisions, so every step runs on Opus.
// There is no fallback model: if Opus is unavailable the step stops.
export const STEP_DEFAULTS = {
  model: OPUS,
  effort: 'high',
  maxTurns: 500,
  maxBudgetUsd: 200, // runaway brake, API-equivalent estimate
  timeoutMin: 300,
}

// Subagent types allowed to run on a model other than Opus (.claude/agents/*.md).
// Any other subagent that ran on a non-Opus model stops the step.
export const CHEAP_AGENTS = {
  mekanik: /^claude-haiku-/,
  yazici: /^claude-sonnet-/,
}

// {doc} = document number ("06"), {stage} = stage number ("5"), {stage2} = "05",
// {round} = "" for round 1, "_R2", "_R3", ... afterwards.
export const STEP_TYPES = {
  writing: { label: 'Yazım oturumu', needsTask: true, reports: [] },
  audit: { label: 'Audit', reports: ['Docs/AUDIT_REPORTS/{doc}_AUDIT.md'] },
  'deep-review': { label: 'Deep review', reports: ['Docs/AUDIT_REPORTS/{doc}_DEEP_REVIEW.md'] },
  'audit-deep-review': {
    label: 'Audit ve deep review',
    reports: ['Docs/AUDIT_REPORTS/{doc}_AUDIT.md', 'Docs/AUDIT_REPORTS/{doc}_DEEP_REVIEW.md'],
  },
  'cross-review': { label: 'Cross-review', repeat: true, reports: ['Docs/CROSS_REVIEW_REPORTS/{doc}_CROSS_REVIEW{round}.md'] },
  'conflict-scan': { label: 'Çakışma taraması', reports: ['Docs/CHECKPOINT_REPORTS/PHASE{stage}_CONFLICT_SCAN.md'] },
  checkpoint: { label: 'Checkpoint', reports: ['Docs/CHECKPOINT_REPORTS/CP{stage2}_PHASE{stage}_CHECKPOINT.md'] },
  custom: { label: 'Serbest adım', needsTask: true, reports: [] },
}

// Steps that need the owner in the loop or edit .claude/ — never run by the driver.
export const MANUAL_ONLY = {
  'stage-opening': 'Aşama açılışı çalışma modlarını proje sahibine sorar',
  workshop: 'Workshop öneri listeleri proje sahibiyle konuşulur',
  'learning-promotion': 'Öğrenim terfisi .claude/ dosyalarını değiştirir ve 00 önerileri proje sahibinin onayını ister',
  'archive-mark': 'Arşiv işaretinden önce gösterilmemiş ⚠ kararların listesi proje sahibine gösterilir',
}

export const MAX_CROSS_REVIEW_ROUNDS = 10

// Paths a step session may change. Everything else is fenced.
export const FENCE = {
  allow: [/^Docs\//],
  deny: [/^Docs\/00_/, /^Docs\/BYPASS_LOG\.md$/],
  // Written by the driver itself, after its own checks.
  driverOwned: ['.claude/memory/MEMORY.md', '.claude/memory/MEMORY_ARCHIVE.md'],
}

export const TRACKER = 'Docs/PRODUCT_DISCOVERY_STATUS.md'
export const MEMORY = '.claude/memory/MEMORY.md'
export const MEMORY_ARCHIVE = '.claude/memory/MEMORY_ARCHIVE.md'

// Session work area inside the step worktree (gitignored). The ledger is written only by codex.mjs.
export const WORK_DIR = '.autopilot/work'
export const LEDGER = '.autopilot/ledger/codex.jsonl'
export const MEMORY_STAGING = { status: '.autopilot/work/memory/guncel-durum.md', archive: '.autopilot/work/memory/arsiv-ek.md' }

export const CI = {
  gate: 'CI Gate',
  postMergeWorkflows: ['CI', 'Publish'],
  pollSec: 20,
  timeoutMin: 45,
}

export const MONITOR_PORT = 4317
