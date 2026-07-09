// ========================================
// Export (DataTable button trigger)
// ========================================
function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#tblCity')) {
        $('#tblCity').DataTable().button(index).trigger();
    }
}

// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_city';
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
    const companyEl = document.getElementById('ddlFilterCompany');
    const countryEl = document.getElementById('ddlFilterCountry');
    const stateEl = document.getElementById('ddlFilterState');
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (countryEl) document.getElementById('hdnCountryId').value = countryEl.value;
    if (stateEl) document.getElementById('hdnStateId').value = stateEl.value;
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const companyEl = document.getElementById('ddlFilterCompany');
    const countryEl = document.getElementById('ddlFilterCountry');
    const stateEl = document.getElementById('ddlFilterState');
    const searchEl = document.getElementById('txtSearchInput');

    const searchVal = searchEl?.value.trim();
    if (searchVal) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    else delete appliedFilters['txtSearchInput'];

    if (companyEl?.value) {
        appliedFilters['ddlFilterCompany'] = { label: 'Company', text: companyEl.options[companyEl.selectedIndex]?.text || companyEl.value };
    } else delete appliedFilters['ddlFilterCompany'];

    if (countryEl?.value) {
        appliedFilters['ddlFilterCountry'] = { label: 'Country', text: countryEl.options[countryEl.selectedIndex]?.text || countryEl.value };
    } else delete appliedFilters['ddlFilterCountry'];

    if (stateEl?.value) {
        appliedFilters['ddlFilterState'] = { label: 'State', text: stateEl.options[stateEl.selectedIndex]?.text || stateEl.value };
    } else delete appliedFilters['ddlFilterState'];

    saveAppliedFilters();
}

function renderFilterBadges() {
    const container = document.getElementById('badgeContainer');
    const mainContainer = document.getElementById('activeFilterBadges');
    if (!container || !mainContainer) return;

    container.innerHTML = '';
    Object.entries(appliedFilters).forEach(([id, { label, text }]) => {
        const badge = document.createElement('span');
        badge.className = 'badge bg-primary-subtle text-primary border border-primary-subtle d-flex align-items-center gap-1 fw-medium px-2 py-1';
        badge.innerHTML = `${label}: ${text} <i class="ti ti-x ms-1 cursor-pointer" onclick="removeFilter('${id}')" style="font-size:10px;"></i>`;
        container.appendChild(badge);
    });
    mainContainer.style.display = Object.keys(appliedFilters).length > 0 ? 'block' : 'none';
}

function resetSelect2(id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.value = '';
    if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
}

function removeFilter(filterId) {
    if (filterId === 'ddlFilterCountry') {
        resetSelect2('ddlFilterCountry');
        resetSelect2('ddlFilterState'); // clearing country clears dependent state too
    } else if (filterId === 'ddlFilterState') {
        resetSelect2('ddlFilterState');
    } else if (filterId === 'ddlFilterCompany') {
        resetSelect2('ddlFilterCompany');
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
    const search = document.getElementById('txtSearchInput');
    if (search) search.value = '';
    document.getElementById('hdnSearch').value = '';

    ['ddlFilterCompany', 'ddlFilterCountry', 'ddlFilterState'].forEach(resetSelect2);

    clearAppliedFilters();
    document.getElementById('hdnPageIndex').value = 1;
    submitForm();
}

// ========================================
// Country -> State cascade (shared helper)
// ========================================
async function loadStatesByCountry(countryId, targetSelectId, preselectStateId = null) {
    const stateEl = document.getElementById(targetSelectId);
    if (!stateEl) return;

    // Reset target dropdown
    stateEl.innerHTML = '<option value="">-- Select --</option>';
    if (window.jQuery && jQuery(stateEl).data('select2')) jQuery(stateEl).trigger('change');

    if (!countryId) return;

    try {        
        const res = await fetch(`/City/GetStatesByCountry?countryID=${countryId}`);
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

// ========================================
// DOMContentLoaded
// ========================================
document.addEventListener('DOMContentLoaded', () => {

    // ── DataTable (list page only) ──────────────────────────────────
    if (document.getElementById('tblCity')) {
        if ($.fn.DataTable.isDataTable('#tblCity')) $('#tblCity').DataTable().destroy();
        $.fn.dataTable.ext.errMode = 'none';
        $('#tblCity').DataTable({
            dom: 'Bfrtip',
            buttons: [
                { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4] } },
                { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4] } },
                { extend: 'excel', exportOptions: { columns: [1, 2, 3, 4] } },
                { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4] } },
                { extend: 'print', exportOptions: { columns: [1, 2, 3, 4] } }
            ],
            searching: false, paging: false, info: false, ordering: false
        });
    }

    // ── Select2 for filter dropdowns (list page) ────────────────────
    if (window.jQuery && typeof jQuery.fn.select2 === 'function') {
        jQuery('#ddlFilterCountry, #ddlFilterState, #ddlFilterCompany').select2({
            width: '100%',
            dropdownParent: jQuery('#filter-dropdown'),
            allowClear: true,
            placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
        });
    }

    // ── Filter: Country change -> reload State filter ───────────────
    const filterCountry = document.getElementById('ddlFilterCountry');
    if (filterCountry) {
        jQuery ? jQuery(filterCountry).on('change', function () {
            loadStatesByCountry(this.value, 'ddlFilterState', Model_StateId());
        }) : filterCountry.addEventListener('change', function () {
            loadStatesByCountry(this.value, 'ddlFilterState');
        });
        // Preload on page load if a country filter is already applied
        if (filterCountry.value) {
            loadStatesByCountry(filterCountry.value, 'ddlFilterState', document.getElementById('hdnStateId')?.value);
        }
    }

    // ── Add/Edit form: Country change -> reload State select ────────
    const formCountry = document.getElementById('ddlCountry');
    if (formCountry) {
        const preselect = document.getElementById('hdnSelectedStateId')?.value;
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
            loadStatesByCountry(formCountry.value, 'ddlState', preselect);
        }
    }

    document.getElementById('filter-dropdown')?.addEventListener('click', e => e.stopPropagation());

    document.getElementById('btnApplyFilters')?.addEventListener('click', () => {
        document.getElementById('hdnPageIndex').value = 1;
        document.getElementById('hdnSearch').value = document.getElementById('txtSearchInput').value;
        applyFilters();
        submitForm();
    });

    renderFilterBadges();

    function Model_StateId() {
        return document.getElementById('hdnStateId')?.value || null;
    }
});

// ========================================
// Search — debounced
// ========================================
let searchTimer = null;
document.getElementById('txtSearchInput')?.addEventListener('input', function () {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => {
        document.getElementById('hdnSearch').value = this.value;
        document.getElementById('hdnPageIndex').value = 1;

        const val = this.value.trim();
        if (val) appliedFilters['txtSearchInput'] = { label: 'Search', text: val };
        else delete appliedFilters['txtSearchInput'];
        saveAppliedFilters();

        submitForm();
    }, 500);
});

// ========================================
// Row selection / permissions
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

window.selectedCityId = 0;
const cityFieldMap = { ddlCountry: 'errddlCountry', ddlState: 'errddlState', txtCityName: 'errtxtCityName' };
IV.bindAutoClear(cityFieldMap);

function clearFormValidation() {
    IV.clearMap(cityFieldMap);
    IV.clearNotice('classModalNotice');
}

function selectItem(id, row) {
    IV.clearNotice('classPageNotice');
    window.selectedCityId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    row.classList.add('bg-light');

    const checkedCount = $('.student-checkbox:checked').length;
    $('#btnEdit').prop('disabled', checkedCount !== 1 || !permCanEdit);
    $('#btnDelete').prop('disabled', checkedCount === 0 || !permCanDelete);
    $('#btnActive').prop('disabled', checkedCount === 0);
    $('#btnInactive').prop('disabled', checkedCount === 0);
}

async function editSelected() {
    if (!permCanEdit) {
        IV.setNotice('classPageNotice', 'You do not have permission to edit.');
        return;
    }
    if (!window.selectedCityId) return;
    location.href = `/City/Add/${window.selectedCityId}`;
}

async function deleteSelected() {
    if (!permCanDelete) {
        IV.setNotice('classPageNotice', 'You do not have permission to delete.');
        return;
    }

    const selectedIds = [];
    $('.student-checkbox:checked').each(function () { selectedIds.push(parseInt($(this).val())); });
    if (selectedIds.length === 0) {
        alert('Please select at least one City.');
        return;
    }

    Swal.fire({
        title: 'Are you sure?',
        text: 'You are about to delete this City record. This action cannot be undone!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it!',
        customClass: { confirmButton: 'btn btn-danger me-2', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then((result) => {
        if (!result.isConfirmed) return;
        $.ajax({
            url: '/City/DeleteCity',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(selectedIds),
            success: function (res) {
                if (res.success) {
                    Swal.fire('Deleted!', 'City record has been deleted.', 'success').then(() => location.reload());
                } else {
                    Swal.fire('Error!', res.message || 'Failed to delete City.', 'error');
                }
            },
            error: function () { Swal.fire('Error!', 'An unexpected error occurred.', 'error'); }
        });
    });
}

// ========================================
// Save (Add/Edit form)
// ========================================
async function saveItem() {
    const cid = parseInt(document.getElementById('txtItemId').value);
    if (cid === 0 && !permCanAdd) {
        IV.setNotice('classModalNotice', 'You do not have permission to add.');
        return;
    }
    if (cid > 0 && !permCanEdit) {
        IV.setNotice('classModalNotice', 'You do not have permission to edit.');
        return;
    }
    clearFormValidation();

    const countryId = document.getElementById('ddlCountry')?.value.trim();
    if (!countryId) {
        IV.setFieldError('ddlCountry', 'errddlCountry', 'Country is required.');
        return;
    }

    const stateId = document.getElementById('ddlState')?.value.trim();
    if (!stateId) {
        IV.setFieldError('ddlState', 'errddlState', 'State is required.');
        return;
    }

    const name = document.getElementById('txtCityName').value.trim();
    if (!name) {
        IV.setFieldError('txtCityName', 'errtxtCityName', 'City name is required.');
        return;
    } 
    const displayLabel = document.getElementById('txtDisplayLabel').value.trim();
    
    const description = $('#txtDescription').val();
    const isActiveEl = document.getElementById('isActive');
    const isActive = isActiveEl ? isActiveEl.checked : false;

    const data = {
        cityId: cid,
        cityName: name,
        displayLabel: displayLabel,
        countryId: countryId,
        stateId: stateId,
        description: description,
        isActive: isActive
    };

    try {
        const r = await fetch('/City/SaveCity', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const res = await r.json();
        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'City has been saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => { window.location.href = '/City/Index'; });
        } else {
            Swal.fire({
                icon: 'error', title: 'Error', text: res.message || 'Failed to save City.',
                confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-danger' }, buttonsStyling: false
            });
        }
    } catch (err) {
        console.error(err);
        IV.setNotice('classModalNotice', 'Could not save City.');
    }
}

// ========================================
// Toggle Active / Inactive
// ========================================
async function toggleStatus(id, isActive) {
    if (!permCanEdit) {
        IV.setNotice('classPageNotice', 'You do not have permission to change status.');
        location.reload();
        return;
    }

    let ids = [];
    $('.student-checkbox:checked').each(function () { ids.push(parseInt($(this).val())); });
    if (ids.length === 0) {
        if (id !== 0) ids.push(parseInt(id));
        else { showToast?.('Please select at least one record', false); return; }
    }

    const config = isActive
        ? { title: 'Activate Record(s)?', text: 'This will make the record visible and available for use across the system.' }
        : { title: 'Deactivate Record(s)?', text: 'This will hide the record and prevent it from being used.' };

    Swal.fire({
        icon: isActive ? 'question' : 'warning',
        title: config.title,
        text: config.text,
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, proceed!',
        customClass: { confirmButton: 'btn btn-danger me-2', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then((result) => {
        if (!result.isConfirmed) return;
        $.ajax({
            url: '/City/ToggleCity',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ ids: ids.join(','), isActive }),
            success: function (res) {
                if (res.success) {
                    Swal.fire('Updated!', res.message || 'Status updated successfully.', 'success').then(() => location.reload());
                } else {
                    Swal.fire('Error!', res.message || 'Failed to update status.', 'error');
                }
            },
            error: function (xhr) {
                console.error('Status update error:', xhr.status, xhr.responseText);
                Swal.fire('Error!', 'An unexpected error occurred.', 'error');
            }
        });
    });
}