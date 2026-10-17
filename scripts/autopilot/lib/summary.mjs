// Owner-facing run summary (ozet.md, `status`, `wait`). Pure: run state in, Markdown out.

export const STATUS_TR = {
  running: 'Çalışıyor', question: 'Durdu — karar bekliyor', waiting: 'Durdu — merge senin onayında', paused: 'Durduruldu',
  ci: 'Durdu — testler yeşil değil', limit: 'Durdu — bir sınır doldu', stopped: 'Durdu — bir kontrol tutmadı', done: 'Bitti',
}

const pad = (n) => String(n).padStart(2, '0')

// Local wall-clock time of this computer, e.g. "17:47" or "2026-10-17 17:47".
export function localTime(iso, withDate = false) {
  if (!iso) return '—'
  const d = new Date(iso)
  const t = `${pad(d.getHours())}:${pad(d.getMinutes())}`
  return withDate ? `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${t}` : t
}

export function duration(ms) {
  if (!ms || ms < 0) return '—'
  const m = Math.round(ms / 60000)
  if (m < 1) return '<1 dk'
  return m < 60 ? `${m} dk` : `${Math.floor(m / 60)} sa ${pad(m % 60)} dk`
}

export const stepName = (s) => `${s.label}${s.type === 'cross-review' ? ` · ${s.round}. tur` : ''}`

// Where each step's time went. Waiting for the owner (an answer or a merge) is kept apart from
// stops for a failed check, red CI or a limit, so estimates can be compared with what happened.
export function durationRows(steps) {
  const rows = []
  const sum = { session: 0, ci: 0, answer: 0, other: 0, total: 0 }
  for (const s of steps.filter(x => x.startedAt)) {
    const total = s.endedAt ? Date.parse(s.endedAt) - Date.parse(s.startedAt) : 0
    const r = { name: stepName(s), session: s.sessionMs || 0, ci: s.ciMs || 0, answer: s.waitAnswerMs || 0, other: s.waitOtherMs || 0, total, mergedAt: s.mergedAt || null, done: !!s.endedAt }
    for (const k of Object.keys(sum)) sum[k] += r[k]
    rows.push(r)
  }
  return { rows, sum }
}

export function durationsTable(steps, betweenStepsMs = 0) {
  const { rows, sum } = durationRows(steps)
  if (!rows.length) return []
  const between = betweenStepsMs ? ['', `Adımlar arasında karar bekleme (ör. cross-review tur sınırında): ${duration(betweenStepsMs)}`] : []
  const line = (r, name) => `| ${name} | ${duration(r.session)} | ${duration(r.ci)} | ${duration(r.answer)} | ${duration(r.other)} | ${r.done === false ? 'sürüyor' : duration(r.total)} | ${r.mergedAt ? localTime(r.mergedAt) : '—'} |`
  return [
    '## Süreler', '',
    '| Adım | Claude | Testler | Senin cevabın | Durma ve düzeltme | Toplam | Merge |',
    '|---|---|---|---|---|---|---|',
    ...rows.map(r => line(r, r.name)),
    line({ ...sum, done: true }, '**Toplam**'),
    ...between,
    '',
    '*Claude:* adım oturumunun çalıştığı süre. *Testler:* GitHub testlerini bekleme (PR ve merge sonrası). *Senin cevabın:* bir soruyu ya da merge onayını bekleyen süre. *Durma ve düzeltme:* bir kontrol, test ya da sınır yüzünden durup yeniden başlatılana kadar geçen süre. Toplam ile parçaların farkı commit, push ve ön kontrollerdir. Saatler bu bilgisayarın saatiyle.',
  ]
}

export function summaryText(st) {
  const lines = [`# ${st.title}`, '', `**Durum:** ${STATUS_TR[st.status] || st.status} · başladı ${localTime(st.startedAt, true)} · son ${localTime(st.updatedAt, true)}`, '']
  if (st.stop) lines.push(`**Neden durdu:** ${st.stop.message}`, '', st.stop.detail ? '```\n' + st.stop.detail.slice(0, 3000) + '\n```\n' : '')
  if (st.status === 'question' && st.question) lines.push('**Proje sahibine soru:**', '', st.question.text, '')
  lines.push('## Adımlar', '')
  for (const s of st.steps) {
    const mark = { ok: '✓', stop: '✕', run: '…' }[s.status] || '·'
    lines.push(`- ${mark} ${stepName(s)}${s.verdict ? ` (${s.verdict})` : ''}${s.pr ? ` — PR #${s.pr}` : ''}${s.decisions?.length ? ` — ${s.decisions.length} karar` : ''}`)
  }
  const table = durationsTable(st.steps, st.betweenStepsMs)
  if (table.length) lines.push('', ...table)
  if (st.warnings.length) {
    lines.push('', '## Sorulmadan kaydedilen ⚠ kararlar (sana gösterilecek)', '')
    for (const w of st.warnings) lines.push(`- ${w.k} — ${w.sentence} (${w.step})`)
  }
  if (st.criticalQuestions.length) {
    lines.push('', '## Adımı durdurmadan kaydedilen çok kritik sorular', '')
    for (const q of st.criticalQuestions) lines.push(`- ${q.text} (${q.step})`)
  }
  if (st.benignDenials.length) {
    lines.push('', '## Zararsız izin redleri', '')
    for (const d of st.benignDenials.slice(0, 30)) lines.push(`- ${d}`)
  }
  const cost = st.steps.reduce((a, s) => a + (s.costUsd || 0), 0)
  lines.push('', `Tahmini maliyet (API eşdeğeri): ${cost.toFixed(0)} $`)
  return lines.join('\n') + '\n'
}
