#!/usr/bin/env node
// Autopilot driver: runs a plan of document-era steps, one fresh locked Claude session per step,
// and decides CONTINUE and MERGE itself from mechanical evidence. The model can only stop.
// Turkish usage guide: scripts/autopilot/README.md
//
//   node scripts/autopilot/run.mjs start --plan <plan.json> [--detach]
//   node scripts/autopilot/run.mjs answer "<owner's answer>" [--detach]
//   node scripts/autopilot/run.mjs resume [--detach]
//   node scripts/autopilot/run.mjs stop | status | wait [--until stop|step] | monitor [--port N]
//   node scripts/autopilot/run.mjs reset-step   (discards the step's work — owner's approval only)
import crypto from 'node:crypto'
import fs from 'node:fs'
import http from 'node:http'
import path from 'node:path'
import { spawn } from 'node:child_process'
import { fileURLToPath } from 'node:url'
import {
  STEP_DEFAULTS, STEP_TYPES, MANUAL_ONLY, MAX_CROSS_REVIEW_ROUNDS, FENCE, TRACKER, MEMORY, MEMORY_ARCHIVE,
  LEDGER, MEMORY_STAGING, WORK_DIR, MONITOR_PORT,
} from './lib/config.mjs'
import { RunStore, newStepEntry } from './lib/state.mjs'
import { Stop, run, must, git, gitOut, readIf, writeFile, sleep, showFile } from './lib/util.mjs'
import { sessionArgs, sessionEnv, runSession, readTranscripts } from './lib/session.mjs'
import { rulesText, taskText, answerText, docNo } from './lib/prompts.mjs'
import { GitHub } from './lib/github.mjs'
import * as C from './lib/checks.mjs'

const SCRIPT_DIR = path.dirname(fileURLToPath(import.meta.url))

// --- locations -----------------------------------------------------------------

function mainRoot() {
  const common = must0(git(process.cwd(), 'rev-parse', '--path-format=absolute', '--git-common-dir'), 'Bu komut proje reposunun içinde çalıştırılmalı')
  return path.dirname(path.resolve(common))
}
function must0(r, msg) { if (r.code !== 0) { console.error(msg); process.exit(2) } return r.out }

const ROOT = mainRoot()
const CONTROL = path.join(ROOT, '.autopilot')
const STOP_FLAG = path.join(CONTROL, 'stop-requested')

// --- plan ------------------------------------------------------------------------

function loadPlan(file) {
  let plan
  try { plan = JSON.parse(fs.readFileSync(file, 'utf8')) } catch (e) { throw new Stop('plan', `Plan okunamadı: ${file}`, String(e)) }
  const errs = []
  for (const k of ['title', 'stage', 'stageName', 'document', 'steps']) if (plan[k] === undefined) errs.push(`"${k}" eksik`)
  if (!Number.isInteger(plan.stage)) errs.push('"stage" tam sayı olmalı')
  if (!Array.isArray(plan.steps) || !plan.steps.length) errs.push('"steps" boş')
  const ids = new Set()
  for (const s of plan.steps || []) {
    if (!/^[a-z0-9][a-z0-9-]*$/.test(s.id || '')) errs.push(`adım kimliği küçük harf, rakam ve "-" olmalı: ${s.id}`)
    if (ids.has(s.id)) errs.push(`aynı adım kimliği iki kez: ${s.id}`)
    ids.add(s.id)
    if (MANUAL_ONLY[s.type]) errs.push(`"${s.type}" betikle koşmaz — ${MANUAL_ONLY[s.type]}`)
    else if (!STEP_TYPES[s.type]) errs.push(`bilinmeyen adım türü: ${s.type}`)
    else if (STEP_TYPES[s.type].needsTask && !s.task) errs.push(`"${s.id}" adımının görev metni ("task") yok`)
    if (s.type === 'custom' && !s.label) errs.push(`"${s.id}" serbest adımının adı ("label") yok`)
    if (s.model !== undefined || s.effort !== undefined) errs.push(`"${s.id}": model ve düşünme seviyesi plandan değiştirilmez (lib/config.mjs)`)
  }
  if (errs.length) throw new Stop('plan', 'Plan geçersiz', errs.join('\n'))
  return plan
}

// The stage's merge authority must be a decision row on main that says so (INSTRUCTIONS §9).
function verifyMergeAuthority(plan) {
  if (!plan.mergeAuthority) return
  const tracker = gitOut(ROOT, 'show', `origin/main:${TRACKER}`)
  const row = tracker.split('\n').find(l => l.startsWith(`| ${plan.mergeAuthority} |`))
  if (!row) throw new Stop('plan', `Merge yetkisi kararı ${plan.mergeAuthority} karar kaydında yok`)
  if (!/merge yetkisi/i.test(row) || !row.includes(`Aşama ${plan.stage}`)) {
    throw new Stop('plan', `${plan.mergeAuthority} Aşama ${plan.stage} için merge yetkisi vermiyor`, row.slice(0, 600))
  }
}

function expandSteps(plan) {
  return plan.steps.map(s => newStepEntry({ ...s, label: s.label || STEP_TYPES[s.type].label, document: s.document || plan.document }))
}

// --- step context ------------------------------------------------------------------

function expectedReports(step, plan) {
  if (step.type === 'custom') return step.reports || []
  return STEP_TYPES[step.type].reports.map(r => C.expandReport(r, { doc: docNo(step.document), stage: plan.stage, round: step.round }))
}

function branchName(step, plan) {
  return `docs/asama${plan.stage}-${step.id}${step.type === 'cross-review' ? `-r${step.round}` : ''}`
}

// --- driver ------------------------------------------------------------------------

class Driver {
  constructor(store, plan) {
    this.store = store
    this.plan = plan
    this.gh = new GitHub(ROOT)
    this.wt = plan.worktree ? path.resolve(ROOT, plan.worktree) : `${ROOT}-otonom`
  }

  get st() { return this.store.state }
  ev(who, text, kind) { this.store.event(who, text, kind) }
  check(key, status, note) { this.store.setCheck(key, status, note) }

  async drive() {
    try {
      while (this.st.stepIndex < this.st.steps.length) {
        await this.runStep()
        this.st.stepIndex++
        this.store.save()
      }
      this.st.status = 'done'
      this.ev('betik', 'Plan bitti — bütün adımlar projeye eklendi', 'good')
    } catch (e) {
      if (!(e instanceof Stop)) { e = new Stop('check', `Beklenmeyen hata: ${e.message}`, e.stack) }
      const s = this.store.step()
      if (s && s.status === 'run') s.status = 'stop'
      this.st.status = { question: 'question', 'owner-merge': 'waiting', paused: 'paused', ci: 'ci', limit: 'limit' }[e.kind] || 'stopped'
      this.st.stop = { kind: e.kind, message: e.message, detail: e.detail || '', at: new Date().toISOString() }
      this.ev('betik', `DURDU — ${e.message}${e.detail ? `: ${String(e.detail).slice(0, 400)}` : ''}`, 'bad')
    } finally {
      this.st.pid = null
      this.store.save()
      writeFile(path.join(this.store.runDir, 'ozet.md'), summaryText(this.st))
      try { fs.rmSync(STOP_FLAG, { force: true }) } catch { /* ignore */ }
    }
    return this.st.status
  }

  pauseIfRequested() {
    if (fs.existsSync(STOP_FLAG)) throw new Stop('paused', 'Durdurma isteği geldi — adım sınırında durdu')
  }

  async runStep() {
    const s = this.store.step()
    if (s.phase === 'done') return
    s.status = 'run'
    s.startedAt ||= new Date().toISOString()
    this.st.status = 'running'
    this.st.stop = null
    this.store.save()
    this.ev('betik', `${s.label}${s.type === 'cross-review' ? ` · ${s.round}. tur` : ''} — ${s.phase === 'preflight' ? 'başladı' : `${s.phase} aşamasından sürüyor`}`)
    const phases = ['preflight', 'session', 'checks', 'commit', 'ci', 'merge', 'postmerge']
    // A stop inside the session phase without a pending answer starts the step over; the
    // worktree must be clean again first (reset-step, with the owner's approval).
    if (s.phase === 'session' && !this.st.question?.answer) s.phase = 'preflight'
    for (let i = phases.indexOf(s.phase); i < phases.length; i++) {
      s.phase = phases[i]
      this.store.save()
      await this[s.phase](s)
      this.pauseIfRequested()
    }
    s.phase = 'done'
    s.status = 'ok'
    s.endedAt = new Date().toISOString()
    this.store.save()
    this.ev('betik', `${s.label} bitti${s.pr ? ` — PR #${s.pr} projeye eklendi` : ''}`, 'good')
    if (s.type === 'cross-review') this.nextRound(s)
  }

  nextRound(s) {
    if (s.verdict === 'TEMIZ') { this.ev('betik', `Cross-review ${s.round}. turda TEMİZ döndü; etki yansıtma aynı adımda yapıldı`, 'good'); return }
    const planStep = this.plan.steps.find(p => p.id === s.id)
    const max = planStep?.maxRounds || MAX_CROSS_REVIEW_ROUNDS
    if (s.round >= max) throw new Stop('limit', `Cross-review ${max} turda TEMİZ dönmedi`, 'Döngü yakınsamıyor; ciddiyet ölçüsü ya da devam kararı proje sahibinin (cross-review skill\'i, "Ciddiyet ölçüsü").')
    const next = newStepEntry({ ...planStep, label: s.label, document: s.document }, s.round + 1)
    this.st.steps.splice(this.st.stepIndex + 1, 0, next)
    this.store.save()
  }

  // --- phases ---------------------------------------------------------------------

  async preflight(s) {
    this.check('preflight', 'run')
    const lock = JSON.parse(readIf(path.join(CONTROL, 'kilit-testi.json')) || '{}')
    const version = run('claude', ['--version']).out
    this.st.claudeVersion = version
    if (!lock.ok || lock.version !== version) {
      throw new Stop('check', 'Kilit testi bu Claude sürümüyle geçmedi', `Kurulu: ${version}; son geçen test: ${lock.version || 'yok'}. Çalıştır: node scripts/autopilot/kilit-testi.mjs`)
    }
    must(git(ROOT, 'fetch', '-q', 'origin'), 'git fetch')
    const broken = await this.gh.mainProblems((n) => this.check('preflight', 'run', `ana dalda ${n} koşu bitmeyi bekliyor`))
    if (broken.length) throw new Stop('ci', 'Ana dalın son CI koşularından biri kırmızı (GUARDRAILS §8)', broken.join(', '))
    const foreign = this.gh.openPRs().filter(p => p.headRefName.startsWith('docs/') && !this.st.prs.some(x => x.number === p.number))
    if (foreign.length) throw new Stop('check', 'Açık bir docs: PR var — K numarası çakışabilir (INSTRUCTIONS §7)', foreign.map(p => `#${p.number} ${p.title}`).join('\n'))
    this.ensureWorktree()
    // install.sh may write the path relative (scripts/git-hooks) or absolute (<main checkout>/scripts/git-hooks).
    const hooks = git(this.wt, 'config', 'core.hooksPath').out
    const same = (a, b) => process.platform === 'win32' ? path.resolve(a).toLowerCase() === path.resolve(b).toLowerCase() : path.resolve(a) === path.resolve(b)
    if (!hooks || ![this.wt, ROOT].some(r => same(path.resolve(this.wt, hooks), path.join(r, 'scripts', 'git-hooks')))) {
      throw new Stop('check', 'Git hook’ları kurulu değil (core.hooksPath)', `değer: ${hooks || '(yok)'}`)
    }
    s.branch = branchName(s, this.plan)
    if (this.gh.remoteBranchExists(s.branch)) throw new Stop('check', `GitHub'da ${s.branch} dalı zaten var`, 'Önceki bir denemeden kalmış olabilir; bakıp karar ver.')
    s.baseSha = gitOut(ROOT, 'rev-parse', 'origin/main')
    must(git(this.wt, 'checkout', '-q', '-B', s.branch, s.baseSha), 'git checkout')
    for (const d of [WORK_DIR, path.dirname(LEDGER)]) fs.rmSync(path.join(this.wt, d), { recursive: true, force: true })
    fs.mkdirSync(path.join(this.wt, WORK_DIR, 'memory'), { recursive: true })
    this.check('preflight', 'ok', s.baseSha.slice(0, 7))
  }

  ensureWorktree() {
    if (!fs.existsSync(this.wt)) {
      must(git(ROOT, 'worktree', 'add', '-q', '--detach', this.wt, 'origin/main'), 'git worktree add')
      this.ev('betik', `Adım oturumlarının çalışma klasörü açıldı: ${this.wt}`)
    }
    const common = path.resolve(gitOut(this.wt, 'rev-parse', '--path-format=absolute', '--git-common-dir'))
    if (path.dirname(common) !== ROOT) throw new Stop('check', `${this.wt} bu reponun çalışma klasörü değil`)
    const dirty = gitOut(this.wt, 'status', '--porcelain', '-uall')
    if (dirty) throw new Stop('check', 'Adım klasörü temiz değil — önceki adımdan kalan değişiklik var', dirty.slice(0, 1500))
  }

  async session(s) {
    const dir = this.store.stepDir(this.st.stepIndex)
    fs.mkdirSync(dir, { recursive: true })
    const opts = { ...STEP_DEFAULTS, ...pick(this.plan.steps.find(p => p.id === s.id), ['maxTurns', 'maxBudgetUsd', 'timeoutMin']) }
    const rulesFile = path.join(dir, 'rules.md')
    let prompt, resumeId = null
    if (this.st.question?.answer && this.st.question.step === this.st.stepIndex) {
      resumeId = s.sessionId
      prompt = answerText(this.st.question.answer)
      this.ev('sen', `Cevap: ${this.st.question.answer}`)
      this.st.question = null
    } else {
      writeFile(rulesFile, rulesText({
        mergeAuthority: this.plan.mergeAuthority, stage: this.plan.stage, stageName: this.plan.stageName,
        label: s.type === 'cross-review' ? `${s.label}, ${s.round}. tur` : s.label, branch: s.branch,
      }))
      prompt = taskText(s, this.plan)
      s.sessionId = crypto.randomUUID()
      s.sessionIds = [s.sessionId]
    }
    writeFile(path.join(dir, resumeId ? `answer-${Date.now()}.md` : 'prompt.md'), prompt)
    this.store.save()
    this.check('session', 'run', `${opts.model.replace('claude-', '')} · ${opts.effort}`)
    this.ev('betik', `Kilitli Claude oturumu açıldı (${opts.model}, düşünme ${opts.effort})`)
    const res = await runSession({
      cwd: this.wt, env: sessionEnv(CONTROL), prompt, streamFile: path.join(dir, 'stream.jsonl'),
      timeoutMs: opts.timeoutMin * 60000,
      args: sessionArgs({ ...opts, rulesFile, sessionId: s.sessionId, resumeId }),
      onActivity: (who, text, kind) => this.ev(who, text, kind),
    })
    const r = res.result
    writeFile(path.join(dir, `result-${Date.now()}.json`), JSON.stringify({ code: res.code, timedOut: res.timedOut, stderr: res.stderr, result: r }, null, 2))
    s.sessionIds ||= [s.sessionId]
    if (r?.session_id && !s.sessionIds.includes(r.session_id)) s.sessionIds.push(r.session_id)
    s.costUsd = (s.costUsd || 0) + (r?.total_cost_usd || 0)
    this.store.save()
    if (res.timedOut) throw new Stop('limit', `Oturum ${opts.timeoutMin} dakikada bitmedi`)
    if (!r) throw new Stop('check', 'Oturum sonuç vermeden kapandı', res.stderr)
    if (r.subtype === 'error_max_turns') throw new Stop('limit', `Oturum tur sınırına (${opts.maxTurns}) takıldı`)
    if (r.subtype === 'error_max_budget_usd') throw new Stop('limit', `Oturum harcama sınırına (${opts.maxBudgetUsd} $) takıldı`)
    const apiText = `${r.api_error_status || ''} ${r.subtype || ''} ${r.is_error ? r.result || '' : ''}`
    if (/\b(429|529)\b|overloaded|rate.?limit|usage limit/i.test(apiText)) throw new Stop('limit', 'Opus şu an kullanılamıyor (kullanım limiti ya da yoğunluk) — daha zayıf modele geçilmez', apiText.trim().slice(0, 400))
    if (r.subtype !== 'success' || r.is_error || !r.structured_output) throw new Stop('check', `Oturum hatayla bitti (${r.subtype})`, r.result || res.stderr)
    this.check('session', 'ok', `${Math.round((r.duration_ms || 0) / 60000)} dk`)
    s.out = r.structured_output

    // Evidence before the model's own word: denials, transcripts, subagent models.
    this.check('denials', 'run')
    const den = C.classifyDenials(r.permission_denials)
    const tr = readTranscripts(this.wt, s.sessionIds)
    const iso = C.isolationHits(tr.toolUses, s.sessionId)
    if (den.forbidden.length || iso.length) {
      this.check('denials', 'stop', 'yasak deneme')
      throw new Stop('check', 'Oturum yasak bir işlem denedi', [...den.forbidden, ...iso].join('\n'))
    }
    if (den.benign.length) {
      this.st.benignDenials.push(...den.benign.map(d => `${s.label}: ${d}`))
      this.ev('betik', `${den.benign.length} zararsız izin reddi (listede olmayan komut biçimi) — özete yazıldı`, 'info')
    }
    this.check('denials', 'ok', den.benign.length ? `${den.benign.length} zararsız red` : 'yok')
    this.check('models', 'run')
    const bad = C.modelViolations(tr.mainModels, tr.subagents)
    if (bad.length) { this.check('models', 'stop', 'Opus dışı'); throw new Stop('check', 'Bir iş izin verilmeyen modelle çalıştı', bad.join('\n')) }
    const byType = {}
    for (const a of tr.subagents) for (const m of a.models) if (m !== '<synthetic>') byType[`${a.agentType || 'alt ajan'}/${m.replace('claude-', '')}`] = (byType[`${a.agentType || 'alt ajan'}/${m.replace('claude-', '')}`] || 0) + 1
    this.check('models', 'ok', Object.keys(byType).length ? Object.entries(byType).map(([k, n]) => `${n}×${k}`).join(', ') : 'alt ajan yok')

    this.check('outcome', 'run')
    if (s.out.outcome === 'STOP') {
      this.check('outcome', 'stop', 'DUR + soru')
      this.st.question = { step: this.st.stepIndex, text: s.out.question || s.out.summary, summary: s.out.summary, at: new Date().toISOString() }
      this.ev('claude', `Karar gerekiyor: ${s.out.question || s.out.summary}`, 'bad')
      throw new Stop('question', 'Adım oturumu karar için durdu', s.out.question || s.out.summary)
    }
    this.check('outcome', 'ok', 'DEVAM')
    this.ev('claude', s.out.summary)
  }

  async checks(s) {
    const wt = this.wt
    const head = gitOut(wt, 'rev-parse', 'HEAD'), branch = gitOut(wt, 'branch', '--show-current')
    if (head !== s.baseSha || branch !== s.branch) throw new Stop('check', 'Oturum dalı ya da commit\'i değiştirmiş', `dal ${branch}, HEAD ${head.slice(0, 7)}`)
    const changes = C.parseStatusZ(gitOut(wt, 'status', '--porcelain=v1', '-z', '-uall'))
    const show = (p) => showFile(wt, s.baseSha, p)
    const file = (p) => readIf(path.join(wt, p))

    this.check('fence', 'run')
    const fence = C.fenceViolations(changes)
    if (!changes.length) { this.check('fence', 'stop', 'değişiklik yok'); throw new Stop('check', 'Oturum hiçbir dosyayı değiştirmedi') }
    if (fence.length) { this.check('fence', 'stop', fence[0]); throw new Stop('check', 'İzin verilmeyen bir dosya değişti', fence.join('\n')) }
    this.check('fence', 'ok', `${changes.length} dosya`)

    this.check('reports', 'run')
    const reports = expectedReports(s, this.plan)
    const missing = C.missingReports(reports, changes)
    if (missing.length) { this.check('reports', 'stop', missing[0]); throw new Stop('check', 'Beklenen rapor yazılmamış', missing.join('\n')) }
    if (s.type === 'cross-review') this.crossReviewEvidence(s, reports[0])
    this.check('reports', 'ok', reports.map(r => path.basename(r)).join(', ') || 'rapor beklenmiyor')

    this.check('records', 'run')
    const problems = []
    const baseTracker = show(TRACKER), newTracker = file(TRACKER)
    if (!changes.some(c => c.path === TRACKER)) problems.push('karar kaydı değişmemiş — sürüm başlığı her PR\'da artar (K-38)')
    else if (!C.versionGreater(C.docVersion(newTracker), C.docVersion(baseTracker))) problems.push('karar kaydının sürümü artmamış')
    const kn = C.decisionNumbering(baseTracker, newTracker)
    if (kn.dups.length) problems.push(`çift K numarası: ${kn.dups.join(', ')}`)
    if (kn.removed.length) problems.push(`silinmiş K satırı: ${kn.removed.join(', ')}`)
    if (kn.gaps.length) problems.push(`K numarası ana daldaki son numaradan boşluksuz devam etmiyor: ${kn.gaps.join(', ')}`)
    for (const c of changes.filter(c => /^Docs\/\d\d_[^/]+\.md$/.test(c.path))) {
      const before = C.docVersion(show(c.path) || '')
      if (C.versionGreater(C.docVersion(file(c.path) || ''), before)) continue
      // A document not written yet (template version v0.1) takes park lines without a version bump.
      if (before && !C.versionGreater(before, [0, 1])) this.ev('betik', `${c.path} henüz yazılmamış (v0.1); sürümü artmadan değişti — park satırı olabilir`)
      else problems.push(`${c.path} değişmiş ama sürümü artmamış`)
    }
    const statusBody = readIf(path.join(wt, MEMORY_STAGING.status))
    const archiveAdd = readIf(path.join(wt, MEMORY_STAGING.archive)) || ''
    if (!statusBody?.trim()) problems.push('hafızanın Güncel Durum gövdesi yazılmamış (.autopilot/work/memory/guncel-durum.md)')
    else {
      try { C.replaceCurrentStatus(show(MEMORY), statusBody) } catch (e) { problems.push(e.message) }
    }
    if (problems.length) { this.check('records', 'stop', problems[0]); throw new Stop('check', 'Kayıtlar eksik ya da tutarsız', problems.join('\n')) }
    s.decisions = kn.added
    this.check('records', 'ok', `${kn.added.length} yeni K · karar kaydı v${C.docVersion(newTracker).join('.')}`)

    this.check('text', 'run')
    const tp = []
    const trackerAdded = C.addedLines(baseTracker, newTracker)
    for (const c of changes.filter(c => /\.(md|txt|csv|json)$/i.test(c.path))) {
      const now = file(c.path) || ''
      if (C.hasCR(now)) tp.push(`${c.path}: CR (Windows satır sonu) var`)
      const added = C.addedLines(show(c.path) || '', now)
      for (const l of C.backrefLeftovers(now, added)) tp.push(`${c.path}: "$1" artığı olabilir → ${l}`)
      for (const l of added) { const h = C.attributionHit(l); if (h) tp.push(`${c.path}: AI atıf ifadesi → ${h}`) }
    }
    for (const t of [statusBody, archiveAdd]) if (C.hasCR(t || '')) tp.push('hafıza metninde CR var')
    const baseBody = /## Güncel Durum([\s\S]*?)\n---/.exec(show(MEMORY) || '')?.[1] || ''
    tp.push(...C.stageBoundary(trackerAdded, statusBody, baseBody, this.plan.stage).map(h => `${h} — arşiv işareti ve aşama açılışı proje sahibiyle yapılır`))
    tp.push(...C.prTitleProblems(s.out.prTitle, this.plan.stage))
    for (const [k, v] of Object.entries({ 'PR başlığı': s.out.prTitle, 'PR açıklaması': s.out.prBody, 'commit gövdesi': s.out.commitBody, hafıza: statusBody + archiveAdd })) {
      const h = C.attributionHit(v); if (h) tp.push(`${k}: AI atıf ifadesi → ${h}`)
    }
    if (!s.out.prBody?.trim()) tp.push('PR açıklaması boş')
    if (tp.length) { this.check('text', 'stop', tp[0].slice(0, 60)); throw new Stop('check', 'Metin denetimi tutmadı', tp.join('\n')) }
    this.check('text', 'ok', 'temiz')

    // ⚠ decisions recorded without being shown: the session's list plus what the record says.
    const listed = new Map((s.out.unshownWarnings || []).map(w => [w.k.trim(), w.sentence]))
    for (const l of trackerAdded) {
      const m = /^\| ?(K-\d+) ?\|/.exec(l)
      // only rows of this step's new decisions; older rows change when a back-reference is added
      if (m && s.decisions.includes(m[1]) && /öneriyle kaydedildi — ⚠/.test(l) && !listed.has(m[1])) listed.set(m[1], '(oturum listelemedi — karar kaydındaki satıra bak)')
    }
    for (const [k, sentence] of listed) this.st.warnings.push({ k, sentence, step: s.label })
    for (const q of s.out.criticalQuestions || []) this.st.criticalQuestions.push({ text: q, step: s.label })
    if (listed.size) this.ev('betik', `${listed.size} ⚠ karar sorulmadan kaydedildi — sana gösterilecek listeye eklendi`)
    for (const q of s.out.criticalQuestions || []) this.ev('claude', `Çok kritik soru kaydedildi, adım durmadı: ${q}`, 'bad')
    this.store.save()
  }

  crossReviewEvidence(s, reportPath) {
    const ledgerText = readIf(path.join(this.wt, LEDGER)) || ''
    const calls = ledgerText.split('\n').filter(Boolean).map(l => JSON.parse(l))
    const fail = (msg, d = '') => { this.check('reports', 'stop', msg.slice(0, 60)); throw new Stop('check', msg, d) }
    if (!calls.length) fail('İkinci model hiç çağrılmamış (codex.mjs kaydı boş)')
    const report = readIf(path.join(this.wt, reportPath)) || ''
    const outputs = []
    for (const c of calls) {
      if (c.exit !== 0) fail(`İkinci modelin bir çağrısı hatayla bitti: ${c.label}`)
      const out = readIf(path.join(this.wt, c.out))
      if (out === null || crypto.createHash('sha256').update(out).digest('hex') !== c.sha256.output) fail(`İkinci modelin çıktısı kayıttakiyle aynı değil: ${c.out}`)
      const miss = C.missingRawLines(report, out)
      if (miss.length) fail(`Ham çıktı rapora düzenlenmeden girmemiş: ${c.label}`, miss.slice(0, 10).join('\n'))
      outputs.push(out)
    }
    s.verdict = C.crossReviewVerdict(outputs)
    s.codexCalls = calls.length
    if (s.out.roundResult && s.out.roundResult !== s.verdict) fail(`Oturum turu ${s.out.roundResult} dedi, ikinci modelin çıktıları ${s.verdict}`)
    if (s.verdict === 'TEMIZ' && (!s.out.impactDone || !/etki yansıtma/i.test(report))) fail('Tur TEMİZ döndü ama etki yansıtma (Faz 5) yapılmamış')
    this.ev('betik', `İkinci model ${calls.length} çağrı; sonuç ${s.verdict === 'TEMIZ' ? 'SONUÇ: TEMİZ' : 'bulgu var'} (betik çıktılardan okudu)`, s.verdict === 'TEMIZ' ? 'good' : 'info')
  }

  async commit(s) {
    const wt = this.wt
    this.check('commit', 'run')
    if (!s.headSha) {
      const base = (p) => showFile(wt, s.baseSha, p) ?? ''
      writeFile(path.join(wt, MEMORY), C.replaceCurrentStatus(base(MEMORY), readIf(path.join(wt, MEMORY_STAGING.status))))
      const archiveAdd = readIf(path.join(wt, MEMORY_STAGING.archive)) || ''
      if (archiveAdd.trim()) writeFile(path.join(wt, MEMORY_ARCHIVE), C.appendArchive(base(MEMORY_ARCHIVE), archiveAdd))
      must(git(wt, 'add', '-A', '--', 'Docs', ...FENCE.driverOwned), 'git add')
      const staged = gitOut(wt, 'diff', '--cached', '--name-only').split('\n').filter(Boolean)
      const outside = staged.filter(p => !p.startsWith('Docs/') && !FENCE.driverOwned.includes(p))
      if (outside.length) throw new Stop('check', 'Commit\'e izinsiz dosya girdi', outside.join('\n'))
      const msgFile = path.join(this.store.stepDir(this.st.stepIndex), 'commit-msg.txt')
      writeFile(msgFile, `${s.out.prTitle.trim()}\n\n${(s.out.commitBody || '').trim()}\n`)
      const c = git(wt, 'commit', '-q', '-F', msgFile)
      if (c.code !== 0) { this.check('commit', 'stop', 'hook reddetti'); throw new Stop('check', 'Commit hook\'u reddetti (sır taraması ya da mesaj kuralı)', c.err || c.out) }
      s.headSha = gitOut(wt, 'rev-parse', 'HEAD')
      this.store.save()
    }
    if (!s.pr) {
      const p = git(wt, 'push', '-u', 'origin', s.branch)
      if (p.code !== 0) { this.check('commit', 'stop', 'push reddedildi'); throw new Stop('check', 'Push reddedildi (pre-push hook\'u ya da GitHub)', p.err || p.out) }
      const bodyFile = path.join(this.store.stepDir(this.st.stepIndex), 'pr-body.md')
      writeFile(bodyFile, s.out.prBody.trim() + '\n')
      const pr = this.gh.createPR(s.branch, s.out.prTitle.trim(), bodyFile)
      s.pr = pr.number; s.prUrl = pr.url
      this.st.prs.push({ number: pr.number, url: pr.url, title: s.out.prTitle.trim(), step: s.label })
      this.store.save()
      this.ev('betik', `PR #${pr.number} açıldı: ${pr.url}`)
    }
    this.check('commit', 'ok', `PR #${s.pr}`)
  }

  async ci(s) {
    this.check('ci', 'run', 'bekleniyor')
    const pr = this.gh.pr(s.pr)
    if (pr.state === 'MERGED') { this.check('ci', 'ok', 'zaten merge edilmiş'); return }
    if (pr.state !== 'OPEN' || pr.baseRefName !== 'main') throw new Stop('check', `PR #${s.pr} açık değil ya da base main değil`, `${pr.state} · ${pr.baseRefName}`)
    // Only the commit the driver checked is ever merged by the driver. A fix pushed by hand after a
    // stop is not re-checked here: the owner reviews and merges that PR, then `resume` continues.
    if (pr.headRefOid !== s.headSha) {
      this.check('ci', 'stop', 'uç elle değişti')
      throw new Stop('owner-merge', `PR #${s.pr}'ın ucu betiğin denetlediği commit değil — betik bu PR'ı merge etmez`, `${s.headSha.slice(0, 7)} → ${pr.headRefOid.slice(0, 7)}\n${s.prUrl}\nProje sahibi bakıp merge ettikten sonra: node scripts/autopilot/run.mjs resume`)
    }
    const r = await this.gh.waitChecks(s.headSha, (n, pending) => this.check('ci', 'run', `${n} check · ${pending} bekliyor`))
    if (!r.ok) { this.check('ci', 'stop', `CI Gate ${r.gate}`); throw new Stop('ci', `PR #${s.pr} testleri yeşil değil (CI Gate: ${r.gate})`, r.bad.join(', ') || `check sayısı ${r.count}`) }
    this.check('ci', 'ok', `${r.count} check · CI Gate ✓`)
    this.ev('betik', `PR #${s.pr}: ${r.count} check, CI Gate yeşil`, 'good')
  }

  async merge(s) {
    this.check('merge', 'run')
    const pr = this.gh.pr(s.pr)
    if (pr.state === 'MERGED') s.mergeSha = pr.mergeCommit.oid
    else if (!this.plan.mergeAuthority) {
      this.check('merge', 'stop', 'senin onayında')
      throw new Stop('owner-merge', `PR #${s.pr} hazır, testleri yeşil — merge proje sahibinde`, `${s.prUrl}\nMerge ettikten sonra: node scripts/autopilot/run.mjs resume`)
    } else {
      if (pr.headRefOid !== s.headSha) throw new Stop('check', 'PR denetlendikten sonra değişmiş', `${s.headSha.slice(0, 7)} → ${pr.headRefOid.slice(0, 7)}`)
      s.mergeSha = this.gh.merge(s.pr, s.headSha)
    }
    const subject = this.gh.commitSubject(s.mergeSha)
    if (!new RegExp(`\\(#${s.pr}\\)$`).test(subject)) throw new Stop('check', `Ana daldaki commit başlığında (#${s.pr}) yok — guard job kırmızı yanar`, subject)
    this.store.save()
    this.check('merge', 'ok', s.mergeSha.slice(0, 7))
    this.ev('betik', `PR #${s.pr} merge edildi (${s.mergeSha.slice(0, 7)}, ${this.plan.mergeAuthority ? `yetki ${this.plan.mergeAuthority}` : 'proje sahibi'})`, 'good')
  }

  async postmerge(s) {
    this.check('postmerge', 'run')
    const r = await this.gh.waitPostMerge(s.mergeSha, (n) => this.check('postmerge', 'run', `${n} iş akışı`))
    if (!r.ok) { this.check('postmerge', 'stop', r.bad[0]); throw new Stop('ci', 'Merge sonrası iş akışlarından biri yeşil değil', r.runs.join(', ')) }
    must(git(ROOT, 'fetch', '-q', 'origin'), 'git fetch')
    must(git(this.wt, 'checkout', '-q', '--detach', 'origin/main'), 'git checkout')
    this.check('postmerge', 'ok', r.runs.join(' · '))
  }
}

function pick(o, keys) { const r = {}; for (const k of keys) if (o?.[k] !== undefined) r[k] = o[k]; return r }

// --- summary ---------------------------------------------------------------------

const STATUS_TR = { running: 'Çalışıyor', question: 'Durdu — karar bekliyor', waiting: 'Durdu — merge senin onayında', paused: 'Durduruldu', ci: 'Durdu — testler yeşil değil', limit: 'Durdu — bir sınır doldu', stopped: 'Durdu — bir kontrol tutmadı', done: 'Bitti' }

export function summaryText(st) {
  const lines = [`# ${st.title}`, '', `**Durum:** ${STATUS_TR[st.status] || st.status} · başladı ${st.startedAt.slice(0, 16).replace('T', ' ')} · son ${st.updatedAt.slice(0, 16).replace('T', ' ')}`, '']
  if (st.stop) lines.push(`**Neden durdu:** ${st.stop.message}`, '', st.stop.detail ? '```\n' + st.stop.detail.slice(0, 3000) + '\n```\n' : '')
  if (st.status === 'question' && st.question) lines.push('**Proje sahibine soru:**', '', st.question.text, '')
  lines.push('## Adımlar', '')
  for (const s of st.steps) {
    const mark = { ok: '✓', stop: '✕', run: '…' }[s.status] || '·'
    lines.push(`- ${mark} ${s.label}${s.type === 'cross-review' ? ` · ${s.round}. tur${s.verdict ? ` (${s.verdict})` : ''}` : ''}${s.pr ? ` — PR #${s.pr}` : ''}${s.decisions?.length ? ` — ${s.decisions.length} karar` : ''}`)
  }
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

// --- commands ---------------------------------------------------------------------

function alive(pid) { if (!pid) return false; try { process.kill(pid, 0); return true } catch { return false } }

function detach(argv) {
  fs.mkdirSync(CONTROL, { recursive: true })
  const log = path.join(CONTROL, 'driver.log')
  const args = [fileURLToPath(import.meta.url), ...argv.filter(a => a !== '--detach')]
  if (process.platform === 'win32') {
    const q = (s) => `'${String(s).replace(/'/g, "''")}'`
    const ps = `Start-Process -WindowStyle Hidden -FilePath ${q(process.execPath)} -WorkingDirectory ${q(ROOT)} -ArgumentList @(${args.map(a => q(`"${String(a).replace(/"/g, '\\"')}"`)).join(',')}) -RedirectStandardOutput ${q(log)} -RedirectStandardError ${q(log + '.err')}`
    const r = run('powershell', ['-NoProfile', '-Command', ps])
    if (r.code !== 0) { console.error(r.err); process.exit(1) }
  } else {
    const fd = fs.openSync(log, 'a')
    spawn(process.execPath, args, { cwd: ROOT, detached: true, stdio: ['ignore', fd, fd] }).unref()
  }
  console.log(`Betik arka planda başladı. Günlük: ${log}`)
}

async function main() {
  const [cmd, ...rest] = process.argv.slice(2)
  const flag = (n) => rest.includes(n)
  const opt = (n) => { const i = rest.indexOf(n); return i >= 0 ? rest[i + 1] : undefined }
  const store = new RunStore(CONTROL)
  const exitFor = (status) => process.exit(status === 'done' ? 0 : status === 'running' ? 0 : 3)

  if (['start', 'resume', 'answer'].includes(cmd) && flag('--detach')) { detach(process.argv.slice(2)); return }

  switch (cmd) {
    case 'start': {
      const prev = store.load()
      if (prev && alive(prev.state.pid)) throw new Stop('plan', `Başka bir koşu çalışıyor (${prev.runId})`)
      if (!opt('--plan')) throw new Stop('plan', '--plan <dosya> gerekli')
      fs.mkdirSync(CONTROL, { recursive: true })
      must(git(ROOT, 'fetch', '-q', 'origin'), 'git fetch')
      const plan = loadPlan(opt('--plan'))
      if (git(ROOT, 'cat-file', '-e', `origin/main:${plan.document}`).code !== 0) throw new Stop('plan', `${plan.document} ana dalda yok`)
      verifyMergeAuthority(plan)
      store.create(plan, expandSteps(plan))
      store.event('yonetici', `Koşu başladı: ${plan.title} (${plan.steps.length} adım${plan.mergeAuthority ? `, merge yetkisi ${plan.mergeAuthority}` : ', merge proje sahibinde'})`)
      exitFor(await new Driver(store, plan).drive())
      break
    }
    case 'resume':
    case 'answer': {
      if (!store.load()) throw new Stop('plan', 'Sürdürülecek koşu yok')
      if (alive(store.state.pid)) throw new Stop('plan', 'Koşu zaten çalışıyor')
      const plan = JSON.parse(fs.readFileSync(path.join(store.runDir, 'plan.json'), 'utf8'))
      if (cmd === 'answer') {
        const text = rest.filter(a => a !== '--detach').join(' ').trim()
        if (store.state.status !== 'question' || !store.state.question) throw new Stop('plan', 'Bekleyen bir soru yok')
        if (!text) throw new Stop('plan', 'Cevap metni boş')
        store.state.question.answer = text
        store.step().phase = 'session'
      } else if (store.state.status === 'question') throw new Stop('plan', 'Koşu bir soruda bekliyor — "answer" ile cevap ver')
      else if (store.state.status === 'done') throw new Stop('plan', 'Koşu zaten bitti')
      store.state.pid = process.pid
      store.save()
      store.event('yonetici', cmd === 'answer' ? 'Cevap verildi; aynı oturum sürdürülüyor' : 'Koşu kaldığı yerden sürüyor')
      exitFor(await new Driver(store, plan).drive())
      break
    }
    case 'reset-step': {
      // Discards the current step's uncommitted work in the step worktree. Irreversible, so only
      // with the owner's explicit approval (GUARDRAILS §3). The step then starts over.
      if (!store.load()) throw new Stop('plan', 'Koşu yok')
      if (alive(store.state.pid)) throw new Stop('plan', 'Koşu çalışıyor; önce "stop"')
      const plan = JSON.parse(fs.readFileSync(path.join(store.runDir, 'plan.json'), 'utf8'))
      const s = store.step()
      if (!s || s.headSha) throw new Stop('plan', 'Bu adımın commit\'i var; adım sıfırlanamaz')
      const wt = plan.worktree ? path.resolve(ROOT, plan.worktree) : `${ROOT}-otonom`
      must(git(wt, 'checkout', '-q', '-f', '--detach', 'origin/main'), 'git checkout')
      must(git(wt, 'clean', '-fdq'), 'git clean')
      Object.assign(s, { phase: 'preflight', status: '', sessionId: null, sessionIds: [] })
      store.state.question = null
      store.save()
      store.event('yonetici', `${s.label}: adımın kaydedilmemiş değişiklikleri silindi (proje sahibinin onayıyla); adım baştan koşacak`, 'bad')
      console.log('Adım sıfırlandı. Sürdürmek için: node scripts/autopilot/run.mjs resume')
      break
    }
    case 'stop':
      fs.mkdirSync(CONTROL, { recursive: true })
      fs.writeFileSync(STOP_FLAG, new Date().toISOString())
      console.log('Durdurma isteği yazıldı; betik bu aşama bitince duracak.')
      break
    case 'status':
      if (!store.load()) { console.log('Koşu yok.'); break }
      console.log(summaryText(store.state))
      break
    case 'wait': {
      const until = opt('--until') || 'stop'
      if (!store.load()) { console.log('Koşu yok.'); break }
      const start = { i: store.state.stepIndex, n: store.state.steps.length }
      for (;;) {
        store.load()
        const st = store.state
        const running = st.status === 'running' && alive(st.pid)
        if (!running || (until === 'step' && (st.stepIndex !== start.i || st.steps.length !== start.n))) { console.log(summaryText(st)); break }
        await sleep(5000)
      }
      break
    }
    case 'monitor': monitor(Number(opt('--port')) || MONITOR_PORT); break
    default:
      console.log(fs.readFileSync(fileURLToPath(import.meta.url), 'utf8').split('\n').slice(1, 11).map(l => l.replace(/^\/\/ ?/, '')).join('\n'))
  }
}

function monitor(port) {
  const page = path.join(SCRIPT_DIR, 'izle.html')
  http.createServer((req, res) => {
    const url = new URL(req.url, 'http://x')
    const store = new RunStore(CONTROL)
    const send = (code, type, body) => { res.writeHead(code, { 'content-type': type, 'cache-control': 'no-store' }); res.end(body) }
    if (url.pathname === '/') return send(200, 'text/html; charset=utf-8', fs.readFileSync(page))
    if (!store.load()) return send(200, 'application/json', JSON.stringify({ state: null, events: [] }))
    if (url.pathname === '/api/state') {
      const from = Number(url.searchParams.get('from') || 0)
      const lines = (readIf(path.join(store.runDir, 'events.jsonl')) || '').split('\n').filter(Boolean)
      const st = { ...store.state, alive: alive(store.state.pid) }
      return send(200, 'application/json; charset=utf-8', JSON.stringify({ state: st, events: lines.slice(from).map(l => JSON.parse(l)), next: lines.length }))
    }
    send(404, 'text/plain', 'yok')
  }).listen(port, '127.0.0.1', () => console.log(`İzleme ekranı: http://127.0.0.1:${port}/`))
}

main().catch((e) => {
  if (e instanceof Stop) { console.error(`${e.message}${e.detail ? `\n${e.detail}` : ''}`); process.exit(2) }
  console.error(e); process.exit(1)
})
