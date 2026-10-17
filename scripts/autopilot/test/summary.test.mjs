// node --test scripts/autopilot/test/
import test from 'node:test'
import assert from 'node:assert/strict'
import * as S from '../lib/summary.mjs'

const MIN = 60000
const at = (min) => new Date(Date.UTC(2026, 9, 17, 10, 0) + min * MIN).toISOString()

test('durations are formatted for the owner', () => {
  assert.equal(S.duration(0), '—')
  assert.equal(S.duration(20000), '<1 dk')
  assert.equal(S.duration(42 * MIN), '42 dk')
  assert.equal(S.duration(125 * MIN), '2 sa 05 dk')
  assert.match(S.localTime(at(0)), /^\d\d:\d\d$/)
  assert.match(S.localTime(at(0), true), /^2026-10-17 \d\d:\d\d$/)
})

test('time is split into Claude, tests, owner waits and other stops; totals add up', () => {
  const steps = [
    { label: 'Audit ve deep review', type: 'audit-deep-review', round: 1, startedAt: at(0), endedAt: at(100), sessionMs: 80 * MIN, ciMs: 5 * MIN, waitAnswerMs: 10 * MIN, waitOtherMs: 0, mergedAt: at(98) },
    { label: 'Cross-review', type: 'cross-review', round: 2, startedAt: at(100), endedAt: at(140), sessionMs: 20 * MIN, ciMs: 4 * MIN, waitAnswerMs: 0, waitOtherMs: 12 * MIN, mergedAt: at(138) },
    { label: 'Checkpoint', type: 'checkpoint', round: 1, startedAt: at(140), endedAt: null, sessionMs: 3 * MIN },
    { label: 'Henüz başlamadı', type: 'custom', round: 1, startedAt: null },
  ]
  const { rows, sum } = S.durationRows(steps)
  assert.equal(rows.length, 3)
  assert.equal(rows[1].name, 'Cross-review · 2. tur')
  assert.deepEqual(sum, { session: 103 * MIN, ci: 9 * MIN, answer: 10 * MIN, other: 12 * MIN, total: 140 * MIN })
  const table = S.durationsTable(steps, 30 * MIN).join('\n')
  assert.match(table, /\| Audit ve deep review \| 1 sa 20 dk \| 5 dk \| 10 dk \| — \| 1 sa 40 dk \| \d\d:\d\d \|/)
  assert.match(table, /\| Checkpoint \| 3 dk \| — \| — \| — \| sürüyor \| — \|/)
  assert.match(table, /\| \*\*Toplam\*\* \| 1 sa 43 dk \| 9 dk \| 10 dk \| 12 dk \| 2 sa 20 dk \| — \|/)
  assert.match(table, /Adımlar arasında karar bekleme .*: 30 dk/)
  assert.deepEqual(S.durationsTable([]), [])
})

test('the summary carries status, steps, the durations table and the ⚠ list', () => {
  const st = {
    title: 'Veri Modeli kalite döngüsü', status: 'limit', startedAt: at(0), updatedAt: at(140),
    stop: { message: 'Cross-review 10 turda TEMİZ dönmedi', detail: 'Devam: resume --more-rounds <N>' }, question: null,
    steps: [{ label: 'Cross-review', type: 'cross-review', round: 10, status: 'ok', verdict: 'BULGULU', pr: 140, decisions: ['K-1'], startedAt: at(0), endedAt: at(30), sessionMs: 20 * MIN }],
    warnings: [{ k: 'K-1', sentence: 'deneme', step: 'Cross-review' }], criticalQuestions: [], benignDenials: [],
  }
  const text = S.summaryText(st)
  assert.match(text, /\*\*Durum:\*\* Durdu — bir sınır doldu/)
  assert.match(text, /- ✓ Cross-review · 10\. tur \(BULGULU\) — PR #140 — 1 karar/)
  assert.match(text, /## Süreler/)
  assert.match(text, /- K-1 — deneme \(Cross-review\)/)
})
