// One step = one fresh headless Claude session. This module starts it with the fixed flags,
// streams its activity to the event log, and reads its transcripts afterwards for evidence.
import { spawn, spawnSync } from 'node:child_process'
import fs from 'node:fs'
import os from 'node:os'
import path from 'node:path'
import { ALLOW, DENY } from './perm.mjs'
import { claudeProjectDir, cleanEnv } from './util.mjs'

export const OUTPUT_SCHEMA = {
  type: 'object',
  properties: {
    outcome: { type: 'string', enum: ['CONTINUE', 'STOP'], description: 'CONTINUE: adım bitti. STOP: karar ya da proje sahibi gerekiyor.' },
    summary: { type: 'string', description: 'Ne yapıldı — sade dille birkaç cümle' },
    prTitle: { type: 'string', description: 'PR başlığı: "docs: asama N ..." — önceki PR başlıklarının biçiminde, ASCII' },
    prBody: { type: 'string', description: 'PR açıklaması — önceki PR açıklamalarının biçiminde' },
    commitBody: { type: 'string', description: 'Commit mesajının gövdesi (başlık hariç)' },
    unshownWarnings: {
      type: 'array', description: '⚠ öneriyle kaydedilip proje sahibine gösterilmemiş kararlar',
      items: { type: 'object', properties: { k: { type: 'string' }, sentence: { type: 'string' } }, required: ['k', 'sentence'] },
    },
    criticalQuestions: { type: 'array', items: { type: 'string' }, description: 'Adımı durdurmadan kaydedilen çok kritik sorular (INSTRUCTIONS §9 işletim kuralı 2)' },
    question: { type: 'string', description: 'outcome=STOP ise proje sahibine sorulacak soru: sade dil, numaralı seçenekler, önerin ilk sırada' },
    roundResult: { type: 'string', enum: ['TEMIZ', 'BULGULU', 'YOK'], description: 'Cross-review adımında ikinci modelin bu turdaki sonucu; diğer adımlarda YOK' },
    impactDone: { type: 'boolean', description: 'Cross-review adımında etki yansıtma (Faz 5) bu adımda yapıldı mı' },
  },
  required: ['outcome', 'summary'],
}

// The session gets no git or GitHub credentials: a push or an API call fails server-side,
// whatever command shape the model tries.
export function sessionEnv(controlDir) {
  const ghEmpty = path.join(controlDir, 'gh-empty')
  fs.mkdirSync(ghEmpty, { recursive: true })
  const env = cleanEnv({
    CLAUDE_CODE_DISABLE_AUTO_MEMORY: '1',
    BASH_DEFAULT_TIMEOUT_MS: '600000',
    BASH_MAX_TIMEOUT_MS: '1800000',
    GIT_TERMINAL_PROMPT: '0',
    GCM_INTERACTIVE: 'never',
    GIT_CONFIG_COUNT: '1', GIT_CONFIG_KEY_0: 'credential.helper', GIT_CONFIG_VALUE_0: '',
    GH_CONFIG_DIR: ghEmpty,
  })
  // A placeholder value: gh prefers it over the keyring and the API answers "Bad credentials".
  env.GH_TOKEN = '<none>'
  env.GITHUB_TOKEN = '<none>'
  return env
}

export function sessionArgs({ model, effort, maxTurns, maxBudgetUsd, rulesFile, sessionId, resumeId }) {
  const args = ['-p']
  if (resumeId) args.push('--resume', resumeId)
  else args.push('--session-id', sessionId)
  args.push(
    '--model', model, '--effort', effort,
    '--output-format', 'stream-json', '--verbose', '--json-schema', JSON.stringify(OUTPUT_SCHEMA),
    '--setting-sources', 'project',
    '--settings', JSON.stringify({ attribution: { commit: '', pr: '', sessionUrl: false } }),
    '--permission-mode', 'dontAsk',
    '--max-turns', String(maxTurns), '--max-budget-usd', String(maxBudgetUsd),
    '--append-system-prompt-file', rulesFile,
    '--allowedTools', ...ALLOW, '--disallowedTools', ...DENY,
  )
  return args
}

function killTree(child) {
  if (process.platform === 'win32') spawnSync('taskkill', ['/pid', String(child.pid), '/T', '/F'], { windowsHide: true })
  else child.kill('SIGKILL')
}

const short = (input) => {
  const v = input?.command || input?.file_path || input?.pattern || input?.description || input?.url || input?.query || input?.skill || ''
  return String(v).replace(/\s+/g, ' ').slice(0, 160)
}

// Resolves with { code, result, timedOut, stderr }. `result` is the stream's final result event
// (subtype, structured_output, permission_denials, total_cost_usd, session_id, ...).
export function runSession({ cwd, args, prompt, env, timeoutMs, streamFile, onActivity }) {
  return new Promise((resolve) => {
    const child = spawn('claude', args, { cwd, env, windowsHide: true, stdio: ['pipe', 'pipe', 'pipe'] })
    const out = fs.createWriteStream(streamFile, { flags: 'a' })
    let buf = '', stderr = '', result = null, timedOut = false
    const timer = setTimeout(() => { timedOut = true; killTree(child) }, timeoutMs)
    child.stdout.on('data', (d) => {
      out.write(d)
      buf += d.toString('utf8')
      let nl
      while ((nl = buf.indexOf('\n')) >= 0) {
        const line = buf.slice(0, nl); buf = buf.slice(nl + 1)
        let ev; try { ev = JSON.parse(line) } catch { continue }
        if (ev.type === 'result') result = ev
        else if (ev.type === 'assistant') {
          const sub = ev.parent_tool_use_id ? 'alt ajan' : 'claude'
          for (const c of ev.message?.content || []) {
            if (c.type === 'tool_use' && c.name !== 'StructuredOutput') onActivity?.(sub, `${c.name}: ${short(c.input)}`, 'info')
          }
        } else if (ev.type === 'system' && ev.subtype === 'permission_denied') {
          onActivity?.('betik', `izin reddi: ${JSON.stringify(ev).slice(0, 240)}`, 'bad')
        }
      }
    })
    child.stderr.on('data', (d) => { stderr += d.toString('utf8') })
    child.stdin.end(prompt)
    child.on('close', (code) => {
      clearTimeout(timer)
      out.end()
      resolve({ code, result, timedOut, stderr: stderr.slice(-4000) })
    })
    child.on('error', (e) => { clearTimeout(timer); out.end(); resolve({ code: -1, result: null, timedOut, stderr: String(e) }) })
  })
}

function readJsonl(file) {
  const res = []
  let text = ''
  try { text = fs.readFileSync(file, 'utf8') } catch { return res }
  for (const l of text.split('\n')) { try { res.push(JSON.parse(l)) } catch { /* partial line */ } }
  return res
}

function collect(lines, models, toolUses) {
  for (const x of lines) {
    if (x.message?.model) models.add(x.message.model)
    const c = x.message?.content
    if (Array.isArray(c)) for (const u of c) if (u.type === 'tool_use') toolUses.push({ name: u.name, input: u.input })
  }
}

// Transcripts of the step session and its subagents, as Claude Code wrote them.
export function readTranscripts(cwd, sessionIds) {
  const dir = claudeProjectDir(os.homedir(), cwd)
  const mainModels = new Set(), toolUses = [], subagents = []
  for (const sid of [...new Set(sessionIds.filter(Boolean))]) {
    collect(readJsonl(path.join(dir, `${sid}.jsonl`)), mainModels, toolUses)
    const subDir = path.join(dir, sid, 'subagents')
    let files = []
    try { files = fs.readdirSync(subDir) } catch { /* no subagents */ }
    for (const f of files.filter(f => f.endsWith('.jsonl'))) {
      let meta = {}
      try { meta = JSON.parse(fs.readFileSync(path.join(subDir, f.replace(/\.jsonl$/, '.meta.json')), 'utf8')) } catch { /* older CLI */ }
      const models = new Set()
      collect(readJsonl(path.join(subDir, f)), models, toolUses)
      subagents.push({ file: f, agentType: meta.agentType, description: meta.description, model: meta.model, effort: meta.effort, models })
    }
  }
  return { dir, mainModels, toolUses, subagents }
}
