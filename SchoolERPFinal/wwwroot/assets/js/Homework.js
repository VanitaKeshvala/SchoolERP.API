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
    const classEl = document.getElementById('ddlFilterClasses');
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

    const classEl = document.getElementById('ddlFilterClasses');
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
        appliedFilters['ddlFilterClasses'] = {
            label: 'Class',
            text: classEl.options[classEl.selectedIndex]?.text || classEl.value
        };
    } else delete appliedFilters['ddlFilterClasses'];

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
document.addEventListener('DOMContentLoaded', async () => {

    // ── Restore Mode tab state ──────────────────────────────────────
    const currentMode = getStoredMode();
    const hdnMode = document.getElementById('hdnMode');
    if (hdnMode) hdnMode.value = currentMode;
    isUpcomingTab = (currentMode === 'UPCOMING');

    document.getElementById('tabUpcoming')?.classList.toggle('active', currentMode === 'UPCOMING');
    document.getElementById('tabClose')?.classList.toggle('active', currentMode === 'CLOSE');

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
    if (classId)
    {
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


const permCanAdd = window.canAdd;
const permCanEdit = window.canEdit;
const permCanDelete = window.canDelete;

const IV = {
    clearMap: (map) => Object.keys(map).forEach(k => {
        document.getElementById(k)?.classList.remove('is-invalid');
        const err = document.getElementById(map[k]); if (err) err.textContent = '';
    }),
    setFieldError: (id, errId, msg) => {
        document.getElementById(id)?.classList.add('is-invalid');
        const err = document.getElementById(errId); if (err) err.textContent = msg;
    },
    setNotice: (id, msg, type = 'danger') => {
        const el = document.getElementById(id);
        if (el) el.innerHTML = `<div class="alert alert-${type} alert-dismissible fade show" role="alert">${msg}<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>`;
    },
    clearNotice: (id) => { const el = document.getElementById(id); if (el) el.innerHTML = ''; }
};

const fieldMap = {
    ddlClass: 'errDdlClass',
    ddlSection: 'errDdlSection',
    ddlSubjectGroup: 'errDdlSubjectGroup',
    ddlSubject: 'errDdlSubject',
    txtHomeworkDate: 'errTxtHomeworkDate',
    txtSubmissionDate: 'errTxtSubmissionDate',
    txtDescription: 'errTxtDescription'
};


let selectedId = 0;
let isUpcomingTab = true;
let editorInstance;

// Custom Base64 Upload Adapter
class MyUploadAdapter {
    constructor(loader) {
        this.loader = loader;
    }
    upload() {
        return this.loader.file
            .then(file => new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = () => {
                    resolve({ default: reader.result });
                };
                reader.onerror = error => reject(error);
                reader.onabort = () => reject();
                reader.readAsDataURL(file);
            }));
    }
    abort() { }
}

function MyCustomUploadAdapterPlugin(editor) {
    editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
        return new MyUploadAdapter(loader);
    };
}

$(document).ready(function () {
    const table = $('#homeworkTable').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } }
        ],
        searching: true,
        paging: false,
        info: false
    });

    // Custom Search function for Tabs
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            if (settings.nTable.id !== 'homeworkTable') return true;
            const row = settings.aoData[dataIndex].nTr;
            const isActive = (row.getAttribute('data-active') || '').toLowerCase() === 'true';
            const submissionDate = row.getAttribute('data-submission');
            const today = new Date().toLocaleDateString('en-CA');
            if (isUpcomingTab) {
                return (isActive && submissionDate >= today);
            } else {
                return (!isActive || submissionDate < today);
            }
        }
    );

   
    ClassicEditor
        .create(document.querySelector('#txtDescription'), {
            extraPlugins: [MyCustomUploadAdapterPlugin],
            toolbar: [
                'heading', '|', 'bold', 'italic', 'link', 'bulletedList', 'numberedList', 'blockQuote', 'insertTable', 'uploadImage', 'undo', 'redo'
            ]
        })
        .then(editor => {
            editorInstance = editor;
        })
        .catch(error => {
            console.error(error);
        });
});



function selectItem(id, row) {
    selectedId = id;
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


async function editSelected() {
    if (selectedId === 0) return;
    IV.clearMap(fieldMap);
    IV.clearNotice('modalNotice');
    try {

        if (selectedId) {
            location.href = `/Homework/Add/${selectedId}`;
        }

    } catch (err) {
        console.error(err);
    }
}
async function saveHomework() {
    IV.clearMap(fieldMap);
    const description = editorInstance ? editorInstance.getData() : '';

    const homeworkID = parseInt(document.getElementById('txtHomeworkId').value);
    const classID = parseInt(document.getElementById('ddlClass').value);
    const sectionID = parseInt(document.getElementById('ddlSection').value);
    const subjectGroupID = parseInt(document.getElementById('ddlSubjectGroup').value);
    const subjectID = parseInt(document.getElementById('ddlSubject').value);
    const homeworkDate = document.getElementById('txtHomeworkDate').value;
    const submissionDate = document.getElementById('txtSubmissionDate').value;
    const maxMarksRaw = document.getElementById('txtMaxMarks').value;
    const isActive = document.getElementById('chkIsActive').checked;

    let hasError = false;
    if (!classID) { IV.setFieldError('ddlClass', 'errDdlClass', 'Please select a class'); hasError = true; }
    if (!sectionID) { IV.setFieldError('ddlSection', 'errDdlSection', 'Please select a section'); hasError = true; }
    if (!subjectGroupID) { IV.setFieldError('ddlSubjectGroup', 'errDdlSubjectGroup', 'Please select a subject group'); hasError = true; }
    if (!subjectID) { IV.setFieldError('ddlSubject', 'errDdlSubject', 'Please select a subject'); hasError = true; }
    if (!homeworkDate) { IV.setFieldError('txtHomeworkDate', 'errTxtHomeworkDate', 'Please select a homework date'); hasError = true; }
    if (!submissionDate) { IV.setFieldError('txtSubmissionDate', 'errTxtSubmissionDate', 'Please select a submission date'); hasError = true; }

    if (homeworkDate && submissionDate) {
        if (new Date(submissionDate) < new Date(homeworkDate)) {
            IV.setFieldError('txtSubmissionDate', 'errTxtSubmissionDate', 'Submission date cannot be before homework date');
            hasError = true;
        }
    }

    if (!description.trim() || description === '<p>&nbsp;</p>' || description === '') {
        IV.setFieldError('txtDescription', 'errTxtDescription', 'Description is mandatory');
        hasError = true;
    }

    if (hasError) return;

    // ── Build multipart form: individual fields matching HomeworkUpsertRequest properties ──
    const formData = new FormData();
    formData.append('HomeworkID', homeworkID);
    formData.append('ClassID', classID);
    formData.append('SectionID', sectionID);
    formData.append('SubjectGroupID', subjectGroupID);
    formData.append('SubjectID', subjectID);
    formData.append('HomeworkDate', homeworkDate);
    formData.append('SubmissionDate', submissionDate);
    if (maxMarksRaw) formData.append('MaxMarks', parseFloat(maxMarksRaw));
    formData.append('Description', description);
    formData.append('IsActive', isActive);

    const fileInput = document.getElementById('fileAttachment');
    if (fileInput && fileInput.files.length > 0) {
        Array.from(fileInput.files).forEach(file => {
            formData.append('attachmentFiles', file);
        });
    }

    const $btn = $('#btnSaveHomework');
    const originalHtml = $btn.html();
    $btn.prop('disabled', true).html('<div class="spinner-border spinner-border-sm me-1"></div> Saving...');

    try {
        const resp = await fetch('/Homework/UpsertHomework', {
            method: 'POST',
            body: formData
            // Do NOT set Content-Type manually — the browser sets the multipart boundary automatically.
        });
        const res = await resp.json();
        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: res.message || 'Homework has been saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => window.location.href = '/Homework/Index');
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
    } catch (err) {
        console.error(err);
        $btn.prop('disabled', false).html(originalHtml);
    }
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
        alert("Please select at least one Country.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this Country record. This action cannot be undone!",
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
                url: '/Homework/DeleteHomework',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Homework record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete homework.', 'error');
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
                    url: '/Homework/ToggleHomeworkStatus',
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

// ========================================
// Mode (Upcoming / Close) — persisted like other filters
// ========================================
const MODE_KEY = 'appliedMode_homework';

function getStoredMode() {
    try { return sessionStorage.getItem(MODE_KEY) || 'UPCOMING'; }
    catch (e) { return 'UPCOMING'; }
}

function saveMode(mode) {
    try { sessionStorage.setItem(MODE_KEY, mode); }
    catch (e) { console.warn('sessionStorage unavailable'); }
}

function switchMode(mode) {
    isUpcomingTab = (mode === 'UPCOMING'); // keep in sync if still used anywhere client-side
    saveMode(mode);

    const hdnMode = document.getElementById('hdnMode');
    if (hdnMode) hdnMode.value = mode;

    // Reset paging when switching tabs
    const pageEl = document.getElementById('hdnPageIndex');
    if (pageEl) pageEl.value = 1;

    submitForm();
}

let deletedAttachmentIds = [];

function viewAttachment(filePath) {
    const ext = (filePath.split('.').pop() || '').toLowerCase();
    const url = filePath.startsWith('http') ? filePath : filePath;

    let html = '';
    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
        html = `<img src="${url}" class="img-fluid" style="max-height:70vh;" />`;
    } else if (ext === 'pdf') {
        html = `<iframe src="${url}" style="width:100%;height:70vh;" frameborder="0"></iframe>`;
    } else {
        html = `<a href="${url}" target="_blank" class="btn btn-primary">Open Document</a>`;
    }

    document.getElementById('attachmentPreviewBody').innerHTML = html;
    const modal = new bootstrap.Modal(document.getElementById('attachmentPreviewModal'));
    modal.show();
}

function removeAttachment(attachmentId, btn) {
    Swal.fire({
        title: 'Delete attachment?',
        text: 'This document will be removed from homework.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete'
    }).then((result) => {
        if (result.isConfirmed) {
            deletedAttachmentIds.push(attachmentId);
            const row = btn.closest('tr');
            row.remove();
        }
    });
}