// Hand-written imperative DOM - the floor. No framework, no reactivity: every op
// mutates the DOM directly. This is the smallest possible bundle and the fastest
// possible render for this row shape; everything else is measured against it.

const main = document.getElementById('main');
const table = document.createElement('table');
const tbody = document.createElement('tbody');
table.appendChild(tbody);
main.appendChild(table);

let rows = []; // { id, label, tr, labelNode }
let nextId = 1;
let selected = null;

function makeRow(id) {
    const label = 'row ' + id;
    const tr = document.createElement('tr');
    const td1 = document.createElement('td');
    td1.textContent = id;
    const td2 = document.createElement('td');
    const a = document.createElement('a');
    a.textContent = label;
    td2.appendChild(a);
    const td3 = document.createElement('td');
    const rm = document.createElement('a');
    rm.textContent = 'x';
    td3.appendChild(rm);
    const td4 = document.createElement('td');
    tr.append(td1, td2, td3, td4);
    return { id, label, tr, labelNode: a };
}

function fill(n) {
    const frag = document.createDocumentFragment();
    for (let i = 0; i < n; i++) {
        const r = makeRow(nextId++);
        rows.push(r);
        frag.appendChild(r.tr);
    }
    tbody.appendChild(frag);
}

window.__bench = {
    create(n) {
        tbody.textContent = '';
        rows = [];
        selected = null;
        fill(n);
    },
    append(n) {
        fill(n);
    },
    updateEvery10th() {
        for (let i = 0; i < rows.length; i += 10) {
            rows[i].label += ' !!!';
            rows[i].labelNode.textContent = rows[i].label;
        }
    },
    swap() {
        if (rows.length < 999) return;
        const a = 1, b = rows.length - 2;
        const r1 = rows[a].tr, r2 = rows[b].tr;
        const r2next = r2.nextSibling;
        tbody.insertBefore(r2, r1);
        tbody.insertBefore(r1, r2next);
        const t = rows[a]; rows[a] = rows[b]; rows[b] = t;
    },
    select(i) {
        if (selected) selected.tr.classList.remove('danger');
        selected = rows[i] || null;
        if (selected) selected.tr.classList.add('danger');
    },
    remove(i) {
        const [r] = rows.splice(i, 1);
        if (r) r.tr.remove();
    },
    clear() {
        tbody.textContent = '';
        rows = [];
        selected = null;
    },
};
