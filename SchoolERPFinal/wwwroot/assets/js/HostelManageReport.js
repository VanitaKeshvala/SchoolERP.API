// ========================================================================
// Hostel Manage Report — AJAX grid controller
// Modes: LIST | RATE | REPORT  (each has its own column set / row renderer)
// ========================================================================

const FILTER_KEY = 'appliedFilters_hostelManage';
let appliedFilters = {};
try {
    const stored = sessionStorage.getItem(FILTER_KEY);
    if (stored) appliedFilters = JSON.parse(stored);
} catch (e) { appliedFilters = {}; }

// Current search state — seeded from the server-rendered initial payload.
let state = Object.assign({
    Mode: 'REPORT',
    CompanyID: 0,
    SessionID: 0,
    HostelID: null,
    HostelTypeID: null,
    RoomTypeID: null,
    RoomCoolingTypeId: null,
    RoomTitle: null,
    SearchText: '',
    IsActive: null,
    AsOnDate: null,
    RoomId: null,
    PageNumber: 1,
    PageSize: 10
}, window.initialSearch || {});

function saveAppliedFilters() {
    try { sessionStorage.setItem(FILTER_KEY, JSON.stringify(appliedFilters)); }
    catch (e) { console.warn('sessionStorage unavailable'); }
}

function clearAppliedFilters() {
    appliedFilters = {};
    try { sessionStorage.removeItem(FILTER_KEY); } catch (e) { }
}

// ------------------------------------------------------------------
// Mode switching — toggles visible columns + row renderer + AsOnDate
// ------------------------------------------------------------------
function switchMode(mode) {
    state.Mode = mode;
    state.PageNumber = 1;

    document.querySelectorAll('.mode-pill').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.mode === mode);
    });

    //document.querySelectorAll('.col-list, .col-rate, .col-report').forEach(el => el.style.display = 'none');
    //document.querySelectorAll('.col-' + mode.toLowerCase()).forEach(el => el.style.display = 'table-cell');

    //document.getElementById('grpAsOnDate').style.display = (mode === 'RATE') ? '' : 'none';

   // loadGrid();
}

// ------------------------------------------------------------------
// Filters
// ------------------------------------------------------------------
function readFiltersIntoState() {
    const hostelEl = document.getElementById('ddlFilterHostel');
    const hostelTypeEl = document.getElementById('ddlFilterHostelType');
    const roomTypeEl = document.getElementById('ddlFilterRoomType');
    const coolingEl = document.getElementById('ddlFilterCoolingType');
    const statusEl = document.getElementById('ddlFilterStatus');
    const asOnDateEl = document.getElementById('txtAsOnDate');
    const searchEl = document.getElementById('txtSearchInput');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sessionEl = document.getElementById('ddlFilterSessions');

    state.HostelID = hostelEl?.value ? parseInt(hostelEl.value) : null;
    state.HostelTypeID = hostelTypeEl?.value ? parseInt(hostelTypeEl.value) : null;
    state.RoomTypeID = roomTypeEl?.value ? parseInt(roomTypeEl.value) : null;
    state.RoomCoolingTypeId = coolingEl?.value ? parseInt(coolingEl.value) : null;
    state.IsActive = statusEl?.value ? (statusEl.value === 'true') : null;
    state.AsOnDate = (state.Mode === 'RATE' && asOnDateEl?.value) ? asOnDateEl.value : null;
    state.SearchText = searchEl?.value.trim() ?? '';


    state.CompanyId = companyEl?.value ? parseInt(companyEl.value) : null;
    state.SessionId = sessionEl?.value ? parseInt(sessionEl.value) : null;
}

function applyFilters() {
    readFiltersIntoState();

    const hostelEl = document.getElementById('ddlFilterHostel');
    const hostelTypeEl = document.getElementById('ddlFilterHostelType');
    const roomTypeEl = document.getElementById('ddlFilterRoomType');
    const coolingEl = document.getElementById('ddlFilterCoolingType');
    const statusEl = document.getElementById('ddlFilterStatus');
    const companyEl = document.getElementById('ddlFilterCompany');
    const sessionEl = document.getElementById('ddlFilterSessions');
    const asOnDateEl = document.getElementById('txtAsOnDate');
    const searchEl = document.getElementById('txtSearchInput');

    if (searchEl?.value.trim()) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchEl.value.trim() };
    else delete appliedFilters['txtSearchInput'];

    if (hostelEl?.value) appliedFilters['ddlFilterHostel'] = { label: 'Hostel', text: hostelEl.options[hostelEl.selectedIndex]?.text || hostelEl.value };
    else delete appliedFilters['ddlFilterHostel'];

    if (hostelTypeEl?.value) appliedFilters['ddlFilterHostelType'] = { label: 'Hostel Type', text: hostelTypeEl.options[hostelTypeEl.selectedIndex]?.text || hostelTypeEl.value };
    else delete appliedFilters['ddlFilterHostelType'];

    if (roomTypeEl?.value) appliedFilters['ddlFilterRoomType'] = { label: 'Room Type', text: roomTypeEl.options[roomTypeEl.selectedIndex]?.text || roomTypeEl.value };
    else delete appliedFilters['ddlFilterRoomType'];

    if (coolingEl?.value) appliedFilters['ddlFilterCoolingType'] = { label: 'Cooling', text: coolingEl.options[coolingEl.selectedIndex]?.text || coolingEl.value };
    else delete appliedFilters['ddlFilterCoolingType'];

    if (statusEl?.value) appliedFilters['ddlFilterStatus'] = { label: 'Status', text: statusEl.value === 'true' ? 'Active' : 'Inactive' };
    else delete appliedFilters['ddlFilterStatus'];

    if (state.Mode === 'RATE' && asOnDateEl?.value) appliedFilters['txtAsOnDate'] = { label: 'As On', text: asOnDateEl.value };
    else delete appliedFilters['txtAsOnDate'];

    if (companyEl?.value) appliedFilters['ddlFilterCompany'] = { label: 'Company', text: companyEl.options[companyEl.selectedIndex]?.text || companyEl.value };
    else delete appliedFilters['ddlFilterCompany'];

    if (sessionEl?.value) appliedFilters['ddlFilterSessions'] = { label: 'Session', text: sessionEl.options[sessionEl.selectedIndex]?.text || sessionEl.value };
    else delete appliedFilters['ddlFilterSessions'];
    saveAppliedFilters();
    renderFilterBadges();
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
    const map = {
        ddlFilterHostel: 'ddlFilterHostel',
        ddlFilterHostelType: 'ddlFilterHostelType',
        ddlFilterRoomType: 'ddlFilterRoomType',
        ddlFilterCoolingType: 'ddlFilterCoolingType',
        ddlFilterStatus: 'ddlFilterStatus',
        txtAsOnDate: 'txtAsOnDate'
    };

    if (map[filterId]) {
        const el = document.getElementById(map[filterId]);
        if (el) {
            el.value = '';
            if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
        }
    } else if (filterId === 'txtSearchInput') {
        const el = document.getElementById('txtSearchInput');
        if (el) el.value = '';
    }

    delete appliedFilters[filterId];
    saveAppliedFilters();
    state.PageNumber = 1;
    applyFilters();
    //loadGrid();
}

function resetAllFilters() {
    document.getElementById('txtSearchInput').value = '';
    ['ddlFilterHostel', 'ddlFilterHostelType', 'ddlFilterRoomType', 'ddlFilterCoolingType', 'ddlFilterStatus'].forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        el.value = '';
        if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
    });
    const asOnDateEl = document.getElementById('txtAsOnDate');
    if (asOnDateEl) asOnDateEl.value = '';

    clearAppliedFilters();
    renderFilterBadges();
    state.PageNumber = 1;
    readFiltersIntoState();
    //loadGrid();
}

// ------------------------------------------------------------------
// Grid load (AJAX)
// ------------------------------------------------------------------
async function loadGrid() {
    const tbody = document.getElementById('tblBody');
    const colCount = document.querySelectorAll(`.col-${state.Mode.toLowerCase()}`).length / 2 || 6;
    tbody.innerHTML = `<tr><td colspan="${colCount}" class="text-center text-muted py-4">Loading...</td></tr>`;

    try {
        const res = await fetch('/Hostel/GetHostelManageReport', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(state)
        });

        const raw = await res.text();
        if (!res.ok) throw new Error(`HTTP ${res.status}: ${raw || '(empty body)'}`);
        if (!raw) throw new Error('Empty response body from server');

        const result = JSON.parse(raw);

        if (!result.success) {
            tbody.innerHTML = `<tr><td colspan="${colCount}" class="text-center text-danger py-4">${result.message || 'Failed to load data.'}</td></tr>`;
            return;
        }

       // renderRows(result.data || []);
        renderPagination(result.totalRecords || 0, result.pageNumber || state.PageNumber, state.PageSize);
    } catch (err) {
        console.error(err);
        tbody.innerHTML = `<tr><td colspan="${colCount}" class="text-center text-danger py-4">Unable to load report data.</td></tr>`;
    }
}

function esc(v) {
    if (v === null || v === undefined) return '';
    return String(v);
}

function fmtDate(d) {
    if (!d) return '';
    const dt = new Date(d);
    if (isNaN(dt.getTime())) return '';
    return dt.toLocaleDateString('en-GB');
}

function fmtMoney(v) {
    if (v === null || v === undefined) return '';
    return Number(v).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function renderRows(rows) {
    const tbody = document.getElementById('tblBody');

    if (!rows || rows.length === 0) {
        const colCount = document.querySelectorAll(`.col-${state.Mode.toLowerCase()}`).length / 2 || 6;
        tbody.innerHTML = `<tr><td colspan="${colCount}" class="text-center text-muted py-4">No records found.</td></tr>`;
        return;
    }

    let html = '';

    if (state.Mode === 'LIST') {
        rows.forEach(r => {
            html += `<tr>
                <td>${esc(r.hostelName)}</td>
                <td>${esc(r.hostelTypeName)}</td>
                <td>${esc(r.roomCoolingTypeName)}</td>
                <td>${esc(r.roomTypeTitle)}</td>
                <td>${esc(r.totalRoomsAdded)}</td>
                <td>${esc(r.totalBedsAdded)}</td>
                <td>${esc(r.totalExtraBedsAllowed)}</td>
                <td>${esc(r.availableCapacity)}</td>
                <td>${esc(r.availableCapacityWithExtraBed)}</td>
                <td>${esc(r.rentPerBed)}</td>
                <td>${esc(r.beds)}</td>
                <td>${fmtMoney(r.costPerBed)}</td>
               
            </tr>`;
        });
    } else if (state.Mode === 'RATE') {
        rows.forEach(r => {
            html += `<tr>
                <td>${esc(r.hostelName)}</td>
                <td>${esc(r.roomTitle)}</td>
                <td>${esc(r.roomType)}</td>
                <td>${fmtMoney(r.costPerBed)}</td>
                <td>${fmtMoney(r.securityAmount)}</td>
                <td>${fmtDate(r.effectiveFrom)}</td>
                <td>${fmtDate(r.effectiveFrom)}</td>
            </tr>`;
        });
    } else { // REPORT
        rows.forEach(r => {
            html += `<tr>
                <td>${esc(r.hostelName)}</td>
                <td>${esc(r.hostelTypeName)}</td>
                <td>${esc(r.intakeCapacity)}</td>
                <td>${esc(r.totalRoomsAdded)}</td>
                <td>${esc(r.totalBedsAdded)}</td>
                <td>${esc(r.availableCapacity)}</td>
            </tr>`;
        });
    }

    tbody.innerHTML = html;
}

// ------------------------------------------------------------------
// Pagination (rendered manually, same look as the reference page)
// ------------------------------------------------------------------
function renderPagination(totalRecords, pageNumber, pageSize) {
    const wrap = document.getElementById('paginationWrap');
    const pagerList = document.getElementById('pagerList');
    const recordInfo = document.getElementById('recordInfo');

    const totalPages = pageSize > 0 ? Math.ceil(totalRecords / pageSize) : 0;

    if (totalPages <= 1) {
        wrap.style.display = 'none';
        return;
    }
    wrap.style.display = 'flex';

    const startRec = totalRecords === 0 ? 0 : ((pageNumber - 1) * pageSize) + 1;
    const endRec = Math.min(pageNumber * pageSize, totalRecords);
    recordInfo.innerHTML = `Showing <strong>${startRec}</strong> – <strong>${endRec}</strong> of <strong>${totalRecords}</strong> records`;

    let start = Math.max(1, pageNumber - 2);
    let end = Math.min(totalPages, start + 4);
    start = Math.max(1, end - 4);

    let html = '';
    html += pagerItem(1, pageNumber <= 1, '<i class="ti ti-chevrons-left"></i>', pageNumber <= 1);
    html += pagerItem(pageNumber - 1, pageNumber <= 1, '<i class="ti ti-chevron-left"></i>', pageNumber <= 1);

    for (let pg = start; pg <= end; pg++) {
        html += pagerItem(pg, false, String(pg), false, pg === pageNumber);
    }

    html += pagerItem(pageNumber + 1, pageNumber >= totalPages, '<i class="ti ti-chevron-right"></i>', pageNumber >= totalPages);
    html += pagerItem(totalPages, pageNumber >= totalPages, '<i class="ti ti-chevrons-right"></i>', pageNumber >= totalPages);

    pagerList.innerHTML = html;
}

function pagerItem(page, disabled, label, isNavArrow, active) {
    return `<li class="page-item ${disabled ? 'disabled' : ''} ${active ? 'active' : ''}">
        <a class="page-link" href="javascript:void(0)" onclick="goToPage(${page})">${label}</a>
    </li>`;
}

function goToPage(page) {
    if (page < 1) return;
    state.PageNumber = page;
    //loadGrid();
}

// ------------------------------------------------------------------
// Export / Print
// ------------------------------------------------------------------
function triggerExport(format) {
    readFiltersIntoState();
    const params = new URLSearchParams();
    Object.entries(state).forEach(([k, v]) => {
        if (v !== null && v !== undefined && v !== '') params.append(k, v);
    });
    params.append('format', format);
    window.open('/Hostel/ExportHostelManageReport?' + params.toString(), '_blank');
}

// ========================================================================
// Hostel Manage Report — UI-only script.
// All filtering/paging is done by the browser navigating to plain GET URLs
// that HostelManageReport(HotelReportSearchRequest) model-binds directly.
// There is no fetch/AJAX here anymore — this file only wires up widgets.
// ========================================================================

document.addEventListener('DOMContentLoaded', () => {
    // Select2 for the filter dropdowns (still native <select> elements that
    // submit as part of #filterForm, select2 is purely cosmetic)
    try {
        if (window.jQuery && typeof jQuery.fn.select2 === 'function') {
            jQuery('#ddlFilterCompany,#ddlFilterSessions,#ddlFilterHostel, #ddlFilterHostelType, #ddlFilterRoomType, #ddlFilterCoolingType, #ddlFilterStatus')
                .add('select[name="HostelID"], select[name="HostelTypeID"], select[name="RoomTypeID"], select[name="RoomCoolingTypeId"], select[name="IsActive"]')
                .select2({
                    width: '100%',
                    dropdownParent: jQuery('#filter-dropdown'),
                    allowClear: true,
                    placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
                });
        }
    } catch (e) {
        console.warn('Select2 init skipped:', e);
    }

    // Keep the filter dropdown open when interacting with its contents
    document.getElementById('filter-dropdown')?.addEventListener('click', e => e.stopPropagation());

    // Enter in the search box submits the form (native GET, full page reload)
    const searchInput = document.querySelector('#filterForm input[name="SearchText"]');
    searchInput?.addEventListener('keyup', (e) => {
        if (e.key === 'Enter') {
            document.getElementById('filterForm').submit();
        }
    });
});