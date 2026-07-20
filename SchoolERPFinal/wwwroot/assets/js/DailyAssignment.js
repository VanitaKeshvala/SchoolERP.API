// ========================================================================
// Daily Assignment — Add / Edit page
// Handles: field validation, create + edit save (multipart, multiple
// attachments), attachment preview popup, attachment removal.
// Remark / Evaluation Date are posted as-is on the admin side; on the
// student side the fields are read-only so they simply round-trip.
// ========================================================================


function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#assignmentsTable')) {
        const table = $('#assignmentsTable').DataTable();
        table.button(index).trigger();
    }
}
// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_dailyassignments';
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
    const sectionEl = document.getElementById('ddlFilterSection');
    const classEl = document.getElementById('ddlFilterClassID');
   
    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (sectionEl) document.getElementById('hdnSectionID').value = sectionEl.value;
    if (classEl) document.getElementById('hdnClassID').value = classEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sectionEl = document.getElementById('ddlFilterSection');
    const classEl = document.getElementById('ddlFilterClassID');
    const sectionID = document.getElementById('ddlFilterSectionID');
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
        appliedFilters['ddlFilterSection'] = {
            label: 'Section',
            text: sectionEl.options[sectionEl.selectedIndex]?.text || sectionEl.value
        };
    } else delete appliedFilters['ddlFilterSection'];
    if (classEl?.value && classEl.value !== '') {
        appliedFilters['ddlFilterClassID'] = {
            label: 'Section',
            text: classEl.options[classEl.selectedIndex]?.text || classEl.value
        };
    } else delete appliedFilters['ddlFilterClassID'];

    if (sectionID?.value && sectionID.value !== '') {
        appliedFilters['ddlFilterSectionID'] = {
            label: 'Section',
            text: sectionID.options[sectionID.selectedIndex]?.text || sectionID.value
        };
    } else delete appliedFilters['ddlFilterSectionID'];

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
    } else if (filterId === 'ddlFilterSection') {
        const el = document.getElementById('ddlFilterSection');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'ddlFilterClassID') {
        const el = document.getElementById('ddlFilterClassID');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'ddlFilterSection') {
        const el = document.getElementById('ddlFilterSection');
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

    ['ddlFilterSessions', 'ddlFilterCompany', 'ddlFilterSection', 'ddlFilterClassID', 'ddlFilterSection'].forEach(id => {
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
// ========================================
// Student — row selection (Edit/Delete)
// ========================================
document.addEventListener('DOMContentLoaded', () => {
    const selectAll = document.getElementById('chkSelectAll');
    const btnEdit = document.getElementById('btnEdit');
    const btnDelete = document.getElementById('btnDelete');

    function getCheckedBoxes() {
        return Array.from(document.querySelectorAll('.row-checkbox:checked'));
    }

    function updateActionButtons() {
        const checked = getCheckedBoxes();

        // Edit only makes sense for exactly one selected, non-evaluated row
        if (btnEdit) btnEdit.disabled = checked.length !== 1;

        // Delete can apply to one or more selected rows
        if (btnDelete) btnDelete.disabled = checked.length === 0;
    }

    // Select All — only checks rows that are NOT evaluated (not disabled)
    selectAll?.addEventListener('change', function () {
        const boxes = document.querySelectorAll('.row-checkbox:not(:disabled)');
        boxes.forEach(cb => cb.checked = selectAll.checked);
        updateActionButtons();
    });

    // Individual row checkbox change
    document.addEventListener('change', function (e) {
        if (!e.target.classList.contains('row-checkbox')) return;
        updateActionButtons();

        // Keep "select all" in sync
        const allBoxes = document.querySelectorAll('.row-checkbox:not(:disabled)');
        const allChecked = allBoxes.length > 0 && Array.from(allBoxes).every(cb => cb.checked);
        if (selectAll) selectAll.checked = allChecked;
    });

    updateActionButtons();
});


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
    ddlSubject: 'errDdlSubject',
    txtDescription: 'errTxtDescription',
    txtEvaluationDate: 'errTxtEvaluationDate'
};

// Attachment ids the user removed from an existing record (sent to server so
// it can soft-delete / drop them on save).
let deletedAttachmentIds = [];

function viewAttachment(filePath) {
    const ext = (filePath.split('.').pop() || '').toLowerCase();
    const url = filePath;

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
        text: 'This document will be removed from the assignment.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete',
        cancelButtonText: 'Cancel',
        customClass: {
            confirmButton: 'btn btn-danger me-2',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            deletedAttachmentIds.push(attachmentId);
            const row = btn.closest('tr');
            row.remove();
        }
    });
}

async function saveHomework() {
    IV.clearMap(fieldMap);
    IV.clearNotice('pageNotice');




    const assignmentId = parseInt(document.getElementById('txtHomeworkId').value) || 0;
    const sectionId = parseInt(document.getElementById('txtSectionID').value) || 0;
    const studentID = parseInt(document.getElementById('txtStudentID').value) || 0;
    const classId = parseInt(document.getElementById('txtClassID').value) || 0;
    const subjectGroupId = parseInt(document.getElementById('txtSubjectGroupID').value) || 0;
    //const subjectId = parseInt(document.getElementById('ddlSubject').value) || 0;
    const subjectId = document.getElementById('ddlSubject')
        ? document.getElementById('ddlSubject').value.trim()
        : document.getElementById('txtSectionID').value.trim();
    //const title = document.getElementById('txtTitle')?.value.trim() || '';
    const title = document.getElementById('txtTitle')
        ? document.getElementById('txtTitle').value
        : document.getElementById('hdnTitle').value.trim();
    const description = document.getElementById('txtDescription')
        ? document.getElementById('txtDescription').value
        : document.getElementById('hdnDescription').value.trim();
    const remark = document.getElementById('txtRemark')?.value.trim() || '';
    const evaluationDate = document.getElementById('txtEvaluationDate')
        ? document.getElementById('txtEvaluationDate').value.trim()
        : null;
    const isActive = true;// document.getElementById('chkIsActive').checked;

    // ── Validation ──────────────────────────────────────────────────
    let hasError = false;
    if (!subjectId) { IV.setFieldError('ddlSubject', 'errDdlSubject', 'Please select a subject'); hasError = true; }
    if (!description) { IV.setFieldError('txtDescription', 'errTxtDescription', 'Description is mandatory'); hasError = true; }
    //if (!evaluationDate) { IV.setFieldError('txtEvaluationDate', 'errTxtEvaluationDate', 'Please select an evaluation date'); hasError = true; }

    if (hasError) return;

    // ── Build multipart form matching AssignmentUpsertRequest ─────────
    const formData = new FormData();
    formData.append('AssignmentID', assignmentId);
    formData.append('SectionID', sectionId);
    formData.append('ClassID', classId);
    formData.append('StudentID', studentID);
    formData.append('SubjectGroupID', subjectGroupId);
    formData.append('SubjectID', subjectId);
    formData.append('Title', title);
    formData.append('Description', description);
    formData.append('Remark', remark);
    formData.append('EvaluationDate', evaluationDate);
    formData.append('EvaluationDate', evaluationDate);
    formData.append('EvaluationDate', evaluationDate);
    formData.append('IsActive', isActive);

    if (deletedAttachmentIds.length > 0) {
        formData.append('DeletedAttachmentIds', deletedAttachmentIds.join(','));
    }

    const fileInput = document.getElementById('fileAttachment');
    if (fileInput && fileInput.files.length > 0) {
        Array.from(fileInput.files).forEach(file => {
            formData.append('attachmentFiles', file);
        });
    }

    const $btn = document.getElementById('btnSaveHomework');
    const originalHtml = $btn ? $btn.innerHTML : '';
    if ($btn) {
        $btn.disabled = true;
        $btn.innerHTML = '<div class="spinner-border spinner-border-sm me-1"></div> Saving...';
    }

    try {
        const resp = await fetch('/dailyAssignment/UpsertDailyAssignment', {
            method: 'POST',
            body: formData
            // Do NOT set Content-Type manually — the browser sets the multipart boundary automatically.
        });
        const res = await resp.json();

        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: res.message || 'Assignment has been saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => window.location.href = '/DailyAssignment/Index');
        } else {
            IV.setNotice('pageNotice', res.message || 'Failed to save assignment.');
            if ($btn) { $btn.disabled = false; $btn.innerHTML = originalHtml; }
        }
    } catch (err) {
        console.error(err);
        IV.setNotice('pageNotice', 'An unexpected error occurred while saving.');
        if ($btn) { $btn.disabled = false; $btn.innerHTML = originalHtml; }
    }
}
async function loadSectionsByClass(classId, targetId) {
    const ddl = $(`#${targetId}`);
    ddl.empty();
    if (!classId) {
        ddl.append(new Option(targetId.includes('filter') ? 'All Sections' : 'Select', ''));
        ddl.trigger('change');
        return null;
    }
    //window.editSectionIds
    try {
        const resp = await fetch(`/Academics/GetSectionsByClass?id=${classId}`);
        const res = await resp.json();
        if (res.success && res.data) {
            if (targetId.includes('filter')) ddl.append(new Option('All Sections', ''));

            res.data.forEach(s => {
                ddl.append(new Option(s.sectionName, s.sectionID));
            });
        }
        ddl.trigger('change');
        return res.data || [];
    } catch (err) {
        console.error(err);
        return [];
    }
}

function editSelected() {
    const checked = document.querySelectorAll('.row-checkbox:checked');
    if (checked.length !== 1) {
        Swal.fire('Select one record', 'Please select exactly one assignment to edit.', 'warning');
        return;
    }

    const checkbox = checked[0];
    // Extra safety — even though disabled checkboxes can't be checked,
    // this guards against any stale/manipulated DOM state.
    if (checkbox.dataset.evaluated === 'true') {
        Swal.fire('Cannot Edit', 'This assignment has already been evaluated and cannot be edited.', 'warning');
        return;
    }

    const assignmentId = checkbox.value;
    window.location.href = `/dailyAssignment/Add/${assignmentId}`;
}

function deleteSelected() {
    const checked = Array.from(document.querySelectorAll('.row-checkbox:checked'));
    if (checked.length === 0) return;

    // Safety check — block if any selected row is evaluated
    const blocked = checked.filter(cb => cb.dataset.evaluated === 'true');
    if (blocked.length > 0) {
        Swal.fire('Cannot Delete', 'One or more selected assignments have already been evaluated and cannot be deleted.', 'warning');
        return;
    }

    const ids = checked.map(cb => cb.value);

    Swal.fire({
        title: `Delete ${ids.length} assignment(s)?`,
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete',
        cancelButtonText: 'Cancel',
        customClass: {
            confirmButton: 'btn btn-danger me-2',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (!result.isConfirmed) return;

        fetch('/dailyAssignment/DeleteDailyAssignments', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(ids)
        })
            .then(resp => resp.json())
            .then(res => {
                if (res.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: res.message || 'Selected assignment(s) removed.',
                        confirmButtonText: 'OK',
                        customClass: { confirmButton: 'btn btn-success' },
                        buttonsStyling: false
                    }).then(() => window.location.reload());
                } else {
                    Swal.fire('Error', res.message || 'Failed to delete.', 'error');
                }
            })
            .catch(err => {
                console.error(err);
                Swal.fire('Error', 'An unexpected error occurred while deleting.', 'error');
            });
    });
}
document.addEventListener('click', function (e) {
    const btn = e.target.closest('.view-attachment-btn');
    if (!btn) return;

    const raw = btn.getAttribute('data-attachments');
    const container = document.getElementById('attachmentContainer');
    container.innerHTML = '';

    if (!raw) {
        container.innerHTML = '<p class="text-muted">No attachments found.</p>';
        return;
    }

    let attachments = [];
    try {
        attachments = JSON.parse(raw);
    } catch (err) {
        container.innerHTML = '<p class="text-danger">Unable to load attachments.</p>';
        return;
    }

    if (!attachments.length) {
        container.innerHTML = '<p class="text-muted">No attachments found.</p>';
        return;
    }

    attachments.forEach(a => {
        const fullUrl = a.ATTACHMENTPATH; // prepend base URL below if needed
        const isImage = a.ATTACHMENTTYPE && a.ATTACHMENTTYPE.startsWith('image/');

        const wrapper = document.createElement('div');
        wrapper.classList.add('text-center');

        if (isImage) {
            wrapper.innerHTML = `
                <a href="${fullUrl}" target="_blank">
                    <img src="${fullUrl}" alt="${a.ATTACHMENTNAME}" 
                         style="max-width:150px; max-height:150px; border-radius:6px;" 
                         class="border"/>
                </a>
                <div class="small mt-1">${a.ATTACHMENTNAME}</div>
            `;
        } else {
            wrapper.innerHTML = `
                <a href="${fullUrl}" target="_blank" class="d-block">
                    <i class="fa fa-file fa-3x"></i>
                    <div class="small mt-1">${a.ATTACHMENTNAME}</div>
                </a>
            `;
        }

        container.appendChild(wrapper);
    });
});