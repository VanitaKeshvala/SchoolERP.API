
// ========================================
// Export (DataTable button trigger)
// ========================================
function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#tblDepartment')) {
        $('#tblDepartment').DataTable().button(index).trigger();
    }
}

// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_department';
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
    const countryEl = document.getElementById('ddlFilterSessions');
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (countryEl) document.getElementById('hdnSessionId').value = countryEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const companyEl = document.getElementById('ddlFilterCompany');
    const countryEl = document.getElementById('ddlFilterSessions');
    const searchEl = document.getElementById('txtSearchInput');

    const searchVal = searchEl?.value.trim();
    if (searchVal) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    else delete appliedFilters['txtSearchInput'];

    if (companyEl?.value) {
        appliedFilters['ddlFilterCompany'] = { label: 'Company', text: companyEl.options[companyEl.selectedIndex]?.text || companyEl.value };
    } else delete appliedFilters['ddlFilterCompany'];

    if (countryEl?.value) {
        appliedFilters['ddlFilterSessions'] = { label: 'Session', text: countryEl.options[countryEl.selectedIndex]?.text || countryEl.value };
    } else delete appliedFilters['ddlFilterSessions'];


    saveAppliedFilters();
}

function renderFilterBadges() {
    const container = document.getElementById('badgeContainer');
    const mainContainer = document.getElementById('activeFilterBadges');
    if (!container || !mainContainer) return;

    container.innerHTML = '';
    Object.entries(appliedFilters).forEach(([id, { label, text }]) => {
        const badge = document.createElement('span');
        badge.className = 'badge bg-primary-subtle text-primary border border-primary-subtle d-flex align-items-center gap-1 fw-medium px-2 py-1';
        badge.innerHTML = `${label}: ${text} <i class="ti ti-x ms-1 cursor-pointer" onclick="removeFilter('${id}')" style="font-size:10px;"></i>`;
        container.appendChild(badge);
    });
    mainContainer.style.display = Object.keys(appliedFilters).length > 0 ? 'block' : 'none';
}

function resetSelect2(id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.value = '';
    if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
}

function removeFilter(filterId) {
    if (filterId === 'ddlFilterSessions') {
        resetSelect2('ddlFilterSessions');
        resetSelect2('ddlFilterState'); // clearing country clears dependent state too
    } else if (filterId === 'ddlFilterState') {
        resetSelect2('ddlFilterState');
    } else if (filterId === 'ddlFilterCompany') {
        resetSelect2('ddlFilterCompany');
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
    const search = document.getElementById('txtSearchInput');
    if (search) search.value = '';
    document.getElementById('hdnSearch').value = '';

    ['ddlFilterCompany', 'ddlFilterSessions'].forEach(resetSelect2);

    clearAppliedFilters();
    document.getElementById('hdnPageIndex').value = 1;
    submitForm();
}

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

let selectedId = 0;
const canEdit = window.canEdit;
const canDelete = window.canDelete;

$(document).ready(function () {
    $('#tblDepartment').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1] } },
            { extend: 'csv', exportOptions: { columns: [1] } },
            { extend: 'excel', exportOptions: { columns: [1] } },
            { extend: 'pdf', exportOptions: { columns: [1] } },
            { extend: 'print', exportOptions: { columns: [1] } }
        ],
        searching: false,
        paging: false,
        info: false
    });
})


function selectItem(id, row) {
    selectedId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    row.classList.add('bg-light');
    //row.querySelector('input[type="radio"]').checked = true;

    if (canEdit) {
        const btnEdit = document.getElementById('btnEdit');
        if (btnEdit) btnEdit.disabled = false;
    }
    if (canDelete) {
        const btnDelete = document.getElementById('btnDelete');
        if (btnDelete) btnDelete.disabled = false;
    }
    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    // Edit sirf 1 record select hone par
    $('#btnEdit').prop('disabled', checkedCount !== 1);
}

document.getElementById('txtSearchInput').addEventListener('keyup', function () {
    const q = this.value.toLowerCase();
    document.querySelectorAll('#tblDepartment tbody tr').forEach(row => {
        if (row.classList.contains('item-row')) {
            row.style.display = row.textContent.toLowerCase().includes(q) ? '' : 'none';
        }
    });
});

function editSelected() {
    if (!selectedId) return;
    location.href = `/HumanResource/AddDepartment/${selectedId}`;
}

async function saveRecord() {
    const form = document.getElementById('entryForm');
    if (!form.checkValidity()) { form.reportValidity(); return; }

    const data = {
        departmentID: parseInt(document.getElementById('hdnDepartmentID').value) || 0,
        departmentName: document.getElementById('txtName').value.trim(),
        isActive: document.getElementById('chkActive').checked
    };

    fetch('/UpdateDepartment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Saved!',
                    text: 'Department has been saved successfully.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-success' },
                    buttonsStyling: false
                }).then(() => { window.location.href = '/HumanResource/Department'; });
            } else {
                Swal.fire({
                    icon: 'error', title: 'Error', text: res.message || 'Failed to save department.',
                    confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-danger' }, buttonsStyling: false
                });
            }
        })
        .catch(err => {
            console.error(err);
        });
}

function deleteSelected() {
    if (!selectedId) return;

    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one Department.");
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
                url: '/HumanResource/DeleteDepartment',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Department record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete Department.', 'error');
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
    fetch(`/HumanResource/ToggleDepartmentStatus?id=${id}&isActive=${isActive}`, { method: 'POST' })
        .then(r => r.json())
        .then(res => showToast(res.message, res.success ? 'success' : 'danger'));
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