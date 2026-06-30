
// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_hotel';
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
    const roolTypeEl = document.getElementById('ddlFilterRoolType');
    const searchEl = document.getElementById('txtSearchInput');   

    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (roolTypeEl) document.getElementById('hdnRoomTypeID').value = roolTypeEl.value;
    if (searchEl) document.getElementById('hdnSearch').value = searchEl.value.trim();
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const roolTypeEl = document.getElementById('ddlFilterRoolType');
    const searchEl = document.getElementById('txtSearchInput');

    // Search
    const searchVal = searchEl?.value.trim() ?? '';
    document.getElementById('hdnSearch').value = searchVal;     

    if (searchVal) {
        appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    } else {
        delete appliedFilters['txtSearchInput'];
    }

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

    // Rool Type
    if (roolTypeEl?.value && roolTypeEl.value !== '') {
        appliedFilters['ddlFilterRoolType'] = {
            label: 'Rool Type',
            text: roolTypeEl.options[roolTypeEl.selectedIndex]?.text || roolTypeEl.value
        };
    } else delete appliedFilters['ddlFilterRoolType'];

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
    } else if (filterId === 'ddlFilterRoolType') {
        const el = document.getElementById('ddlFilterRoolType');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'txtSearchInput') {
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

    // all three in one selector string
    jQuery('#ddlFilterSessions, #ddlFilterCompany, #ddlFilterRoolType').select2({
        width: '100%',
        dropdownParent: jQuery('#filter-dropdown'),
        allowClear: true,
        placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
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
    if ($.fn.DataTable.isDataTable('#tblRoomType')) {
        $('#tblRoomType').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#tblRoomType').DataTable({
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
            jQuery('#ddlFilterSessions, #ddlFilterCompany','#ddlFilterRoolType').select2({
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
        ['ddlFilterSessions', 'ddlFilterCompany','ddlFilterRoolType'].forEach(id => {
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


let selectedId = 0;
window.selectedId = 0;
const canEdit = window.canEdit;
const canDelete = window.canDelete;

function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#c')) {
        const table = $('#tblHostel').DataTable();
        table.button(index).trigger();
    }
}

$(document).ready(function () {
    $('#tblHostel').DataTable({
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
        info: false
    });
})

// ✅ Auto-apply filter as user types (300ms debounce)
const searchInput = document.getElementById('txtSearchInput');
if (searchInput) {
    let debounceTimer = null;

    searchInput.addEventListener('input', function () {
        const val = this.value.trim();

        // Update hidden field immediately
        document.getElementById('hdnSearch').value = val;

        // Update applied filters badge
        if (val) {
            appliedFilters['txtSearchInput'] = { label: 'Search', text: val };
        } else {
            delete appliedFilters['txtSearchInput'];
        }
        saveAppliedFilters();

        // Debounce — wait 300ms after user stops typing, then submit
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () {
            document.getElementById('hdnPageIndex').value = 1;
            submitForm();
        }, 300);
    });

    // Enter key — submit immediately without waiting for debounce
    searchInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            clearTimeout(debounceTimer);
            document.getElementById('hdnSearch').value = this.value.trim();
            document.getElementById('hdnPageIndex').value = 1;
            applyFilters();
            submitForm();
        }
    });
}

function selectItem(id, row) {
    selectedId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    window.selectedId = id;
    row.classList.add('bg-light');
    //row.querySelector('input[type="radio"]').checked = true;

    if (canEdit) document.getElementById('btnEdit').disabled = false;
    if (canDelete) document.getElementById('btnDelete').disabled = false;
    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    // Edit sirf 1 record select hone par
    $('#btnEdit').prop('disabled', checkedCount !== 1);
    $('#btnActive').prop('disabled', checkedCount === 0);
    $('#btnInactive').prop('disabled', checkedCount === 0);
}


function openAddModal() {
    document.getElementById('modalTitle').textContent = 'Add Hostel';
    document.getElementById('hdnHostelID').value = 0;
    document.getElementById('hostelForm').reset();
    document.getElementById('chkActive').checked = true;

    // Reset Select2
    $('#ddlRoomType').val('').trigger('change');

    new bootstrap.Modal(document.getElementById('hostelModal')).show();
}

function editSelected() {
    if (!window.selectedId) return;
    try {

        if (window.selectedId) {
            location.href = `/Hostel/AddHostel/${window.selectedId}`;
        }

    } catch (err) {
        console.error(err);
    }
}

async function saveRecord() {
    const form = document.getElementById('hostelForm');
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const hostelID = parseInt(document.getElementById('hdnHostelID').value);
    if (hostelID === 0 && !permCanAdd) {
        IV.setNotice('classModalNotice', 'You do not have permission to add.');
        return;
    }
    const data = {
        hostelID: hostelID || 0,
        hostelName: document.getElementById('txtHostelName').value.trim(),
        roomTypeID: parseInt(document.getElementById('ddlRoomType').value),
        hostelIntake: parseInt(document.getElementById('txtIntake').value),
        hostelAddress: document.getElementById('txtAddress').value.trim() || null,
        hostelDescription: document.getElementById('txtDescription').value.trim() || null,
        isActive: document.getElementById('chkActive').checked,
        hostelTypeID: document.getElementById('ddlHostelType').value.trim() || null,
        hostelCode: document.getElementById('txtHostelCode').value.trim() || null,
        wardenName: document.getElementById('txtWardenName').value.trim() || null,
        wardenContact: document.getElementById('txtWardenContact').value.trim() || null,
        emergencyContact: document.getElementById('txtEmergencyContact').value.trim() || null,
        hostelEmail: document.getElementById('txtHostelEmail').value.trim() || null,
        hostelRules: document.getElementById('txtHostelRules').value.trim() || null
    };


    try {

        const r = await fetch('/Hostel/UpsertHostel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        const res = await r.json();
        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'Hostel has been added successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = '/Hostel/Index';
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save hostel.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
        }

    } catch (err) {
        console.error(err);
        IV.setNotice('classModalNotice', 'Could not save hostel.');
    }      
}

function deleteSelected() {
    if (!selectedId) return;

    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one student.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this student record. This action cannot be undone!",
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
                url: '/Hostel/DeleteHostel',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Student record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete student.', 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error!', 'An unexpected error occurred.', 'error');
                }
            });
        }
    });
}

function toggleStatus(id, isActive) {


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
                    // fixed typo bluk → bulk
                    url: '/Hostel/ToggleHostelStatus',
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
                        // log xhr for debugging
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

function showToast(msg, type = 'success') {
    const wrapper = document.createElement('div');
    wrapper.innerHTML = `<div class="toast align-items-center text-bg-${type} border-0 show position-fixed bottom-0 end-0 m-3" role="alert" style="z-index:9999">
            <div class="d-flex"><div class="toast-body">${msg}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div></div>`;
    document.body.appendChild(wrapper);
    setTimeout(() => wrapper.remove(), 3000);
}