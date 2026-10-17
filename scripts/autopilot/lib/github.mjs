// GitHub side of a step, run with the owner's own gh/git credentials (never the session's).
import { CI } from './config.mjs'
import { Stop, run, must, sleep } from './util.mjs'

export class GitHub {
  constructor(cwd) {
    this.cwd = cwd
    this.slug = must(this.gh(['repo', 'view', '--json', 'nameWithOwner', '-q', '.nameWithOwner']), 'gh repo view')
  }

  gh(args) { return run('gh', args, { cwd: this.cwd }) }
  json(args, what) { const out = must(this.gh(args), what); return out ? JSON.parse(out) : null }
  api(pathname) { return this.json(['api', pathname], `gh api ${pathname}`) }

  openPRs() { return this.json(['pr', 'list', '--state', 'open', '--json', 'number,headRefName,title,url'], 'gh pr list') }

  // GUARDRAILS §8: no work on top of a broken main. Runs still in progress are waited for, not skipped.
  async mainProblems(onWait) {
    const deadline = Date.now() + CI.timeoutMin * 60000
    for (;;) {
      const runs = this.json(['run', 'list', '--branch', 'main', '--limit', '3', '--json', 'workflowName,status,conclusion,headSha'], 'gh run list')
      const pending = runs.filter(r => r.status !== 'completed')
      if (!pending.length) return runs.filter(r => !['success', 'skipped', 'neutral'].includes(r.conclusion)).map(r => `${r.workflowName}: ${r.conclusion} (${r.headSha.slice(0, 7)})`)
      if (Date.now() > deadline) return [`${pending.length} koşu ${CI.timeoutMin} dakikada bitmedi`]
      onWait?.(pending.length)
      await sleep(CI.pollSec * 1000)
    }
  }

  remoteBranchExists(branch) {
    return run('git', ['ls-remote', '--exit-code', '--heads', 'origin', branch], { cwd: this.cwd }).code === 0
  }

  createPR(branch, title, bodyFile) {
    const out = must(this.gh(['pr', 'create', '--base', 'main', '--head', branch, '--title', title, '--body-file', bodyFile]), 'gh pr create')
    const m = /\/pull\/(\d+)/.exec(out)
    if (!m) throw new Stop('check', 'PR numarası okunamadı', out)
    return { number: Number(m[1]), url: out.trim().split('\n').pop() }
  }

  pr(n) {
    return this.json(['pr', 'view', String(n), '--json', 'number,state,baseRefName,headRefName,headRefOid,files,body,title,commits,mergeCommit,url'], 'gh pr view')
  }

  // INSTRUCTIONS §3.2: silence is not green; zero checks is not green; wait for all of them.
  async waitChecks(sha, onTick) {
    const deadline = Date.now() + CI.timeoutMin * 60000
    while (Date.now() < deadline) {
      const runs = this.api(`repos/${this.slug}/commits/${sha}/check-runs?per_page=100`)?.check_runs || []
      const pending = runs.filter(r => r.status !== 'completed')
      onTick?.(runs.length, pending.length)
      if (runs.length && !pending.length) {
        const bad = runs.filter(r => ['failure', 'cancelled', 'timed_out', 'action_required', 'startup_failure', 'stale'].includes(r.conclusion))
        const gate = runs.find(r => r.name === CI.gate)
        return { ok: !bad.length && gate?.conclusion === 'success', count: runs.length, gate: gate?.conclusion || 'yok', bad: bad.map(r => `${r.name}=${r.conclusion}`) }
      }
      await sleep(CI.pollSec * 1000)
    }
    return { ok: false, count: 0, gate: 'zaman aşımı', bad: [`${CI.timeoutMin} dakikada bitmedi`] }
  }

  // Squash-merge only the commit the driver checked. No branch deletion: it is outside the
  // stage's merge authority (force-push, branch deletion and irreversible git operations).
  merge(n, headSha) {
    must(this.gh(['pr', 'merge', String(n), '--squash', '--match-head-commit', headSha]), 'gh pr merge')
    const p = this.pr(n)
    if (p.state !== 'MERGED' || !p.mergeCommit?.oid) throw new Stop('check', `PR #${n} merge edilmedi`, `durum: ${p.state}`)
    return p.mergeCommit.oid
  }

  commitSubject(sha) { return this.api(`repos/${this.slug}/commits/${sha}`).commit.message.split('\n')[0] }

  // Every workflow tied to the merge commit must finish green (00 §F.4).
  async waitPostMerge(sha, onTick) {
    const deadline = Date.now() + CI.timeoutMin * 60000
    let runs = []
    while (Date.now() < deadline) {
      runs = this.json(['run', 'list', '--commit', sha, '--json', 'workflowName,status,conclusion'], 'gh run list') || []
      onTick?.(runs.length)
      const names = new Set(runs.map(r => r.workflowName))
      if (CI.postMergeWorkflows.every(w => names.has(w)) && runs.every(r => r.status === 'completed')) {
        const bad = runs.filter(r => r.conclusion !== 'success').map(r => `${r.workflowName}=${r.conclusion}`)
        return { ok: !bad.length, runs: runs.map(r => `${r.workflowName}=${r.conclusion}`), bad }
      }
      await sleep(CI.pollSec * 1000)
    }
    return { ok: false, runs: runs.map(r => `${r.workflowName}=${r.status}/${r.conclusion}`), bad: ['zaman aşımı'] }
  }
}
