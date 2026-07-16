/*****************************************************************************************
  BARCODE LABELS SCREEN
  Wire-up:
    - GET /Library/GetAccessionForLabels?companyId=..&mode=RANGE&from=..&to=..
    - GET /Library/GetAccessionForLabels?companyId=..&mode=SINGLE&accessionNo=..
          -> thin controller actions calling SP_LIBRARY_ACCESSION_LABEL_LIST,
             returns { success, data: [{ AccessionId, AccessionNo, BookTitle,
                       SubTitle, Location, DocumentStatusId, StatusName }, ...] }
    - Requires JsBarcode + SweetAlert2 already loaded on the page.
*****************************************************************************************/
'use strict';

const COMPANY_ID = window.CURRENT_COMPANY_ID;

let items = [];   // { id, accessionId, acc, desc, checked }
let seq = 0;

/* ============================= List management ============================= */

function toastError(message) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message,
        confirmButtonText: 'OK',
        customClass: { confirmButton: 'btn btn-danger' },
        buttonsStyling: false
    });
}

async function showRange() {
    const from = document.getElementById('txtRangeFrom').value.trim();
    const to = document.getElementById('txtRangeTo').value.trim();

    if (!from || !to) {
        toastError('Enter both "From" and "To" accession numbers.');
        return;
    }

    try {
        const resp = await fetch(
            `/Library/GetAccessionForLabels?companyId=${encodeURIComponent(COMPANY_ID)}&mode=RANGE&from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`
        );
        const res = await resp.json();

        if (!res.success || !res.data || res.data.length === 0) {
            toastError('No accession numbers found in that range.');
            return;
        }

        res.data.forEach(row => addItem(row));
        renderList();
    } catch (err) {
        console.error('Range lookup failed', err);
        toastError('Could not load accession range. Please try again.');
    }
}

async function addSingleAcc() {
    const el = document.getElementById('txtSingleAcc');
    const err = document.getElementById('errSingleAcc');
    const val = el.value.trim();

    err.textContent = '';
    el.classList.remove('is-invalid');

    if (!val) return;

    try {
        const resp = await fetch(
            `/Library/GetAccessionForLabels?companyId=${encodeURIComponent(COMPANY_ID)}&mode=SINGLE&accessionNo=${encodeURIComponent(val)}`
        );
        const res = await resp.json();

        if (!res.success || !res.data || res.data.length === 0) {
            el.classList.add('is-invalid');
            err.textContent = 'No copy found with this accession no';
            return;
        }

        res.data.forEach(row => addItem(row));
        renderList();
        el.value = '';
        el.focus();
    } catch (e) {
        console.error('Single lookup failed', e);
        toastError('Lookup failed. Please try again.');
    }
}

// A barcode scanner behaves like a fast typist + Enter key
document.getElementById('txtSingleAcc')?.addEventListener('keydown', function (e) {
    if (e.key !== 'Enter') return;
    e.preventDefault();
    addSingleAcc();
});

function addItem(row) {
    // avoid duplicate accession numbers in the list
    //if (items.some(i => i.acc === row.AccessionNo)) return;
    items.push({
        id: ++seq,
        accessionId: row.accessionId,
        acc: row.accessionNo,
        desc: row.bookTitle || '',
        checked: true
    });
}

function removeRow(id) {
    items = items.filter(i => i.id !== id);
    renderList();
}

function clearList() {
    if (items.length === 0) return;
    Swal.fire({
        icon: 'warning',
        title: 'Clear all?',
        text: 'This will remove every accession no from the list.',
        showCancelButton: true,
        confirmButtonText: 'Yes, clear',
        customClass: { confirmButton: 'btn btn-danger', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then(result => {
        if (result.isConfirmed) {
            items = [];
            renderList();
        }
    });
}

function toggleAll(cb) {
    items.forEach(i => i.checked = cb.checked);
    renderList();
}

function setChecked(id, val) {
    const it = items.find(i => i.id === id);
    if (it) it.checked = val;
    document.getElementById('chkSelectAll').checked = items.length > 0 && items.every(i => i.checked);
}

function setDesc(id, val) {
    const it = items.find(i => i.id === id);
    if (it) it.desc = val;
}

function renderList() {
    const body = document.getElementById('tblAccBody');
    body.innerHTML = '';

    if (items.length === 0) {
        body.innerHTML = `<tr id="rowEmptyState"><td colspan="5" class="text-center py-4 text-muted">No accession numbers added yet.</td></tr>`;
    } else {
        items.forEach((item, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="ps-3">
                    <input type="checkbox" class="form-check-input" ${item.checked ? 'checked' : ''}
                           onchange="setChecked(${item.accessionId}, this.checked)">
                </td>
                <td>${idx + 1}</td>
                <td>${item.acc}</td>
                <td>
                    <input type="text" class="desc-input" placeholder="e.g. book title"
                           value="${escapeAttr(item.desc)}"
                           onchange="setDesc(${item.accessionId}, this.value)">
                </td>
                <td>
                    <i class="ti ti-trash text-danger" style="cursor:pointer;" title="Remove"
                       onclick="removeRow(${item.accessionId})"></i>
                </td>
            `;
            body.appendChild(tr);
        });
    }

    document.getElementById('lblListCount').textContent = items.length;
    document.getElementById('chkSelectAll').checked = items.length > 0 && items.every(i => i.checked);
}

/* ================================ Label rendering ================================ */

function renderLabels() {
    const selected = items.filter(i => i.checked);
    if (selected.length === 0) {
        toastError('Select at least one accession no from the list.');
        return;
    }

    const copies = Math.max(1, parseInt(document.getElementById('txtCopies').value, 10) || 1);
    const startRow = Math.max(1, parseInt(document.getElementById('txtStartRow').value, 10) || 1);
    const startCol = Math.max(1, parseInt(document.getElementById('txtStartCol').value, 10) || 1);
    const cols = Math.max(1, parseInt(document.getElementById('txtCols').value, 10) || 5);
    const header = document.getElementById('txtLabelHeader').value || '';

    // flatten with repeated copies
    const flat = [];
    selected.forEach(it => {
        for (let c = 0; c < copies; c++) flat.push(it);
    });

    // leading blank cells to honor Start Row / Start Column
    const leadingBlank = (startRow - 1) * cols + (startCol - 1);

    const grid = document.getElementById('labelGrid');
    grid.style.setProperty('--cols', cols);
    grid.innerHTML = '';

    for (let i = 0; i < leadingBlank; i++) {
        const cell = document.createElement('div');
        cell.className = 'label-cell empty';
        grid.appendChild(cell);
    }

    flat.forEach((it, idx) => {
        const cell = document.createElement('div');
        cell.className = 'label-cell';
        const svgId = `bc_${it.id}_${idx}`;
        cell.innerHTML = `
            <div class="lbl-header">${escapeHtml(header)}</div>
            <svg id="${svgId}"></svg>
            <div class="lbl-desc">${escapeHtml(it.desc || '')}</div>
        `;
        grid.appendChild(cell);
    });

    document.getElementById('emptyPreview').style.display = 'none';
    grid.style.display = 'grid';

    flat.forEach((it, idx) => {
        const svgId = `bc_${it.id}_${idx}`;
        try {
            JsBarcode(`#${svgId}`, it.acc, {
                format: 'CODE39',
                displayValue: true,
                fontSize: 12,
                height: 32,
                width: 1.5,
                margin: 2
            });
        } catch (e) {
            console.warn('Barcode render failed for', it.acc, e);
        }
    });
}

function printLabels() {
    if (document.getElementById('labelGrid').style.display === 'none') {
        toastError('Generate the labels first, then print.');
        return;
    }
    window.print();
}

/* ================================ Helpers ================================ */

function escapeHtml(s) {
    return String(s).replace(/[&<>"']/g, ch => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[ch]));
}

function escapeAttr(s) {
    return String(s).replace(/"/g, '&quot;');
}

document.addEventListener('DOMContentLoaded', () => {
    renderList();
});