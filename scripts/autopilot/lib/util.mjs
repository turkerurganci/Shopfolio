import { spawnSync } from 'node:child_process'
import fs from 'node:fs'
import path from 'node:path'

// A Stop ends the run at a defined point. `kind` decides how the owner sees it:
//   question — the step session needs a decision; check — a mechanical check failed;
//   ci — GitHub checks are not green; limit — a time/turn/budget limit; manual — a step the
//   driver never runs; plan — the plan is invalid; done — nothing left to do.
export class Stop extends Error {
  constructor(kind, message, detail = '') {
    super(message)
    this.kind = kind
    this.detail = detail
  }
}

export function run(cmd, args, opts = {}) {
  const r = spawnSync(cmd, args, { encoding: 'utf8', maxBuffer: 512 * 1024 * 1024, windowsHide: true, ...opts })
  return { code: r.status, out: (r.stdout || '').replace(/\s+$/, ''), err: (r.stderr || '').trim(), error: r.error }
}

export function must(r, what) {
  if (r.code !== 0) throw new Stop('check', `${what} başarısız oldu`, (r.err || r.out || String(r.error || '')).slice(0, 2000))
  return r.out
}

export const git = (cwd, ...args) => run('git', args, { cwd })
export const gitOut = (cwd, ...args) => must(git(cwd, ...args), `git ${args[0]}`)

export const sleep = (ms) => new Promise(res => setTimeout(res, ms))

export function readIf(file) {
  try { return fs.readFileSync(file, 'utf8') } catch { return null }
}

export function writeFile(file, text) {
  fs.mkdirSync(path.dirname(file), { recursive: true })
  fs.writeFileSync(file, text)
}

export const nowIso = () => new Date().toISOString()

// Directory Claude Code uses for a working directory's transcripts.
export function claudeProjectDir(home, cwd) {
  return path.join(home, '.claude', 'projects', path.resolve(cwd).replace(/[^A-Za-z0-9]/g, '-'))
}

// Child environment without the parent's Claude session variables.
export function cleanEnv(extra = {}) {
  const env = { ...process.env }
  for (const k of Object.keys(env)) if (/^(CLAUDE|CLAUDECODE|AI_AGENT)/.test(k)) delete env[k]
  return { ...env, ...extra }
}

// File content at a revision, byte-exact (run() trims trailing whitespace). null if absent.
export function showFile(cwd, rev, file) {
  const r = spawnSync('git', ['show', `${rev}:${file}`], { cwd, encoding: 'utf8', maxBuffer: 512 * 1024 * 1024, windowsHide: true })
  return r.status === 0 ? r.stdout : null
}
