// Permission lists for step sessions (--permission-mode dontAsk: anything not allowed is denied
// without a prompt; deny wins over allow). These lists stop the model from drifting; the hard
// guarantees come from elsewhere: the session has no git/gh credentials (session.mjs) and the
// driver checks the result before anything leaves the machine (checks.mjs).
import os from 'node:os'
import path from 'node:path'

const USER = os.userInfo().username
const HOME_SLASH = '//' + os.homedir().split(path.sep).join('/').replace(/^([A-Za-z]):/, (m, d) => d.toLowerCase())

// Legal bases are verified against official texts only.
const LEGAL_WEB = ['mevzuat.gov.tr', 'www.mevzuat.gov.tr', 'resmigazete.gov.tr', 'www.resmigazete.gov.tr', 'kvkk.gov.tr', 'www.kvkk.gov.tr']
  .map(d => `WebFetch(domain:${d})`)

export const ALLOW = [
  'Agent', 'Skill', 'Glob', 'Grep', 'TodoWrite', 'WebSearch', ...LEGAL_WEB,
  'Edit(Docs/**)', 'Write(Docs/**)', 'Edit(.autopilot/work/**)', 'Write(.autopilot/work/**)',
  // read-only git
  'Bash(git status*)', 'Bash(git diff*)', 'Bash(git log*)', 'Bash(git show *)', 'Bash(git rev-parse*)', 'Bash(git ls-files*)',
  'Bash(git grep *)', 'Bash(git cat-file *)', 'Bash(git merge-base *)', 'Bash(git branch --show-current)', 'Bash(git config core.hooksPath)',
  // scripted checks
  'Bash(node *)', 'Bash(wc *)', 'Bash(grep *)', 'Bash(awk *)', 'Bash(sed -n *)', 'Bash(sort *)', 'Bash(uniq *)', 'Bash(head *)',
  'Bash(tail *)', 'Bash(cut *)', 'Bash(tr *)', 'Bash(cmp *)', 'Bash(diff *)', 'Bash(comm *)', 'Bash(ls *)', 'Bash(echo *)',
  'Bash(printf *)', 'Bash(cat *)', 'Bash(find *)', 'Bash(test *)', 'Bash(sha256sum *)', 'Bash(mkdir -p Docs/*)', 'Bash(mkdir -p .autopilot/work*)',
]

// Isolation locks: other sessions, the owner's auto-memory and transcripts stay closed.
// Claude's own tool-results files (random .txt names) stay readable.
const homeReads = (base) => [`Read(${base}/.claude/**/*.jsonl)`, `Read(${base}/.claude/**/*.json)`, `Read(${base}/.claude/**/memory/**)`, `Read(${base}/.claude/*.md)`]
export const LOCK_DENY = ['PowerShell', 'ListAgents', 'SendMessage', ...homeReads('~'), ...homeReads(HOME_SLASH),
  'Bash(*~/.claude*)', 'Bash(*$HOME*)', 'Bash(*USERPROFILE*)', `Bash(*${USER}/.claude*)`, `Bash(*${USER}\\.claude*)`]

export const DENY = [...LOCK_DENY,
  // the driver owns every git write and every GitHub call
  'Bash(git add*)', 'Bash(git commit*)', 'Bash(git push*)', 'Bash(git checkout*)', 'Bash(git switch*)', 'Bash(git reset*)',
  'Bash(git stash*)', 'Bash(git rebase*)', 'Bash(git merge *)', 'Bash(git tag*)', 'Bash(git fetch*)', 'Bash(git pull*)',
  'Bash(git -c *)', 'Bash(git config --*)', 'Bash(* --no-verify*)', 'Bash(gh *)', 'Bash(PB_ALLOW_*)',
  // the second model only through the fixed-flag wrapper
  'Bash(codex *)', 'Bash(*codex.js*)',
  'Edit(Docs/00_*)', 'Write(Docs/00_*)', 'Edit(Docs/BYPASS_LOG.md)', 'Write(Docs/BYPASS_LOG.md)',
  'Edit(.autopilot/ledger/**)', 'Write(.autopilot/ledger/**)',
]

// Denials that mean the session tried something forbidden (as opposed to a harmless command
// shape that is simply not on the allow list). A forbidden attempt stops the step.
// Reaching into the home .claude folder is checked separately (touchesHomeClaude): the repo's own
// .claude/ is readable.
export const FORBIDDEN_ATTEMPT = /\bgit\s+(-c\b|add|commit|push|checkout|switch|reset|stash|rebase|merge|tag|fetch|pull|config\s+--)|\bgh\s|--no-verify|PB_ALLOW_|\.jsonl\b|\bcodex(\.js)?\b/i
export const FORBIDDEN_TOOLS = ['PowerShell', 'ListAgents', 'SendMessage']

// The home .claude folder in every spelling a tool input may use: C:\Users\u\.claude, C:/Users/u/.claude,
// /c/Users/u/.claude, //c/Users/u/.claude, ~/.claude, $HOME, %USERPROFILE%. Any .claude/projects/ path
// is home by definition (the repo has no such folder).
const HOME_FORMS = (() => {
  const h = os.homedir().replace(/\\/g, '/').toLowerCase()
  const m = /^([a-z]):(.*)$/.exec(h)
  return (m ? [h, `/${m[1]}${m[2]}`, `//${m[1]}${m[2]}`] : [h]).map(f => `${f}/.claude`)
})()
export function touchesHomeClaude(text) {
  const t = String(text).replace(/\\+/g, '/').toLowerCase()
  return /~\/\.claude|\$home|userprofile|\.claude\/projects\//.test(t) || HOME_FORMS.some(f => t.includes(f))
}

export const TOOL_RULE = `- Dosya okumak, aramak ve yazmak için Read, Grep, Glob, Write ve Edit araçlarını kullan. Bash'te döngü (for/while), değişken ataması, cd ve heredoc (<<) içeren komutlar reddedilir; Bash'i yalnız salt okuma git komutları, node betikleri ve tek satırlık wc/grep/awk/sort için kullan. Geçici dosyaları .autopilot/work/ altına yaz. Bu kuralı alt ajanlara verdiğin talimata da yaz.
- Yasal dayanakları yalnız resmî kaynaktan (mevzuat.gov.tr, resmigazete.gov.tr, kvkk.gov.tr) doğrula.`
