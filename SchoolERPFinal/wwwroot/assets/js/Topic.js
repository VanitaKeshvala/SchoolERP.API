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
        var lessonId = $("#txtLessonId").val();
        // Edit mode - select saved section
        if (lessonId > 0) {
            ddl.val(lessonId).trigger('change');
        }
    } catch (e) {
        console.error(e);
    }
}
// ------------------------------------------------------------------
// ROW MANAGEMENT (same pattern as the Timetable "Add New" row)
// ------------------------------------------------------------------
function addNewLessonRow() {
    const container = document.getElementById('lessonRowsContainer');
    const template = document.getElementById('lessonRowTemplate').innerHTML;
    container.insertAdjacentHTML('beforeend', template);
}
function deleteLessonRow(btn) {
    const $row = $(btn).closest('.lesson-row');
    const lessonMapId = parseInt($row.find('.lesson-map-id').val()) || 0;

    if (lessonMapId > 0) {
        // Existing (persisted) chapter: don't rip it out of the DOM outright.
        // Flag it for soft delete so it still gets sent to the server
        // on Save, mirroring IsDelete in SP_MST_LESSON_INSERTUPDATE.
        $row.find('.lesson-isdelete').val('1');
        $row.hide();
    } else {
        // Brand new, unsaved chapter row: just remove it from the page.
        $row.remove();
    }
}


// ------------------------------------------------------------------
// WHEN CLASS / SECTION / SUBJECTGROUP CHANGES, THE SUBJECT (AND
// THEREFORE THE MASTER LESSON) IS NO LONGER VALID FOR THIS SCREEN
// UNTIL A SUBJECT IS RE-SELECTED, SO RESET THE MASTER + ROWS.
// ------------------------------------------------------------------
function onScopeChanged() {
    var topicId = $("#hdnTopicId").val();
    if (topicId > 0) {
        // $('#lessonRowsContainer').empty();
    }
    else {
        addNewLessonRow();
    }

}


// ------------------------------------------------------------------
// SAVE ALL CHAPTER ROWS IN ONE CALL. MATCHES LessonPlanModelRequest
// (LessonId, ClassID, SectionID, SubjectGroupID, SubjectID, IsActive,
// LessonJson) POSTED AS FORM DATA TO /Homework/UpsertLessonPlan,
// WHICH IN TURN FEEDS SP_MST_LESSON_INSERTUPDATE.
// ------------------------------------------------------------------
async function saveAllLessons() {
    const classId = $('#ddlClass').val();
    const sectionId = $('#ddlSection').val();
    const subjectGroupId = $('#ddlSubjectGroup').val();
    const subjectId = $('#ddlSubject').val();
    const lessonId = parseInt($('#ddlLesson').val()) || 0;
    const topicId = parseInt($('#TopicId').val()) || 0;

    let hasError = false;

    $('#errDdlClass').text('');
    $('#errDdlSection').text('');
    $('#errDdlSubjectGroup').text('');
    $('#errDdlSubject').text('');
    $('#ddlClass, #ddlSection, #ddlSubjectGroup, #ddlSubject').removeClass('is-invalid');

    if (!classId) { $('#ddlClass').addClass('is-invalid'); $('#errDdlClass').text('Class is required'); hasError = true; }
    if (!sectionId) { $('#ddlSection').addClass('is-invalid'); $('#errDdlSection').text('Section is required'); hasError = true; }
    if (!subjectGroupId) { $('#ddlSubjectGroup').addClass('is-invalid'); $('#errDdlSubjectGroup').text('Subject Group is required'); hasError = true; }
    if (!subjectId) { $('#ddlSubject').addClass('is-invalid'); $('#errDdlSubject').text('Subject is required'); hasError = true; }

    if (hasError) {
        showToast('Please fill all required fields', 'warning');
        return;
    }

    // NOTE: only chapter-level fields go in LessonJson now. Class/Section/
    // SubjectGroup/Subject belong to the master and are sent once, top-level.
    const lessonChapters = [];
    let rowError = false;

    $('.lesson-row').each(function () {
        const $row = $(this);
        const isDelete = parseInt($row.find('.lesson-isdelete').val()) || 0;
        const lessonMapId = parseInt($row.find('.lesson-map-id').val()) || 0;
        const lessonName = $row.find('.lesson-name').val()?.trim();
        const description = $row.find('.lesson-description').val()?.trim();

        $row.find('.lesson-name').removeClass('is-invalid');

        // Rows marked for delete are always sent, even without a name.
        if (isDelete === 1) {
            if (lessonMapId > 0) {
                lessonChapters.push({
                    TopicMapId: lessonMapId,
                    TopicName: lessonName || '',
                    Description: description || '',
                    IsActive: 1,
                    IsDelete: 1
                });
            }
            return;
        }

        // Skip a completely blank, never-touched new row.
        if (!lessonName && lessonMapId === 0) {
            return;
        }

        if (!lessonName) {
            $row.find('.lesson-name').addClass('is-invalid');
            rowError = true;
            return;
        }

        lessonChapters.push({
            TopicMapId: lessonMapId,
            TopicName: lessonName,
            Description: description || '',
            IsActive: 1,
            IsDelete: 0
        });
    });

    if (rowError) {
        showToast('Please fill Lesson Name for every row', 'warning');
        return;
    }

    if (lessonChapters.length === 0) {
        showToast('Please add at least one lesson', 'warning');
        return;
    }

    showToast('Saving...', 'info');

    const formData = new FormData();
    formData.append('TopicId', topicId);
    formData.append('LessonId', lessonId);
    formData.append('ClassID', classId);
    formData.append('SectionID', sectionId);
    formData.append('SubjectGroupID', subjectGroupId);
    formData.append('SubjectID', subjectId);
    formData.append('IsActive', true);
    formData.append('TopicJson', JSON.stringify(lessonChapters));


    try {
        const res = await fetch('/Topic/UpsertTopic', {
            method: 'POST',
            body: formData
        }).then(r => r.json());

        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: res.message || 'Lesson plan has been saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => window.location.href = '/Topic/Index');
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save homework.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
            $btn.prop('disabled', false).html(originalHtml);
        }
    } catch (e) {
        showToast('Failed to save lessons', 'error');
        return;
    }


}

async function editSelected() {
    if (!permCanEdit) {
        IV.setNotice('classPageNotice', 'You do not have permission to edit.');
        return;
    }
    if (window.selectedClassId === 0) return;
    IV.clearNotice('classPageNotice');
    //clearModalValidation();
    try {

        if (window.selectedClassId) {
            location.href = `/Topic/Add/${window.selectedClassId}`;
        }

    } catch (err) {
        console.error(err);
    }
}


function selectItem(id, row) {
    IV.clearNotice('classPageNotice');
    window.selectedClassId = id;
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
}
async function deleteSelected() {
    if (!permCanDelete) {
        IV.setNotice('classPageNotice', 'You do not have permission to delete.');
        return;
    }

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
                url: '/Topic/DeleteTopic',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Topic record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete topic.', 'error');
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
                    url: '/Topic/ToggleTopicStatus',
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