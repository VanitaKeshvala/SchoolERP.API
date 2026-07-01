
// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_hotel';
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
    const roolTypeEl = document.getElementById('ddlFilterRoolType');
    const hostelTypeEl = document.getElementById('ddlFilterHostels');
    const searchEl = document.getElementById('txtSearchInput');

    if (sessionEl) document.getElementById('hdnSessionID').value = sessionEl.value;
    if (companyEl) document.getElementById('hdnCompanyId').value = companyEl.value;
    if (roolTypeEl) document.getElementById('hdnRoomTypeID').value = roolTypeEl.value;
    if (hostelTypeEl) document.getElementById('hdnHostelsID').value = hostelTypeEl.value;
    if (searchEl) document.getElementById('hdnSearch').value = searchEl.value.trim();
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const sessionEl = document.getElementById('ddlFilterSessions');
    const companyEl = document.getElementById('ddlFilterCompany');
    const roolTypeEl = document.getElementById('ddlFilterRoolType');
    const hostelEl = document.getElementById('ddlFilterHostels');
    const searchEl = document.getElementById('txtSearchInput');

    // Search
    const searchVal = searchEl?.value.trim() ?? '';
    document.getElementById('hdnSearch').value = searchVal;

    if (searchVal) {
        appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    } else {
        delete appliedFilters['txtSearchInput'];
    }

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
        appliedFilters['ddlFilterHostels'] = {
            label: 'Hostel',
            text: hostelEl.options[hostelEl.selectedIndex]?.text || hostelEl.value
        };
    } else delete appliedFilters['ddlFilterHostels'];
    // Rool Type
    if (roolTypeEl?.value && roolTypeEl.value !== '') {
        appliedFilters['ddlFilterRoolType'] = {
            label: 'Rool Type',
            text: roolTypeEl.options[roolTypeEl.selectedIndex]?.text || roolTypeEl.value
        };
    } else delete appliedFilters['ddlFilterRoolType'];

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
    } else if (filterId === 'ddlFilterRoolType') {
        const el = document.getElementById('ddlFilterRoolType');
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    }
    else if (filterId === 'ddlFilterHostels') {
        const el = document.getElementById('ddlFilterHostels');
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
    jQuery('#ddlFilterSessions, #ddlFilterCompany, #ddlFilterRoolType','#ddlFilterHostels').select2({
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
document.addEventListener('DOMContentLoaded', () => {

    // ── DataTable (export only) ───────────────────────────────────────
    if ($.fn.DataTable.isDataTable('#tblRoom')) {
        $('#tblRoom').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#tblRoom').DataTable({
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
            jQuery('#ddlFilterSessions, #ddlFilterCompany', '#ddlFilterRoolType','#ddlFilterHostels').select2({
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
        ['ddlFilterSessions', 'ddlFilterCompany', 'ddlFilterRoolType','ddlFilterHostels'].forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        });
        clearAppliedFilters();
        document.getElementById('hdnPageIndex').value = 1;
        submitForm();
    });

    // ── Render badges on load ─────────────────────────────────────────
    renderFilterBadges();
});



let selectedId = 0;
const canEdit = window.canEdit;
const canDelete = window.canDelete;
window.selectedId = 0;
function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#tblRoom')) {
        const table = $('#tblRoom').DataTable();
        table.button(index).trigger();
    }
}

$(document).ready(function () {
    $('#tblRoom').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4, 5] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4, 5] } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3, 4, 5] } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4, 5] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3, 4, 5] } }
        ],
        searching: false,
        paging: false,
        info: false
    });
    // Call only if a Room Type is selected
    var roomTypeId = $('#ddlRoomType').val();

    if (roomTypeId) {
        fachrecordforcust(roomTypeId);
    }
})


//$(document).ready(function () {
//    // Initialize Select2
//    $('#ddlHostel, #ddlRoomType').select2({
//        dropdownParent: $('#roomModal'),
//        placeholder: "-- Select --",
//        allowClear: true
//    });
//});

function selectItem(id, row) {
    selectedId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    row.classList.add('bg-light');
    //row.querySelector('input[type="radio"]').checked = true;
    window.selectedId = id;
    if (canEdit) document.getElementById('btnEdit').disabled = false;
    if (canDelete) document.getElementById('btnDelete').disabled = false;
    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    // Edit sirf 1 record select hone par
    $('#btnEdit').prop('disabled', checkedCount !== 1);
    $('#btnActive').prop('disabled', checkedCount === 0);
    $('#btnInactive').prop('disabled', checkedCount === 0);
}


// Search
document.getElementById('txtSearchInput').addEventListener('keyup', function () {
    const q = this.value.toLowerCase();
    document.querySelectorAll('#tblRoom tbody tr').forEach(row => {
        if (row.classList.contains('item-row')) {
            row.style.display = row.textContent.toLowerCase().includes(q) ? '' : 'none';
        }
    });
});

function openAddModal() {
    document.getElementById('modalTitle').textContent = 'Add Room';
    document.getElementById('hdnRoomId').value = 0;
    document.getElementById('roomForm').reset();
    document.getElementById('chkActive').checked = true;

    $('#ddlHostel, #ddlRoomType').val('').trigger('change');

    new bootstrap.Modal(document.getElementById('roomModal')).show();
}

function editSelected() {
    if (!window.selectedId) return;
    try {

        if (window.selectedId) {
            location.href = `/Hostel/AddHostelRoom/${window.selectedId}`;
        }

    } catch (err) {
        console.error(err);
    }
}
async function saveRecord() {
    const form = document.getElementById('roomForm');
    if (!form.checkValidity()) { form.reportValidity(); return; }

    const allowExtraBedRaw = document.getElementById('ddlAllowExtraBed').value;
    const maxExtraBedsRaw = document.getElementById('txtMaxExtraBeds').value.trim();

    if (allowExtraBedRaw === "1" && maxExtraBedsRaw === "") {
        showToast('Please enter Max Extra Beds.', false);
        document.getElementById('txtMaxExtraBeds').focus();
        return;
    }

    // ---- Safe numeric parsing helpers ----
    const toIntOrNull = (val) => {
        const n = parseInt(val, 10);
        return isNaN(n) ? null : n;
    };
    const toFloatOrNull = (val) => {
        const n = parseFloat(val);
        return isNaN(n) ? null : n;
    };
    const toBoolOrNull = (val) => {
        if (val === "1" || val === "true" || val === true) return true;
        if (val === "0" || val === "false" || val === false) return false;
        return null;
    };

    // ---- Validate required numeric fields BEFORE building payload ----
    const hostelId = toIntOrNull(document.getElementById('ddlHostel').value);
    const roomTypeId = toIntOrNull(document.getElementById('ddlRoomType').value);
    const noOfBed = toIntOrNull(document.getElementById('txtBeds').value);
    const costPerBed = toFloatOrNull(document.getElementById('txtCost').value);

    if (hostelId === null) {
        showToast('Please select a hostel.', false);
        return;
    }
    if (roomTypeId === null) {
        showToast('Please select a room type.', false);
        return;
    }
    if (noOfBed === null || noOfBed <= 0) {
        showToast('Please enter a valid number of beds.', false);
        document.getElementById('txtBeds').focus();
        return;
    }
    if (costPerBed === null || costPerBed < 0) {
        showToast('Please enter a valid cost per bed.', false);
        document.getElementById('txtCost').focus();
        return;
    }

    const data = {
        roomId: toIntOrNull(document.getElementById('hdnRoomId').value) || 0,
        hostelID: hostelId,
        roomTypeID: roomTypeId,
        roomTitle: document.getElementById('txtRoomTitle').value.trim(),
        noOfBed: noOfBed,
        costPerBed: costPerBed,
        roomDescription: document.getElementById('txtDescription').value.trim() || null,
        isActive: document.getElementById('chkActive').checked,
        floorNumber: toIntOrNull(document.getElementById('txtFloorNumber').value.trim()),
        allowExtraBed: toBoolOrNull(allowExtraBedRaw),
        maxExtraBeds: toIntOrNull(maxExtraBedsRaw)
    };

    try {
        const r = await fetch('/Hostel/UpsertHostelRoom', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (!r.ok) {
            console.error('Server returned status:', r.status);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: `Server error (${r.status}). Please try again.`,
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
            return;
        }

        const res = await r.json();

        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: 'Hostel has been added successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = '/Hostel/HostelRoom';
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save hostel.',
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

function deleteSelected() {
    if (!selectedId) return;

    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one hostel room.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this hostel room  record. This action cannot be undone!",
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
                url: '/Hostel/DeleteHostelRoom',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Hostel room record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete hostel room.', 'error');
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
                    url: '/Hostel/ToggleHostelRoomStatus',
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

async function fachrecordforcust(roomTypeId) {
    if (!roomTypeId) {
        document.getElementById('txtBeds').value = '';
        document.getElementById('txtCost').value = '0';
        document.getElementById('spnRoomType').value = '';
        return;
    }

    try {
        const response = await fetch(`/Hostel/GetRoomTypeByID?id=${roomTypeId}`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        if (!response.ok) {
            throw new Error('Failed to fetch room type details');
        }

        const data = await response.json();

        // Map the 3 keys you specified
        const bedCapacity = data.data.BedCapacity ?? data.data.bedCapacity ?? 1;
        const acNonAcFlag = data.data.ACNonACflag ?? data.data.acNonACflag ?? 'Non-AC';
        const rentPerBed = data.data.RentPerBed ?? data.data.rentPerBed ?? 0;
        const roomTypeTitle = data.data.RentPerBed ?? data.data.roomTypeTitle ?? 0;
        const roomCoolingTypeTitle = data.data.RentPerBed ?? data.data.roomCoolingTypeName ?? 0;

        // Auto-fill beds (read-only, comes from room type)
        const txtBeds = document.getElementById('txtBeds');
        txtBeds.value = bedCapacity;
        //txtBeds.setAttribute('readonly', 'readonly');

        // Auto-fill cost per bed (editable override allowed)
        const txtCost = document.getElementById('txtCost');
        txtCost.value = parseFloat(rentPerBed).toFixed(2);


        // Auto-fill cost per bed (editable override allowed)
        const txtCoolingRoomType = document.getElementById('spnRoomCoolingType');
        txtCoolingRoomType.innerText = roomCoolingTypeTitle;

        const txtRoomType = document.getElementById('spnRoomType');
        txtRoomType.innerText = roomTypeTitle;
        //spnRoomCoolingType
        // Optional: show AC/Non-AC badge near the room type dropdown
        //showAcBadge(acNonAcFlag);
        loadRoomRates(roomTypeId);
    } catch (err) {
        console.error('fachrecordforcust error:', err);
        // Fallback — don't block the form, just clear auto-fill
        document.getElementById('txtBeds').removeAttribute('readonly');
    }
}


async function loadRoomRates(roomTypeId) {

    //const roomTypeId = $('#ddlFilterRoolType').val();

    if (!roomTypeId) {
        $('#tblRateBody').html('');
        return;
    }

    try {

        const response = await fetch(`/Hostel/GetHostelRoomRateByID/${roomTypeId}`);

        const result = await response.json();

        if (!result.success) {
            alert(result.message);
            document.getElementById('divRate').classList.add('d-none');
            return;
        }
        
        bindRateTable(result.data);

    }
    catch (e) {
        console.error(e);
        alert('Unable to load room rates.');
    }
}
function bindRateTable(data) {
    //divRate   
    document.getElementById('divRate').classList.remove('d-none');
    let html = '';

    data.forEach(function (item) {

        html += `
            <tr>
                <td>${item.costPerBed}</td>
                <td>${item.securityAmount}</td>
                <td>${formatDate(item.effectiveFrom)}</td>
                <td>${formatDate(item.effectiveTO)}</td>
            </tr>
        `;
    });

    $('#tblRateBody').html(html);
    if (data.length === 1 && data[0].rateID === 0) {
        document.getElementById('divRate').classList.add('d-none');
    }
}
function showAcBadge(flag) {
    let badge = document.getElementById('acBadge');
    if (!badge) {
        badge = document.createElement('span');
        badge.id = 'acBadge';
        badge.className = 'badge ms-2';
        document.querySelector('label[for="roomTypeSelect"]')?.appendChild(badge);
    }
    badge.textContent = flag;
    badge.className = 'badge ms-2 ' + (flag === 'AC' ? 'bg-info' : 'bg-secondary');
}
function formatDate(date) {

    if (!date)
        return '';

    return new Date(date).toLocaleDateString('en-GB');
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