#!/usr/bin/env python
"""Section check for the open process items of the decision log (checklist section 7, step 3).

Usage: python -I -X utf8 scripts/doc-scans/karar_bolum_denetle.py <first K> <section no> [file]

Every phase has one section for its open process items (for example 24.1 for phase 10).
Quality-loop and closing workers copy the last written section and put their item into the
previous phase's section (phase 10: 29 items landed in 22.1 and 12.1; found only by the
conflict scan). This script lists the checklist items (`- [ ] **title**` / `- [x] **title**`)
whose bold title names a decision K-n with n >= <first K> but which sit under a numbered
`## N.` heading other than <section no>. Output: one line per candidate, exit code 1 when any.
Candidates are read by a human: an item that legitimately updates an older section (a
back-reference) is dropped with a reason; a misplaced one moves to the phase's section.
The section number of the phase is written to the tracker when the phase opens (opening step).
"""
import re
import sys

LOG = 'Docs/PRODUCT_DISCOVERY_STATUS.md'
HEADING = re.compile(r'^## (\d+)\. ')
ITEM = re.compile(r'^- \[[ x]\] \*\*(.+?)\*\*')


def main():
    args = sys.argv[1:]
    if len(args) not in (2, 3):
        print(__doc__)
        return 2
    first = int(args[0])
    own = args[1]
    path = args[2] if len(args) == 3 else LOG
    section = None
    found = []
    with open(path, encoding='utf-8', newline='') as handle:
        for number, line in enumerate(handle, 1):
            line = line.rstrip('\r\n')
            head = HEADING.match(line)
            if head:
                section = head.group(1)
                continue
            item = ITEM.match(line)
            if not item or section is None or section == own:
                continue
            ks = [int(k) for k in re.findall(r'K-(\d+)', item.group(1))]
            if any(k >= first for k in ks):
                found.append((number, section, item.group(1)[:90]))
    print('candidates: %d (items outside section %s naming K-%d or later in the title)' % (len(found), own, first))
    for number, sec, title in found:
        print('line %d, section %s: %s' % (number, sec, title))
    return 1 if found else 0


if __name__ == '__main__':
    sys.exit(main())
