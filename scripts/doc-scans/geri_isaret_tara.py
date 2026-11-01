#!/usr/bin/env python
"""Back-reference check for the decision log (checklist section 3, back-reference rule).

Usage: python -I -X utf8 scripts/doc-scans/geri_isaret_tara.py <first K> <last K> [--tum]

With --tum (all mentions) the relation-word window and the eliminated-option filter are
dropped: every older K number the row names becomes a pair. The default pass misses
phrases such as "narrows the wording of K-n" when the relation word sits outside the
window (Phase 9 conflict scan: 5 real back-references were found only by an all-mentions
pass over 359 pairs). The all-mentions list is long; the relation word is used only to
sort the output (relation-word pairs first) and every line is read by a human.

For every decision row K-n in the range, the script finds the older K numbers the row
names next to a relation word (narrows, widens, changes, reverses, replaces, fixes ...)
and checks that the older row's impact column (last cell) names K-n back.
Output: pair count, missing back-references. Exit code 1 when any is missing, so the
step-end script can chain it next to the duplicate-K and column-count checks.
Pairs are candidates: every missing line is read by a human; a pair that only quotes
an older decision is dropped with a reason, a real one gets the back-reference.
"""
import re
import sys

LOG = 'Docs/PRODUCT_DISCOVERY_STATUS.md'
RELATION = re.compile(
    r'daralt|genişlet|değiştir|geri al|geçersiz|yerine|elenen seçeneğini|yeniden açar|hükmünü',
    re.IGNORECASE,
)
WINDOW = 60


def load_rows():
    rows = {}
    with open(LOG, encoding='utf-8', newline='') as handle:
        for line in handle:
            match = re.match(r'\| K-(\d+) \|', line)
            if not match:
                continue
            cells = line.rstrip('\n').split(' | ')
            rows[int(match.group(1))] = cells
    return rows


def main():
    args = [a for a in sys.argv[1:] if a != '--tum']
    every = len(args) != len(sys.argv) - 1
    if len(args) != 2:
        print(__doc__)
        return 2
    first, last = int(args[0]), int(args[1])
    rows = load_rows()
    pairs = 0
    missing = []
    for number in sorted(rows):
        if number < first or number > last:
            continue
        cells = rows[number]
        body = ' '.join(cells[3:5]) if len(cells) > 4 else ' '.join(cells[2:])
        seen = set()
        for found in re.finditer(r'K-(\d+)', body):
            old = int(found.group(1))
            if old >= number or old in seen or old not in rows:
                continue
            around = body[max(0, found.start() - WINDOW):found.end() + WINDOW]
            related = bool(RELATION.search(around)) and not re.search(r'elenen', around, re.IGNORECASE)
            if not related and not every:
                continue  # an eliminated option quotes an older decision, it does not change it
            seen.add(old)
            pairs += 1
            impact = rows[old][-1]
            if ('K-%d' % number) not in impact:
                missing.append((number, old, related))
    # --tum: relation-word pairs first (most likely real), then the rest
    missing.sort(key=lambda item: (not item[2], item[0], item[1]))
    print('pairs: %d; missing back-reference: %d%s' % (pairs, len(missing), ' (all mentions)' if every else ''))
    for number, old, related in missing:
        print('K-%d -> K-%d: the impact column of K-%d does not name K-%d%s' % (
            number, old, old, number, '' if related or not every else ' [no relation word]'))
    return 1 if missing else 0


if __name__ == '__main__':
    sys.exit(main())
