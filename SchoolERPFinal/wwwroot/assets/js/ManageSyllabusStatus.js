function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#homeworkTable')) {
        const table = $('#homeworkTable').DataTable();
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
    const classEl = document.getElementById('ddlClass');
    const sectionEl = document.getElementById('ddlSection');
    const subGroupEl = document.getElementById('ddlSubjectGroup');
    const subjectEl = document.getElementById('ddlSubject');


    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (classEl) document.getElementById('hdnClassID').value = classEl.value;
    if (sectionEl) document.getElementById('hdnSectionID').value = sectionEl.value;
    if (subGroupEl) document.getElementById('hdnSubjectGroupID').value = subGroupEl.value;
    if (subjectEl) document.getElementById('hdnSubjectID').value = subjectEl.value;

    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const searchEl = document.getElementById('txtSearchInput');

    const classEl = document.getElementById('ddlClass');
    const sectionEl = document.getElementById('ddlSection');
    const subGroupEl = document.getElementById('ddlSubjectGroup');
    const subjectEl = document.getElementById('ddlSubject');

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
    if (classEl?.value && classEl.value !== '') {
        appliedFilters['ddlClass'] = {
            label: 'Class',
            text: classEl.options[classEl.selectedIndex]?.text || classEl.value
        };
    } else delete appliedFilters['ddlClass'];

    // Section
    if (sectionEl?.value && sectionEl.value !== '') {
        appliedFilters['ddlSection'] = {
            label: 'Section',
            text: sectionEl.options[sectionEl.selectedIndex]?.text || sectionEl.value
        };
    } else delete appliedFilters['ddlSection'];

    // SubjectGroup
    if (subGroupEl?.value && subGroupEl.value !== '') {
        appliedFilters['ddlSubjectGroup'] = {
            label: 'SubjectGroup',
            text: subGroupEl.options[subGroupEl.selectedIndex]?.text || subGroupEl.value
        };
    } else delete appliedFilters['ddlSubjectGroup'];

    // Subject
    if (subjectEl?.value && subjectEl.value !== '') {
        appliedFilters['ddlSubject'] = {
            label: 'Subject',
            text: subjectEl.options[subjectEl.selectedIndex]?.text || subjectEl.value
        };
    } else delete appliedFilters['ddlSubject'];
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
    }
    else if (filterId === 'ddlClass') {
        const el = document.getElementById('ddlClass');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'ddlSubject') {
        const el = document.getElementById('ddlSubject');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'ddlFilterCompany') {
        const el = document.getElementById('ddlFilterCompany');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'ddlFilterCompany') {
        const el = document.getElementById('ddlFilterCompany');
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

    ['ddlFilterSessions', 'ddlFilterCompany', 'ddlClass', 'ddlSection', 'ddlSubjectGroup', 'ddlSubject'].forEach(id => {
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
document.addEventListener('DOMContentLoaded', async () => {


    // ── DataTable (export only) ───────────────────────────────────────
    try {
        if ($.fn.DataTable.isDataTable('#homeworkTable')) {
            $('#homeworkTable').DataTable().destroy();
        }
        $.fn.dataTable.ext.errMode = 'none';
        window.exportTable = $('#homeworkTable').DataTable({
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
    } catch (e) {
        console.error('DataTable init failed:', e);
    }

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

    const classId = $('#ddlClass').val();
    if (classId) {
        $('#ddlClass').trigger('change');
        await loadSections(classId, window.editSectionId);
    }
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

// ========================================
// Search input — debounced server submit
// ========================================

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



async function loadSections(classId) {
    const ddl = $('#ddlSection');
    ddl.empty().append(new Option('Select Section', ''));
    $('#ddlSubjectGroup').empty().append(new Option('Select Group', ''));
    $('#ddlSubject').empty().append(new Option('Select Subject', ''));

    if (!classId) return;

    const sectionId = $('#txtSectionID').val();

    try {
        const resp = await fetch(`/Academics/GetSectionsByClass?id=${classId}`);
        const res = await resp.json();

        if (res.success && res.data) {

            ddl.empty();
            ddl.append(new Option('-- Select Section --', ''));

            res.data.forEach(s => {
                ddl.append(new Option(s.sectionName, s.sectionID));
            });

            // Edit mode - select saved section
            if (sectionId) {
                ddl.val(sectionId).trigger('change');
            }
        }
    }
    catch (err) {
        console.error(err);
    }
}

async function loadSubjectGroups() {
    const classId = $('#ddlClass').val();
    const sectionId = $('#ddlSection').val();
    const sectionName = $('#ddlSection option:selected').text().trim();

    const ddl = $('#ddlSubjectGroup');
    ddl.empty().append(new Option('Select Group', ''));
    $('#ddlSubject').empty().append(new Option('Select Subject', ''));

    if (!classId || !sectionId) return;

    const subjectGroupId = $('#txtSubjectGroupID').val();

    try {
        const resp = await fetch('/Homework/GetSubjectGroups');
        const res = await resp.json();

        if (res.success && res.data) {

            ddl.empty();
            ddl.append(new Option('-- Select Subject Group --', ''));

            const filtered = res.data.filter(g =>
                g.classID == classId &&
                (g.sectionNames || "")
                    .split(',')
                    .map(s => s.trim())
                    .includes(sectionName)
            );

            filtered.forEach(g => {
                ddl.append(new Option(g.name, g.subjectGroupID));
            });

            // Edit mode - select saved Subject Group
            if (subjectGroupId) {
                ddl.val(subjectGroupId).trigger('change');
            }
        }
    }
    catch (err) {
        console.error(err);
    }
}

async function loadSubjects(groupId) {
    const ddl = $('#ddlSubject');
    ddl.empty().append(new Option('Select Subject', ''));

    if (!groupId) return;

    const subjectId = $('#txtSubjectID').val();

    try {
        const resp = await fetch(`/Homework/GetSubjectGroupByID?id=${groupId}`);
        const res = await resp.json();

        ddl.empty();
        ddl.append(new Option('-- Select Subject --', ''));

        if (res.success && Array.isArray(res.data) && res.data.length > 0) {

            res.data.forEach(s => {
                ddl.append(new Option(s.name, s.id));
            });

            // Edit mode - select saved Subject
            if (subjectId) {
                ddl.val(subjectId).trigger('change');
            }
        }
        else {
            ddl.append(new Option('No subjects found', ''));
        }
    }
    catch (err) {
        console.error(err);
    }
}


async function loadLesson(subjectId) {

    const ddl = $('#ddlLesson');
    ddl.empty().append(new Option('-- Select Lesson --', ''));

    const req = {
        ClassID: $('#ddlClass').val(),
        SectionID: $('#ddlSection').val(),
        SubjectGroupID: $('#ddlSubjectGroup').val(),
        SubjectID: subjectId
    };

    if (!req.ClassID || !req.SectionID || !req.SubjectGroupID || !req.SubjectID)
        return;

    try {

        const response = await fetch('/Topic/BindLessonDropDwonList', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(req)
        });

        const res = await response.json();

        ddl.empty().append(new Option('-- Select Lesson --', ''));

        if (res.success && Array.isArray(res.data)) {

            res.data.forEach(item => {
                ddl.append(new Option(item.lessonName, item.lessonMapId));
            });
        }
        else {
            ddl.append(new Option('No Lesson Found', ''));
        }

    } catch (e) {
        console.error(e);
    }
}





async function toggleStatus(id, isActive) {
    if (!permCanEdit) {
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
                    url: '/Topic/ToggleTopicCompleteStatus',
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