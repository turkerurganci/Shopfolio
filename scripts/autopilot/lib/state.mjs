// Run state and event log under <main checkout>/.autopilot/ (gitignored).
//   current.json            -> { runId }
//   runs/<runId>/plan.json   the plan as given
//   runs/<runId>/state.json  what the monitor and `status` read
//   runs/<runId>/events.jsonl one line per event, append-only
//   runs/<runId>/steps/<n>-<id>/  prompt, rules, session stream, result
import fs from 'node:fs'
import path from 'node:path'
import { nowIso, readIf, writeFile } from './util.mjs'

export const LADDER = [
  ['preflight', 'Ön kontrol', 'temiz dal · hook’lar · kilit testi · ana dal CI · açık PR'],
  ['session', 'Kilitli Claude oturumu', 'izin listesi · GitHub anahtarı yok'],
  ['outcome', 'Oturum DUR dedi mi?', 'karar gerekiyor mu, bir şeye takıldı mı'],
  ['denials', 'Yasak deneme var mı?', 'izin redleri ve oturum kayıtları, alt ajanlar dahil'],
  ['models', 'Alt ajan modelleri', 'bulgu bulan iş Opus’ta mı'],
  ['fence', 'Dokunulan dosyalar izinli mi?', 'kurallar, hook’lar, CI, betik, 00 yasak'],
  ['reports', 'Beklenen rapor yerinde mi?', 'adımın kendi raporu'],
  ['records', 'Kayıtlar düzgün mü?', 'karar kaydı sürümü · K numaraları · doküman sürümleri · hafıza'],
  ['text', 'Metin temiz mi?', 'CR · $1 artığı · AI atıf satırı · aşama sınırı'],
  ['commit', 'Commit ve PR', 'commit hook’ları (sır taraması) çalışır'],
  ['ci', 'GitHub testleri', 'check sayısı > 0 · hepsi bitti · CI Gate yeşil'],
  ['merge', 'Merge', 'merge yetkisi · yalnız kontrol edilen sürüm'],
  ['postmerge', 'Merge sonrası', 'ana dalda CI ve Publish'],
]

export class RunStore {
  constructor(controlDir) {
    this.controlDir = controlDir
    this.runId = null
    this.state = null
  }

  get runDir() { return path.join(this.controlDir, 'runs', this.runId) }
  stepDir(i) { const s = this.state.steps[i]; return path.join(this.runDir, 'steps', `${String(i + 1).padStart(2, '0')}-${s.id}${s.round > 1 ? '-r' + s.round : ''}`) }

  create(plan, steps) {
    this.runId = nowIso().replace(/[-:]/g, '').replace(/\..+/, '').replace('T', '-')
    this.state = {
      runId: this.runId, title: plan.title, stage: plan.stage, stageName: plan.stageName, document: plan.document,
      mergeAuthority: plan.mergeAuthority || null, startedAt: nowIso(), updatedAt: nowIso(),
      status: 'running', stop: null, question: null, stepIndex: 0, steps, prs: [], warnings: [], criticalQuestions: [],
      benignDenials: [], pid: process.pid, claudeVersion: null,
    }
    writeFile(path.join(this.runDir, 'plan.json'), JSON.stringify(plan, null, 2))
    this.save()
    writeFile(path.join(this.controlDir, 'current.json'), JSON.stringify({ runId: this.runId }))
    return this
  }

  load() {
    const cur = readIf(path.join(this.controlDir, 'current.json'))
    if (!cur) return null
    this.runId = JSON.parse(cur).runId
    const st = readIf(path.join(this.runDir, 'state.json'))
    if (!st) return null
    this.state = JSON.parse(st)
    return this
  }

  save() {
    this.state.updatedAt = nowIso()
    const file = path.join(this.runDir, 'state.json')
    writeFile(file + '.tmp', JSON.stringify(this.state, null, 2))
    fs.renameSync(file + '.tmp', file)
  }

  // who: betik | claude | yonetici | sen ; kind: info | good | bad
  event(who, text, kind = 'info') {
    const line = JSON.stringify({ t: nowIso(), who, kind, text: String(text).slice(0, 2000) })
    fs.mkdirSync(this.runDir, { recursive: true })
    fs.appendFileSync(path.join(this.runDir, 'events.jsonl'), line + '\n')
    console.log(`${new Date().toTimeString().slice(0, 8)} [${who}] ${text}`)
  }

  step() { return this.state.steps[this.state.stepIndex] }

  setCheck(key, status, note = '') {
    const s = this.step()
    if (!s) return
    const c = s.checks.find(x => x.key === key)
    if (c) { c.status = status; c.note = note }
    this.save()
  }
}

export function newStepEntry(planStep, round = 1) {
  return {
    id: planStep.id, type: planStep.type, label: planStep.label, round, document: planStep.document,
    task: planStep.task, note: planStep.note, reports: planStep.reports,
    status: '', phase: 'preflight', branch: null, baseSha: null, sessionId: null, pr: null, prUrl: null,
    headSha: null, mergeSha: null, summary: '', startedAt: null, endedAt: null, costUsd: 0,
    checks: LADDER.map(([key, label, hint]) => ({ key, label, hint, status: '', note: '' })),
  }
}
