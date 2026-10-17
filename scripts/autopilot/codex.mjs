#!/usr/bin/env node
// Fixed-flag wrapper for the cross-review second model (SETUP: backup method, Codex CLI).
// Step sessions may call the second model only through this file; the flags are not theirs to set.
//
//   node scripts/autopilot/codex.mjs --prompt <file> --doc <file> --template <file>
//        [--chunk-note <file>] --out .autopilot/work/codex/<name>.md [--label <text>]
//
// Input = prompt [+ chunk note] + "=== DOKÜMAN ===" + doc + "=== ŞABLON ===" + template, on stdin,
// in an empty temporary folder, read-only, without personal config. Every call is appended to
// .autopilot/ledger/codex.jsonl with SHA-256 of each input and of the output; the driver checks
// the report against the ledger.
import { spawn, spawnSync } from 'node:child_process'
import crypto from 'node:crypto'
import fs from 'node:fs'
import os from 'node:os'
import path from 'node:path'

const MODEL = 'gpt-5.6-terra'
const EFFORT = 'high'
const TIMEOUT_MS = 30 * 60 * 1000
// cross-review skill, Phase 1 item 4 — carried unchanged in every round.
const DELIBERATE_DECISION = 'Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.'
const TRACKER_ROW = /^\| ?K-\d+ ?\| ?\d{4}-\d{2}-\d{2} ?\|/m

const sha = (buf) => crypto.createHash('sha256').update(buf).digest('hex')
const squash = (s) => s.replace(/\s+/g, ' ').trim()
function fail(msg) { console.error(`codex.mjs: ${msg}`); process.exit(2) }

function parseArgs(argv) {
  const a = {}
  for (let i = 0; i < argv.length; i++) {
    const k = argv[i]
    if (k === '--help' || k === '-h') { a.help = true; continue }
    if (!k.startsWith('--')) fail(`bilinmeyen argüman: ${k}`)
    a[k.slice(2)] = argv[++i]
  }
  return a
}

function codexEntry() {
  if (process.env.CODEX_JS && fs.existsSync(process.env.CODEX_JS)) return process.env.CODEX_JS
  const root = spawnSync('npm', ['root', '-g'], { encoding: 'utf8', shell: process.platform === 'win32', windowsHide: true }).stdout?.trim()
  const js = root && path.join(root, '@openai', 'codex', 'bin', 'codex.js')
  if (!js || !fs.existsSync(js)) fail('Codex CLI bulunamadı (npm i -g @openai/codex ya da CODEX_JS)')
  return js
}

const args = parseArgs(process.argv.slice(2))
if (args.help) {
  console.log(fs.readFileSync(new URL(import.meta.url), 'utf8').split('\n').slice(1, 12).map(l => l.replace(/^\/\/ ?/, '')).join('\n'))
  process.exit(0)
}
for (const k of ['prompt', 'doc', 'template', 'out']) if (!args[k]) fail(`--${k} gerekli`)

const repo = process.cwd()
const inRepo = (p) => { const r = path.relative(repo, path.resolve(p)); return r && !r.startsWith('..') && !path.isAbsolute(r) }
const outRel = path.relative(repo, path.resolve(args.out)).split(path.sep).join('/')
if (!outRel.startsWith('.autopilot/work/codex/')) fail('--out .autopilot/work/codex/ altında olmalı')

const files = { prompt: args.prompt, doc: args.doc, template: args.template }
if (args['chunk-note']) files.chunkNote = args['chunk-note']
const text = {}
for (const [k, f] of Object.entries(files)) {
  if (!inRepo(f) || !fs.existsSync(f)) fail(`${k} dosyası repo içinde değil ya da yok: ${f}`)
  if (path.resolve(f) === path.resolve('Docs/PRODUCT_DISCOVERY_STATUS.md')) fail('karar kaydı ikinci modele verilmez (K-431)')
  text[k] = fs.readFileSync(f, 'utf8')
  if (TRACKER_ROW.test(text[k])) fail(`${k} karar kaydı satırı taşıyor — girdi yalnız doküman ve şablondur (K-431)`)
}
if (!squash(text.prompt).includes(squash(DELIBERATE_DECISION))) fail('istem bilinçli karar cümlesini değiştirilmeden taşımıyor (cross-review Faz 1 madde 4)')

const input = [text.prompt.trimEnd(), text.chunkNote?.trimEnd(), '=== DOKÜMAN ===', text.doc.trimEnd(), '=== ŞABLON ===', text.template.trimEnd(), '']
  .filter(x => x !== undefined).join('\n\n')
const work = fs.mkdtempSync(path.join(os.tmpdir(), 'codex-cr-'))
const lastFile = path.join(work, 'last.md')
const t0 = Date.now()
const child = spawn(process.execPath, [codexEntry(), 'exec', '-s', 'read-only', '-C', work, '--skip-git-repo-check', '--ephemeral',
  '--ignore-user-config', '--color', 'never', '-m', MODEL, '-c', `model_reasoning_effort="${EFFORT}"`, '-o', lastFile, '-'],
{ cwd: work, stdio: ['pipe', 'pipe', 'pipe'], windowsHide: true })
let log = ''
child.stdout.on('data', d => { log += d })
child.stderr.on('data', d => { log += d })
const timer = setTimeout(() => child.kill(), TIMEOUT_MS)
child.stdin.end(input)
child.on('close', (code) => {
  clearTimeout(timer)
  const output = fs.existsSync(lastFile) ? fs.readFileSync(lastFile, 'utf8') : ''
  fs.mkdirSync(path.dirname(path.resolve(args.out)), { recursive: true })
  fs.writeFileSync(args.out, output)
  const entry = {
    t: new Date().toISOString(), label: args.label || path.basename(args.out), out: outRel, exit: code,
    seconds: Math.round((Date.now() - t0) / 1000), model: MODEL, effort: EFFORT, bytes: Buffer.byteLength(input),
    sha256: { input: sha(input), output: sha(output), ...Object.fromEntries(Object.entries(text).map(([k, v]) => [k, sha(v)])) },
  }
  fs.mkdirSync(path.join(repo, '.autopilot', 'ledger'), { recursive: true })
  fs.appendFileSync(path.join(repo, '.autopilot', 'ledger', 'codex.jsonl'), JSON.stringify(entry) + '\n')
  fs.rmSync(work, { recursive: true, force: true })
  if (code !== 0 || !output.trim()) { console.error(log.slice(-3000)); fail(`Codex hata verdi (çıkış ${code})`) }
  console.log(JSON.stringify({ out: outRel, seconds: entry.seconds, sha256: entry.sha256.output }))
})
