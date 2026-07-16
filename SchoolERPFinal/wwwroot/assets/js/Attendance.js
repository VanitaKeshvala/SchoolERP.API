// Status codes: 1 = Present, 2 = Late, 3 = Absent, 4 = Half Day, 5 = Leave
const NOTE_REQUIRED_STATUSES = [2, 4, 5];

$(document).ready(function () {
    $('.select2').select2({ dropdownParent: $('#filter-dropdown') });

    // Stop dropdown from closing when clicking inside
    $('#filter-dropdown').on('click', function (e) {
        e.stopPropagation();
    });


    $('#classSelect').on('change', function () {
        loadSections($(this).val(), '#sectionSelect');

    });

    $('#sectionSelect').on('change', function () {
        loadStudents($('#classSelect').val(), $(this).val());
    });

    // Apply Filters submits the real GET form so the controller re-populates
    // Model.StudentAttendanceModel and Razor re-renders the tbody server-side.
    $('#btnApplyFilters').on('click', function (e) {
        e.preventDefault();
        const classId = $('#classSelect').val();
        const sectionId = $('#sectionSelect').val();
        const date = $('#hdnAttendanceDate').val();

        if (!classId || !sectionId || !date) {
            Swal.fire('Error', 'Please select class, section and date', 'error');
            return;
        }

        $('#hdnClassId').val(classId);
        $('#hdnSectionID').val(sectionId);
        $('#hdnStudentId').val($('#sectionStudent').val());
        $('#hdnAttendanceDate').val(date);
        $('#hdnPageIndex').val(1);
        $('#frmSearch').submit();
    });

    // Enter key in the search box submits the same form
    $('#txtSearchInput').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#hdnSearch').val($(this).val());
            $('#hdnPageIndex').val(1);
            $('#frmSearch').submit();
        }
    });

    // Quick client-side filter of already-rendered rows (no reload)
    $('#txtSearchInput').on('keyup', function () {
        filterAttendanceTable();
    });

    // Per-column header checkboxes — toggles that status checkbox on every row
    $('.select-all-col').on('change', function () {
        const status = $(this).data('status');
        const isChecked = $(this).is(':checked');
        $(`#attendanceTableBody .atd-check[value="${status}"]`).prop('checked', isChecked);
        $('#attendanceTableBody tr.attendance-row').each(function () {
            validateRowNote($(this));
        });
    });

    // Delegated: any status checkbox change re-validates that row's note requirement
    $('#attendanceTableBody').on('change', '.atd-check', function () {
        validateRowNote($(this).closest('tr'));
    });

    // Delegated: typing in the note clears the invalid state once filled
    $('#attendanceTableBody').on('input', '.attendance-note', function () {
        validateRowNote($(this).closest('tr'));
    });

    // Run note validation once on initial (server-rendered) load, in case a
    // student already has Late/Half Day/Leave set without a note.
    $('#attendanceTableBody tr.attendance-row').each(function () {
        validateRowNote($(this));
    });

    $('#btnSaveAttendance').on('click', function () {
        const classId = $('#hdnClassId').val();
        const sectionId = $('#hdnSectionID').val();
        const date = $('#attendanceDate').val();

        if (!classId || !sectionId || !date) {
            Swal.fire('Error', 'Please select class, section and date, then click Apply first', 'error');
            return;
        }

        let hasError = false;
        const attendanceData = [];

        $('#attendanceTableBody tr.attendance-row').each(function () {
            const row = $(this);
            const studentId = row.attr('data-student-id');
            if (!studentId) return;

            const statuses = row.find('.atd-check:checked').val();
                //.map(function () { return parseInt($(this).val()); })
                //.get();
            const note = row.find('.attendance-note').val().trim();

            if (statuses.length === 0) {
                hasError = true;
                row.addClass('table-danger');
            } else {
                row.removeClass('table-danger');
            }

            if (!validateRowNote(row)) {
                hasError = true;
            }

            attendanceData.push({
                StudentID: parseInt(studentId),
                Status: statuses,
                Note: note
            });
        });
        console.log(attendanceData);
        if (hasError) {
            Swal.fire('Error', 'Please select at least one status for every student, and add a Note wherever Late, Half Day, or Leave is checked.', 'error');
            return;
        }

        if (attendanceData.length === 0) {
            Swal.fire('Info', 'No students found to mark attendance', 'info');
            return;
        }

        const req = {
            classId: parseInt(classId),
            sectionId: parseInt(sectionId),
            attendanceDate: date,
            attendanceData: attendanceData
        };

        $.ajax({
            url: '/Attendance/SaveAttendance',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(req),
            success: function (res) {
                if (res.success) {
                    Swal.fire('Success', res.message, 'success')
                        .then(() => location.reload());
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            }
        });
    });
});


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
                $('#sectionStudent').html(html).trigger('change');
            }
        });
    } else {
        $('#sectionStudent').html('<option value="">Select Student</option>').trigger('change');
        return $.Deferred().resolve().promise();
    }
}

// Returns true if the row is valid (note present when required), false otherwise.
// Also toggles the .is-invalid class on the note field for visual feedback.
function validateRowNote(row) {
    const checkedStatuses = row.find('.atd-check:checked')
        .map(function () { return parseInt($(this).val()); })
        .get();
    const noteInput = row.find('.attendance-note');
    const noteRequired = checkedStatuses.some(v => NOTE_REQUIRED_STATUSES.includes(v));

    if (noteRequired) {
        noteInput.attr('placeholder', 'Note required');
        if (!noteInput.val().trim()) {
            noteInput.addClass('is-invalid');
            return false;
        } else {
            noteInput.removeClass('is-invalid');
            return true;
        }
    } else {
        noteInput.removeClass('is-invalid');
        noteInput.attr('placeholder', '');
        return true;
    }
}

// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_attendance';
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
    const statusEL = document.getElementById('sectionStudent');
    const dateEL = document.getElementById('attendanceDate');

    if (classEL) document.getElementById('hdnClassId').value = classEL.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (sectionEl) document.getElementById('hdnSectionID').value = sectionEl.value;
    if (statusEL) document.getElementById('hdnStudentId').value = statusEL.value;
    if (dateEL) document.getElementById('hdnAttendanceDate').value = dateEL.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const classEl = document.getElementById('classSelect');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sectionEl = document.getElementById('sectionSelect');
    const statusEL = document.getElementById('sectionStudent');
    const searchEl = document.getElementById('txtSearchInput');
    const dateEl = document.getElementById('attendanceDate');

    // Search
    const searchVal = searchEl?.value.trim();
    if (searchVal) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    else delete appliedFilters['txtSearchInput'];

    // Date
    const dateVal = dateEl?.value.trim();
    if (dateVal) appliedFilters['attendanceDate'] = { label: 'Date', text: dateVal };
    else delete appliedFilters['attendanceDate'];

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
        appliedFilters['sectionStudent'] = {
            label: 'Status',
            text: statusEL.options[statusEL.selectedIndex]?.text || statusEL.value
        };
    } else delete appliedFilters['sectionStudent'];

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
            jQuery('#classSelect, #ddlFilterCompany', '#ddlFilterSection', '#sectionSelect', '#statusSelect', '#modalClassSelect').select2({
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
        ['classSelect', 'ddlFilterCompany', 'ddlFilterSection', 'sectionSelect', 'statusSelect'].forEach(id => {
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

    const table = document.getElementById('attendanceTableBody');

    // Enforce single-status-per-row (radio-like) behavior
    table.addEventListener('change', function (e) {
        if (!e.target.classList.contains('atd-check')) return;

        const row = e.target.closest('tr');
        const rowChecks = row.querySelectorAll('.atd-check');

        if (e.target.checked) {
            rowChecks.forEach(chk => {
                if (chk !== e.target) chk.checked = false;
            });
        } else {
            // Prevent unchecking the only selected status with nothing replacing it
            const anyChecked = Array.from(rowChecks).some(chk => chk.checked);
            if (!anyChecked) e.target.checked = true; // fall back to keep a status selected
        }
    });

    // Default any row with nothing checked to "Present" (value=1) on load
    table.querySelectorAll('tr.attendance-row').forEach(row => {
        const rowChecks = row.querySelectorAll('.atd-check');
        const anyChecked = Array.from(rowChecks).some(chk => chk.checked);
        if (!anyChecked) {
            const presentBox = row.querySelector('.atd-check[value="1"]');
            if (presentBox) presentBox.checked = true;
        }
    });
});
document.querySelectorAll('.select-all-col').forEach(headerCheck => {
    headerCheck.addEventListener('change', function () {
        const status = this.dataset.status;
        const isChecked = this.checked;

        document.querySelectorAll('#attendanceTableBody tr.attendance-row').forEach(row => {
            const target = row.querySelector(`.atd-check[value="${status}"]`);
            if (!target) return;
            target.checked = isChecked;
            if (isChecked) {
                row.querySelectorAll('.atd-check').forEach(chk => {
                    if (chk !== target) chk.checked = false;
                });
            }
        });
    });
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
