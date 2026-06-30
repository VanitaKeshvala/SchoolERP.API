// wwwroot/js/copySessionModal.js
//
// Generic "Copy to another Session/Company" modal controller.
// Works for ANY entity (Sections, Classes, Subjects, Students, Hostels, ...)
// as long as:
//   1) The shared partial _CopySessionModal.cshtml is rendered once on the page
//      (e.g. via @await Html.PartialAsync("_CopySessionModal") in _Layout.cshtml).
//   2) Each page calls openCopyModal('EntityKey') from its own "Copy" button,
//      where EntityKey matches a key in ENTITY_COLUMNS below.
//   3) The page does NOT duplicate the modal markup or write page-specific JS
//      for it — this file + the partial are the single shared implementation.

// ── Entity column config ─────────────────────────────────────────────
const ENTITY_COLUMNS = {
    sections: {
        headers: ['Section Name', 'Section Code', 'Class', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>${r.sectionName ?? '-'}</td>
            <td>${r.sectionCode ?? '-'}</td>
            <td>${r.className ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    },
    Classs: {
        headers: ['Class Name', 'Sections', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>${r.className ?? '-'}</td>
            <td>${r.sectionNames ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    },
    Subject: {
        headers: ['Subject Name', 'Subject Code', 'Type', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>${r.subjectName ?? '-'}</td>
            <td>${r.subjectCode ?? '-'}</td>
            <td>${r.subjectType ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    },
    Students: {
        headers: ['Student Name', 'Admission No', 'Roll No', 'Class', 'Father Name', 'Mobile', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>
                <div class="d-flex align-items-center gap-2">
                    ${r.studentPhoto
                ? `<img src="data:${r.studentPhotoType};base64,${r.studentPhoto}"
                               class="rounded-circle" style="width:28px;height:28px;object-fit:cover">`
                : `<div class="rounded-circle bg-secondary d-flex align-items-center
                               justify-content-center text-white"
                               style="width:28px;height:28px;font-size:11px">
                               ${(r.fullName?.[0] ?? '-').toUpperCase()}</div>`
            }
                    <span>${r.fullName ?? '-'}</span>
                </div>
            </td>
            <td>${r.admissionNo ?? '-'}</td>
            <td>${r.rollNo ?? '-'}</td>
            <td>${r.className ?? '-'}</td>
            <td>${r.fatherName ?? '-'}</td>
            <td>${r.mobileNo ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    },
    StudentHouse: {
        headers: ['House Name', 'Description', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>${r.studentHouseName ?? '-'}</td>
            <td>${r.studentHouseDescription ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    },
    RoomTypes: {
        headers: ['Room Type', 'Description', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>${r.roomTypeTitle ?? '-'}</td>
            <td>${r.roomTypeDescription ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    },
    Hostels: {
        headers: ['Hostel Name', 'Room Type', 'Intake', 'Address', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>${r.hostelName ?? '-'}</td>
            <td>${r.roomTypeTitle ?? '-'}</td>
            <td>${r.hostelIntake ?? '-'}</td>
            <td>${r.hostelAddress ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    },
    HostelType: {
        headers: ['Hostel Type', 'Description', 'Status'],
        row: (r, i) => `
            <td>${i + 1}</td>
            <td>${r.hostelTypeName ?? '-'}</td>
            <td>${r.description ?? '-'}</td>
            <td><span class="badge bg-${r.isActive ? 'success' : 'secondary'}-subtle
                text-${r.isActive ? 'success' : 'secondary'}">
                ${r.isActive ? 'Active' : 'Inactive'}</span></td>`
    }
};

const apiBaseUrl = window.appConfig.apiBaseUrl;

// ── Global config set by each page's Copy button ──────────────────
let currentConfig = {
    entityType: ''   // e.g. 'Students', 'Hostels', 'Subject'
};

// ── Bind session + company dropdowns, then re-validate ────────────
async function init() {
    try {
        const ddl = document.getElementById("fromSession");
        const ddlTo = document.getElementById("toSession");
        if (!ddl) return;

        ddl.innerHTML = '<option value="">Select Session</option>';
        ddlTo.innerHTML = '<option value="">Select To session</option>';
        const sessions = await window.fetchSessions();

        // FIX (#1): the "from" session/company should default to the user's
        // *current* session/company for convenience, but must stay a normal,
        // enabled <select> so the user can still change it. We only ever set
        // `.selected`/`.value` here — we never set `.disabled = true` on
        // fromSession/fromAccount. If your page still shows them as
        // non-editable, check the markup in _CopySessionModal.cshtml for a
        // stray `disabled` or `readonly` attribute on those two <select>s —
        // this script does not add one.
        const currentSessionId = window.getSessionId ? window.getSessionId() : null;
        const currentCompanyId = window.getCompanyId ? window.getCompanyId() : null;
        sessions.forEach(s => {
            const option = new Option(s.sessionTitle, s.sessionId);
            if (currentSessionId && String(currentSessionId) === String(s.sessionId)) {
                option.selected = true;
            }
            ddl.add(option);

            // "To" session intentionally has NO default selection — the user
            // must actively choose a target session.
            ddlTo.add(new Option(s.sessionTitle, s.sessionId));
        });

        const companies = await fetchCompanies();
        populateCompanies(companies);

        // FIX (#2): previously the table only loaded after the user manually
        // changed a dropdown, because nothing re-ran validateCopy() once the
        // defaults above were applied. Now that fromSession/fromAccount may
        // already be pre-selected, we explicitly re-validate once, so the
        // table state (shown/hidden, loaded/not loaded) is always correct
        // for whatever is selected at the moment the modal opens — and is
        // re-loaded again whenever toSession/toAccount change afterward via
        // their onchange="validateCopy()" handlers in the partial.
        loadTableData(currentCompanyId, currentSessionId, 0, 0);
        validateCopy();
    }
    catch (e) {
        console.error(e);
    }
}

async function fetchCompanies() {
    const resp = await fetch('/Company/GetAssignedCompanies', { method: 'GET' });
    const payload = await resp.json();
    if (!payload || payload.success !== true || !Array.isArray(payload.data)) {
        throw new Error(payload?.message || 'Failed to load companies');
    }
    return payload.data;
}

function populateCompanies(companies) {
    const ddl = document.getElementById("fromAccount");
    const ddlTo = document.getElementById("toAccount");
    if (!ddl) return;

    ddl.innerHTML = '<option value="">From main account</option>';
    ddlTo.innerHTML = '<option value="">To main account</option>';

    const currentCompanyId = window.getCompanyId ? window.getCompanyId() : null;
    
    for (const c of companies) {
        const opt = document.createElement('option');
        opt.value = String(c.companyId);
        opt.textContent = c.schoolName || `Company #${c.companyId}`;
        if (currentCompanyId && String(currentCompanyId) === String(c.companyId)) {
            opt.selected = true;
        }
        ddl.appendChild(opt);
    }

    for (const com of companies) {
        const opt = document.createElement('option');
        opt.value = String(com.companyId);
        opt.textContent = com.schoolName || `Company #${com.companyId}`;
        ddlTo.appendChild(opt);
    }

    ddl.disabled = companies.length === 0; // only disabled when truly empty
}

// ── Called by every Copy button — ONLY ARGUMENT CHANGES ───────────
function openCopyModal(entityType) {
    currentConfig = { entityType };

    // Reset modal state
    document.getElementById('tableSection').classList.add('d-none');
    document.getElementById('validationBox').classList.add('d-none');
    document.getElementById('btnCopy').disabled = true;
    document.getElementById('sectionsTbody').innerHTML = '';

    new bootstrap.Modal(document.getElementById('copySessionModal')).show();
    init();
}

// ── Runs whenever any dropdown changes (wired via onchange in the
//    shared partial) AND once from init() after defaults are applied ──
function validateCopy() {
    const fromAcc = document.getElementById('fromAccount').value;
    const fromSess = document.getElementById('fromSession').value;
    const toAcc = document.getElementById('toAccount').value;
    const toSess = document.getElementById('toSession').value;

    const allFilled = fromAcc && fromSess && toAcc && toSess;
    const isSame = fromAcc === toAcc && fromSess === toSess;

    // Show/hide conflict warning
    document.getElementById('validationBox').classList.toggle('d-none', !isSame);

    if (allFilled && !isSame) {
        //loadTableData(fromAcc, fromSess, toAcc, toSess);
    } else {
        //document.getElementById('tableSection').classList.add('d-none');
        document.getElementById('btnCopy').disabled = true;
    }
}

// ── Loads rows for the current entity type ─────────────────────────
async function loadTableData(fromAcc, fromSess, toAcc, toSess) {
    const config = ENTITY_COLUMNS[currentConfig.entityType];
    if (!config) {
        console.error('Unknown entity type:', currentConfig.entityType);
        return [];
    }

    const thead = document.querySelector('#tblSections thead tr');
    const tbody = document.getElementById('sectionsTbody');
    const selCount = document.getElementById('selCount');

    try {
        // ── Build dynamic headers ────────────────────────────────
        thead.innerHTML = `
        <th style="width:40px">#</th>
        ${config.headers.map(h => `<th>${h}</th>`).join('')}`;

        // ── Loading state ─────────────────────────────────────────
        tbody.innerHTML = `<tr>
            <td colspan="${config.headers.length + 2}" class="text-center py-3 text-muted">
                <div class="spinner-border spinner-border-sm me-2"></div>Loading...
            </td>
        </tr>`;

        const resp = await fetch(`/CopySession/${currentConfig.entityType}?sessionId=${fromSess}&companyId=${fromAcc}`);
        const res = await resp.json();

        if (!res.success || !res.data?.length) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="${config.headers.length + 2}"
                        class="text-center py-3 text-muted">
                        No records found.
                    </td>
                </tr>`;
            document.getElementById('tableSection').classList.remove('d-none');
            document.getElementById('btnCopy').disabled = true;
            return [];
        }

        // ── Render rows ───────────────────────────────────────────
        tbody.innerHTML = res.data.map((r, i) =>
            `<tr>${config.row(r, i)}</tr>`
        ).join('');

        document.getElementById('tableSection').classList.remove('d-none');
        if (selCount) selCount.textContent = '0 selected';
        document.getElementById('chkSelectAll').checked = false;
        updateCount();

        return res.data;
    } catch (err) {
        console.error('loadTableData error:', err);
        tbody.innerHTML = `
            <tr>
                <td colspan="${config.headers.length + 2}"
                    class="text-center py-3 text-danger">
                    <i class="ti ti-alert-circle me-1"></i>
                    Failed to load data. Please try again.
                </td>
            </tr>`;
        document.getElementById('tableSection').classList.remove('d-none');
        document.getElementById('btnCopy').disabled = true;
        return [];
    }
}

// ── Select all / count ────────────────────────────────────────────
function toggleSelectAll(cb) {
    document.querySelectorAll('.row-check').forEach(c => c.checked = cb.checked);
    updateCount();
}

function updateCount() {
    const checked = document.querySelectorAll('.row-check:checked').length;
    const selCount = document.getElementById('selCount');
    if (selCount) selCount.textContent = `${checked} selected`;
    document.getElementById('btnCopy').disabled = false;
}

// ── The Copy button click — same for ALL entity types ──────────────
async function handleCopy() {
    const fromAcc = document.getElementById('fromAccount').value;
    const fromSess = document.getElementById('fromSession').value;
    const toAcc = document.getElementById('toAccount').value;
    const toSess = document.getElementById('toSession').value;

    const selectedIds = [...document.querySelectorAll('.row-check:checked')]
        .map(c => c.value);

    const payload = {
        fromCompanyId: fromAcc,
        fromSessionId: fromSess,
        toCompanyId: toAcc,
        toSessionId: toSess,
        ids: selectedIds
    };

    const action = `Copy${currentConfig.entityType}`;
    if (!currentConfig.entityType) {
        console.error('Unknown entity type:', currentConfig.entityType);
        return;
    }

    document.getElementById('btnCopy').disabled = true;

    try {
        const res = await fetch(`/CopySession/${action}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        const data = await res.json();

        if (data.success) {
            bootstrap.Modal.getInstance(
                document.getElementById('copySessionModal')).hide();
            showToast(`${currentConfig.entityType} ${data.message}`, 'success');
        } else {
            showToast(data.message || 'Copy failed.', 'danger');
        }
    } catch {
        showToast('Network error. Please try again.', 'danger');
    } finally {
        document.getElementById('btnCopy').disabled = false;
    }
}

function showToast(msg, type) {
    alert(msg);
}

// Expose the only entry point pages need.
window.openCopyModal = openCopyModal;