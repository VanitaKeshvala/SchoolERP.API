function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#sectionsTable')) {
        const table = $('#sectionsTable').DataTable();
        table.button(index).trigger();
    }
}
// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_class';
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
    // Sync hidden form fields from dropdowns before submit
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const searchEl = document.getElementById('txtSearchInput');

    // Search
    const searchVal = searchEl?.value.trim();
    if (searchVal) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    else delete appliedFilters['txtSearchInput'];

    // Session
    if (sessionEl?.value && sessionEl.value !== '') {
        appliedFilters['ddlFilterSessions'] = {
            label: 'Session',
            text: sessionEl.options[sessionEl.selectedIndex]?.text || sessionEl.value
        };
    } else delete appliedFilters['ddlFilterSessions'];

    // Company
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
    // Clear the matching dropdown/input
    if (filterId === 'ddlFilterSessions') {
        const el = document.getElementById('ddlFilterSessions');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    } else if (filterId === 'ddlFilterCompany') {
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

    ['ddlFilterSessions', 'ddlFilterCompany'].forEach(id => {
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
// DOMContentLoaded — init DataTable + UI
// ========================================
document.addEventListener('DOMContentLoaded', () => {

    // ── DataTable (export only) ───────────────────────────────────────
    if ($.fn.DataTable.isDataTable('#sectionsTable')) {
        $('#sectionsTable').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#sectionsTable').DataTable({
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
            jQuery('#ddlFilterSessions, #ddlFilterCompany').select2({
                width: '100%',
                dropdownParent: jQuery('#filter-dropdown'),
                allowClear: true,
                placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
            });
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
        ['ddlFilterSessions', 'ddlFilterCompany'].forEach(id => {
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

        // ── Sync search into appliedFilters before submit ─────────────
        const val = this.value.trim();
        if (val) appliedFilters['txtSearchInput'] = { label: 'Search', text: val };
        else delete appliedFilters['txtSearchInput'];
        saveAppliedFilters(); // ← persist before submit

        submitForm();
    }, 500);
});
const permCanAdd = window.canAdd;
const permCanEdit = window.canEdit;
const permCanDelete = window.canDelete;
const IV = window.InlineFormValidation;
const sectionFieldMap = { txtSectionName: 'errTxtSectionName' };
let selectedSectionId = 0;
async function saveItem() {
    const sid = parseInt(document.getElementById('txtItemId').value);
    if (sid === 0 && !permCanAdd) {
        IV.setNotice('sectionModalNotice', 'You do not have permission to add.');
        return;
    }
    if (sid > 0 && !permCanEdit) {
        IV.setNotice('sectionModalNotice', 'You do not have permission to edit.');
        return;
    }
    clearSectionModalValidation();
    const name = document.getElementById('txtSectionName').value.trim();
    if (!name) {
        IV.setFieldError('txtSectionName', 'errTxtSectionName', 'Section name is required.');
        return;
    }

    const data = {
        sectionID: parseInt(document.getElementById('txtItemId').value),
        sectionName: name,
        isActive: document.getElementById('isActive').checked
    };

    try {
        const resp = await fetch('/Section/Save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const res = await resp.json();
        if (res.success) { location.href = `/Section/Index`; }
        else IV.setNotice('sectionModalNotice', res.message || 'Save failed.');
    } catch (err) {
        console.error(err);
        IV.setNotice('sectionModalNotice', 'Could not save section.');
    }
}

function clearSectionModalValidation() {
    IV.clearMap(sectionFieldMap);
    IV.clearNotice('sectionModalNotice');
}

IV.bindAutoClear(sectionFieldMap);

function selectItem(id, row) {
    IV.clearNotice('sectionPageNotice');
    selectedSectionId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    row.classList.add('bg-light');
    //row.querySelector('input[type="radio"]').checked = true;
    const btnEdit = document.getElementById('btnEdit');
    const btnDelete = document.getElementById('btnDelete');
    if (btnEdit && permCanEdit) btnEdit.disabled = false;
    if (btnDelete && permCanDelete) btnDelete.disabled = false;
    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    // Edit sirf 1 record select hone par
    $('#btnEdit').prop('disabled', checkedCount !== 1);
    $('#btnActive').prop('disabled', checkedCount === 0);
    $('#btnInactive').prop('disabled', checkedCount === 0);
    $('#btnCopy').prop('disabled', checkedCount === 0);

}

function openAddModal() {
    if (!permCanAdd) {
        IV.setNotice('sectionPageNotice', 'You do not have permission to add.');
        return;
    }
    IV.clearNotice('sectionPageNotice');
    clearSectionModalValidation();
    document.getElementById('modalTitle').textContent = 'Add Section';
    document.getElementById('itemForm').reset();
    document.getElementById('txtItemId').value = "0";
    document.getElementById('isActive').checked = true;
    
}

async function editSelected() {
    if (!permCanEdit) {
        IV.setNotice('sectionPageNotice', 'You do not have permission to edit.');
        return;
    }

    if (selectedSectionId === 0) return;
    IV.clearNotice('sectionPageNotice');
    clearSectionModalValidation();
    try {

        if (selectedSectionId) {
            location.href = `/Section/Add/${selectedSectionId}`;
        }

    } catch (err) {
        console.error(err);
        IV.setNotice('sectionPageNotice', 'Could not load section details.');
    }
}

async function deleteSelected() {
    if (!permCanDelete) {
        IV.setNotice('sectionPageNotice', 'You do not have permission to delete.');
        return;
    }


    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one section.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this section record. This action cannot be undone!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it!',
        customClass: {
            confirmButton: 'btn btn-danger me-2',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Section/Delete',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'section record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete section.', 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error!', 'An unexpected error occurred.', 'error');
                }
            });
        }
    });
}

async function toggleStatus(id, isActive) {

    if (!permCanEdit) {
        IV.setNotice('sectionPageNotice', 'You do not have permission to change status.');
        if (window.toast) window.toast.error('You do not have permission to change status.');
        location.reload();
        return;
    }

    let sectionIds = [];
    $('.student-checkbox:checked').each(function () {
        sectionIds.push(parseInt($(this).val()));
    });

    if (sectionIds.length === 0) {
        if (id != 0) {
            sectionIds.push(parseInt(id));
        }
        else {
            showToast('Please select at least one user', false);
            return;
        }

    }

    const request = {
        ids: sectionIds.join(','),
        isActive: isActive
    };
    try {
        const config = isActive
            ? {
                icon: 'question',
                title: 'Activate Record(s)?',
                text: 'Activating this record will make it visible and available for use across the system. Do you want to continue?',
                confirmText: 'Yes, Activate',
                confirmBtnClass: 'btn btn-success me-2',
                successTitle: 'Activated Successfully!',
                successText: 'The selected record(s) have been activated and are now available for use.'
            }
            : {
                icon: 'warning',
                title: 'Deactivate Record(s)?',
                text: 'Deactivating this record will hide it from the system and prevent it from being used. Do you want to continue?',
                confirmText: 'Yes, Deactivate',
                confirmBtnClass: 'btn btn-danger me-2',
                successTitle: 'Deactivated Successfully!',
                successText: 'The selected record(s) have been deactivated and are no longer available for use.'
            };


        Swal.fire({
            icon: config.icon,
            title: config.title,
            text: config.text,
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, proceed!',
            customClass: {
                confirmButton: 'btn btn-danger me-2',
                cancelButton: 'btn btn-secondary'
            },
            buttonsStyling: false
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    // ✅ fixed typo bluk → bulk
                    url: '/Section/ToggleStatus',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(request),
                    success: function (res) {
                        if (res.success) {
                            Swal.fire('Updated!', res.message || 'Status updated successfully.', 'success')
                                .then(() => location.reload());
                        } else {
                            Swal.fire('Error!', res.message || 'Failed to update status.', 'error');
                        }
                    },
                    error: function (xhr) {
                        // ✅ log xhr for debugging
                        console.error('Status update error:', xhr.status, xhr.responseText);
                        Swal.fire('Error!', 'An unexpected error occurred.', 'error');
                    }
                });
            }
        });
    }
    catch (err) {
        console.error(err);
        IV.setNotice('sectionPageNotice', 'Could not update status.');
        location.reload();
    }
}

let exportTable;
$(document).ready(function () {
    if ($.fn.DataTable.isDataTable('#sectionsTable')) {
        $('#sectionsTable').DataTable().destroy();
    }
    exportTable = $('#sectionsTable').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2], rows: ':visible' } },
            { extend: 'csv', exportOptions: { columns: [1, 2], rows: ':visible' } },
            { extend: 'excel', exportOptions: { columns: [1, 2], rows: ':visible' } },
            { extend: 'pdf', exportOptions: { columns: [1, 2], rows: ':visible' } },
            { extend: 'print', exportOptions: { columns: [1, 2], rows: ':visible' } }
        ],
        searching: false,
        paging: false,
        info: false,
        ordering: false
    });
});

function triggerExport(type) {
    if (!exportTable) return;
    if (type === 'copy') exportTable.button('.buttons-copy').trigger();
    else if (type === 'csv') exportTable.button('.buttons-csv').trigger();
    else if (type === 'excel') exportTable.button('.buttons-excel').trigger();
    else if (type === 'pdf') exportTable.button('.buttons-pdf').trigger();
    else if (type === 'print') exportTable.button('.buttons-print').trigger();
}

// ── COPY TO SESSION ───────────────────────────────────────────────────────

// ── COPY TO SESSION ───────────────────────────────────────────────────────
async function openCopyModal() {
    const ids = getSelectedIds();
    const names = getSelectedNames();

    if (ids.length === 0) {
        Swal.fire({
            icon: 'info',
            title: 'No Selection',
            text: 'Please select at least one section to copy.',
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn btn-primary' },
            buttonsStyling: false
        });
        return;
    }

    // ── Show selected section badges ──────────────────────────────────
    const badgeContainer = document.getElementById('copySelectedBadges');
    badgeContainer.innerHTML = names
        .map(n => `<span class="badge bg-primary-subtle text-primary border border-primary-subtle">${n}</span>`)
        .join('');

    // ── Populate session dropdown ─────────────────────────────────────
    const ddl = document.getElementById('ddlTargetSession');
    ddl.innerHTML = '<option value="">Loading sessions...</option>';
    ddl.disabled = true;

    try {
        // ✅ Both functions come from window — defined in layout
        const [sessions, currentSessionId] = await Promise.all([
            window.fetchSessions(),
            window.fetchUserCurrentSession()
        ]);

        ddl.innerHTML = '<option value="">-- Select Session --</option>';

        const filtered = sessions.filter(s =>
            (s.sessionId || s.sessionId) !== currentSessionId
        );

        if (filtered.length === 0) {
            ddl.innerHTML = '<option value="">No other sessions available</option>';
        } else {
            filtered.forEach(s => {
                const id = s.sessionId ?? s.sessionId;
                const name = s.sessionTitle ?? s.sessionTitle;
                ddl.innerHTML += `<option value="${id}">${name}</option>`;
            });
        }

        ddl.disabled = false;

    } catch (err) {
        console.error('Failed to load sessions:', err);
        ddl.innerHTML = '<option value="">Failed to load sessions</option>';
        ddl.disabled = false;
    }

    // reset validation
    ddl.classList.remove('is-invalid');

    // open modal
    new bootstrap.Modal(document.getElementById('modalCopySession')).show();
}

function confirmCopy() {
    const ids = getSelectedIds();
    const targetSession = document.getElementById('ddlTargetSession').value;
    const ddl = document.getElementById('ddlTargetSession');

    if (!targetSession) {
        ddl.classList.add('is-invalid');
        return;
    }
    ddl.classList.remove('is-invalid');

    const selectedName = ddl.options[ddl.selectedIndex].text;

    Swal.fire({
        icon: 'question',
        title: 'Confirm Copy',
        text: `Copy ${ids.length} section(s) to "${selectedName}"?`,
        showCancelButton: true,
        confirmButtonText: 'Yes, Copy',
        cancelButtonText: 'Cancel',
        customClass: {
            confirmButton: 'btn btn-primary me-2',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then(result => {
        if (!result.isConfirmed) return;

        // ── Show loading ─────────────────────────────────────────────
        Swal.fire({
            title: 'Copying...',
            text: 'Please wait while sections are being copied.',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => Swal.showLoading()
        });

        $.ajax({
            url: `/Section/CopyToSession`,
            type: 'POST',
            contentType: 'application/json',
            headers: { 'Authorization': 'Bearer ' + window.jwtToken },
            data: JSON.stringify({
                sectionIds: ids.join(','),
                targetSessionId: parseInt(targetSession)
            }),
            success: function (res) {
                bootstrap.Modal.getInstance(
                    document.getElementById('modalCopySession')).hide();

                if (res.success) {
                    let msg = res.message;
                    Swal.fire({
                        icon: 'success',
                        title: 'Copied Successfully!',
                        text: msg,
                        confirmButtonText: 'OK',
                        customClass: { confirmButton: 'btn btn-primary' },
                        buttonsStyling: false
                    }).then(() => location.reload());
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Copy Failed',
                        text: res.message || 'Unable to copy sections.',
                        confirmButtonText: 'OK',
                        customClass: { confirmButton: 'btn btn-danger' },
                        buttonsStyling: false
                    });
                }
            },
            error: function (xhr) {
                console.error(xhr.status, xhr.responseText);
                Swal.fire({
                    icon: 'error',
                    title: 'Unexpected Error',
                    text: 'Something went wrong. Please try again.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-danger' },
                    buttonsStyling: false
                });
            }
        });
    });
}

// ── HELPERS ───────────────────────────────────────────────────────────────

function getSelectedIds() {
    let ids = [];
    $('.student-checkbox:checked').each(function () {
        ids.push(parseInt($(this).val()));
    });
    return ids;
}

function getSelectedNames() {
    let names = [];
    $('.student-checkbox:checked').each(function () {
        // get section name from same row's td
        names.push($(this).closest('tr').find('td:eq(1)').text().trim());
    });
    return names;
}

// ── Update copy button state with other buttons ───────────────────────────
// Add this inside your existing checkbox change handler
function updateButtonStates() {
    const count = $('.student-checkbox:checked').length;
    const single = count === 1;
    const any = count > 0;

    $('#btnEdit').prop('disabled', !single);
    $('#btnDelete').prop('disabled', !any);
    $('#btnActive').prop('disabled', !any);
    $('#btnInactive').prop('disabled', !any);
    $('#btnCopy').prop('disabled', !any);   // ← enable copy button
}