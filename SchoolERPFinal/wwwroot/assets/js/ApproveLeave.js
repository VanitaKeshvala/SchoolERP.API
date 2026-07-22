// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_approveLeave';
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
    const classEL = document.getElementById('classSelect');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sectionEl = document.getElementById('sectionSelect');
    const statusEL = document.getElementById('statusSelect');
    if (classEL) document.getElementById('hdnSessionID').value = classEL.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (sectionEl) document.getElementById('hdnSectionID').value = sectionEl.value;
    if (statusEL) document.getElementById('hdnstatus').value = statusEL.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const classEl = document.getElementById('classSelect');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sectionEl = document.getElementById('sectionSelect');
    const statusEL = document.getElementById('statusSelect');
    const searchEl = document.getElementById('txtSearchInput');

    // Search
    const searchVal = searchEl?.value.trim();
    if (searchVal) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    else delete appliedFilters['txtSearchInput'];

    // Class
    if (classEl?.value && classEl.value !== '') {
        appliedFilters['classSelect'] = {
            label: 'Class',
            text: classEl.options[classEl.selectedIndex]?.text || classEl.value
        };
    } else delete appliedFilters['classSelect'];

    // Company
    if (companyEl?.value && companyEl.value !== '') {
        appliedFilters['ddlFilterCompany'] = {
            label: 'Company',
            text: companyEl.options[companyEl.selectedIndex]?.text || companyEl.value
        };
    } else delete appliedFilters['ddlFilterCompany'];

    // Section
    if (sectionEl?.value && sectionEl.value !== '') {
        appliedFilters['ddlFilterSection'] = {
            label: 'Section',
            text: sectionEl.options[sectionEl.selectedIndex]?.text || sectionEl.value
        };
    } else delete appliedFilters['ddlFilterSection'];

    // Status
    if (statusEL?.value && statusEL.value !== '') {
        appliedFilters['statusSelect'] = {
            label: 'Status',
            text: statusEL.options[statusEL.selectedIndex]?.text || statusEL.value
        };
    } else delete appliedFilters['statusSelect'];

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
    if (filterId === 'classSelect') {
        const el = document.getElementById('classSelect');
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
    } else if (filterId === 'sectionSelect') {
        const el = document.getElementById('sectionSelect');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'statusSelect') {
        const el = document.getElementById('statusSelect');
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

    ['ddlFilterSessions', 'ddlFilterCompany', 'ddlFilterSection'].forEach(id => {
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
    if ($.fn.DataTable.isDataTable('#tblLeave')) {
        $('#tblLeave').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#tblLeave').DataTable({
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
            jQuery('#classSelect, #ddlFilterCompany', '#ddlFilterSection', '#sectionSelect', '#statusSelect','#modalClassSelect').select2({
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
        ['classSelect', 'ddlFilterCompany', 'ddlFilterSection', 'sectionSelect','statusSelect'].forEach(id => {
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
    var classId = $("#modalClassSelect").val();
   
    if (classId > 0)
    {
        $('#modalClassSelect').trigger('change').trigger('change.select2');
        var sectionID = $("#hdnSectionId").val();
        var studentID = $("#hdnStudentID").val();
        loadSections(classId, '#modalSectionSelect').then(() => {
            $('#modalSectionSelect').val(sectionID).trigger('change.select2');
            return loadStudents(classId, sectionID);
        }).then(() => {
            $('#modalStudentSelect').val(studentID).trigger('change.select2');
        });
    }

    $('#classSelect').on('change', function () {
        loadSections($(this).val(), '#modalClassSelect');
        
    });
    $('#classSelect').on('change', function () {
        loadSections($(this).val(), '#sectionSelect');

    });
    $('#modalClassSelect').on('change', function () {
        loadSections($(this).val(), '#modalSectionSelect');
    });

    $('#modalSectionSelect').on('change', function () {
        loadStudents($('#modalClassSelect').val(), $(this).val());
    });

    // Set initial min on page load based on existing fromDate value
    setToDateMin();

    // Update min whenever fromDate changes
    $('#fromDate').on('change', function () {
        setToDateMin();

        // If toDate is already less than the new fromDate, reset/clear it
        const fromVal = $('#fromDate').val();
        const toVal = $('#toDate').val();
        if (fromVal && toVal && toVal < fromVal) {
            $('#toDate').val(fromVal); // or use '' to clear instead
        }
    });

    function setToDateMin() {
        const fromVal = $('#fromDate').val();
        if (fromVal) {
            $('#toDate').attr('min', fromVal);
        }
    }
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

function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#tblLeave')) {
        const table = $('#tblLeave').DataTable();
        table.button(index).trigger();
    }
}

function initDataTable() {
    if ($.fn.DataTable.isDataTable('#tblLeave')) {
        $('#tblLeave').DataTable().clear().destroy();
    }
    $('#tblLeave').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } },
            {
                extend: 'excel', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] },
                format: {
                    body: function (data, row, column, node) {
                        return node ? node.textContent.replace(/\s+/g, ' ').trim() : data;
                    }
                }
            },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } }
        ],
        searching: false,
        paging: false,
        info: false,
        ordering: false
    });
}


$(document).ready(function () {
    //$('.select2').select2({ dropdownParent: $('#filter-dropdown') });
    //$('.select2-modal').select2({ dropdownParent: $('#addLeaveModal'), width: '100%' });

    

    $('#ddlFilterCompany').select2({
        dropdownParent: $('#filter-dropdown'),
        width: '100%'
    });
    $('#classSelect').select2({
        dropdownParent: $('#filter-dropdown'),
        width: '100%'
    });
    $('#sectionSelect').select2({
        dropdownParent: $('#filter-dropdown'),
        width: '100%'
    });
    $('#statusSelect').select2({
        dropdownParent: $('#filter-dropdown'),
        width: '100%'
    });
    $('#btnFilterToggle').on('shown.bs.dropdown', function () {
        initFilterSelect2('#ddlFilterCompany');
        initFilterSelect2('#classSelect');
        initFilterSelect2('#sectionSelect');
        initFilterSelect2('#statusSelect');
    });

    function initFilterSelect2(selector) {
        var $el = $(selector);
        // Only init if not already a select2 instance
        if (!$el.hasClass('select2-hidden-accessible')) {
            $el.select2({
                dropdownParent: $('#filter-dropdown'),
                width: '100%'
            });
        }
    }



    $('#btnSaveLeave').on('click', function () {
        saveLeave();
    });

    $('#filter-dropdown').on('click', function (e) {
        e.stopPropagation();
    });

});

let selectedId = 0;
const IV = window.InlineFormValidation || {
    clearMap: (map) => Object.keys(map).forEach(k => {
        document.getElementById(k)?.classList.remove('is-invalid');
        const err = document.getElementById(map[k]); if (err) err.textContent = '';
    }),
    setFieldError: (id, errId, msg) => {
        document.getElementById(id)?.classList.add('is-invalid');
        const err = document.getElementById(errId); if (err) err.textContent = msg;
    },
    setNotice: (id, msg) => {
        const el = document.getElementById(id);
        if (el) el.innerHTML = `<div class="alert alert-danger alert-dismissible fade show" role="alert">${msg}<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>`;
    },
    clearNotice: (id) => { const el = document.getElementById(id); if (el) el.innerHTML = ''; },
    bindAutoClear: (map) => Object.keys(map).forEach(k => document.getElementById(k)?.addEventListener('input', () => {
        document.getElementById(k)?.classList.remove('is-invalid');
        const err = document.getElementById(map[k]); if (err) err.textContent = '';
    }))
};
function selectItem(id, row) {
    selectedId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    row.classList.add('bg-light');
    //row.querySelector('input[type="radio"]').checked = true;

    document.getElementById('btnEdit').disabled = false;
    document.getElementById('btnDelete').disabled = false;

    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    // Edit sirf 1 record select hone par
    $('#btnEdit').prop('disabled', checkedCount !== 1);
}

function handleEditAction() {
    if (selectedId) {
        editLeave(selectedId);
    }
}


async function handleDeleteAction() {
    
    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one Class.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this Class record. This action cannot be undone!",
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
                url: '/Attendance/DeleteLeaveApplication',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Leave application record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete leave application.', 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error!', 'An unexpected error occurred.', 'error');
                }
            });
        }
    });
}


function resetForm() {
    $('#modalTitle').text('Add Leave Request');
    $('#leaveAppId').val(0);
    $('#leaveForm')[0].reset();
    $('.select2-modal').val('').trigger('change');
    $('#pen').prop('checked', true);

    // Clear selection
    selectedId = 0;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    document.querySelectorAll('input[name="selectedItem"]').forEach(r => r.checked = false);
    document.getElementById('btnEdit').disabled = true;
    document.getElementById('btnDelete').disabled = true;
    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    // Edit sirf 1 record select hone par
    $('#btnEdit').prop('disabled', checkedCount !== 1);
}

function loadSections(classId, targetSelector) {
    if (classId) {
        return $.get('/Attendance/GetSectionsByClass', { classId: classId }, function (res) {
            if (res.success) {
                let html = `<option value="">Select Section</option>`;
                res.data.forEach(s => {
                    html += `<option value="${s.sectionID}">${s.sectionName}</option>`;
                });
                $(targetSelector).html(html).trigger('change');
                renderFilterBadges();
            }
        });
    } else {
        $(targetSelector).html('<option value="">Select Section</option>').trigger('change');
        return $.Deferred().resolve().promise();
    }
}

function loadStudents(classId, sectionId) {
    if (classId && sectionId) {
        return $.get('/Attendance/GetStudentsForLeave', { classId: classId, sectionId: sectionId }, function (res) {
            if (res.success) {
                let html = '<option value="">Select Student</option>';
                res.data.forEach(s => {
                    html += `<option value="${s.studentID}">${s.fullName} (${s.rollNo})</option>`;
                });
                $('#modalStudentSelect').html(html).trigger('change');
            }
        });
    } else {
        $('#modalStudentSelect').html('<option value="">Select Student</option>').trigger('change');
        return $.Deferred().resolve().promise();
    }
}

function editLeave(id) {

    if (id === 0) return;
    try {

        if (id) {
            location.href = `/Attendance/AddLeaveRequest/${id}`;
        }

    } catch (err) {
        console.error(err);
    }

}

function saveLeave() {
    const formData = new FormData();
    formData.append('LeaveAppID', $('#leaveAppId').val());
    formData.append('StudentID', $('#modalStudentSelect').val());
    formData.append('FromDate', $('#fromDate').val());
    formData.append('ToDate', $('#toDate').val());
    formData.append('ApplyDate', $('#applyDate').val());
    formData.append('Reason', $('#reason').val());
    formData.append('Status', $('input[name="leaveStatus"]:checked').val());


    const fileInput = document.getElementById('attachFile');
    if (fileInput.files[0]) {
        formData.append('attachment', fileInput.files[0]);
    }

    // ✅ Validate ToDate should not be less than FromDate
    const fromDateVal = $('#fromDate').val();
    const toDateVal = $('#toDate').val();
    if (fromDateVal && toDateVal) {
        const fromDate = new Date(fromDateVal);
        const toDate = new Date(toDateVal);
        if (toDate < fromDate) {
            showToast('To Date cannot be earlier than From Date', 'error');
            return;
        }
    }

    const reason = document.getElementById('reason').value.trim();
    if (!reason) {
        IV.setFieldError('reason', 'errreason', 'Reason is required.');
        return;
    }

    if (!$('#modalStudentSelect').val() || !$('#toDate').val()) {
        showToast('Please fill all required fields', 'error');
        return;
    }

    $.ajax({
        url: '/Attendance/UpsertLeave',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            if (res.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Saved!',
                    text: res.message,
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-success' },
                    buttonsStyling: false
                }).then(() => {
                    window.location.href = '/Attendance/ApproveLeave';
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: res.message || 'Failed to save leave.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-danger' },
                    buttonsStyling: false
                });
            }
        }
    });
}

function updateStatus(id, status) {
    const statusText = status === 1 ? 'Approve' : 'Reject';
    Swal.fire({
        title: 'Are you sure?',
        text: `You want to ${statusText} this leave application?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: status === 1 ? '#28a745' : '#dc3545',
        confirmButtonText: `Yes, ${statusText} it!`
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Attendance/UpdateLeaveStatus',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ LeaveAppID: id, Status: status }),
                success: function (res) {
                    if (res.success) {
                        showToast(res.message, 'success');
                       
                    } else {
                        showToast(res.message, 'error');
                    }
                }
            });
        }
    });
}


function removeExistingFile() {
    document.getElementById('existingFileInfo')?.remove();
    document.getElementById('hdnRemoveFile').value = 'true';
    document.getElementById('hdnExistingFile').value = '';
}
const maxFileSizeMB = 1;
const maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;

document.getElementById('attachFile').addEventListener('change', function () {

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