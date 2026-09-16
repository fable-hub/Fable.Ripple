// VanJS: a ~1 KB direct-DOM reactive runtime. Per-row state for label and
// selection; rows are added/removed from the tbody imperatively.
import van from 'vanjs-core';

const { table, tbody, tr, td, a } = van.tags;

const main = document.getElementById('main');
const tb = tbody();
van.add(main, table(tb));

let rows = []; // { id, label(state), selected(state), node }
let nextId = 1;
let selectedRow = null;

function makeRow(id) {
    const label = van.state('row ' + id);
    const selected = van.state(false);
    const node = tr(
        { class: () => (selected.val ? 'danger' : '') },
        td(id),
        td(a(label)),
        td(a('x')),
        td(),
    );
    return { id, label, selected, node };
}

function build(n) {
    const created = [];
    for (let i = 0; i < n; i++) {
        const r = makeRow(nextId++);
        rows.push(r);
        created.push(r.node);
    }
    van.add(tb, created);
}

window.__bench = {
    create(n) {
        tb.replaceChildren();
        rows = [];
        selectedRow = null;
        build(n);
    },
    append(n) {
        build(n);
    },
    updateEvery10th() {
        for (let i = 0; i < rows.length; i += 10) rows[i].label.val += ' !!!';
    },
    swap() {
        if (rows.length < 999) return;
        const a2 = 1, b2 = rows.length - 2;
        const r1 = rows[a2].node, r2 = rows[b2].node;
        const r2next = r2.nextSibling;
        tb.insertBefore(r2, r1);
        tb.insertBefore(r1, r2next);
        const t = rows[a2]; rows[a2] = rows[b2]; rows[b2] = t;
    },
    select(i) {
        if (selectedRow) selectedRow.selected.val = false;
        selectedRow = rows[i] || null;
        if (selectedRow) selectedRow.selected.val = true;
    },
    remove(i) {
        const [r] = rows.splice(i, 1);
        if (r) r.node.remove();
    },
    clear() {
        tb.replaceChildren();
        rows = [];
        selectedRow = null;
    },
};
