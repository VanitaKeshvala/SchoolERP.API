function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#tblBooks')) {
        const table = $('#tblBooks').DataTable();
        table.button(index).trigger();
    }
}

// ========================================
// Filter badges — sessionStorage persistence
// ========================================
const FILTER_KEY = 'appliedFilters_book';
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

// Single source of truth for every dropdown filter: select element id,
// hidden input id that actually gets posted to the server, and badge label.
// Adding a new filter later only means adding one entry here.
const FILTER_DROPDOWNS = [
    { select: 'ddlFilterCompany', hidden: 'hdnCompanyId', label: 'Company' },
    { select: 'ddlFilterPublisher', hidden: 'hdnPublisherId', label: 'Publisher' },
    { select: 'ddlFilterAuthor', hidden: 'hdnAuthorId', label: 'Author' },
    { select: 'ddlFilterCategory', hidden: 'hdnCategoryId', label: 'Category' },
    { select: 'ddlFilterSubject', hidden: 'hdnSubjectId', label: 'Subject' },
    { select: 'ddlFilterLanguage', hidden: 'hdnLanguageId', label: 'Language' },
    { select: 'ddlFilterSeries', hidden: 'hdnSeriesId', label: 'Series' },
    { select: 'ddlFilterDocumentType', hidden: 'hdnDocumentId', label: 'Document type' },
    { select: 'ddlFilterIsActive', hidden: 'hdnIsActive', label: 'Status' }
];

function submitForm() {
    // Push every filter dropdown's current value into its hidden field so the
    // GET form (server-side) picks it up — nothing here filters client-side.
    FILTER_DROPDOWNS.forEach(({ select, hidden }) => {
        const selEl = document.getElementById(select);
        const hidEl = document.getElementById(hidden);
        if (selEl && hidEl) hidEl.value = selEl.value;
    });
    document.getElementById('frmSearch').submit();
}

function applyFilters() {
    const searchEl = document.getElementById('txtSearchInput');
    const searchVal = searchEl?.value.trim();
    if (searchVal) appliedFilters['txtSearchInput'] = { label: 'Search', text: searchVal };
    else delete appliedFilters['txtSearchInput'];

    FILTER_DROPDOWNS.forEach(({ select, label }) => {
        const el = document.getElementById(select);
        if (el?.value && el.value !== '') {
            appliedFilters[select] = {
                label,
                text: el.options[el.selectedIndex]?.text || el.value
            };
        } else {
            delete appliedFilters[select];
        }
    });

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
    if (filterId === 'txtSearchInput') {
        const el = document.getElementById('txtSearchInput');
        if (el) el.value = '';
        document.getElementById('hdnSearch').value = '';
    } else {
        const entry = FILTER_DROPDOWNS.find(f => f.select === filterId);
        if (entry) {
            const el = document.getElementById(entry.select);
            if (el) {
                el.value = '';
                if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
            }
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

    FILTER_DROPDOWNS.forEach(({ select }) => {
        const el = document.getElementById(select);
        if (!el) return;
        el.value = '';
        if (window.jQuery && jQuery(el).data('select2')) jQuery(el).val('').trigger('change');
    });

    clearAppliedFilters();
    document.getElementById('hdnPageIndex').value = 1;
    submitForm();
}

// ========================================
// Wizard field maps (single source of truth)
// ========================================
// Plain text/number/date/textarea inputs: form field id -> BookUpsertRequest property name
const SIMPLE_FIELD_MAP = {
    txtTitle: 'BookTitle',
    txtSubTitle: 'SubTitle',
    ddlDocumentType: 'DocumentId',
    ddlCategory: 'CategoryId',
    ddlSubject: 'SubjectId',
    ddlLanguage: 'LanguageId',
    ddlPublisher: 'PublisherId',
    txtEdition: 'Edition',
    txtDate: 'PostDate',
    txtISBN: 'ISBNNo',
    txtClassNo: 'ClassNo',
    txtNo: 'BookNo',
    txtVolume: 'Volume',
    txtNoOfPages: 'NoOfPages',
    ddlSeries: 'SeriesId',
    txtKeyWord: 'KeyWord',
    txtRemarks: 'Remarks',
    txtDescription: 'Description',
    ddlBudget: 'BudgetId',
    ddlSupplier: 'SupplierId',
    txtBillNo: 'BillNo',
    txtBillDate: 'BillDate',
    txtLocation: 'Location',
    txtPrice: 'Price',
    txtAccessionNo: 'AccessionNo',
    txtQuantity: 'Quantity'
};

// ========================================
// DOMContentLoaded — init DataTable + UI
// ========================================
document.addEventListener('DOMContentLoaded', () => {

    // ── DataTable (export only) ───────────────────────────────────────
    if ($.fn.DataTable.isDataTable('#tblBooks')) {
        $('#tblBooks').DataTable().destroy();
    }
    $.fn.dataTable.ext.errMode = 'none';
    window.exportTable = $('#tblBooks').DataTable({
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

    // ── Select2 (all filter dropdowns) ──────────────────────────────────
    try {
        if (window.jQuery && typeof jQuery.fn.select2 === 'function') {
            FILTER_DROPDOWNS.forEach(({ select }) => {
                const el = document.getElementById(select);
                if (!el) return;
                jQuery(el).select2({
                    width: '100%',
                    dropdownParent: jQuery('#filter-dropdown'),
                    allowClear: true,
                    placeholder: function () { return jQuery(this).data('placeholder') || 'Select'; }
                });
            });
        }
    } catch (e) {
        console.warn('Select2 init skipped:', e);
    }

    // ── Select2 (wizard form dropdowns) ─────────────────────────────────
    // NOTE: these use a dedicated class (.js-wizard-select2), NOT the generic
    // ".select2" class, on purpose. Many admin templates ship a site-wide
    // script that auto-initializes every ".select2" element as a remote/AJAX
    // search box (for "search employee/customer" style pickers). If these
    // wizard dropdowns shared that class, that global script would hijack
    // them, strip their server-rendered local <option> list, and replace it
    // with an AJAX search that returns "No results found" for every field —
    // which is exactly the symptom of empty-looking, unselectable dropdowns
    // at page load. Keeping a separate class + explicitly destroying any
    // prior instance below makes this immune to that collision either way.
    try {
        if (window.jQuery && typeof jQuery.fn.select2 === 'function') {
            jQuery('#itemForm .js-wizard-select2').each(function () {
                const $el = jQuery(this);
                if ($el.data('select2')) {
                    $el.select2('destroy'); // clear out any stray/global init first
                }
                $el.select2({
                    width: '100%',
                    placeholder: $el.data('placeholder') || '-- Select --',
                    allowClear: !$el.prop('multiple'),
                    minimumResultsForSearch: $el.prop('multiple') ? 0 : 6
                    // No `ajax` key here — these are always local, server-rendered options.
                });
            });
        } else {
            console.warn('select2 plugin not found on window.jQuery.fn — wizard dropdowns will render as plain <select> elements.');
        }
    } catch (e) {
        console.warn('Select2 wizard init skipped:', e);
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
        resetAllFilters();
    });

    // ── Book Find (autocomplete) ────────────────────────────────────────
    initBookFind();

    // ── File input change → show name/size, clear previous errors ──────
    document.getElementById('attachmentFile')?.addEventListener('change', function () {
        const fileError = document.getElementById('fileError');
        const fileInfo = document.getElementById('fileInfo');
        fileError.style.display = 'none';
        fileError.textContent = '';
        const file = this.files && this.files[0];
        if (!file) { fileInfo.textContent = ''; return; }

        const err = validateAttachment(file);
        if (err) {
            fileError.textContent = err;
            fileError.style.display = 'block';
            this.value = '';
            fileInfo.textContent = '';
            return;
        }
        fileInfo.textContent = `${file.name} (${(file.size / 1024).toFixed(1)} KB)`;
    });

    // ── Edit-mode bindings: cascading subject + multi-select authors ──
    initEditModeBindings();

    // ── Render badges on load ─────────────────────────────────────────
    renderFilterBadges();
});

// ========================================
// Book Find — autocomplete that fully populates the form
// ========================================
function initBookFind() {
    const input = document.getElementById('txtTitleFind');
    const list = document.getElementById('bookSuggestions');
    if (!input || !list) {
        console.warn('Books autocomplete: input or list element not found');
        return;
    }
    let debounceTimer;
    let currentItems = [];

    input.addEventListener('input', function () {
        const term = this.value.trim();
        clearTimeout(debounceTimer);

        if (term.length < 2) {
            hideList();
            return;
        }
        debounceTimer = setTimeout(() => fetchSuggestions(term), 300);
    });

    document.addEventListener('click', (e) => {
        if (!list.contains(e.target) && e.target !== input) hideList();
    });

    async function fetchSuggestions(term) {
        try {
            const res = await fetch(`/Library/SearchBookTitle?term=${encodeURIComponent(term)}`);
            const result = await res.json();

            if (result.success && result.data && result.data.length > 0) {
                currentItems = result.data;
                renderList(currentItems);
            } else {
                hideList();
            }
        } catch (err) {
            console.error('SearchBookTitle error:', err);
            hideList();
        }
    }

    function renderList(items) {
        list.innerHTML = '';
        items.forEach((item, idx) => {
            const li = document.createElement('li');
            li.className = 'list-group-item list-group-item-action';
            li.style.cursor = 'pointer';
            li.textContent = item.bookTitle ?? '';
            li.addEventListener('click', () => selectSuggestion(idx));
            list.appendChild(li);
        });
        list.style.display = 'block';
    }

    async function selectSuggestion(idx) {
        const item = currentItems[idx];
        input.value = item.bookTitle ?? '';
        hideList();

        const bookId = item.bookID ?? item.bookId;
        if (!bookId) return;

        try {
            // Expects an endpoint that returns the FULL book record (all wizard-step
            // fields), not just title/id. Adjust the URL if your route differs.
            const res = await fetch(`/Library/GetBookDetails?id=${encodeURIComponent(bookId)}`);
            const result = await res.json();
            if (result.success && result.data) {
                populateFormFromBook(result.data);
                showToast('Book details loaded — you are now editing this record.', 'info');
            } else {
                showToast(result.message || 'Could not load book details.', 'warning');
            }
        } catch (err) {
            console.error('GetBookDetails error:', err);
            showToast('Could not load book details.', 'warning');
        }
    }

    function hideList() {
        list.style.display = 'none';
        list.innerHTML = '';
    }
}

// Fills every wizard field (all 4 steps) from a full book record and switches
// the form into edit mode. `data` is expected to roughly mirror BookUpsertRequest
// plus display-only fields (FileName / CoverPageImage).
function populateFormFromBook(data) {
    document.getElementById('hdnId').value = data.bookID ?? data.bookId ?? 0;

    const setVal = (id, val) => {
        const el = document.getElementById(id);
        if (el) el.value = (val ?? '');
    };
    const setSelect = (id, val) => {
        const el = document.getElementById(id);
        if (!el) return;
        el.value = val ? String(val) : '';
        if (window.jQuery && jQuery(el).data('select2')) jQuery(el).trigger('change');
    };

    // Step 1
    setVal('txtTitle', data.bookTitle);
    setVal('txtSubTitle', data.subTitle);
    setSelect('ddlDocumentType', data.documentId);
    setSelect('ddlLanguage', data.languageId);
    setSelect('ddlPublisher', data.publisherId);
    setVal('txtEdition', data.edition);
    setVal('txtDate', data.postDate ? data.postDate.substring(0, 10) : '');
    setVal('txtISBN', data.isbnNo);
    setVal('txtClassNo', data.classNo);
    setVal('txtNo', data.bookNo);
    setVal('txtVolume', data.volume);
    setVal('txtNoOfPages', data.noOfPages);
    setSelect('ddlSeries', data.seriesId);
    setVal('txtKeyWord', data.keyWord);
    setVal('txtRemarks', data.remarks);
    setVal('txtDescription', data.description);
    setVal('chkActive', data.isActive);

    // Category -> Subject cascade, then select the subject once loaded
    setSelect('ddlCategory', data.categoryId);
    if (data.categoryId) {
        loadSubjectByCategory(data.categoryId).then(() => setSelect('ddlSubject', data.subjectId));
    } else {
        setSelect('ddlSubject', data.subjectId);
    }

    // Step 2 — cover/attachment (existing file link only; browsers cannot
    // pre-populate a <input type="file">'s selected file for security reasons)
    document.getElementById('hdnExistingFile').value = data.fileName ?? '';
    document.getElementById('hdnRemoveFile').value = 'false';
    const existingWrap = document.getElementById('existingFileInfo');
    if (data.coverPageImage) {
        if (existingWrap) {
            existingWrap.querySelector('a').href = data.coverPageImage;
            existingWrap.querySelector('a').innerHTML = `<i class="ti ti-paperclip me-1"></i>${data.fileName ?? ''}`;
            existingWrap.classList.remove('d-none');
        }
    } else if (existingWrap) {
        existingWrap.classList.add('d-none');
    }

    // Step 3 — authors (multi-select, comma separated ids from server)
    const ids = (data.authorId ?? data.authorIds ?? '')
        .toString().split(',').map(s => s.trim()).filter(Boolean);
    const ddlAuthor = document.getElementById('ddlAuthor');
    if (ddlAuthor) {
        $(ddlAuthor).val(ids).trigger('change');
    }

    // Step 4 — accessioning
    setSelect('ddlBudget', data.budgetId);
    setSelect('ddlSupplier', data.supplierId);
    setVal('txtBillNo', data.billNo);
    setVal('txtBillDate', data.billDate ? data.billDate.substring(0, 10) : '');
    setVal('txtLocation', data.location);
    setVal('txtPrice', data.price);
    setVal('txtAccessionNo', data.accessionNo);
    setVal('txtQuantity', data.quantity ?? data.totalQty);
    setVal('ddlDocumentStatus', data.documentStatusId ?? data.documentStatusId);

    // Jump back to step 1 so the user sees what was loaded
    if (typeof currentStep !== 'undefined' && currentStep !== 1) {
        goToWizardStep(1);
    }
}

// ========================================
// Edit-mode bootstrap (runs once on page load if editing)
// ========================================
async function initEditModeBindings() {
    const editData = window.editBookData;
    if (!editData) return;

    if (editData.categoryId) {
        await loadSubjectByCategory(editData.categoryId);
        if (editData.subjectId) {
            const el = document.getElementById('ddlSubject');
            if (el) {
                el.value = String(editData.subjectId);
                if (window.jQuery && jQuery(el).data('select2')) jQuery(el).trigger('change');
            }
        }
    }

    if (editData.authorIds) {
        const ids = String(editData.authorIds).split(',').map(s => s.trim()).filter(Boolean);
        const ddlAuthor = document.getElementById('ddlAuthor');
        if (ddlAuthor) $(ddlAuthor).val(ids).trigger('change');
    }
}

// ========================================
// Search input — debounced server submit (list page)
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

function initializeDataTable() {
    if ($.fn.DataTable.isDataTable('#tblBooks')) {
        $('#tblBooks').DataTable().destroy();
    }

    $('#tblBooks').DataTable({
        destroy: true,
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] } }
        ],
        searching: false,
        paging: false,
        info: false,
        ordering: false,
        language: {
            emptyTable: "No books found in catalog."
        }
    });
}

let selectedId = 0;

function selectItem(id, element) {
    selectedId = id;
    $(element).addClass('bg-light').siblings().removeClass('bg-light');
    $('#btnEdit, #btnDelete').prop('disabled', false);
    let checkedCount = $('.student-checkbox:checked').length;
    $('#btnDelete').prop('disabled', checkedCount === 0);
    $('#btnEdit').prop('disabled', checkedCount !== 1);
    $('#btnActive').prop('disabled', checkedCount === 0);
    $('#btnInactive').prop('disabled', checkedCount === 0);
}

function editSelected() {
    if (selectedId <= 0) return;
    try {
        if (selectedId) {
            location.href = `/Library/AddBooks/${selectedId}`;
        }
    } catch (err) {
        console.error(err);
    }
}

// ========================================
// Validation
// ========================================
function validateAttachment(file) {
    const maxSizeMB = window.maxUploadSizeMB || 10; // fallback if server config isn't surfaced to JS
    const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'];
    if (file.size > maxSizeMB * 1024 * 1024) {
        return `File size exceeds the ${maxSizeMB} MB limit.`;
    }
    if (allowedTypes.length && !allowedTypes.includes(file.type)) {
        return 'Only JPG, PNG or PDF files are allowed.';
    }
    return null;
}

function validateForm() {
    const form = document.getElementById('itemForm');
    let firstInvalidStep = null;
    let isValid = true;

    // Required native inputs across all steps (not just the currently visible one)
    form.querySelectorAll('[required]').forEach(el => {
        const val = (el.value ?? '').toString().trim();
        const invalid = !val;
        el.classList.toggle('is-invalid', invalid);
        if (invalid) {
            isValid = false;
            const pane = el.closest('.wizard-pane');
            if (pane && firstInvalidStep === null) {
                firstInvalidStep = parseInt(pane.id.replace('step', ''), 10);
            }
        }
    });

    // Author required (at least one)
    const authorVal = $('#ddlAuthor').val();
    if (!authorVal || authorVal.length === 0) {
        isValid = false;
        if (firstInvalidStep === null) firstInvalidStep = 3;
        showToast('Please select at least one author.', 'warning');
    }

    // File error already surfaced live on the input; block submit if still present
    const fileError = document.getElementById('fileError');
    if (fileError && fileError.style.display === 'block') {
        isValid = false;
        if (firstInvalidStep === null) firstInvalidStep = 2;
    }

    if (!isValid && firstInvalidStep !== null && typeof goToWizardStep === 'function') {
        goToWizardStep(firstInvalidStep);
    }

    return isValid;
}

// ========================================
// Save (multipart/form-data — required because a file upload cannot travel
// inside a JSON request body; the previous version sent JSON.stringify(data)
// with contentType 'application/json', which silently drops the file).
// ========================================
function collectAuthorIdsCsv() {
    const vals = $('#ddlAuthor').val() || [];
    return vals.join(',');
}

function saveData() {
    if (!validateForm()) {
        showToast('Please complete all required fields.', 'warning');
        return;
    }

    const formData = new FormData();
    formData.append('BookID', document.getElementById('hdnId').value || '0');
    formData.append('TotalQty', document.getElementById('txtQuantity').value || '0');
    formData.append('AvailableQty', document.getElementById('txtQuantity').value || '0');
    formData.append('BookPrice', document.getElementById('txtPrice').value || '0');
    formData.append('DocumentStatusId', document.getElementById('ddlDocumentStatus').value || '0');
    formData.append('AuthorId', collectAuthorIdsCsv());
    formData.append('IsActive', document.getElementById('chkActive').checked);
    for (const [fieldId, propName] of Object.entries(SIMPLE_FIELD_MAP)) {
        const el = document.getElementById(fieldId);
        if (!el) continue;
        formData.append(propName, el.value ?? '');
    }

    const fileEl = document.getElementById('attachmentFile');
    if (fileEl && fileEl.files && fileEl.files[0]) {
        formData.append('attachmentFile', fileEl.files[0]);
    }
    formData.append('ExistingFileName', document.getElementById('hdnExistingFile')?.value || '');
    formData.append('RemoveAttachment', document.getElementById('hdnRemoveFile')?.value || 'false');

    const btn = document.querySelector('.top-btn.btn-success');
    const originalHtml = btn ? btn.innerHTML : null;
    if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Saving...';
    }

    $.ajax({
        url: '/Library/UpsertBook',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            if (res.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Saved!',
                    text: 'Book has been saved successfully.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-success' },
                    buttonsStyling: false
                }).then(() => {
                    window.location.href = '/Library/Books';
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: res.message || 'Failed to save book.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-danger' },
                    buttonsStyling: false
                });
            }
        },
        error: function (xhr) {
            console.error('UpsertBook error:', xhr.status, xhr.responseText);
            Swal.fire('Error!', 'An unexpected error occurred while saving.', 'error');
        },
        complete: function () {
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = originalHtml;
            }
        }
    });
}

// Wizard's "Save" (finish) button reuses the same save routine
function finishWizard() {
    saveData();
}

function removeExistingFile() {
    document.getElementById('hdnRemoveFile').value = 'true';
    document.getElementById('existingFileInfo')?.classList.add('d-none');
}

function deleteSelected() {
    if (selectedId <= 0) return;

    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one book.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this book record. This action cannot be undone!",
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
                url: '/Library/DeleteBook',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Book record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete book.', 'error');
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
    let sectionIds = [];
    $('.student-checkbox:checked').each(function () {
        sectionIds.push(parseInt($(this).val()));
    });

    if (sectionIds.length === 0) {
        if (id != 0) {
            sectionIds.push(parseInt(id));
        } else {
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
                text: 'Activating this record will make it visible and available for use across the system. Do you want to continue?'
            }
            : {
                icon: 'warning',
                title: 'Deactivate Record(s)?',
                text: 'Deactivating this record will hide it from the system and prevent it from being used. Do you want to continue?'
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
                    url: '/Library/ToggleStatus',
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
                        console.error('Status update error:', xhr.status, xhr.responseText);
                        Swal.fire('Error!', 'An unexpected error occurred.', 'error');
                    }
                });
            }
        });
    } catch (err) {
        console.error(err);
        IV.setNotice('sectionPageNotice', 'Could not update status.');
        location.reload();
    }
}

// Returns a promise so callers (autofill/edit-mode init) can await it.
async function loadSubjectByCategory(categoryId) {
    const subjectEl = document.getElementById("ddlSubject");
    subjectEl.innerHTML = '<option value="">-- Select Subject --</option>';

    if (!categoryId) return;

    try {
        const response = await fetch(`/Library/GetLibrarySubjectDropdownList?categoryId=${categoryId}`);
        const result = await response.json();
        if (!result.success) return;

        result.data.forEach(item => {
            const option = document.createElement("option");
            option.value = item.id;
            option.text = item.name;
            subjectEl.appendChild(option);
        });

        if (window.jQuery && jQuery(subjectEl).data('select2')) {
            jQuery(subjectEl).trigger("change");
        }
    } catch (e) {
        console.error(e);
    }
}

// ========================================
// Wizard navigation
// ========================================
let currentStep = 1;

async function moveWizard(delta) {
    const nextStep = currentStep + delta;
    await goToWizardStep(nextStep);
}

async function goToWizardStep(nextStep) {
    if (nextStep < 1 || nextStep > 4 || nextStep === currentStep) return;

    const userId = parseInt(document.getElementById('hdnId').value) || 0;
    const isEdit = userId > 0;

    // Validate Step 1 required fields before allowing forward navigation
    if (nextStep > currentStep && currentStep === 1) {
        const title = document.getElementById('txtTitle').value.trim();
        const subTitle = document.getElementById('txtSubTitle').value.trim();
        let ok = true;

        if (!title) {
            document.getElementById('txtTitle').classList.add('is-invalid');
            ok = false;
        } else {
            document.getElementById('txtTitle').classList.remove('is-invalid');
        }
        if (!subTitle) {
            document.getElementById('txtSubTitle').classList.add('is-invalid');
            ok = false;
        } else {
            document.getElementById('txtSubTitle').classList.remove('is-invalid');
        }

        if (!ok) {
            showToast('Please fill required fields before continuing.', 'warning');
            return;
        }

        const btnNext = document.getElementById('btnWizardNext');
        const originalHtml = btnNext.innerHTML;
        btnNext.disabled = true;
        btnNext.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
        btnNext.disabled = false;
        btnNext.innerHTML = originalHtml;
    }

    // Sync UI Indicators
    document.querySelector(`.wizard-pane[id="step${currentStep}"]`).classList.remove('active');
    document.querySelector(`.nav-link[data-step="${currentStep}"]`).classList.remove('active');

    if (nextStep > currentStep) {
        document.querySelector(`.nav-link[data-step="${currentStep}"]`).classList.add('completed');
    }

    currentStep = nextStep;
    if (currentStep != 1) {
        updateBookTitle(currentStep);
    }

    document.querySelector(`.wizard-pane[id="step${currentStep}"]`).classList.add('active');
    document.querySelector(`.nav-link[data-step="${currentStep}"]`).classList.add('active');

    // Button visibility
    document.getElementById('btnWizardPrev').classList.toggle('d-none', currentStep === 1);
    document.getElementById('btnWizardNext').classList.toggle('d-none', currentStep === 4);
    document.getElementById('btnBack').classList.toggle('d-none', currentStep !== 1);

    const btnFinish = document.getElementById('btnWizardFinish');
    if (isEdit) {
        btnFinish.classList.remove('d-none');
    } else {
        btnFinish.classList.toggle('d-none', currentStep !== 4);
    }
    btnFinish.innerHTML = '<i class="ti ti-cloud me-1"></i> Save';
}
function updateBookTitle(currentStep) {
    const title = document.getElementById("txtTitle").value.trim();
    document.getElementById(`lblBookTitle${currentStep}`).textContent = title;
    title || "No Book Title";
}