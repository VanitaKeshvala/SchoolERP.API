function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#tblCountry')) {
        const table = $('#tblCountry').DataTable();
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
    const hostelEl = document.getElementById('ddlHostel');
    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (hostelEl) document.getElementById('hdnHostelID').value = hostelEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const hostelEl = document.getElementById('ddlHostel');
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

    // Hostel
    if (hostelEl?.value && hostelEl.value !== '') {
        appliedFilters['ddlHostel'] = {
            label: 'Company',
            text: hostelEl.options[hostelEl.selectedIndex]?.text || hostelEl.value
        };
    } else delete appliedFilters['ddlHostel'];


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
    else if (filterId === 'ddlHostel') {
        const el = document.getElementById('ddlHostel');
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

    ['ddlFilterSessions', 'ddlFilterCompany','ddlHostel'].forEach(id => {
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
    // Select2 init
    try {
        if (window.jQuery && typeof jQuery.fn.select2 === 'function') {
            jQuery('#ddlHostel, #ddlCountry, #ddlState, #ddlGender').select2({
                width: '100%',
                allowClear: true,
                placeholder: 'Select'
            });
        }
    } catch (e) {
        console.warn('Select2 init skipped:', e);
    }

    // Photo preview
    document.getElementById('filePhoto')?.addEventListener('change', function (e) {
        const file = e.target.files[0];
        const preview = document.getElementById('imgPhotoPreview');
        if (!file) { return; }

        const allowed = ['image/png', 'image/jpeg', 'image/jpg'];
        if (!allowed.includes(file.type)) {
            IV.setFieldError('filePhoto', 'errfilePhoto', 'Only JPG/PNG images are allowed.');
            this.value = '';
            return;
        }
        if (file.size > 2 * 1024 * 1024) {
            IV.setFieldError('filePhoto', 'errfilePhoto', 'Photo must be under 2MB.');
            this.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (ev) {
            preview.src = ev.target.result;
            preview.style.display = 'inline-block';
        };
        reader.readAsDataURL(file);
    });

    // Country -> State cascading
    const ddlCountry = document.getElementById('ddlCountry');
    ddlCountry?.addEventListener('change', function () {
        loadStatesByCountry(this.value, 0);
    });

    // On page load, if editing and country already selected, load its states
    if (window.editSelectedCountryId && window.editSelectedCountryId > 0) {
        loadStatesByCountry(window.editSelectedCountryId, window.editSelectedStateId);
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
    clearNotice: (id) => { const el = document.getElementById(id); if (el) el.innerHTML = ''; }
};

// Map of field id -> error div id, used for clearing/validating together
const wardenFieldMap = {
    ddlHostel: 'errddlHostel',
    txtWardenName: 'errtxtWardenName',
    ddlGender: 'errddlGender',
    txtWardenContact: 'errtxtWardenContact',
    txtWardenEmail: 'errtxtWardenEmail',
    txtEmergencyContactNumber: 'errtxtEmergencyContactNumber',
    txtBirthDate: 'errtxtBirthDate',
    txtJoiningDate: 'errtxtJoiningDate',
    txtExperienceYears: 'errtxtExperienceYears',
    txtAadharNumber: 'errtxtAadharNumber',
    filePhoto: 'errfilePhoto',
    txtPinCode: 'errtxtPinCode'
};

function clearWardenValidation() {
    IV.clearMap(wardenFieldMap);
    IV.clearNotice('wardenPageNotice');
}

// Auto-clear invalid state as user types
Object.keys(wardenFieldMap).forEach(id => {
    document.getElementById(id)?.addEventListener('input', () => {
        document.getElementById(id)?.classList.remove('is-invalid');
        const err = document.getElementById(wardenFieldMap[id]); if (err) err.textContent = '';
    });
});

window.selectedClassId = 0;
const classFieldMap = { txtClassName: 'errTxtClassName', ddlSections: 'errDdlSections' };



$(document).ready(function () {
    $('#classesTable').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3] } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3] } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3] } }
        ],
        searching: false,
        paging: false,
        info: false
    });

    $('#ddlSections').select2({
        placeholder: "Select Sections",
        allowClear: true
    });
});

function clearModalValidation() {
    IV.clearMap(classFieldMap);
    IV.clearNotice('classModalNotice');
}

IV.bindAutoClear(classFieldMap);

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



async function editSelected() {
    if (!permCanEdit) {
        IV.setNotice('classPageNotice', 'You do not have permission to edit.');
        return;
    }
    if (window.selectedClassId === 0) return;
    IV.clearNotice('classPageNotice');
    clearModalValidation();
    try {

        if (window.selectedClassId) {
            location.href = `/Wardens/Add/${window.selectedClassId}`;
        }

    } catch (err) {
        console.error(err);
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
                url: '/Wardens/DeleteWardens',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Wardens record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete Wardens.', 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error!', 'An unexpected error occurred.', 'error');
                }
            });
        }
    });
}

function fileToBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result); // includes "data:image/png;base64," prefix
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

// ── Save ─────────────────────────────────────────────────────────────
async function saveItem() {
    const wid = parseInt(document.getElementById('txtItemId').value);
    if (wid === 0 && !permCanAdd) {
        IV.setNotice('wardenPageNotice', 'You do not have permission to add.');
        return;
    }
    if (wid > 0 && !permCanEdit) {
        IV.setNotice('wardenPageNotice', 'You do not have permission to edit.');
        return;
    }

    if (!validateWardenForm()) {
        return;
    }

    const saveBtn = document.querySelector('.top-btn.btn-success');
    if (saveBtn) { saveBtn.disabled = true; saveBtn.innerHTML = '<i class="ti ti-loader-2"></i> Saving...'; }

    try {
        // Upload photo first if a new file was chosen
        const fileInput = document.getElementById('filePhoto');
        let photoBase64 = null;
        let photoFileName = null;

        if (fileInput.files.length > 0) {
            const file = fileInput.files[0];
            photoBase64 = await fileToBase64(file); // "data:image/jpeg;base64,...."
            photoFileName = file.name;
        }

        const data = {
            wardenId: wid,
            hostelID: parseInt(document.getElementById('ddlHostel').value) || null,
            wardenName: document.getElementById('txtWardenName').value.trim(),
            wardenContact: document.getElementById('txtWardenContact').value.trim(),
            wardenEmail: document.getElementById('txtWardenEmail').value.trim() || null,
            address: document.getElementById('txtAddress').value.trim() || null,
            countryId: parseInt(document.getElementById('ddlCountry').value) || null,
            stateId: parseInt(document.getElementById('ddlState').value) || null,
            pinCode: document.getElementById('txtPinCode').value.trim() || null,
            gender: document.getElementById('ddlGender').value,
            birthDate: document.getElementById('txtBirthDate').value || null,
            joiningDate: document.getElementById('txtJoiningDate').value || null,
            qualification: document.getElementById('txtQualification').value.trim() || null,
            experienceYears: document.getElementById('txtExperienceYears').value !== '' ? parseInt(document.getElementById('txtExperienceYears').value) : null,
            aadharNumber: document.getElementById('txtAadharNumber').value.trim() || null,
            //photo: fileInput,
            emergencyContactNumber: document.getElementById('txtEmergencyContactNumber').value.trim() || null,
            companyID: parseInt(document.getElementById('hdnCompanyID').value) || null,
            sessionID: parseInt(document.getElementById('hdnSessionID').value) || null,
            isActive: document.getElementById('isActive').checked,
            isDelete: false,
            userID: parseInt(document.getElementById('hdnUserID').value) || 0,

            // photo payload — sent as plain string, no separate upload call
            photoBase64: photoBase64,
            photoFileName: photoFileName,
            removePhoto: false
        };

        const r = await fetch('/Wardens/SaveWardens', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const res = await r.json();

        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'Warden has been saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = '/Wardens/Index';
            });
        } else {
            IV.setNotice('wardenPageNotice', res.message || 'Failed to save warden.');
            resetSaveButton(saveBtn);
        }
    } catch (err) {
        console.error(err);
        IV.setNotice('wardenPageNotice', 'Could not save warden. Please try again.');
        resetSaveButton(saveBtn);
    }
}

function resetSaveButton(btn) {
    if (btn) { btn.disabled = false; btn.innerHTML = '<i class="ti ti-device-floppy me-1"></i> Save'; }
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
                    url: '/Wardens/ToggleWardens',
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

function loadStatesByCountry(countryId, selectedStateId) {
    const ddlState = document.getElementById('ddlState');
    if (!ddlState) return;

    ddlState.innerHTML = '<option value="">-- Select --</option>';
    if (window.jQuery && jQuery(ddlState).data('select2')) jQuery(ddlState).val('').trigger('change');

    if (!countryId) {
        ddlState.innerHTML = '<option value="">-- Select Country First --</option>';
        return;
    }

    $.ajax({
        url: '/Wardens/GetStatesByCountry',
        type: 'GET',
        data: { countryId: countryId },
        success: function (res) {
            if (res && res.success && Array.isArray(res.data)) {
                res.data.forEach(st => {
                    const opt = document.createElement('option');
                    opt.value = st.stateId;
                    opt.textContent = st.stateName;
                    if (selectedStateId && parseInt(selectedStateId) === st.stateId) {
                        opt.selected = true;
                    }
                    ddlState.appendChild(opt);
                });
                if (window.jQuery && jQuery(ddlState).data('select2')) jQuery(ddlState).trigger('change');
            }
        },
        error: function () {
            console.warn('Failed to load states for country ' + countryId);
        }
    });
}


// ── Validation ───────────────────────────────────────────────────────
function validateWardenForm() {
    clearWardenValidation();
    let isValid = true;

    const hostelId = document.getElementById('ddlHostel').value;
    if (!hostelId) {
        IV.setFieldError('ddlHostel', 'errddlHostel', 'Hostel is required.');
        isValid = false;
    }

    const name = document.getElementById('txtWardenName').value.trim();
    if (!name) {
        IV.setFieldError('txtWardenName', 'errtxtWardenName', 'Warden name is required.');
        isValid = false;
    }

    const gender = document.getElementById('ddlGender').value;
    if (!gender) {
        IV.setFieldError('ddlGender', 'errddlGender', 'Gender is required.');
        isValid = false;
    }

    const contact = document.getElementById('txtWardenContact').value.trim();
    if (!contact) {
        IV.setFieldError('txtWardenContact', 'errtxtWardenContact', 'Contact number is required.');
        isValid = false;
    } else if (!/^[6-9]\d{9}$/.test(contact)) {
        IV.setFieldError('txtWardenContact', 'errtxtWardenContact', 'Enter a valid 10-digit mobile number.');
        isValid = false;
    }

    const email = document.getElementById('txtWardenEmail').value.trim();
    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        IV.setFieldError('txtWardenEmail', 'errtxtWardenEmail', 'Enter a valid email address.');
        isValid = false;
    }

    const emergency = document.getElementById('txtEmergencyContactNumber').value.trim();
    if (emergency && !/^[6-9]\d{9}$/.test(emergency)) {
        IV.setFieldError('txtEmergencyContactNumber', 'errtxtEmergencyContactNumber', 'Enter a valid 10-digit mobile number.');
        isValid = false;
    }

    const birthDate = document.getElementById('txtBirthDate').value;
    if (birthDate && new Date(birthDate) > new Date()) {
        IV.setFieldError('txtBirthDate', 'errtxtBirthDate', 'Date of birth cannot be in the future.');
        isValid = false;
    }

    const joiningDate = document.getElementById('txtJoiningDate').value;
    if (!joiningDate) {
        IV.setFieldError('txtJoiningDate', 'errtxtJoiningDate', 'Date of joining is required.');
        isValid = false;
    }

    const exp = document.getElementById('txtExperienceYears').value;
    if (exp !== '' && (isNaN(exp) || exp < 0 || exp > 60)) {
        IV.setFieldError('txtExperienceYears', 'errtxtExperienceYears', 'Enter a valid number of years (0-60).');
        isValid = false;
    }

    const aadhar = document.getElementById('txtAadharNumber').value.trim();
    if (aadhar && !/^\d{12}$/.test(aadhar)) {
        IV.setFieldError('txtAadharNumber', 'errtxtAadharNumber', 'Aadhar number must be exactly 12 digits.');
        isValid = false;
    }

    const pinCode = document.getElementById('txtPinCode').value.trim();
    if (pinCode && !/^\d{6}$/.test(pinCode)) {
        IV.setFieldError('txtPinCode', 'errtxtPinCode', 'PinCode must be exactly 6 digits.');
        isValid = false;
    }

    return isValid;
}
