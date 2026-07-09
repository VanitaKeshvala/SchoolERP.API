
const canEdit = window.canEdit;
const canDelete = window.canDelete;
// jQuery helper to serialize form to JSON object
$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name]) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#inquiryTable')) {
        const table = $('#inquiryTable').DataTable();
        table.button(index).trigger();
    }
}

$(document).ready(function () {
    $('#inquiryTable').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } }
        ],
        searching: false,
        paging: false,
        info: false
    });
})


$(document).ready(function () {
    // Modal Select2
    $('#addModal .select2').select2({
        dropdownParent: $('#addModal')
    });
   // if (canEdit) document.getElementById('btnEdit').disabled = true;
    //if (canEdit) document.getElementById('btnFollowUP').disabled = true;
    // Filter Select2
    

    // Follow Up Form Submission
    $('#followUpForm').on('submit', function (e) {
        e.preventDefault();
        const data = $(this).serializeObject();

        $.ajax({
            url: '/FrontOffice/SaveFollowUp',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (r) {
                if (r.success) {
                    showToast(r.message, 'success');
                    openFollowUp(data.InquiryID); // Refresh timeline
                } else {
                    showToast(r.message, 'error');
                }
            }
        });
    });

    // Table Search
    $('#tableSearch').on('keyup', function () {
        const val = $(this).val().toLowerCase();
        $("#inquiryTable tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(val) > -1)
        });
        renderFilterBadges();
    });


    // Prevent dropdown from closing when clicking inside
    $('.filter-dropdown').on('click', function (e) {
        e.stopPropagation();
    });
});

async function saveRecord()
{
    $('#inquiryForm').on('submit', function (e) {
        e.preventDefault();

        // Reset and validate
        InlineFormValidation.clearMap({ 'InqPhone': 'errPhone', 'InqEmail': 'errEmail' });
        const isPhoneValid = InlineFormValidation.validateMobile('InqPhone', 'errPhone', true);
        const isEmailValid = InlineFormValidation.validateEmail('InqEmail', 'errEmail', false);

        if (!this.checkValidity() || !isPhoneValid || !isEmailValid) {
            this.reportValidity();
            return;
        }

        const data = $(this).serializeObject();

        $.ajax({
            url: '/FrontOffice/SaveInquiry',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (r) {
                if (r.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Saved!',
                        text: 'Inquiry has been added successfully.',
                        confirmButtonText: 'OK',
                        customClass: { confirmButton: 'btn btn-success' },
                        buttonsStyling: false
                    }).then(() => {
                        window.location.href = '/FrontOffice/AdmissionInquiry';
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: r.message || 'Failed to inquiry.',
                        confirmButtonText: 'OK',
                        customClass: { confirmButton: 'btn btn-danger' },
                        buttonsStyling: false
                    });
                }
            }
        });
    });
}

// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_inquiry';
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
    // Sync hidden form fields from filter controls before submit
    const searchEl = document.getElementById('txtSearchInput');
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const classEl = document.getElementById('filterClass');
    const sourceEl = document.getElementById('filterSource');
    const fromDateEl = document.getElementById('filterFromDate');
    const toDateEl = document.getElementById('filterToDate');
    const statusEl = document.getElementById('filterStatus');

    if (searchEl) document.getElementById('hdnSearch').value = searchEl.value.trim();
    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (classEl) document.getElementById('hdnclassId').value = classEl.value;
    if (sourceEl) document.getElementById('hdnsourceId').value = sourceEl.value;
    if (fromDateEl) document.getElementById('hdnfromDate').value = fromDateEl.value;
    if (toDateEl) document.getElementById('hdntoDate').value = toDateEl.value;
    if (statusEl) document.getElementById('hdnstatus').value = statusEl.value;

    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const searchEl = document.getElementById('txtSearchInput');
    const classEl = document.getElementById('filterClass');
    const sourceEl = document.getElementById('filterSource');
    const fromDateEl = document.getElementById('filterFromDate');
    const toDateEl = document.getElementById('filterToDate');
    const statusEl = document.getElementById('filterStatus');

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

    // Class
    if (classEl?.value && classEl.value !== '0' && classEl.value !== '') {
        appliedFilters['filterClass'] = {
            label: 'Class',
            text: classEl.options[classEl.selectedIndex]?.text || classEl.value
        };
    } else delete appliedFilters['filterClass'];

    // Source
    if (sourceEl?.value && sourceEl.value !== '0' && sourceEl.value !== '') {
        appliedFilters['filterSource'] = {
            label: 'Source',
            text: sourceEl.options[sourceEl.selectedIndex]?.text || sourceEl.value
        };
    } else delete appliedFilters['filterSource'];

    // Enquiry From Date
    if (fromDateEl?.value) {
        appliedFilters['filterFromDate'] = { label: 'From', text: fromDateEl.value };
    } else delete appliedFilters['filterFromDate'];

    // Enquiry To Date
    if (toDateEl?.value) {
        appliedFilters['filterToDate'] = { label: 'To', text: toDateEl.value };
    } else delete appliedFilters['filterToDate'];

    // Status
    if (statusEl?.value && statusEl.value !== '') {
        appliedFilters['filterStatus'] = {
            label: 'Status',
            text: statusEl.options[statusEl.selectedIndex]?.text || statusEl.value
        };
    } else delete appliedFilters['filterStatus'];

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
    } else if (filterId === 'filterClass') {
        const el = document.getElementById('filterClass');
        if (el) {
            el.value = '0';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('0').trigger('change');
        }
    } else if (filterId === 'filterSource') {
        const el = document.getElementById('filterSource');
        if (el) {
            el.value = '0';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('0').trigger('change');
        }
    } else if (filterId === 'filterFromDate') {
        const el = document.getElementById('filterFromDate');
        if (el) el.value = '';
    } else if (filterId === 'filterToDate') {
        const el = document.getElementById('filterToDate');
        if (el) el.value = '';
    } else if (filterId === 'filterStatus') {
        const el = document.getElementById('filterStatus');
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

    ['ddlFilterSessions', 'ddlFilterCompany', 'filterClass', 'filterSource', 'filterStatus'].forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        el.value = (id === 'filterClass' || id === 'filterSource') ? '0' : '';
        if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val(el.value).trigger('change');
    });

    ['filterFromDate', 'filterToDate'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = '';
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
    if ($.fn.DataTable.isDataTable('#HolidayTable')) {
        $('#HolidayTable').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#HolidayTable').DataTable({
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
            jQuery('#ddlFilterSessions, #ddlFilterCompany, #filterClass, #filterSource, #filterStatus').select2({
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

    // ── Search box — txtSearchInput lives outside form#frmSearch, so it
    //    has no native way to submit. Wire Enter key and icon click. ───
    document.getElementById('txtSearchInput')?.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            document.getElementById('hdnPageIndex').value = 1;
            applyFilters();
            submitForm();
        }
    });

    document.querySelector('.search-icon-input')?.addEventListener('click', () => {
        document.getElementById('hdnPageIndex').value = 1;
        applyFilters();
        submitForm();
    });

    // ── Reset Filters ─────────────────────────────────────────────────
    document.getElementById('btnResetFilters')?.addEventListener('click', () => {
        resetAllFilters();
    });

    // ── Render badges on load ─────────────────────────────────────────
    renderFilterBadges();
});
// ========================================
// DOMContentLoaded — init DataTable + UI
// ========================================
document.addEventListener('DOMContentLoaded', () => {

    // ── DataTable (export only) ───────────────────────────────────────
    if ($.fn.DataTable.isDataTable('#HolidayTable')) {
        $('#HolidayTable').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#HolidayTable').DataTable({
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
            jQuery('#ddlFilterSessions, #ddlFilterCompany, #filterClass, #filterSource, #filterStatus').select2({
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
        resetAllFilters();
    });

    // ── Render badges on load ─────────────────────────────────────────
    renderFilterBadges();
});
function resetInquiryForm() {
    $('#inquiryForm')[0].reset();
    $('#InquiryID').val(0);
    $('#modalTitle').text('Add Admission Enquiry');
    $('.select2').val('').trigger('change');
    $('#InqDate').val(new Date().toISOString().split('T')[0]);
}

function editSelectedInquiry() {
    const checked = $('.student-checkbox:checked');
    if (checked.length !== 1) {
        showToast('Please select a record to edit', 'warning');
        return;
    }
    try {

        if (checked.val()) {
            location.href = `/FrontOffice/AddAdmissionEnquiry/${checked.val()}`;
        }

    } catch (err) {
        console.error(err);
    }
    //loadInquiry(checked.val());
}

function followUpSelectedInquiry() {
    const checked = $('.student-checkbox:checked');
    if (checked.length !== 1) {
        showToast('Please select a record for follow up', 'warning');
        return;
    }
    openFollowUp(checked.val());
}

function loadInquiry(id) {
    $.get('/FrontOffice/GetInquiry/' + id, function (r) {
        if (r.success) {
            const d = r.data;
            $('#InquiryID').val(d.inquiryID);
            $('#InqName').val(d.name);
            $('#InqPhone').val(d.phone);
            $('#InqEmail').val(d.email);
            $('#InqAddress').val(d.address);
            $('#InqDescription').val(d.description);
            $('#InqNote').val(d.note);
            $('#InqDate').val(d.date ? d.date.split('T')[0] : '');
            $('#InqNextFollowUpDate').val(d.nextFollowUpDate ? d.nextFollowUpDate.split('T')[0] : '');
            $('#InqAssignedTo').val(d.assignedTo).trigger('change');
            $('#InqReferenceID').val(d.referenceID).trigger('change');
            $('#InqSourceID').val(d.sourceID).trigger('change');
            $('#InqClassID').val(d.classID).trigger('change');
            $('#InqNoOfChild').val(d.noOfChild);
            $('#InqStatus').val(d.status).trigger('change');

            $('#modalTitle').text('Edit Admission Enquiry');
            $('#addModal').modal('show');
        }
    });
}

function deleteSelectedInquiry() {
    const checked = $('.student-checkbox:checked');
    if (checked.length === 0) {
        showToast('Please select a record to delete', 'warning');
        return;
    }


    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one enquiry.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this enquiry record. This action cannot be undone!",
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
                url: '/FrontOffice/DeleteInquiry',
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

function openFollowUp(id) {

    try {

        if (id) {
            location.href = `/FrontOffice/InquiryFollowUp/${id}`;
        }

    } catch (err) {
        console.error(err);
    }
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
    $('#btnFollowUP').prop('disabled', checkedCount !== 1);
}