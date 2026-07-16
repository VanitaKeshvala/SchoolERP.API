// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_issueBook';
let appliedFilters = {};
try {
    const stored = sessionStorage.getItem(FILTER_KEY);
    if (stored) appliedFilters = JSON.parse(stored);
} catch (e) { appliedFilters = {}; }

function saveAppliedFilters() {
    try { sessionStorage.setItem(FILTER_KEY, JSON.stringify(appliedFilters)); }
    catch (e) { console.warn('sessionStorage unavailable'); }
}

function clearAppliedFilters() {
    appliedFilters = {};
    try { sessionStorage.removeItem(FILTER_KEY); } catch (e) { }
}

function submitForm() {
    const companyEl = document.getElementById('ddlFilterCompany');
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const companyEl = document.getElementById('ddlFilterCompany');
    const searchEl = document.getElementById('txtSearchInput');

    const searchVal = searchEl?.value.trim();
    if (searchVal) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    else delete appliedFilters['txtSearchInput'];

    if (companyEl?.value && companyEl.value !== '') {
        appliedFilters['ddlFilterCompany'] = {
            label: 'Company',
            text: companyEl.options[companyEl.selectedIndex]?.text || companyEl.value
        };
    } else delete appliedFilters['ddlFilterCompany'];

    saveAppliedFilters();
}

function renderFilterBadges() {
    const container = document.getElementById('badgeContainer');
    const mainContainer = document.getElementById('activeFilterBadges');
    if (!container || !mainContainer) return;

    container.innerHTML = '';
    const activeCount = Object.keys(appliedFilters).length;

    Object.entries(appliedFilters).forEach(([id, { label, text }]) => {
        const badge = document.createElement('span');
        badge.className = 'badge bg-primary-subtle text-primary border border-primary-subtle d-flex align-items-center gap-1 fw-medium px-2 py-1';
        badge.innerHTML = `${label}: ${text} <i class="ti ti-x ms-1 cursor-pointer" onclick="removeFilter('${id}')" style="font-size:10px;"></i>`;
        container.appendChild(badge);
    });

    mainContainer.style.display = activeCount > 0 ? 'block' : 'none';
}

function removeFilter(filterId) {
    if (filterId === 'ddlFilterCompany') {
        const el = document.getElementById('ddlFilterCompany');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    } else if (filterId === 'txtSearchInput') {
        const el = document.getElementById('txtSearchInput');
        if (el) el.value = '';
        document.getElementById('hdnSearch').value = '';
    }
    delete appliedFilters[filterId];
    saveAppliedFilters();
    document.getElementById('hdnPageIndex').value = 1;
    submitForm();
}

function resetAllFilters() {
    document.getElementById('txtSearchInput').value = '';
    document.getElementById('hdnSearch').value = '';

    ['ddlFilterCompany'].forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        el.value = '';
        if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
    });

    clearAppliedFilters();
    document.getElementById('hdnPageIndex').value = 1;
    submitForm();
}

// ========================================
// MEMBER LIMITS — module-level so every function below can see it
// ========================================
let memberLimits = { noOfDocuments: 0, maxDays: 0, expiryDate: null };

// ========================================
// Issue queue — module-level
// ========================================
'use strict';
const COMPANY_ID = window.CURRENT_COMPANY_ID;
let MEMBER_ID = window.CURRENT_MEMBER_ID;

let queue = [];   // { id, accessionId, acc, title, author, docType, dueDate, remarks }
let seq = 0;

const els = {
    accessionNo: document.getElementById('txtAccessionNo'),
    bookTitle: document.getElementById('txtBookTitle'),
    author: document.getElementById('txtAuthor'),
    docType: document.getElementById('ddlDocumentStatus'),
    dueDate: document.getElementById('txtDueDate'),
    remarks: document.getElementById('txtRemarks'),
    bookBank: document.getElementById('chkBookBank'),
    errAccessionNo: document.getElementById('errAccessionNo'),
    errDueDate: document.getElementById('errDueDate'),
};

function setError(el, errEl, message) {
    el.classList.toggle('is-invalid', !!message);
    if (errEl) errEl.textContent = message || '';
}

function clearLookupFields() {
    els.bookTitle.value = '';
    els.author.value = '';
    els.docType.value = '';
}

async function lookupAccession(accessionNo) {
    if (!accessionNo) return;

    setError(els.accessionNo, els.errAccessionNo, '');
    els.accessionNo.disabled = true;

    try {
        const resp = await fetch(
            `/Library/GetAccessionById?accessionNo=${encodeURIComponent(accessionNo)}`
        );
        const data = await resp.json();

        if (!data.success) {
            clearLookupFields();
            setError(els.accessionNo, els.errAccessionNo, data.MESSAGE || 'No copy found with this accession no');
            return;
        }

        els.bookTitle.value = data.data.bookTitle || '';
        els.author.value = data.data.author || '';
        els.docType.value = data.data.documentStatus || '';
        els.accessionNo.dataset.accessionId = data.data.accessionId;
        els.accessionNo.dataset.accessionNoResolved = data.data.accessionNo;
        $(els.docType).val(data.data.documentStatusId ?? '').trigger('change');
    } catch (err) {
        console.error('Accession lookup failed', err);
        setError(els.accessionNo, els.errAccessionNo, 'Lookup failed. Please try again.');
    } finally {
        els.accessionNo.disabled = false;
        els.accessionNo.focus();
    }
}

// Barcode scanner = fast typist + Enter
els.accessionNo.addEventListener('keydown', function (e) {
    if (e.key !== 'Enter') return;
    e.preventDefault();
    lookupAccession(this.value.trim());
});

// ========================================
// Member limit enforcement (expiry / quota / max days)
// ========================================
function applyMemberLimitUI() {
    const alertBox = document.getElementById('memberLimitAlert');
    const addBtn = document.querySelector('button[onclick="addAccession()"]');
    const saveBtn = document.querySelector('.top-btn.btn-success');
    const dueDateInput = els.dueDate;

    let blocked = false;
    let msg = '';

    // 1) Card expiry check
    const today = new Date(); today.setHours(0, 0, 0, 0);
    if (memberLimits.expiryDate && memberLimits.expiryDate < today) {
        blocked = true;
        msg = `This member's library card expired on ${formatDate(memberLimits.expiryDate)}. Renew before issuing books.`;
    }

    // 2) Quota check (against what's already queued locally)
    if (!blocked && memberLimits.noOfDocuments > 0 && queue.length >= memberLimits.noOfDocuments) {
        blocked = true;
        msg = `Issue limit reached — this member can hold a maximum of ${memberLimits.noOfDocuments} book(s) at a time.`;
    }

    if (alertBox) {
        if (msg) {
            alertBox.textContent = msg;
            alertBox.style.display = 'block';
        } else {
            alertBox.style.display = 'none';
        }
    }

    if (addBtn) addBtn.disabled = blocked;
    if (saveBtn) saveBtn.disabled = blocked && queue.length === 0;

    // 3) Cap the Due Return Date input by MAXDAYS
    if (dueDateInput) {
        const todayStr = new Date().toISOString().split('T')[0];
        dueDateInput.min = todayStr;

        if (memberLimits.maxDays > 0) {
            const maxDate = new Date();
            maxDate.setDate(maxDate.getDate() + memberLimits.maxDays);
            dueDateInput.max = maxDate.toISOString().split('T')[0];

            if (!dueDateInput.value) {
                dueDateInput.value = maxDate.toISOString().split('T')[0];
            }
        } else {
            dueDateInput.removeAttribute('max');
        }
    }
}

function validateDueDateAgainstLimits() {
    if (!els.dueDate.value) return true;

    const due = new Date(els.dueDate.value);
    const today = new Date(); today.setHours(0, 0, 0, 0);

    if (due < today) {
        setError(els.dueDate, els.errDueDate, 'Due date cannot be in the past.');
        return false;
    }

    if (memberLimits.maxDays > 0) {
        const maxDate = new Date();
        maxDate.setDate(maxDate.getDate() + memberLimits.maxDays);
        maxDate.setHours(0, 0, 0, 0);
        if (due > maxDate) {
            setError(els.dueDate, els.errDueDate,
                `Due date can't exceed ${memberLimits.maxDays} day(s) from today (max: ${formatDate(maxDate)}).`);
            return false;
        }
    }

    setError(els.dueDate, els.errDueDate, '');
    return true;
}

// ========================================
// Add to queue / render / remove
// ========================================
function addAccession() {
    const accessionId = els.accessionNo.dataset.accessionId;
    const accNo = els.accessionNo.dataset.accessionNoResolved;

    if (!accessionId) {
        setError(els.accessionNo, els.errAccessionNo, 'Scan/lookup a valid accession no first');
        return;
    }
    if (!els.dueDate.value) {
        setError(els.dueDate, els.errDueDate, 'Due return date is required');
        return;
    }

    // Expiry check
    const today = new Date(); today.setHours(0, 0, 0, 0);
    if (memberLimits.expiryDate && memberLimits.expiryDate < today) {
        Swal.fire({
            icon: 'error', title: 'Card Expired',
            text: `This member's library card expired on ${formatDate(memberLimits.expiryDate)}.`,
            confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-danger' }, buttonsStyling: false
        });
        return;
    }

    // Quota check
    if (memberLimits.noOfDocuments > 0 && queue.length >= memberLimits.noOfDocuments) {
        Swal.fire({
            icon: 'warning', title: 'Limit Reached',
            text: `This member can hold a maximum of ${memberLimits.noOfDocuments} book(s) at a time.`,
            confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-warning' }, buttonsStyling: false
        });
        return;
    }

    // Due date / maxdays check
    if (!validateDueDateAgainstLimits()) return;

    // Prevent adding the same accession twice in the same batch
    if (queue.some(q => q.accessionId === parseInt(accessionId, 10))) {
        setError(els.accessionNo, els.errAccessionNo, 'This accession is already in the list.');
        return;
    }

    setError(els.dueDate, els.errDueDate, '');
    setError(els.accessionNo, els.errAccessionNo, '');

    queue.push({
        id: ++seq,
        accessionId: parseInt(accessionId, 10),
        acc: accNo,
        title: els.bookTitle.value,
        author: els.author.value,
        docType: els.docType.value,
        dueDate: els.dueDate.value,
        remarks: els.remarks.value
    });

    renderQueue();
    applyMemberLimitUI();

    els.accessionNo.value = '';
    delete els.accessionNo.dataset.accessionId;
    delete els.accessionNo.dataset.accessionNoResolved;
    clearLookupFields();
    els.remarks.value = '';
    els.accessionNo.focus();
}

function removeQueueRow(id) {
    queue = queue.filter(q => q.id !== id);
    renderQueue();
    applyMemberLimitUI();
}

function setRemarks(id, val) {
    const it = queue.find(q => q.id === id);
    if (it) it.remarks = val;
}

function renderQueue() {
    const body = document.getElementById('tblIssueBody');
    body.innerHTML = '';

    if (queue.length === 0) {
        body.innerHTML = `<tr id="rowEmptyState"><td colspan="6" class="text-center py-4 text-muted">No accessions added yet.</td></tr>`;
    } else {
        queue.forEach((it, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="ps-3">${idx + 1}</td>
                <td>${it.acc}</td>
                <td>
                    <div class="fw-semibold">${escapeHtml(it.title)}</div>
                    <div class="text-muted fs-12">
                        ${escapeHtml(it.author) || '-'}
                        ${it.docType ? ' &middot; ' + escapeHtml(it.docType) : ''}
                    </div>
                </td>
                <td>${it.dueDate}</td>
                <td><input type="text" class="rmk-input" value="${escapeAttr(it.remarks)}" onchange="setRemarks(${it.id}, this.value)"></td>
                <td><i class="ti ti-trash text-danger" style="cursor:pointer;" title="Remove" onclick="removeQueueRow(${it.id})"></i></td>
            `;
            body.appendChild(tr);
        });
    }

    document.getElementById('lblListCount').textContent = queue.length;
}

// ========================================
// Save issue (single source of truth — matches sp_Library_Issue_Save,
// which expects AccessionNo, not AccessionID/BookID)
// ========================================
async function saveIssue() {
    if (queue.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Nothing to save',
            text: 'Add at least one accession to the list before saving.',
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn btn-warning' },
            buttonsStyling: false
        });
        return;
    }

    if (window.FINE_DUE > 0) {
        const confirmResult = await Swal.fire({
            icon: 'warning',
            title: 'Outstanding fine',
            text: `This member has a pending fine of ${window.FINE_DUE}. Issue anyway?`,
            showCancelButton: true,
            confirmButtonText: 'Issue anyway',
            customClass: { confirmButton: 'btn btn-danger', cancelButton: 'btn btn-secondary' },
            buttonsStyling: false
        });
        if (!confirmResult.isConfirmed) return;
    }

    try {
        const body = {
            libraryMemberID: MEMBER_ID,
            issueDate: new Date().toISOString(),
            issueNo: parseInt(document.getElementById('txtIssueNo').value, 10) || 0,
            issueListJson: JSON.stringify(queue.map(q => ({
                AccessionNo: q.acc,
                DueReturnDate: q.dueDate,
                Remarks: q.remarks,
                BookBank: document.getElementById('chkBookBank').checked
            })))
        };

        const resp = await fetch('/Library/SaveIssueBook', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        const res = await resp.json();

        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Issued!',
                text: res.message ||`${queue.length} book(s) issued successfully.`,
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = `/Library/Issue/${MEMBER_ID}`;
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save issue.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
        }
    } catch (err) {
        console.error('Save failed', err);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Save failed. Please try again.',
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn btn-danger' },
            buttonsStyling: false
        });
    }
}

function openReturnModal(id) {
    selectedIssueId = id;
    $('#returnDate').val(new Date().toISOString().split('T')[0]);
    $('#return').modal('show');
}

function escapeHtml(s) {
    return String(s || '').replace(/[&<>"']/g, ch => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[ch]));
}
function escapeAttr(s) {
    return String(s || '').replace(/"/g, '&quot;');
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    const d = (dateStr instanceof Date) ? dateStr : new Date(dateStr);
    if (isNaN(d)) return '-';
    return d.toLocaleDateString();
}

// ========================================
// Accession No autocomplete
// ========================================
(function initAccessionAutocomplete() {
    const accInput = document.getElementById('txtAccessionNo');
    const accList = document.getElementById('accessionSuggestions');
    if (!accInput || !accList) return;

    let accDebounce;
    let accItems = [];

    accInput.addEventListener('input', function () {
        const term = this.value.trim();
        clearTimeout(accDebounce);
        delete accInput.dataset.accessionId;
        delete accInput.dataset.accessionNoResolved;
        clearLookupFields();

        if (term.length < 2) { hideAccList(); return; }
        accDebounce = setTimeout(() => fetchAccessions(term), 300);
    });

    document.addEventListener('click', function (e) {
        if (!accInput.contains(e.target) && !accList.contains(e.target)) hideAccList();
    });

    async function fetchAccessions(term) {
        try {
            const res = await fetch(`/Library/SearchAccession?searchText=${encodeURIComponent(term)}`);
            const result = await res.json();
            if (result.success && result.data && result.data.length > 0) {
                accItems = result.data;
                renderAccList(accItems);
            } else {
                hideAccList();
            }
        } catch (err) {
            console.error('SearchAccession error:', err);
            hideAccList();
        }
    }

    function renderAccList(items) {
        accList.innerHTML = '';
        items.forEach((item, idx) => {
            const li = document.createElement('li');
            li.className = 'list-group-item list-group-item-action';
            li.style.cursor = 'pointer';
            const availTag = item.isAvailable
                ? ''
                : ' <span class="badge bg-danger-subtle text-danger ms-1">Issued</span>';
            li.innerHTML = `<strong>${item.accessionNo ?? '-'}</strong> &nbsp;-&nbsp; ${item.bookTitle ?? ''}${availTag}`;
            li.addEventListener('click', () => selectAccessionItem(idx));
            accList.appendChild(li);
        });
        accList.style.display = 'block';
    }

    function selectAccessionItem(idx) {
        const item = accItems[idx];
        if (!item) return;

        accInput.value = item.accessionNo ?? '';

        if (!item.isAvailable) {
            setError(accInput, els.errAccessionNo, 'This copy is currently not available for issue.');
            hideAccList();
            return;
        }

        els.bookTitle.value = item.bookTitle || '';
        els.author.value = item.author || '';
        els.docType.value = item.documentStatus || '';
        accInput.dataset.accessionId = item.accessionId;
        accInput.dataset.accessionNoResolved = item.accessionNo;
        if (item.documentStatusId) $(els.docType).val(item.documentStatusId).trigger('change');
        setError(accInput, els.errAccessionNo, '');

        hideAccList();
    }

    function hideAccList() {
        accList.style.display = 'none';
        accList.innerHTML = '';
    }
})();

// ========================================
// Member card search — autocomplete + detail card
// ========================================
function initMemberAutocomplete() {
    const input = document.getElementById('txtsearchText');
    const list = document.getElementById('pinCardSuggestions');
    const detailCard = document.getElementById('memberDetailCard');

    if (!input || !list) {
        console.warn('Card autocomplete: input or list element not found');
        return;
    }

    let debounceTimer;
    let currentItems = [];

    input.addEventListener('input', function () {
        const term = this.value.trim();
        clearTimeout(debounceTimer);
        hideDetailCard();
        if (term.length < 2) {
            hideList();
            return;
        }
        debounceTimer = setTimeout(() => fetchMembers(term), 300);
    });

    document.addEventListener('click', function (e) {
        if (!input.contains(e.target) && !list.contains(e.target)) {
            hideList();
        }
    });

    async function fetchMembers(term) {
        try {
            var memberType = $("#ddlMemberType").val();
            const res = await fetch(`/Library/SearchMember?memberType=${memberType}&searchText=${encodeURIComponent(term)}`);
            const result = await res.json();
            console.log('API response:', result);

            if (result.success && result.data && result.data.length > 0) {
                currentItems = result.data;
                renderList(currentItems);
            } else {
                hideList();
            }
        } catch (err) {
            console.error('SearchMember error:', err);
            hideList();
        }
    }

    function renderList(items) {
        list.innerHTML = '';
        items.forEach((item, idx) => {
            const li = document.createElement('li');
            li.className = 'list-group-item list-group-item-action';
            li.style.cursor = 'pointer';

            const name = getName(item);
            const cardNo = item.libraryCardNo ?? '';

            li.innerHTML = `<strong>${cardNo || '-'}</strong> &nbsp;-&nbsp; ${name || '-'}`;
            li.addEventListener('click', () => selectMemberItem(idx));
            list.appendChild(li);
        });
        list.style.display = 'block';
    }

    function selectMemberItem(idx) {
        const item = currentItems[idx];
        if (!item) return;

        input.value = item.libraryCardNo ?? getName(item) ?? '';
        hideList();
        showMemberDetail(item);
    }

    function showMemberDetail(item) {
        const isStudent = (item.memberType || '').toLowerCase() === 'student';

        document.getElementById('memberName').textContent = getName(item) || '-';
        document.getElementById('memberTypeBadge').textContent = item.memberType || '-';

        document.getElementById('studentOnlyFields').style.display = isStudent ? '' : 'none';
        document.getElementById('staffOnlyFields').style.display = isStudent ? 'none' : '';
        document.querySelectorAll('.studentOnlyRow').forEach(el => el.style.display = isStudent ? '' : 'none');
        document.querySelectorAll('.staffOnlyRow').forEach(el => el.style.display = isStudent ? 'none' : '');

        const photo = document.getElementById('memberPhoto');
        photo.src = item.photo ? item.photo : '/assets/img/doctors/doctor-01.jpg';

        if (isStudent) {
            document.getElementById('memberAdmissionNo').textContent = item.admissionNo ?? '-';
            document.getElementById('memberRollNo').textContent = item.rollNo ?? '-';
            document.getElementById('memberClassName').textContent = item.className ?? '-';
            document.getElementById('memberSectionName').textContent = item.sectionName ?? '-';
        } else {
            document.getElementById('memberStaffCode').textContent = item.staffCode ?? '-';
            document.getElementById('memberDepartment').textContent = item.departmentName ?? '-';
            document.getElementById('memberDesignation').textContent = item.designationID ?? '-';
        }
        document.getElementById('memberNoOfDocuments').textContent = item.noofdocuments ?? '-';
        document.getElementById('memberMAXDays').textContent = item.maxdays ?? '-';
        document.getElementById('memberGender').textContent = item.gender ?? '-';
        document.getElementById('memberMobileNo').textContent = getVal(item, 'student_MobileNo', 'staff_MobileNo', 'mobileNo') ?? '-';
        document.getElementById('memberEmail').textContent = getVal(item, 'student_Email', 'staff_Email', 'email') ?? '-';
        document.getElementById('memberLibraryCardNo').textContent = item.libraryCardNo ?? '-';
        document.getElementById('memberExpiryDate').textContent = formatDate(item.expiryDATE ?? item.expiryDate);
        document.getElementById('memberIsActive').textContent = item.isActive ? 'Active' : 'Inactive';

        // capture library limits from the search result
        memberLimits.noOfDocuments = item.noofdocuments ?? item.NOOFDOCUMENTS ?? 0;
        memberLimits.maxDays = item.maxdays ?? item.MAXDAYS ?? 0;
        const rawExpiry = item.expiryDATE ?? item.expiryDate ?? null;
        memberLimits.expiryDate = rawExpiry ? new Date(rawExpiry) : null;

        // store member id for save
        MEMBER_ID = item.libraryMemberID ?? MEMBER_ID;
        window.CURRENT_MEMBER_ID = MEMBER_ID;

        applyMemberLimitUI();

        detailCard.style.display = 'block';
    }

    function hideDetailCard() {
        if (detailCard) detailCard.style.display = 'none';
    }

    function hideList() {
        list.style.display = 'none';
        list.innerHTML = '';
    }

    function getVal(item, ...keys) {
        for (const k of keys) {
            if (item[k] !== undefined && item[k] !== null && item[k] !== '') return item[k];
        }
        return null;
    }

    function getName(item) {
        const first = getVal(item, 'student_FirstName', 'staff_FirstName', 'firstName');
        const last = getVal(item, 'student_LastName', 'staff_LastName', 'lastName');
        if (!first && !last) return null;
        return `${first ?? ''} ${last ?? ''}`.trim();
    }
}

// ========================================
// DOMContentLoaded — init DataTable + UI
// ========================================
document.addEventListener('DOMContentLoaded', () => {

    // ── DataTable (export only) ───────────────────────────────────────
    if ($.fn.DataTable.isDataTable('#tblIssuebook')) {
        $('#tblIssuebook').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#tblIssuebook').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4] } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3, 4] } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3, 4] } }
        ],
        searching: false,
        paging: false,
        info: false,
        ordering: false
    });

    // ── Select2 ───────────────────────────────────────────────────────
    try {
        if (window.jQuery && typeof jQuery.fn.select2 === 'function') {
            const $companyEl = jQuery('#ddlFilterCompany');
            if ($companyEl.length) {
                $companyEl.select2({
                    width: '100%',
                    dropdownParent: jQuery('#filter-dropdown'),
                    allowClear: true,
                    placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
                });
            }

            const $bookEl = jQuery('#bookSelect');
            if ($bookEl.length) {
                $bookEl.select2({
                    width: '100%',
                    dropdownParent: jQuery('#issueBookForm'),
                    allowClear: true,
                    placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
                });
            }
        }
    } catch (e) {
        console.warn('Select2 init skipped:', e);
    }

    // ── Keep filter dropdown open while interacting inside ────────────
    document.getElementById('filter-dropdown')?.addEventListener('click', e => e.stopPropagation());

    // ── Apply Filters ─────────────────────────────────────────────────
    document.getElementById('btnApplyFilters')?.addEventListener('click', () => {
        document.getElementById('hdnPageIndex').value = 1;
        document.getElementById('hdnSearch').value = document.getElementById('txtSearchInput').value;
        applyFilters();
        submitForm();
    });

    // ── Reset Filters ─────────────────────────────────────────────────
    document.getElementById('btnResetFilters')?.addEventListener('click', () => {
        document.getElementById('txtSearchInput').value = '';
        document.getElementById('hdnSearch').value = '';
        ['ddlFilterCompany'].forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        });
        clearAppliedFilters();
        document.getElementById('hdnPageIndex').value = 1;
        submitForm();
    });

    // ── Render badges on load ─────────────────────────────────────────
    renderFilterBadges();

    // ── Member card autocomplete ────────────────────────────────────
    initMemberAutocomplete();

    // ── Focus accession input on load ─────────────────────────────────
    els.accessionNo?.focus();
});

// ========================================
// Search input — debounced server submit
// ========================================
let searchTimer = null;

document.getElementById('txtSearchInput')?.addEventListener('input', function () {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => {
        document.getElementById('hdnSearch').value = this.value;
        document.getElementById('hdnPageIndex').value = 1;

        const val = this.value.trim();
        if (val) appliedFilters['txtSearchInput'] = { label: 'Search', text: val };
        else delete appliedFilters['txtSearchInput'];
        saveAppliedFilters();

        submitForm();
    }, 500);
});

// ========================================
// Return-book modal
// ========================================
let selectedIssueId = 0;

$(document).ready(function () {
    $('#returnBookForm').on('submit', function (e) {
        e.preventDefault();
        $.post('/Library/ReturnBook', { issueId: selectedIssueId, returnDate: $('#returnDate').val() }, function (res) {
            if (res.success) {
                var hdnLibraryMemberID = $("#hdnLibraryMemberID").val();
                Swal.fire({
                    icon: 'success',
                    title: 'Saved!',
                    text: res.message,
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-success' },
                    buttonsStyling: false
                }).then(() => {
                    window.location.href = '/Library/Issue/' + hdnLibraryMemberID;
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: res.message || 'Failed to save class.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-danger' },
                    buttonsStyling: false
                });
            }
        });
    });
});