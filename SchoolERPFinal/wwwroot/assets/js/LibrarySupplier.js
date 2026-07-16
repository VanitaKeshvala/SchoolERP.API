
// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_supplier';
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
    const companyEl = document.getElementById('ddlFilterCompany');
    const searchEl = document.getElementById('txtSearchInput');

    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (searchEl) document.getElementById('hdnSearch').value = searchEl.value.trim();
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const companyEl = document.getElementById('ddlFilterCompany');
    const searchEl = document.getElementById('txtSearchInput');

    // Search
    const searchVal = searchEl?.value.trim() ?? '';
    document.getElementById('hdnSearch').value = searchVal;

    if (searchVal) {
        appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    } else {
        delete appliedFilters['txtSearchInput'];
    }

    

    // Company
    if (companyEl?.value && companyEl.value !== '') {
        appliedFilters['ddlFilterCompany'] = {
            label: 'Company',
            text: companyEl.options[companyEl.selectedIndex]?.text || companyEl.value
        };
    } else delete appliedFilters['ddlFilterCompany'];

    

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
    if (filterId === 'ddlFilterCompany') {
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

    // all three in one selector string
    jQuery('#ddlFilterSessions, #ddlFilterCompany, #ddlFilterRoolType').select2({
        width: '100%',
        dropdownParent: jQuery('#filter-dropdown'),
        allowClear: true,
        placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
    });

    clearAppliedFilters();
    document.getElementById('hdnPageIndex').value = 1;
    submitForm();
}

// ========================================
// DOMContentLoaded — init DataTable + UI
// ========================================
document.addEventListener('DOMContentLoaded', function () {
    const input = document.getElementById('txtPinCode');
    const list = document.getElementById('pinCodeSuggestions');
    if (!input || !list) {
        console.warn('PinCode autocomplete: input or list element not found');
        return;
    }

    let debounceTimer;
    let currentItems = [];


    // ── Add/Edit form: Hostel change -> reload State select ────────
    const formCountry = document.getElementById('ddlCountry');
    if (formCountry) {
        const preselectState = document.getElementById('hdnSelectedStateId')?.value;
        const preselectCity = document.getElementById('hdnSelectedCityId')?.value;

        if (window.jQuery) {
            jQuery(formCountry).on('change', function () {
                loadStatesByCountry(this.value, 'ddlState');
            });
        } else {
            formCountry.addEventListener('change', function () {
                loadStatesByCountry(this.value, 'ddlState');
            });
        }

        // On edit: preload states for the already-selected country, then select saved state
        if (formCountry.value) {
            loadStatesByCountry(formCountry.value, 'ddlState', preselectState).then(() => {
                // Once state is preselected, cascade into city
                const stateEl = document.getElementById('ddlState');
                if (stateEl && stateEl.value) {
                    loadCitiesByState(stateEl.value, 'ddlCity', preselectCity);
                }
            });
        }
    }

    // ── Add/Edit form: State change -> reload City select ───────────
    const formState = document.getElementById('ddlState');
    if (formState) {
        if (window.jQuery) {
            jQuery(formState).on('change', function () {
                loadCitiesByState(this.value, 'ddlCity');
            });
        } else {
            formState.addEventListener('change', function () {
                loadCitiesByState(this.value, 'ddlCity');
            });
        }
    }

    input.addEventListener('input', function () {
        const term = this.value.trim();
        clearTimeout(debounceTimer);

        if (term.length < 2) {
            hideList();
            return;
        }
        debounceTimer = setTimeout(() => fetchPinCodes(term), 300);


    });

    async function fetchPinCodes(term) {
        try {
            const res = await fetch(`/Hostel/SearchPostalCode?term=${encodeURIComponent(term)}`);
            const result = await res.json();

            console.log('API response:', result); // 🔍 TEMP: check shape of result.data

            if (result.success && result.data && result.data.length > 0) {
                currentItems = result.data;
                renderList(currentItems);
            } else {
                hideList();
            }
        } catch (err) {
            console.error('SearchPostalCode error:', err);
            hideList();
        }
    }

    function renderList(items) {
        list.innerHTML = '';
        items.forEach((item, idx) => {
            const li = document.createElement('li');
            li.className = 'list-group-item list-group-item-action';
            li.style.cursor = 'pointer';
            li.textContent = `${item.postalCode ?? item.postalCode}`;
            li.addEventListener('click', () => selectPinItem(idx));
            list.appendChild(li);
        });
        list.style.display = 'block';
    }

    async function selectPinItem(idx) {
        const item = currentItems[idx];
        input.value = item.postalCode ?? item.postalCode;
        hideList();
        const countryId = item.countryId ?? item.countryId;
        const stateId = item.stateId ?? item.stateId;
        const cityId = item.cityId ?? item.cityId;

        // 1️⃣ Select Country immediately (options already exist server-side)
        if (countryId) {
            $('#ddlCountry').val(countryId).trigger('change.select2');
            $('#ddlCountry').trigger('change');
        }


        // 2️⃣ Load States for that country, then select the right one
        if (countryId && stateId) {
            await loadStatesByCountry(countryId, 'ddlState', stateId);
        }

        // 3️⃣ Load Cities for that state, then select the right one
        if (stateId && cityId) {
            await loadCitiesByState(stateId, 'ddlCity', cityId);
        }

    }

    function hideList() {
        list.style.display = 'none';
        list.innerHTML = '';
    }

    document.addEventListener('click', function (e) {
        if (!input.contains(e.target) && !list.contains(e.target)) {
            hideList();
        }
    });
});


let selectedId = 0;
window.selectedId = 0;
const canEdit = window.canEdit;
const canDelete = window.canDelete;

function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#c')) {
        const table = $('#tblHostel').DataTable();
        table.button(index).trigger();
    }
}

$(document).ready(function () {
    $('#tblHostel').DataTable({
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
        info: false
    });
})

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
    $('#btnEdit').prop('disabled', checkedCount !== 1);
    $('#btnActive').prop('disabled', checkedCount === 0);
    $('#btnInactive').prop('disabled', checkedCount === 0);
}



function editSelected() {
    if (!window.selectedId) return;
    try {

        if (window.selectedId) {
            location.href = `/LibrarySupplier/Add/${window.selectedId}`;
        }

    } catch (err) {
        console.error(err);
    }
}

// ========================================
// Validation
// ========================================
const VALIDATION_FIELDS = ['txtSupplierName', 'txtPhone', 'txtEmail', 'txtFAX', 'txtWebSite', 'txtMobileNo', 'txtContactPerson'];

function clearAllErrors() {
    VALIDATION_FIELDS.forEach(id => {
        const el = document.getElementById(id);
        const errEl = document.getElementById('err' + id);
        if (el) el.classList.remove('is-invalid');
        if (errEl) errEl.textContent = '';
    });
}

function showFieldError(id, msg) {
    const el = document.getElementById(id);
    const errEl = document.getElementById('err' + id);
    if (el) el.classList.add('is-invalid');
    if (errEl) errEl.textContent = msg;
}

function validateForm() {
    clearAllErrors();
    let isValid = true;

    // 1) Supplier Name — required
    const supplierName = document.getElementById('txtSupplierName').value.trim();
    if (!supplierName) {
        showFieldError('txtSupplierName', 'Supplier Name is required.');
        isValid = false;
    }

    // 2) Phone — valid if provided (10-digit, optional +country code)
    const phone = document.getElementById('txtPhone').value.trim();
    const phoneRegex = /^\+?[0-9]{10,13}$/;
    if (phone && !phoneRegex.test(phone)) {
        showFieldError('txtPhone', 'Enter a valid phone number (10-13 digits).');
        isValid = false;
    }

    // 3) FAX — valid if provided
    const fax = document.getElementById('txtFAX').value.trim();
    const faxRegex = /^\+?[0-9\-\s()]{7,15}$/;
    if (fax && !faxRegex.test(fax)) {
        showFieldError('txtFAX', 'Enter a valid FAX number.');
        isValid = false;
    }

    // 4) Website — valid if provided
    const website = document.getElementById('txtWebSite').value.trim();
    const urlRegex = /^(https?:\/\/)?([\w-]+\.)+[a-zA-Z]{2,}([\/\w\-._~:?#[\]@!$&'()*+,;=%]*)?$/;
    if (website && !urlRegex.test(website)) {
        showFieldError('txtWebSite', 'Enter a valid website (e.g. https://example.com).');
        isValid = false;
    }

    // 5) Mobile No — valid Indian 10-digit mobile if provided
    const mobile = document.getElementById('txtMobileNo').value.trim();
    const mobileRegex = /^[6-9][0-9]{9}$/;
    if (mobile && !mobileRegex.test(mobile)) {
        showFieldError('txtMobileNo', 'Enter a valid 10-digit mobile number.');
        isValid = false;
    }

    // 6) Contact Person — letters/spaces only if provided
    const contactPerson = document.getElementById('txtContactPerson').value.trim();
    const nameRegex = /^[A-Za-z\s.'-]{2,50}$/;
    if (contactPerson && !nameRegex.test(contactPerson)) {
        showFieldError('txtContactPerson', "Enter a valid name (letters only).");
        isValid = false;
    }

    // 7) Email — valid if provided
    const email = document.getElementById('txtEmail').value.trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (email && !emailRegex.test(email)) {
        showFieldError('txtEmail', 'Enter a valid email address.');
        isValid = false;
    }

    return isValid;
}

// Clear individual field error as the user fixes it
VALIDATION_FIELDS.forEach(id => {
    const el = document.getElementById(id);
    if (el) {
        el.addEventListener('input', function () {
            this.classList.remove('is-invalid');
            const errEl = document.getElementById('err' + id);
            if (errEl) errEl.textContent = '';
        });
    }
});

// ========================================
// Save (renamed to match the button's onclick="saveItem()")
// ========================================
async function saveItem() {
    if (!validateForm()) return;

    const cId = parseInt(document.getElementById('txtItemId').value);
    if (cId === 0 && !window.canAdd) {
        showToast('You do not have permission to add.', 'danger');
        return;
    }

    const data = {
        supplierId: cId || 0,
        supplierName: document.getElementById('txtSupplierName').value.trim(),
        phone: document.getElementById('txtPhone').value.trim(),
        email: document.getElementById('txtEmail').value.trim(),
        address: document.getElementById('txtAddress').value.trim() || null,
        postalCode: document.getElementById('txtPinCode').value || null,
        countryId: document.getElementById('ddlCountry').value || null,
        stateId: document.getElementById('ddlState').value || null,
        cityId: document.getElementById('ddlCity').value || null,
        fax: document.getElementById('txtFAX').value.trim() || null,
        webSite: document.getElementById('txtWebSite').value.trim() || null,
        mobileNo: document.getElementById('txtMobileNo').value.trim() || null,
        contactPerson: document.getElementById('txtContactPerson').value.trim() || null,
        isActive: document.getElementById('isActive').checked
    };

    try {
        const r = await fetch('/LibrarySupplier/SaveLibrarySupplier', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const res = await r.json();
        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'Supplier has been saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = '/LibrarySupplier/Index';
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save supplier.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
        }
    } catch (err) {
        console.error(err);
        showToast('Could not save supplier.', 'danger');
    }
}

function deleteSelected() {
    if (!selectedId) return;

    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one student.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this student record. This action cannot be undone!",
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
                url: '/LibrarySupplier/DeleteLibrarySupplier',
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

function toggleStatus(id, isActive) {


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
                    // fixed typo bluk → bulk
                    url: '/LibrarySupplier/ToggleLibrarySupplier',
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
                        // log xhr for debugging
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

function showToast(msg, type = 'success') {
    const wrapper = document.createElement('div');
    wrapper.innerHTML = `<div class="toast align-items-center text-bg-${type} border-0 show position-fixed bottom-0 end-0 m-3" role="alert" style="z-index:9999">
            <div class="d-flex"><div class="toast-body">${msg}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div></div>`;
    document.body.appendChild(wrapper);
    setTimeout(() => wrapper.remove(), 3000);
}

// ========================================
// Country -> State -> City cascade (shared helpers)
// ========================================
async function loadStatesByCountry(countryId, targetSelectId, preselectStateId = null) {
    const stateEl = document.getElementById(targetSelectId);
    if (!stateEl) return;

    stateEl.innerHTML = '<option value="">-- Select --</option>';
    if (window.jQuery && jQuery(stateEl).data('select2')) jQuery(stateEl).trigger('change');

    if (!countryId) return;

    try {
        const res = await fetch(`/PostalCode/GetStatesByCountry?countryID=${countryId}`);
        const result = await res.json();
        const states = result.data || [];
        stateEl.innerHTML = '<option value="">Select</option>';
        states.forEach(s => {
            const opt = document.createElement('option');
            opt.value = s.stateId;
            opt.textContent = s.stateName;
            if (preselectStateId && parseInt(preselectStateId) === s.stateId) opt.selected = true;
            stateEl.appendChild(opt);
        });
        if (window.jQuery && jQuery(stateEl).data('select2')) jQuery(stateEl).trigger('change');
    } catch (err) {
        console.error('Failed to load states:', err);
    }
}

async function loadCitiesByState(stateId, targetSelectId, preselectCityId = null) {
    const cityEl = document.getElementById(targetSelectId);
    if (!cityEl) return;

    cityEl.innerHTML = '<option value="">-- Select --</option>';
    if (window.jQuery && jQuery(cityEl).data('select2')) jQuery(cityEl).trigger('change');

    if (!stateId) return;

    try {
        const res = await fetch(`/PostalCode/GetCitiesByState?stateID=${stateId}`);
        const result = await res.json();
        const cities = result.data || [];
        cityEl.innerHTML = '<option value="">Select</option>';
        cities.forEach(c => {
            const opt = document.createElement('option');
            opt.value = c.cityId;
            opt.textContent = c.cityName;
            if (preselectCityId && parseInt(preselectCityId) === c.cityId) opt.selected = true;
            cityEl.appendChild(opt);
        });
        if (window.jQuery && jQuery(cityEl).data('select2')) jQuery(cityEl).trigger('change');
    } catch (err) {
        console.error('Failed to load cities:', err);
    }
}
