function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#groupsTable')) {
        const table = $('#groupsTable').DataTable();
        table.button(index).trigger();
    }
}
$(async function () {
    const editClassId = $("#ddlClass").val();
    if (editClassId > 0) {

        // Load sections
        await loadSectionsByClass(editClassId, 'ddlSections');

        // Select saved sections
        $('#ddlSections')
            .val(editSectionIds)
            .trigger('change');

        // Select saved subjects
        $('#ddlSubjects')
            .val(editSubjectIds)
            .trigger('change');
    }

});
const permCanAdd = window.canAdd;
const permCanEdit = window.canEdit;
const permCanDelete = window.canDelete;
const allSections = window.allSections;

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
    clearNotice: (id) => { const el = document.getElementById(id); if (el) el.innerHTML = ''; },
    bindAutoClear: (map) => Object.keys(map).forEach(k => document.getElementById(k)?.addEventListener('input', () => {
        document.getElementById(k)?.classList.remove('is-invalid');
        const err = document.getElementById(map[k]); if (err) err.textContent = '';
    }))
};

const fieldMap = { txtName: 'errTxtName', ddlClass: 'errDdlClass', ddlSections: 'errDdlSections', ddlSubjects: 'errDdlSubjects' };
const itemModal = new bootstrap.Modal(document.getElementById('itemModal'));
let selectedId = 0;

$(document).ready(function () {
    const exportOpts = {
        columns: [1, 2, 3, 4, 5],
        format: {
            body: function (data, row, column, node) {
                const checkbox = node.querySelector('input.status-switch');
                if (checkbox) {
                    return checkbox.checked ? 'Active' : 'Inactive';
                }
                if (node.querySelectorAll('.badge').length > 0) {
                    return Array.from(node.querySelectorAll('.badge')).map(b => b.textContent.trim()).join(', ');
                }
                const badge = node.querySelector('.badge');
                if (badge) {
                    return badge.textContent.trim();
                }
                // Remove br or small-spaced sections and strip tags
                return data.replace(/<br\s*\/?>/gi, " ").replace(/<[^>]*>/g, "").replace(/\s+/g, " ").trim();
            }
        }
    };

    $('#groupsTable').DataTable({
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

    $('.select2').each(function () {
        const parent = $(this).closest('.modal').length ? $(this).closest('.modal') : $(this).closest('.dropdown-menu');
        $(this).select2({
            dropdownParent: parent,
            placeholder: "Select",
            allowClear: true
        });
    });

    // Prevent dropdown from closing when clicking inside
    $('.filter-dropdown').on('click', function (e) {
        e.stopPropagation();
    });
    IV.bindAutoClear(fieldMap);
});

function selectItem(id, row) {
    IV.clearNotice('pageNotice');
    selectedId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light-primary'));
    row.classList.add('bg-light-primary');
    //row.querySelector('input[type="radio"]').checked = true;
    if (permCanEdit) document.getElementById('btnEdit').disabled = false;
    if (permCanDelete) document.getElementById('btnDelete').disabled = false;
    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    // Edit sirf 1 record select hone par
    $('#btnEdit').prop('disabled', checkedCount !== 1);
    $('#btnActive').prop('disabled', checkedCount === 0);
    $('#btnInactive').prop('disabled', checkedCount === 0);
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

function openAddModal() {
    if (!permCanAdd) { IV.setNotice('pageNotice', 'No permission to add.'); return; }
    IV.clearNotice('pageNotice');
    IV.clearMap(fieldMap);
    IV.clearNotice('modalNotice');
    document.getElementById('modalTitle').textContent = 'Add Subject Group';
    document.getElementById('itemForm').reset();
    document.getElementById('txtItemId').value = "0";
    document.getElementById('isActive').checked = true;
    $('#ddlSections').empty().trigger('change');
    $('#ddlSubjects').val(null).trigger('change');
    itemModal.show();
}

async function editSelected() {
    if (!permCanEdit) { IV.setNotice('pageNotice', 'No permission to edit.'); return; }
    if (selectedId === 0) return;
    IV.clearNotice('pageNotice');
    IV.clearMap(fieldMap);
    IV.clearNotice('modalNotice');
    try {

        if (selectedId) {
            location.href = `/Academics/AddSubjectGroup/${selectedId}`;
        }

    } catch (err) {
        console.error(err);
    }
}

async function saveItem() {
    const sid = parseInt(document.getElementById('txtItemId').value);
    IV.clearMap(fieldMap);

    const name = document.getElementById('txtName').value.trim();
    const classId = document.getElementById('ddlClass').value;
    const sectionIds = $('#ddlSections').val() || [];
    const subjectIds = $('#ddlSubjects').val() || [];

    let hasError = false;
    if (!name) { IV.setFieldError('txtName', 'errTxtName', 'Name is required.'); hasError = true; }
    if (!classId) { IV.setFieldError('ddlClass', 'errDdlClass', 'Class is required.'); hasError = true; }
    if (sectionIds.length === 0) { IV.setFieldError('ddlSections', 'errDdlSections', 'At least one section is required.'); hasError = true; }
    if (subjectIds.length === 0) { IV.setFieldError('ddlSubjects', 'errDdlSubjects', 'At least one subject is required.'); hasError = true; }

    if (hasError) return;

    const data = {
        subjectGroupID: sid,
        name: name,
        classID: parseInt(classId),
        description: document.getElementById('txtDescription').value,
        isActive: document.getElementById('isActive').checked,
        sectionIds: sectionIds.map(v => parseInt(v)),
        subjectIds: subjectIds.map(v => parseInt(v))
    };

    try {

        const r = await fetch('/Academics/SaveSubjectGroup', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        const res = await r.json();
        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'Subject group has been added successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = '/Academics/SubjectGroup';
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save subject group.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
        }

    } catch (err) {
        console.error(err);
        IV.setNotice('classModalNotice', 'Could not save subject group.');
    }
}

async function deleteSelected() {
    if (!permCanDelete) { IV.setNotice('pageNotice', 'No permission to delete.'); return; }

    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one subject group.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this subject group record. This action cannot be undone!",
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
                url: '/Academics/DeleteSubjectGroup',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Subject group record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete subject group.', 'error');
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
    if (!permCanEdit) { location.reload(); return; }

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
                    url: '/Academics/ToggleSubjectGroupStatus',
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

function filterTable() {
    const search = document.getElementById('txtSearchInput').value.toLowerCase();
    document.querySelectorAll('#groupsTable tbody tr').forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(search) ? '' : 'none';
    });
    renderFilterBadges();
}

function applyFilter() {
    const classId = document.getElementById('filterClass').value;
    const sectionId = document.getElementById('filterSection').value;
    const className = document.querySelector(`#filterClass option[value="${classId}"]`)?.getAttribute('data-name');
    const sectionName = document.querySelector(`#filterSection option[value="${sectionId}"]`)?.textContent;

    document.querySelectorAll('#groupsTable tbody tr').forEach(row => {
        let show = true;
        if (className) {
            const rowClass = row.cells[2].querySelector('span').textContent;
            if (rowClass !== className) show = false;
        }
        if (show && sectionName && sectionId) {
            const rowSections = row.cells[2].querySelector('small').textContent;
            if (!rowSections.includes(sectionName)) show = false;
        }
        row.style.display = show ? '' : 'none';
    });
    renderFilterBadges();
    const toggle = document.getElementById('btnFilterToggle');
    if (toggle) {
        bootstrap.Dropdown.getOrCreateInstance(toggle).hide();
    }
}

// --- Filter Badges Logic ---
function renderFilterBadges() {
    const container = document.getElementById('badgeContainer');
    const mainContainer = document.getElementById('activeFilterBadges');
    if (!container) return;
    container.innerHTML = '';

    const filters = [
        { id: 'txtSearchInput', label: 'Search', type: 'text' },
        { id: 'filterClass', label: 'Class', type: 'select' },
        { id: 'filterSection', label: 'Section', type: 'select' }
    ];

    let activeCount = 0;

    filters.forEach(f => {
        const el = document.getElementById(f.id);
        if (!el) return;

        let value = el.value;
        let text = '';

        if (f.type === 'text') {
            if (value.trim()) {
                text = value;
                activeCount++;
            }
        } else if (f.type === 'select') {
            if (value && value !== 'all' && value !== '') {
                const selectedOption = el.options[el.selectedIndex];
                if (selectedOption && selectedOption.value !== "") {
                    text = selectedOption.text;
                    activeCount++;
                }
            }
        }

        if (text) {
            const badge = document.createElement('span');
            badge.className = 'badge bg-primary-subtle text-primary border border-primary-subtle d-flex align-items-center gap-1 fw-medium px-2 py-1';
            badge.innerHTML = `${f.label}: ${text} <i class="ti ti-x ms-1 cursor-pointer" onclick="removeFilter('${f.id}')" style="font-size: 10px;"></i>`;
            container.appendChild(badge);
        }
    });

    if (activeCount > 0) {
        mainContainer.style.display = 'block';
    } else {
        mainContainer.style.display = 'none';
    }
}

function removeFilter(filterId) {
    const el = document.getElementById(filterId);
    if (!el) return;

    if (el.tagName === 'INPUT') {
        el.value = '';
        filterTable();
    } else {
        el.value = '';
        if (filterId === 'filterClass') {
            $('#filterSection').html('<option value="">All Sections</option>');
        }
        if (window.jQuery && jQuery(el).data('select2')) {
            jQuery(el).trigger('change.select2');
        }
        applyFilter();
    }
}

function resetAllFilters() {
    document.getElementById('txtSearchInput').value = '';
    document.getElementById('filterClass').value = '';
    document.getElementById('filterSection').value = '';

    $('.select2').trigger('change.select2');

    // Sync both
    filterTable();
    applyFilter();
}