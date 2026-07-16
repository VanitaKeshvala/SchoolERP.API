/*****************************************************************************************
  SET DOCUMENT STATUS SCREEN - SEARCH/SCAN-TO-AUTOFILL + STATUS-ONLY SAVE

  Flow:
    1. Staff can either scan a barcode (fast typist + Enter, same as before) OR
       type 2+ characters into Accession No to get a live search dropdown.
    2. Once an accession is resolved (via scan or dropdown pick):
         - Book Title / Sub Title / Location / Accession No are locked (read-only).
         - Document Status is the ONLY field that becomes editable.
         - Barcode renders for that accession no.
         - If the copy is currently issued, a warning banner shows and Save
           stays disabled (mirrors the new guard in
           SP_LIBRARY_ACCESSION_STATUS_UPDATE, which rejects the save server-side
           too — this is just so staff don't fill the form out for nothing).
    3. "Clear" (the X button next to Accession No) resets everything so a
       different copy can be searched/scanned.

  Wire-up:
    - GET  /Library/SearchAccession?searchText=..
           -> same endpoint already used on the Issue Book page; must include
              an `isAvailable` (or equivalent "not currently issued") flag per
              row so this screen can warn before Save.
    - GET  /Library/GetAccessionById?accessionNo=..
           -> thin controller action calling SP_LIBRARY_ACCESSION_GET_BY_NO,
              returns { success, data: { accessionId, accessionNo, bookTitle,
                        subTitle, location, documentStatusId, isAvailable } }
           *** isAvailable needs to be added to this response if not already
               present, so scan-then-Enter gets the same live warning as the
               dropdown-search path. ***
    - POST /Library/UpdateAccessionStatus
           body: { accessionId, documentStatusId }
           -> thin controller action calling SP_LIBRARY_ACCESSION_STATUS_UPDATE
              (server fills userId/ipAddress from the logged-in session, never
              trust those from the client)
    - Requires JsBarcode already loaded on the page.
*****************************************************************************************/

'use strict';

const els = {
    accessionNo: document.getElementById('txtAccessionNo'),
    itemId: document.getElementById('txtItemId'),
    bookTitle: document.getElementById('txtBookTitle'),
    subTitle: document.getElementById('txtSubTitle'),
    location: document.getElementById('txtLocation'),
    status: document.getElementById('ddlDocumentStatus'),
    errAccessionNo: document.getElementById('errtxtAccessionNo'),
    errStatus: document.getElementById('errddlDocumentStatus'),
    issuedWarning: document.getElementById('issuedWarning'),
    btnSave: document.getElementById('btnSaveStatus'),
    btnClear: document.getElementById('btnClearAccession'),
    suggestions: document.getElementById('accessionSuggestions'),
};

const COMPANY_ID = window.CURRENT_COMPANY_ID; // set globally, e.g. <script>window.CURRENT_COMPANY_ID = @Model.CompanyId;</script>

let isCurrentlyIssued = false;

function renderBarcode(accessionNo) {
    const box = document.getElementById('barcodeBox');
    if (!accessionNo) {
        document.getElementById('barcodeSvg').innerHTML = '';
        if (box) box.style.display = 'none';
        return;
    }
    JsBarcode('#barcodeSvg', accessionNo, { format: 'CODE128', displayValue: true, height: 40 });
    if (box) box.style.display = 'inline-block';
}

function setError(el, errEl, message) {
    el.classList.toggle('is-invalid', !!message);
    if (errEl) errEl.textContent = message || '';
}

// ── Lock/unlock the read-only lookup fields vs. the status dropdown ───────
function lockFieldsForStatusOnlyEdit(locked) {
    els.accessionNo.readOnly = locked;
    if (locked) els.suggestions.style.display = 'none';
}

function setStatusEnabled(enabled) {
    els.status.disabled = !enabled;
    if (window.jQuery && jQuery.fn.select2) {
        jQuery(els.status).trigger('change.select2');
    }
    if (els.btnSave) els.btnSave.disabled = !enabled;
}

function showIssuedWarning(show) {
    isCurrentlyIssued = show;
    if (els.issuedWarning) els.issuedWarning.style.display = show ? 'block' : 'none';
    // Status stays visible/enterable-looking but Save is blocked while issued
    if (els.btnSave) els.btnSave.disabled = show;
}

function clearFields() {
    els.itemId.value = 0;
    els.bookTitle.value = '';
    els.subTitle.value = '';
    els.location.value = '';
    if (window.jQuery && jQuery.fn.select2) {
        $(els.status).val('').trigger('change');
    } else {
        els.status.value = '';
    }
    renderBarcode('');
    setStatusEnabled(false);
    showIssuedWarning(false);
}

function resetToSearch() {
    clearFields();
    els.accessionNo.value = '';
    lockFieldsForStatusOnlyEdit(false);
    setError(els.accessionNo, els.errAccessionNo, '');
    els.accessionNo.disabled = false;
    els.accessionNo.focus();
}

function applyResolvedAccession(data) {
    // data: { accessionId, accessionNo, bookTitle, subTitle, location, documentStatusId, isAvailable }
    els.itemId.value = data.accessionId;
    els.accessionNo.value = data.accessionNo ?? els.accessionNo.value;
    els.bookTitle.value = data.bookTitle || '';
    els.subTitle.value = data.subTitle || '';
    els.location.value = data.location || '';

    if (window.jQuery && jQuery.fn.select2) {
        $(els.status).val(data.documentStatusId ?? '').trigger('change');
    } else {
        els.status.value = data.documentStatusId ?? '';
    }

    renderBarcode(data.accessionNo);
    lockFieldsForStatusOnlyEdit(true);

    const issued = data.isAvailable === false; // isAvailable === true means NOT issued
    showIssuedWarning(issued);
    setStatusEnabled(!issued);
    setError(els.accessionNo, els.errAccessionNo, '');
}

// ============================================================
// Scan (Enter key) lookup — resolves a single exact accession no
// ============================================================
async function lookupAccession(accessionNo) {
    if (!accessionNo) return;

    setError(els.accessionNo, els.errAccessionNo, '');
    els.accessionNo.disabled = true;
    hideSuggestions();

    try {
        const resp = await fetch(
            `/Library/GetAccessionById?accessionNo=${encodeURIComponent(accessionNo)}`
        );
        const data = await resp.json();

        if (!data.success) {
            clearFields();
            setError(els.accessionNo, els.errAccessionNo, data.MESSAGE || data.message || 'No copy found with this accession no');
            return;
        }

        applyResolvedAccession(data.data);
    } catch (err) {
        console.error('Accession lookup failed', err);
        setError(els.accessionNo, els.errAccessionNo, 'Lookup failed. Please try again.');
    } finally {
        els.accessionNo.disabled = false;
        els.accessionNo.focus();
    }
}

// A barcode scanner acts like a fast typist + Enter key -- listen for Enter
els.accessionNo.addEventListener('keydown', function (e) {
    if (e.key !== 'Enter') return;
    e.preventDefault();
    lookupAccession(this.value.trim());
});

// ============================================================
// Type-to-search dropdown (same endpoint as Issue Book page)
// ============================================================
let searchDebounce;
let searchItems = [];

els.accessionNo.addEventListener('input', function () {
    const term = this.value.trim();
    clearTimeout(searchDebounce);

    if (parseInt(els.itemId.value, 10) > 0) {
        // A resolved accession was already loaded; typing again means they
        // want a different one, so drop the current selection first.
        clearFields();
    }

    if (term.length < 2) { hideSuggestions(); return; }
    searchDebounce = setTimeout(() => fetchAccessions(term), 300);
});

document.addEventListener('click', (e) => {
    if (!els.accessionNo.contains(e.target) && !els.suggestions.contains(e.target)) hideSuggestions();
});

async function fetchAccessions(term) {
    try {
        const res = await fetch(`/Library/SearchAccession?searchText=${encodeURIComponent(term)}`);
        const result = await res.json();
        if (result.success && result.data && result.data.length > 0) {
            searchItems = result.data;
            renderSuggestions(searchItems);
        } else {
            hideSuggestions();
        }
    } catch (err) {
        console.error('SearchAccession error:', err);
        hideSuggestions();
    }
}

function renderSuggestions(items) {
    els.suggestions.innerHTML = '';
    items.forEach((item, idx) => {
        const li = document.createElement('li');
        li.className = 'list-group-item list-group-item-action';
        const issuedTag = item.isAvailable
            ? ''
            : ' <span class="badge bg-danger-subtle text-danger ms-1">Issued</span>';
        li.innerHTML = `<strong>${item.accessionNo ?? '-'}</strong> &nbsp;-&nbsp; ${item.bookTitle ?? ''}${issuedTag}`;
        li.addEventListener('click', () => selectSuggestion(idx));
        els.suggestions.appendChild(li);
    });
    els.suggestions.style.display = 'block';
}

function selectSuggestion(idx) {
    const item = searchItems[idx];
    if (!item) return;
    applyResolvedAccession(item);
    hideSuggestions();
}

function hideSuggestions() {
    els.suggestions.style.display = 'none';
    els.suggestions.innerHTML = '';
}

els.btnClear?.addEventListener('click', resetToSearch);

// ============================================================
// Validation + Save (status field only)
// ============================================================
function validate() {
    let ok = true;

    if (!els.accessionNo.value.trim() || !parseInt(els.itemId.value, 10)) {
        setError(els.accessionNo, els.errAccessionNo, 'Scan/select a valid accession no first');
        ok = false;
    }
    if (!$(els.status).val()) {
        setError(els.status, els.errStatus, 'Document Status is required');
        ok = false;
    }
    return ok;
}

async function saveStatus() {
    if (isCurrentlyIssued) {
        Swal.fire({
            icon: 'warning',
            title: 'Currently Issued',
            text: 'This copy is issued to a member. Return it before changing its status.',
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn btn-warning' },
            buttonsStyling: false
        });
        return;
    }

    if (!validate()) return;

    const accessionId = parseInt(els.itemId.value, 10);
    if (!accessionId) {
        setError(els.accessionNo, els.errAccessionNo, 'Scan/select a valid accession no first');
        return;
    }

    if (els.btnSave) { els.btnSave.disabled = true; els.btnSave.dataset.originalHtml = els.btnSave.innerHTML; els.btnSave.innerHTML = '<i class="ti ti-loader-2 me-1"></i> Saving...'; }

    try {
        const dataReq = {
            accessionId: accessionId,
            documentStatusId: parseInt($(els.status).val(), 10)
        };
        const resp = await fetch('/Library/UpdateAccessionStatus', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataReq),
        });
        const res = await resp.json();

        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'Status has been updated successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = '/Library/Accession';
            });
        } else {
            // Covers the server-side "currently issued" rejection too, in case
            // it changed status between load and save.
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to update status.',
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
    } finally {
        if (els.btnSave) { els.btnSave.disabled = isCurrentlyIssued; els.btnSave.innerHTML = els.btnSave.dataset.originalHtml || els.btnSave.innerHTML; }
    }
}

// ============================================================
// Initial load — if the page opened with an existing accession
// (edit mode, hdnAccessionNo already set server-side), resolve it.
// ============================================================
document.addEventListener('DOMContentLoaded', () => {
    setStatusEnabled(false);
    const existingAccessionNo = document.getElementById('hdnAccessionNo')?.value;
    if (existingAccessionNo) {
        els.accessionNo.value = existingAccessionNo;
        lookupAccession(existingAccessionNo);
    }
});