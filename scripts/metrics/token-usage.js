#!/usr/bin/env node
// Token usage of a Claude Code session, measured from its transcripts (not from agent self-reports).
//
// Usage: node scripts/metrics/token-usage.js <session-transcript.jsonl> [--since <ISO date>]
//   The session's subagent transcripts are read from <session-id>/subagents/ next to the file.
//
// Every assistant message carries API usage: input, cache write, cache read and output tokens.
// A long-running agent re-reads its whole context on every call, so cache reads dominate the
// processed total; cache write + output approximate the newly produced text. Messages are
// deduplicated by message id + request id (a streamed message can span several lines).
// Subagents are grouped under their top-level ancestor (spawnDepth 1) via parentAgentId.
'use strict';
const fs = require('fs');
const path = require('path');

const args = process.argv.slice(2);
if (!args[0]) {
  console.error('usage: node scripts/metrics/token-usage.js <session.jsonl> [--since <ISO date>]');
  process.exit(2);
}
const mainFile = path.resolve(args[0]);
const sinceIdx = args.indexOf('--since');
const since = sinceIdx >= 0 ? Date.parse(args[sinceIdx + 1]) : 0;
const subDir = path.join(path.dirname(mainFile), path.basename(mainFile, '.jsonl'), 'subagents');

const FIELDS = ['calls', 'input', 'cacheWrite', 'cacheRead', 'output'];
const empty = () => ({ calls: 0, input: 0, cacheWrite: 0, cacheRead: 0, output: 0 });
const add = (a, b) => { for (const k of FIELDS) a[k] += b[k]; return a; };
const total = a => a.input + a.cacheWrite + a.cacheRead + a.output;

function usageOf(file) {
  const byModel = {};
  const seen = new Set();
  let first = null;
  for (const line of fs.readFileSync(file, 'utf8').split('\n')) {
    if (!line.includes('"usage"')) continue;
    let o;
    try { o = JSON.parse(line); } catch (_) { continue; }
    const m = o.message;
    if (!m || !m.usage || !m.model || m.model === '<synthetic>') continue;
    if (since && o.timestamp && Date.parse(o.timestamp) < since) continue;
    const key = (m.id || '') + '|' + (o.requestId || '');
    if (m.id && seen.has(key)) continue;
    seen.add(key);
    if (!first && o.timestamp) first = o.timestamp;
    const u = m.usage;
    add(byModel[m.model] || (byModel[m.model] = empty()), {
      calls: 1,
      input: u.input_tokens || 0,
      cacheWrite: u.cache_creation_input_tokens || 0,
      cacheRead: u.cache_read_input_tokens || 0,
      output: u.output_tokens || 0,
    });
  }
  return { byModel, first };
}

const M = n => (n / 1e6).toFixed(2);
const row = (label, a) => `| ${label} | ${a.calls} | ${M(a.cacheRead)} | ${M(a.cacheWrite)} | ${M(a.output)} | ${M(a.cacheWrite + a.output + a.input)} | ${M(total(a))} |`;
const HEAD = '| | Calls | Cache read (M) | Cache write (M) | Output (M) | Write + output + input (M) | Processed (M) |\n|---|---|---|---|---|---|---|';

// Main session
const main = usageOf(mainFile);
console.log('## Main session\n\n' + HEAD);
for (const [model, a] of Object.entries(main.byModel)) console.log(row(model, a));

// Subagents
const agents = {};
if (fs.existsSync(subDir)) {
  for (const f of fs.readdirSync(subDir)) {
    if (!f.endsWith('.meta.json')) continue;
    const id = f.slice('agent-'.length, -'.meta.json'.length);
    const meta = JSON.parse(fs.readFileSync(path.join(subDir, f), 'utf8'));
    const jsonl = path.join(subDir, 'agent-' + id + '.jsonl');
    agents[id] = { meta, usage: fs.existsSync(jsonl) ? usageOf(jsonl) : { byModel: {}, first: null } };
  }
}
const rootOf = id => {
  let cur = id;
  for (let i = 0; i < 10 && agents[cur] && agents[cur].meta.parentAgentId && agents[agents[cur].meta.parentAgentId]; i++) {
    cur = agents[cur].meta.parentAgentId;
  }
  return cur;
};

const steps = {};
const byModel = {};
for (const [id, a] of Object.entries(agents)) {
  const root = rootOf(id);
  const s = steps[root] || (steps[root] = { own: empty(), children: empty(), childCount: 0, first: null });
  for (const [model, u] of Object.entries(a.usage.byModel)) {
    add(byModel[model] || (byModel[model] = empty()), u);
    add(id === root ? s.own : s.children, u);
  }
  if (id !== root && total(Object.values(a.usage.byModel).reduce(add, empty())) > 0) s.childCount++;
}
for (const s of Object.values(steps)) s.all = add(add(empty(), s.own), s.children);

console.log('\n## Subagents by model\n\n' + HEAD);
const sub = empty();
for (const [model, a] of Object.entries(byModel)) { console.log(row(model, a)); add(sub, a); }
console.log(row('**all subagents**', sub));

console.log('\n## By top-level step (own + descendants)\n\n| Step | Started | Descendants | Write + output + input (M) | Processed (M) |\n|---|---|---|---|---|');
const ordered = Object.entries(steps)
  .map(([id, s]) => ({ id, s, first: agents[id].usage.first || '' }))
  .filter(x => total(x.s.all) > 0)
  .sort((x, y) => x.first.localeCompare(y.first));
for (const { id, s, first } of ordered) {
  const d = String(agents[id].meta.description || id).replace(/\|/g, '/');
  console.log(`| ${d} (${agents[id].meta.model || '?'}) | ${first.slice(11, 16)} | ${s.childCount} | ${M(s.all.cacheWrite + s.all.output + s.all.input)} | ${M(total(s.all))} |`);
}
const grand = add(add(empty(), sub), Object.values(main.byModel).reduce(add, empty()));
console.log('\n**Session total (main + subagents):** processed ' + M(total(grand)) + ' M; write + output + input ' + M(grand.cacheWrite + grand.output + grand.input) + ' M; calls ' + grand.calls + '.');
