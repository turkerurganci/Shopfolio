// node --test scripts/autopilot/test/
import test from 'node:test'
import assert from 'node:assert/strict'
import os from 'node:os'
import path from 'node:path'
import * as C from '../lib/checks.mjs'

test('parseStatusZ reads modified, untracked and renamed paths', () => {
  const out = ' M Docs/06_DATA_MODEL.md\0?? Docs/AUDIT_REPORTS/06_AUDIT.md\0R  Docs/new.md\0Docs/old.md\0'
  assert.deepEqual(C.parseStatusZ(out).map(c => c.path), ['Docs/06_DATA_MODEL.md', 'Docs/AUDIT_REPORTS/06_AUDIT.md', 'Docs/new.md'])
})

test('fence allows Docs, blocks protected paths, deletions and new top-level documents', () => {
  const ok = [{ code: ' M', path: 'Docs/06_DATA_MODEL.md' }, { code: '??', path: 'Docs/AUDIT_REPORTS/06_AUDIT.md' }]
  assert.deepEqual(C.fenceViolations(ok), [])
  const bad = [
    { code: ' M', path: '.claude/INSTRUCTIONS.md' }, { code: ' M', path: 'Docs/00_PROJECT_METHODOLOGY.md' },
    { code: ' M', path: 'scripts/git-hooks/pre-push' }, { code: ' M', path: '.github/workflows/ci.yml' },
    { code: ' M', path: 'CLAUDE.md' }, { code: ' M', path: 'Docs/BYPASS_LOG.md' }, { code: ' D', path: 'Docs/02_PRODUCT_REQUIREMENTS.md' },
    { code: '??', path: 'Docs/13_NEW.md' }, { code: ' M', path: '.claude/memory/MEMORY.md' },
  ]
  assert.equal(C.fenceViolations(bad).length, bad.length)
  assert.deepEqual(C.fenceViolations([{ code: 'M ', path: '.claude/memory/MEMORY.md' }], { allowDriverOwned: true }), [])
})

test('report names expand per document, stage and round', () => {
  assert.equal(C.expandReport('Docs/CROSS_REVIEW_REPORTS/{doc}_CROSS_REVIEW{round}.md', { doc: '06', stage: 5, round: 1 }), 'Docs/CROSS_REVIEW_REPORTS/06_CROSS_REVIEW.md')
  assert.equal(C.expandReport('Docs/CROSS_REVIEW_REPORTS/{doc}_CROSS_REVIEW{round}.md', { doc: '06', stage: 5, round: 3 }), 'Docs/CROSS_REVIEW_REPORTS/06_CROSS_REVIEW_R3.md')
  assert.equal(C.expandReport('Docs/CHECKPOINT_REPORTS/CP{stage2}_PHASE{stage}_CHECKPOINT.md', { doc: '06', stage: 5, round: 1 }), 'Docs/CHECKPOINT_REPORTS/CP05_PHASE5_CHECKPOINT.md')
  assert.deepEqual(C.missingReports(['Docs/A.md', 'Docs/B.md'], [{ code: '??', path: 'Docs/A.md' }]), ['Docs/B.md'])
})

test('document versions compare numerically', () => {
  const v = C.docVersion('# X\n\n**Versiyon: v0.125** | **Son güncelleme:** 2026-10-17\n')
  assert.deepEqual(v, [0, 125])
  assert.ok(C.versionGreater([0, 126], [0, 125]))
  assert.ok(C.versionGreater([0, 10], [0, 9]))
  assert.ok(!C.versionGreater([0, 125], [0, 125]))
  assert.equal(C.docVersion('no header'), null)
})

test('decision numbers continue without gaps or duplicates', () => {
  const base = '| K-1 | a |\n| K-2 | b |\n'
  assert.ok(C.decisionNumbering(base, base + '| K-3 | c |\n| K-4 | d |\n').ok)
  assert.deepEqual(C.decisionNumbering(base, base + '| K-3 | c |\n| K-3 | d |\n').dups, ['K-3'])
  assert.deepEqual(C.decisionNumbering(base, base + '| K-5 | c |\n').gaps, ['K-5'])
  assert.deepEqual(C.decisionNumbering(base, '| K-1 | a |\n').removed, ['K-2'])
})

const MEM = `# Proje Hafızası

## Güncel Durum

> Kısa tut. Tarihsel detay arşive taşınır.

- **Son adım — eski**
- **SIRADA — eski**

---

## Task Changelog

| Task |
`

test('memory: only the Güncel Durum body is replaced', () => {
  const out = C.replaceCurrentStatus(MEM, '> Kısa tut. Tarihsel detay arşive taşınır.\n\n- **Son adım — yeni**\n- **SIRADA — yeni**\n')
  assert.ok(out.includes('- **Son adım — yeni**') && !out.includes('eski'))
  assert.equal(out.split('> Kısa tut').length, 2)
  assert.ok(out.endsWith('## Task Changelog\n\n| Task |\n'))
  assert.throws(() => C.replaceCurrentStatus(MEM, '## Başka başlık\n'))
  assert.throws(() => C.replaceCurrentStatus(MEM, '   \n'))
  assert.equal(C.appendArchive('a\n\n', '\n- eski adım\n'), 'a\n- eski adım\n')
})

test('text scans: CR, $1 leftovers, AI attribution, stage boundary, PR title', () => {
  assert.deepEqual(C.addedLines('a\nb\n', 'a\nb\nc\n'), ['c'])
  assert.ok(C.hasCR('x\r\n'))
  const doc = 'ok\nbozuk $1 kaldı\nkomut `awk "{print $1}"`\n```\nawk \'{print $2}\'\n```\n'
  assert.deepEqual(C.backrefLeftovers(doc, doc.split('\n')), ['bozuk $1 kaldı'])
  assert.ok(C.attributionHit('x\n\nCo-Authored-By: Claude Opus <noreply@anthropic.com>'))
  assert.ok(C.attributionHit('🤖 Generated with [Claude Code](https://claude.com/claude-code)'))
  assert.equal(C.attributionHit('Co-authored-by: Türker urgancı <turker@example.com>'), null)
  assert.deepEqual(C.stageBoundary(['> **Aşama 5 kapandı (2026-10-20, ...'], '- **AŞAMA 5 AÇILDI', '- **AŞAMA 5 AÇILDI', 5).length, 1)
  assert.deepEqual(C.stageBoundary([], '- **AŞAMA 6 AÇILDI', '- **AŞAMA 5 AÇILDI', 5).length, 1)
  assert.deepEqual(C.stageBoundary([], '- **AŞAMA 5 AÇILDI', '- **AŞAMA 5 AÇILDI', 5), [])
  assert.deepEqual(C.prTitleProblems('docs: asama 5 kalite dongusu -- 06 audit; K-1040 (#0)', 5), [])
  assert.equal(C.prTitleProblems('docs: aşama 5 kalite döngüsü', 5).length, 1)
  assert.deepEqual(C.prTitleProblems('docs: asama 4 yazim turu 2 -- 05 §6, §10', 4), [])
})

test('cross-review: raw output must be embedded, verdict comes from the outputs', () => {
  const raw = 'BULGU-1\nKriter: Tutarlılık  \nYer: §2\n'
  const report = '## 2. Ham bulgular\n\n> ```text\n> BULGU-1\n> Kriter: Tutarlılık\n> Yer: §2\n> ```\n'
  assert.deepEqual(C.missingRawLines(report, raw), [])
  assert.deepEqual(C.missingRawLines(report.replace('§2', '§3'), raw), ['Yer: §2'])
  assert.equal(C.crossReviewVerdict(['SONUÇ: TEMİZ', 'SONUÇ: TEMİZ\n']), 'TEMIZ')
  assert.equal(C.crossReviewVerdict(['SONUÇ: TEMİZ', 'BULGU-1 ...']), 'BULGULU')
  assert.equal(C.crossReviewVerdict([]), 'BULGULU')
})

test('denials: forbidden attempts are told apart from harmless command shapes', () => {
  const d = (command) => ({ tool_name: 'Bash', tool_input: { command } })
  const r = C.classifyDenials([d('git push origin HEAD'), d('gh pr merge 5'), d('cd Docs && grep x y'), d('git commit -m x'),
    { tool_name: 'Read', tool_input: { file_path: 'C:/Users/u/.claude/projects/x/abc.jsonl' } }, { tool_name: 'ListAgents', tool_input: {} }])
  assert.equal(r.forbidden.length, 5)
  assert.equal(r.benign.length, 1)
})

test('subagent models: findings stay on Opus, cheap models only for the two agent types', () => {
  const ok = C.modelViolations(new Set(['claude-opus-5-5', '<synthetic>']), [
    { agentType: 'general-purpose', models: new Set(['claude-opus-5-5']) },
    { agentType: 'mekanik', models: new Set(['claude-haiku-5-5']) },
    { agentType: 'yazici', models: new Set(['claude-sonnet-5-5']) },
  ])
  assert.deepEqual(ok, [])
  const bad = C.modelViolations(new Set(['claude-opus-5-5']), [
    { agentType: 'general-purpose', description: 'mercek', models: new Set(['claude-haiku-5-5']) },
    { agentType: 'mekanik', models: new Set(['claude-sonnet-5-5']) },
  ])
  assert.equal(bad.length, 2)
  assert.equal(C.modelViolations(new Set(['claude-sonnet-5-5']), []).length, 1)
})

test('isolation: other transcripts and the owner memory are flagged, own tool-results and the ledger are not', () => {
  const sid = '1111-2222'
  const hits = C.isolationHits([
    { name: 'Read', input: { file_path: `C:\\Users\\u\\.claude\\projects\\C--x\\${sid}\\tool-results\\abc.txt` } },
    { name: 'Read', input: { file_path: '.autopilot/ledger/codex.jsonl' } },
    { name: 'Bash', input: { command: 'grep -r kod ~/.claude/projects/other' } },
    { name: 'Read', input: { file_path: 'C:/Users/u/.claude/projects/C--x/memory/notes.md' } },
    { name: 'SendMessage', input: { to: 'x' } },
    { name: 'Read', input: { file_path: 'D:\\work\\repo-otonom\\.claude\\memory\\MEMORY.md' } },
    { name: 'Bash', input: { command: 'git show HEAD -- .claude/memory/MEMORY_ARCHIVE.md' } },
    { name: 'Read', input: { file_path: path.join(os.homedir(), '.claude', 'settings.json') } },
  ], sid)
  assert.equal(hits.length, 4)
  assert.equal(C.classifyDenials([{ tool_name: 'Edit', tool_input: { file_path: 'D:/work/repo/.claude/memory/MEMORY.md' } }]).forbidden.length, 0)
})
