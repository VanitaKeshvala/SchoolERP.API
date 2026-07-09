function triggerExport(index) {
    if ($('#studentsTable tbody tr.student-row').length === 0) {
        if (window.Swal) {
            Swal.fire({
                icon: 'warning',
                title: 'No Data',
                text: 'Please apply filters first to load students.',
                confirmButtonColor: '#3085d6'
            });
        } else {
            alert('Please apply filters first to load students.');
        }
        return;
    }
    if ($.fn.DataTable.isDataTable('#studentsTable')) {
        const table = $('#studentsTable').DataTable();
        table.button(index).trigger();
    }
}



// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_promoteStudents';
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
    const classEl = document.getElementById('FilterClassID');
    const sectionEl = document.getElementById('FilterSectionID');
    if (sessionEl) document.getElementById('hdnSectionID').value = sessionEl.value;
    if (classEl) document.getElementById('hdnclassId').value = classEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (sectionEl) document.getElementById('hdnSectionID').value = sectionEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sectionEl = document.getElementById('FilterSectionID');
    const classEl = document.getElementById('FilterClassID');
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

    // Section
    if (sectionEl?.value && sectionEl.value !== '') {
        appliedFilters['FilterSectionID'] = {
            label: 'Section',
            text: sectionEl.options[sectionEl.selectedIndex]?.text || sectionEl.value
        };
    } else delete appliedFilters['FilterSectionID'];
    // class
    if (classEl?.value && classEl.value !== '') {
        appliedFilters['FilterClassID'] = {
            label: 'Section',
            text: classEl.options[classEl.selectedIndex]?.text || classEl.value
        };
    } else delete appliedFilters['FilterClassID'];
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
    } else if (filterId === 'FilterSectionID') {
        const el = document.getElementById('FilterSectionID');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'FilterClassID') {
        const el = document.getElementById('FilterClassID');
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

    ['ddlFilterSessions', 'ddlFilterCompany', 'FilterSectionID','FilterClassID'].forEach(id => {
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
window.selectedClassId = 0;
const classFieldMap = { txtClassName: 'errTxtClassName', ddlSections: 'errDdlSections' };

$(document).ready(function () {
    updatePromoteButton();
    $.fn.dataTable.ext.errMode = 'none';   // moved here since the other block is gone

    const exportOpts = {
        columns: [1, 2, 3, 4, 5, 6],
        format: {
            body: function (data, row, column, node) {
                const checkedRadio = node.querySelector('input[type="radio"]:checked');
                if (checkedRadio) {
                    return checkedRadio.value;
                }
                return data.replace(/<[^>]*>/g, "").replace(/\s+/g, " ").trim();
            }
        }
    };

    if ($.fn.DataTable.isDataTable('#studentsTable')) {
        $('#studentsTable').DataTable().destroy();
    }

    if ($('#studentsTable tbody tr.student-row').length > 0) {
        window.exportTable = $('#studentsTable').DataTable({
            dom: 'Bfrtip',
            buttons: [
                { extend: 'copy', exportOptions: exportOpts },
                { extend: 'csv', exportOptions: exportOpts },
                { extend: 'excel', exportOptions: exportOpts },
                { extend: 'pdf', exportOptions: exportOpts },
                { extend: 'print', exportOptions: exportOpts }
            ],
            searching: false,
            paging: false,
            info: false
        });
    }

    // ── Select2 ───────────────────────────────────────────────────────
    try {
        if (window.jQuery && typeof jQuery.fn.select2 === 'function') {
            jQuery('#ddlFilterSessions, #ddlFilterCompany, #FilterSectionID, #FilterClassID').select2({
                width: '100%',
                dropdownParent: jQuery('#filter-dropdown'),
                allowClear: true,
                placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
            });
        }
    } catch (e) {
        console.warn('Select2 init skipped:', e);
    }

    document.getElementById('filter-dropdown')?.addEventListener('click', e => e.stopPropagation());

    $('.filter-dropdown').on('click', function (e) {
        e.stopPropagation();
    });

    document.getElementById('btnApplyFilters')?.addEventListener('click', () => {
        document.getElementById('hdnPageIndex').value = 1;
        document.getElementById('hdnSearch').value = document.getElementById('txtSearchInput').value;
        applyFilters();
        submitForm();
    });

    document.getElementById('btnResetFilters')?.addEventListener('click', () => {
        document.getElementById('txtSearchInput').value = '';
        document.getElementById('hdnSearch').value = '';
        ['ddlFilterSessions', 'ddlFilterCompany', 'FilterSectionID', 'FilterClassID'].forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        });
        clearAppliedFilters();
        document.getElementById('hdnPageIndex').value = 1;
        submitForm();
    });

    $('#selectAll').on('change', function () {
        $('.student-checkbox').prop('checked', $(this).is(':checked'));
        let checkedCount = $('.student-checkbox:checked').length;
        $('#btnSave').prop('disabled', checkedCount === 0);
    });

    //const initialClassId = '@Model.SelectedClassId';
    //const initialSectionId = '@Model.SelectedSectionId';
    //if (initialClassId) {
    //    loadSections(initialClassId, '#FilterSectionID', initialSectionId);
    //}

    //const initialNextClassId = '@Model.NextClassId';
    //const initialNextSectionId = '@Model.NextSectionId';
    //if (initialNextClassId) {
    //    loadSections(initialNextClassId, '#NextSectionID', initialNextSectionId);
    //}

    renderFilterBadges();

    $("#txtSearchInput").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $("#studentsTable tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
        renderFilterBadges();
    });
});

function loadSections(classId, targetSelector, selectedId = null) {
    if (!classId) {
        $(targetSelector).html('<option value="">Select</option>');
        return;
    }
    $.get('/Academics/GetSectionsByClass/' + classId, function (res) {
        if (res.success) {
            let html = '<option value="">Select</option>';
            res.data.forEach(s => {
                html += `<option value="${s.sectionID}" ${s.sectionID == selectedId ? 'selected' : ''}>${s.sectionName}</option>`;
            });
            $(targetSelector).html(html);
            renderFilterBadges();
        }
    });
}

function onFilterClassChange() {
    loadSections($('#FilterClassID').val(), '#FilterSectionID');
}

function onNextClassChange() {
    loadSections($('#NextClassID').val(), '#NextSectionID');
}


function selectItem(id, row) {
    updatePromoteButton();
}
function openPromoteModal(e) {
    const checkedCount = $('.student-checkbox:checked').length;
    if (checkedCount === 0) {
        e.preventDefault();
        Swal.fire({
            icon: 'warning',
            title: 'No Data',
            text: 'Please select at least one student.',
            confirmButtonColor: '#3085d6'
        });

        //alert('Please select at least one student.');
        return;
    }

    const nextSession = $('#NextSessionID').val();
    const nextClass = $('#NextClassID').val();
    const nextSection = $('#NextSectionID').val();

    if (!nextSession || !nextClass || !nextSection) {
        e.preventDefault();
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Please select Promote In Session, Class, and Section in the filter dropdown.',
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn btn-danger' },
            buttonsStyling: false
        });
        //alert('Please select Promote In Session, Class, and Section in the filter dropdown.');
        return;
    }

    // Allow default behavior (opening modal)
    bootstrap.Modal.getOrCreateInstance(document.getElementById('promote')).show();
}

async function confirmPromotion() {
    const nextSession = $('#hdnNEXTSESSIONTITLE').val();
    const nextClass = $('#hdnNEXTCLASSID').val();
    const nextSection = $('#FilterSectionID').val();
    const students = [];
    $('.student-checkbox:checked').each(function () {
        const row = $(this).closest('tr');
        const studentId = row.data('id');
        students.push({
            StudentID: studentId,
            Result: row.find(`input[name="pass_${studentId}"]:checked`).val(),
            NextStatus: row.find(`input[name="cont_${studentId}"]:checked`).val()
        });

    });

    const data = {
        CurrentClassID: parseInt($('#FilterClassID').val()),
        CurrentSectionID: parseInt($('#FilterSectionID').val()),
        NextSessionID: parseInt(nextSession),
        NextClassID: parseInt(nextClass),
        NextSectionID: parseInt(nextSection),
        Students: students
    };


    try {

        const r = await fetch('/Academics/SavePromotion', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        const res = await r.json();
        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'Promotion student has been added successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.reload();
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save promotion student.',
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

// Replace .prop('disabled') with a class-based approach for <a> tags

function updatePromoteButton() {
    const checkedCount = $('.student-checkbox:checked').length;

    $('#btnSave').prop('disabled', checkedCount === 0);

}