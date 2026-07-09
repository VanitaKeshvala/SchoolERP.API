
// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_visitorBook';
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
    const sectionEl = document.getElementById('ddlStaff');
    const purposeEl = document.getElementById('filterPurpose');
    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (sectionEl) document.getElementById('hdnStaffId').value = sectionEl.value;
    if (purposeEl) document.getElementById('hdnpurpose').value = purposeEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sectionEl = document.getElementById('ddlStaff');
    const purposeEl = document.getElementById('filterPurpose');
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

    // Staff
    if (sectionEl?.value && sectionEl.value !== '') {
        appliedFilters['ddlStaff'] = {
            label: 'Staff',
            text: sectionEl.options[sectionEl.selectedIndex]?.text || sectionEl.value
        };
    } else delete appliedFilters['ddlStaff'];

    // Purposes
    if (purposeEl?.value && purposeEl.value !== '') {
        appliedFilters['filterPurpose'] = {
            label: 'Purposes',
            text: purposeEl.options[purposeEl.selectedIndex]?.text || purposeEl.value
        };
    } else delete appliedFilters['filterPurpose'];

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
    } else if (filterId === 'ddlStaff') {
        const el = document.getElementById('ddlStaff');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'filterPurpose') {
        const el = document.getElementById('filterPurpose');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    delete appliedFilters[filterId];
    saveAppliedFilters();
    document.getElementById('hdnPageIndex').value = 1;
    submitForm();
}

function resetAllFilters() {
    document.getElementById('txtSearchInput').value = '';
    document.getElementById('hdnSearch').value = '';

    ['ddlFilterSessions', 'ddlFilterCompany', 'ddlStaff','filterPurpose'].forEach(id => {
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
    if ($.fn.DataTable.isDataTable('#HostelTypeTable')) {
        $('#HostelTypeTable').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#HostelTypeTable').DataTable({
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
            jQuery('#ddlFilterSessions, #ddlFilterCompany', '#ddlStaff','#filterPurpose').select2({
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
        ['ddlFilterSessions', 'ddlFilterCompany', 'ddlStaff','filterPurpose'].forEach(id => {
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
let selectedId = 0;
const canEdit = window.canEdit;
const canDelete = window.canDelete;
function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#tblVisitors')) {
        const table = $('#tblVisitors').DataTable();
        table.button(index).trigger();
    }
}

$(document).ready(function () {
    $('#tblVisitors').DataTable({
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



$(document).ready(function () {
    // Initialize Select2 for page filters
    $('.select2:not(#visitorModal .select2)').select2({
        width: '100%'
    });

    // Initialize Select2 for modal with dropdownParent
    $('#visitorModal').on('shown.bs.modal', function () {
        $('#visitorModal .select2').select2({
            dropdownParent: $('#visitorModal'),
            width: '100%'
        });
    });

    // Clear validation errors on change/input
    const clearError = (id, errId) => {
        const el = document.getElementById(id);
        if (el && el.value.trim()) {
            el.classList.remove('is-invalid');
            document.getElementById(errId).style.display = 'none';
        }
    };

    $('#ddlPurpose, #ddlMeetingWith, #ddlStudent, #ddlStaff').on('change', function () {
        clearError(this.id, 'err' + this.id.replace('ddl', ''));
    });

    $('#txtDate').on('change', function () {
        clearError('txtDate', 'errDate');
    });

    $('#txtName').on('keyup', function () {
        clearError('txtName', 'errName');
    });
    initEditForm();
});

function handleMeetingWith(val) {
    document.getElementById('studentSelection').classList.toggle('d-none', val !== 'Student');
    document.getElementById('staffSelection').classList.toggle('d-none', val !== 'Staff');

    if (val === 'Student') {
        document.getElementById('ddlStudent').setAttribute('required', 'required');
        document.getElementById('ddlStaff').removeAttribute('required');
    } else if (val === 'Staff') {
        document.getElementById('ddlStaff').setAttribute('required', 'required');
        document.getElementById('ddlStudent').removeAttribute('required');
    } else {
        document.getElementById('ddlStudent').removeAttribute('required');
        document.getElementById('ddlStaff').removeAttribute('required');
    }
}

function loadSections(classId) {
    const ddl = $('#ddlSection');
    ddl.empty().append('<option value="">Select Section</option>');
    $('#ddlStudent').empty().append('<option value="">Select Student</option>');

    if (!classId) return;

    fetch(`/FrontOffice/GetSections?classId=${classId}`)
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                res.data.forEach(s => ddl.append(`<option value="${s.sectionID}">${s.sectionName}</option>`));
            }
        });
}

function loadStudents() {
    const classId = document.getElementById('ddlClass').value;
    const sectionId = document.getElementById('ddlSection').value;
    const ddl = $('#ddlStudent');
    ddl.empty().append('<option value="">Select Student</option>');

    if (!classId || !sectionId) return;

    fetch(`/FrontOffice/GetStudents?classId=${classId}&sectionId=${sectionId}`)
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                res.data.forEach(s => ddl.append(`<option value="${s.studentID}">${s.fullName} (${s.rollNo})</option>`));
            }
        });
}

function selectItem(id, row) {
    selectedId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
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
    document.getElementById('modalTitle').textContent = 'Add Visitor';
    document.getElementById('hdnVisitorBookID').value = 0;
    document.getElementById('visitorForm').reset();
    document.getElementById('txtDate').value = new Date().toISOString().split('T')[0];
    document.getElementById('chkActive').checked = true;
    document.getElementById('fileInfo').textContent = '';

    // Reset Meeting With
    $('#ddlMeetingWith').val('').trigger('change');
    handleMeetingWith('');

    $('#ddlPurpose, #ddlClass, #ddlSection, #ddlStudent, #ddlStaff').val('').trigger('change');

    new bootstrap.Modal(document.getElementById('visitorModal')).show();
}

async function editSelected() {
    if (!selectedId) return;
    try {

        if (selectedId) {
            location.href = `/FrontOffice/AddVisitorBook/${selectedId}`;
        }

    } catch (err) {
        console.error(err);
    }
}

async function saveVisitor(e) {
    if (e) e.preventDefault();
    const form = document.getElementById('visitorForm');

    // Reset and validate Phone
    InlineFormValidation.clearFieldError('txtPhone', 'errPhone');
    const isPhoneValid = InlineFormValidation.validateMobile('txtPhone', 'errPhone', false);

    let isValid = true;
    const validateField = (id, errId) => {
        const el = document.getElementById(id);
        if (!el || !el.value.trim()) {
            if (el) el.classList.add('is-invalid');
            document.getElementById(errId).style.display = 'block';
            isValid = false;
        } else {
            el.classList.remove('is-invalid');
            document.getElementById(errId).style.display = 'none';
        }
    };

    validateField('ddlPurpose', 'errPurpose');
    validateField('ddlMeetingWith', 'errMeetingWith');
    validateField('txtName', 'errName');
    validateField('txtDate', 'errDate');

    const mw = document.getElementById('ddlMeetingWith').value;
    if (mw === 'Student') validateField('ddlStudent', 'errStudent');
    if (mw === 'Staff') validateField('ddlStaff', 'errStaff');

    if (!isValid || !isPhoneValid) {
        return;
    }

    const formData = new FormData(form);




    fetch('/FrontOffice/SaveVisitor', {
        method: 'POST',
        body: formData
    })
        .then(r => r.json())
        .then(res => {            
            if (res.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Saved!',
                    text: 'Visitor has been added successfully.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-success' },
                    buttonsStyling: false
                }).then(() => {
                    window.location.href = '/FrontOffice/VisitorBook';
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: res.message || 'Failed to save visitor.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-danger' },
                    buttonsStyling: false
                });
            }
        });
}

function deleteSelected() {
    if (!selectedId) return;


    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one Visitor.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this visitor record. This action cannot be undone!",
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
                url: '/FrontOffice/DeleteVisitor',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Visitor record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete visitor.', 'error');
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
    if (!canEdit) {
        IV.setNotice('classPageNotice', 'You do not have permission to change status.');
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
                    url: '/FrontOffice/ToggleVisitorStatus',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(request),
                    success: function (res) {
                        if (res.success) {
                            Swal.fire('Updated!', res.message || 'Visitor updated successfully.', 'success')
                                .then(() => location.reload());
                        } else {
                            Swal.fire('Error!', res.message || 'Failed to update visitor.', 'error');
                        }
                    },
                    error: function (xhr) {
                        // ✅ log xhr for debugging
                        console.error('Visitor update error:', xhr.status, xhr.responseText);
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


async function initEditForm() {
    const d = window.editVisitor;
    if (!d) return; // not edit mode
    console.log(d);
    document.getElementById('hdnVisitorBookID').value = d.VisitorBookID;

    const mw = (d.MeetingWith === 'Student' || d.MeetingWith === 'Staff') ? d.MeetingWith : '';
    $('#ddlMeetingWith').val(mw).trigger('change'); // triggers select2 refresh
    handleMeetingWith(mw); // show/hide the right section

    if (mw === 'Student' && d.studentID) {
        $('#ddlClass').val(d.classID).trigger('change');
        loadSections(d.classID);
        $('#ddlSection').val(d.sectionID).trigger('change');
        loadStudents();
        $('#ddlStudent').val(d.studentID).trigger('change');
    } else if (mw === 'Staff' && d.staffID) {
        $('#ddlStaff').val(d.staffID).trigger('change');
    }
}
function removeExistingFile() {
    document.getElementById('existingFileInfo')?.remove();
    document.getElementById('hdnRemoveFile').value = 'true';
    document.getElementById('hdnExistingFile').value = '';
}
const maxFileSizeMB = 1;
const maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;

document.getElementById('attachmentFile').addEventListener('change', function () {

    const fileInfo = document.getElementById('fileInfo');
    const file = this.files[0];

    if (!file) {
        fileInfo.textContent = '';
        return;
    }

    const sizeMB = (file.size / (1024 * 1024)).toFixed(2);

    if (file.size > maxFileSizeBytes) {
        fileInfo.textContent = `File is ${sizeMB} MB. Max allowed size is ${maxFileSizeMB} MB.`;
        fileInfo.classList.remove('text-muted');
        fileInfo.classList.add('text-danger');
        this.value = '';
    } else {
        fileInfo.textContent = `Selected file size: ${sizeMB} MB`;
        fileInfo.classList.remove('text-danger');
        fileInfo.classList.add('text-muted');

        document.getElementById('hdnRemoveFile').value = 'false';
        document.getElementById('existingFileInfo')?.remove();
    }
});