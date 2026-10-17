// Pure checks on text and file lists. No I/O here, so every rule is unit-tested
// (scripts/autopilot/test/checks.test.mjs).
import { FENCE, CHEAP_AGENTS, OPUS } from './config.mjs'
import { FORBIDDEN_ATTEMPT, FORBIDDEN_TOOLS, touchesHomeClaude } from './perm.mjs'

// --- git status --------------------------------------------------------------

// `git status --porcelain=v1 -z -uall` -> [{ code, path }]; renames report the new path.
export function parseStatusZ(out) {
  const parts = out.split('\0').filter(Boolean)
  const res = []
  for (let i = 0; i < parts.length; i++) {
    const code = parts[i].slice(0, 2)
    res.push({ code, path: parts[i].slice(3) })
    if (code[0] === 'R' || code[0] === 'C') i++ // skip the source path
  }
  return res
}

// --- file fence --------------------------------------------------------------

export function fenceViolations(changes, { allowDriverOwned = false } = {}) {
  const bad = []
  for (const c of changes) {
    const p = c.path.replace(/\\/g, '/')
    if (allowDriverOwned && FENCE.driverOwned.includes(p)) continue
    if (!FENCE.allow.some(r => r.test(p)) || FENCE.deny.some(r => r.test(p))) bad.push(`${p} (korumalı yol)`)
    else if (c.code.includes('D')) bad.push(`${p} (silinmiş — silme proje sahibinin onayıyla yapılır)`)
    else if (/^Docs\/[^/]+$/.test(p) && c.code.includes('?')) bad.push(`${p} (yeni doküman — proje sahibinin onayıyla açılır)`)
  }
  return bad
}

export function expandReport(pattern, { doc, stage, round }) {
  return pattern
    .replace('{doc}', doc)
    .replace('{stage2}', String(stage).padStart(2, '0'))
    .replace('{stage}', String(stage))
    .replace('{round}', round > 1 ? `_R${round}` : '')
}

export function missingReports(expected, changes) {
  const changed = new Set(changes.filter(c => !c.code.includes('D')).map(c => c.path.replace(/\\/g, '/')))
  return expected.filter(r => !changed.has(r))
}

// --- records -----------------------------------------------------------------

// "**Versiyon: v0.125**" on the header line -> [0, 125]
export function docVersion(text) {
  const m = /\*\*Versiyon:\s*v(\d+(?:\.\d+)*)\*\*/.exec(text.split('\n').slice(0, 8).join('\n'))
  return m ? m[1].split('.').map(Number) : null
}

export function versionGreater(a, b) {
  if (!a || !b) return false
  for (let i = 0; i < Math.max(a.length, b.length); i++) {
    const x = a[i] || 0, y = b[i] || 0
    if (x !== y) return x > y
  }
  return false
}

// K numbers of decision rows (same rule as the checklist's awk command: "| K-123 |" rows).
export function decisionRows(text) {
  const ks = []
  for (const line of text.split('\n')) {
    const m = /^\| ?K-(\d+) ?\|/.exec(line)
    if (m) ks.push(Number(m[1]))
  }
  return ks
}

// New K numbers continue main's last number without gaps or duplicates.
export function decisionNumbering(baseText, newText) {
  const base = decisionRows(baseText), now = decisionRows(newText)
  const count = new Map()
  for (const k of now) count.set(k, (count.get(k) || 0) + 1)
  const dups = [...count].filter(([, n]) => n > 1).map(([k]) => `K-${k}`)
  const baseSet = new Set(base)
  const added = [...new Set(now.filter(k => !baseSet.has(k)))].sort((a, b) => a - b)
  const removed = [...baseSet].filter(k => !count.has(k)).map(k => `K-${k}`)
  const max = base.length ? Math.max(...base) : 0
  const gaps = added.filter((k, i) => k !== max + 1 + i).map(k => `K-${k}`)
  return { dups, added: added.map(k => `K-${k}`), removed, gaps, ok: !dups.length && !removed.length && !gaps.length }
}

// --- memory block --------------------------------------------------------------

// Replaces the body of "## Güncel Durum" (after its leading "> ..." note) up to the "---"
// that closes the section. Everything else in MEMORY.md stays byte-identical.
export function replaceCurrentStatus(memory, newBody) {
  const lines = memory.split('\n')
  const h = lines.findIndex(l => l.trim() === '## Güncel Durum')
  if (h < 0) throw new Error('MEMORY.md: "## Güncel Durum" başlığı yok')
  let i = h + 1
  while (i < lines.length && (lines[i].trim() === '' || lines[i].startsWith('>'))) i++
  let end = i
  while (end < lines.length && lines[end].trim() !== '---' && !lines[end].startsWith('## ')) end++
  if (end >= lines.length) throw new Error('MEMORY.md: Güncel Durum bölümünün sonu ("---") bulunamadı')
  // The section's own "> ..." note stays; a copy of it at the top of the new body is dropped.
  const bodyLines = newBody.replace(/\r/g, '').split('\n')
  while (bodyLines.length && (bodyLines[0].trim() === '' || bodyLines[0].startsWith('>'))) bodyLines.shift()
  const body = bodyLines.join('\n').replace(/\s+$/, '')
  if (!body) throw new Error('Güncel Durum gövdesi boş')
  if (/^## /m.test(body) || /^---$/m.test(body)) throw new Error('Güncel Durum gövdesi başlık ya da "---" taşıyamaz')
  return [...lines.slice(0, i), ...body.split('\n'), '', ...lines.slice(end)].join('\n')
}

export function appendArchive(archive, addition) {
  const add = addition.replace(/\r/g, '').replace(/^\s+/, '').replace(/\s+$/, '')
  if (!add) return archive
  return archive.replace(/\s*$/, '') + '\n' + add + '\n'
}

// --- text integrity ------------------------------------------------------------

export function addedLines(baseText, newText) {
  const pool = new Map()
  for (const l of (baseText || '').split('\n')) pool.set(l, (pool.get(l) || 0) + 1)
  const res = []
  for (const l of newText.split('\n')) {
    const n = pool.get(l) || 0
    if (n > 0) pool.set(l, n - 1)
    else res.push(l)
  }
  return res
}

export const hasCR = (text) => text.includes('\r')

// A regex replacement that left "$1" in the text (INSTRUCTIONS §7, second scan). Script commands
// written into documents (awk's $1) live in code spans or fenced blocks and are not counted.
export function backrefLeftovers(newText, added) {
  const want = new Set(added)
  const hits = []
  let fenced = false
  for (const line of newText.split('\n')) {
    if (/^\s*(>\s*)*```/.test(line)) { fenced = !fenced; continue }
    if (fenced || !want.has(line)) continue
    if (/\$[0-9]/.test(line.replace(/`[^`]*`/g, ''))) hits.push(line.trim().slice(0, 160))
  }
  return hits
}

const ATTRIBUTION = /Co-Authored-By:[^\n]*(Claude|Anthropic|noreply@anthropic\.com)|Generated with \[?Claude|claude\.(ai|com)\/(code|claude-code)|\(Added\/Updated via AI\)|🤖/i
export const attributionHit = (text) => ATTRIBUTION.exec(text || '')?.[0] || null

// Archive marks and stage openings belong to the owner-in-the-loop steps (MANUAL_ONLY).
export function stageBoundary(trackerAdded, statusBody, baseStatusBody, stage) {
  const hits = []
  for (const l of trackerAdded) if (/^>\s*\*\*Aşama \d+ kapandı/.test(l)) hits.push('karar kaydına arşiv işareti eklenmiş')
  const opened = (t) => [...(t || '').matchAll(/AŞAMA (\d+) AÇILDI/g)].map(m => Number(m[1]))
  const before = Math.max(0, ...opened(baseStatusBody)), after = Math.max(0, ...opened(statusBody))
  if (after > before || after > stage) hits.push(`hafızada yeni aşama açılmış (Aşama ${after})`)
  return hits
}

export function prTitleProblems(title, stage) {
  const p = []
  if (!new RegExp(`^docs: asama ${stage} `).test(title || '')) p.push(`başlık "docs: asama ${stage} " ile başlamıyor`)
  if ((title || '').length > 300) p.push('başlık 300 karakterden uzun')
  return p
}

// --- cross-review --------------------------------------------------------------

const norm = (l) => l.replace(/^[\s>]*/, '').replace(/\s+$/, '')

// Raw second-model output must appear in the report unedited (trailing spaces aside).
export function missingRawLines(report, raw) {
  const have = new Set(report.split('\n').map(norm))
  return raw.split('\n').map(norm).filter(l => l && !/^```/.test(l) && !have.has(l))
}

export function crossReviewVerdict(outputs) {
  const clean = outputs.length > 0 && outputs.every(o => /SONUÇ:\s*TEMİZ/.test(o) && !/BULGU-\d+/.test(o))
  return clean ? 'TEMIZ' : 'BULGULU'
}

// --- session evidence ------------------------------------------------------------

export function classifyDenials(denials) {
  const forbidden = [], benign = []
  for (const d of denials || []) {
    const input = d.tool_input || {}
    const text = [input.command, input.file_path, input.path, input.url, input.pattern].filter(Boolean).join(' ')
    const item = `${d.tool_name}: ${text.slice(0, 200)}`
    if (FORBIDDEN_TOOLS.includes(d.tool_name) || FORBIDDEN_ATTEMPT.test(text) || touchesHomeClaude(text)) forbidden.push(item)
    else benign.push(item)
  }
  return { forbidden, benign }
}

// entries: [{ file, agentType, models: Set|string[] }]; main: models of the main transcript
export function modelViolations(mainModels, entries) {
  const bad = []
  const real = (ms) => [...ms].filter(m => m && m !== '<synthetic>')
  for (const m of real(mainModels)) if (m !== OPUS) bad.push(`ana oturum ${m} ile çalıştı`)
  for (const e of entries) {
    for (const m of real(e.models)) {
      if (m === OPUS) continue
      const allowed = CHEAP_AGENTS[e.agentType]
      if (!allowed || !allowed.test(m)) bad.push(`${e.agentType || 'alt ajan'} (${e.description || e.file}) ${m} ile çalıştı`)
    }
  }
  return bad
}

const strings = (v) => typeof v === 'string' ? [v] : v && typeof v === 'object' ? Object.values(v).flatMap(strings) : []

// Tool inputs that reach for other sessions' transcripts or the owner's auto-memory (home .claude).
// The repo's own .claude/ (rules, skills, repo memory) is fine; so are Claude's own tool-results.
export function isolationHits(toolUses, ownSessionId) {
  const hits = []
  for (const u of toolUses) {
    if (FORBIDDEN_TOOLS.includes(u.name)) { hits.push(`${u.name} kullanıldı`); continue }
    for (const raw of strings(u.input)) {
      const s = raw.replace(/\\+/g, '/').replace(/\.autopilot\/+ledger\/+codex\.jsonl/g, '')
      const own = ownSessionId && s.includes(`${ownSessionId}/`) && s.includes('/tool-results/')
      if ((touchesHomeClaude(s) && !own) || /\.jsonl\b/.test(s)) { hits.push(`${u.name}: ${raw.slice(0, 200)}`); break }
    }
  }
  return hits
}
