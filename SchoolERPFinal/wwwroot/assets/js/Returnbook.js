/*****************************************************************************************
CONFIRM RETURN — search a member OR scan an Accession No, load issued books,
batch return with shared Return Date / auto-calculated Fine / Remark.

Endpoints used:
  GET  /Library/SearchMember?memberType=&searchText=
       (existing — same one used on IssueBookAdd.cshtml)

  GET  /Library/GetIssuedBooksByMember?libraryMemberID={id}&pageIndex=&pageSize=
       -> { success, data: [ { issueReturnID, bookTitle, bookNo,
                                issueDate, dueReturnDate, status } ],
            totalRecords, pageNumber }

  GET  /Library/SearchAccession?searchText=xxx           *** NEW, add on backend ***
       -> { success, data: [ { accessionNo, bookTitle, memberName, libraryCardNo } ] }
          (only copies that are currently issued — for the suggestion dropdown)

  GET  /Library/GetAccessionById?accessionNo=xxx         *** NEW, add on backend ***
       Backend SP matches the input against Accession No *or* Library Card No,
       and returns the full member record (StaffID, StudentID, MemberType,
       LibraryCardNo, name/admission/staff fields, etc.) alongside the
       book/issue fields — everything needed to bind in ONE call.
       -> { success, message, data: {
              issueReturnID, bookNo, bookTitle,
              libraryMemberID,
              StaffID, StudentID, LibraryCardNo, MemberType,
              // ...plus all the other fields showMemberDetail() reads
              // (student_FirstName / staff_FirstName, admissionNo, rollNo,
              //  className, sectionName, staffCode, departmentName,
              //  designationID, noofdocuments, maxdays, gender,
              //  mobileNo, email, expiryDATE, isActive, photo, ...)
          } }

  POST /Library/ReturnIssueBook
       body: { ReturnListJson }
       item shape: { IssueReturnID, ReturnDate, FineAmount, ModifyFine, Remarks }
*****************************************************************************************/
'use strict';

let currentIssuedBooks = [];     // last fetched batch for the selected member
let selectedMemberId = window.CURRENT_MEMBER_ID || 0;
const FINE_PER_DAY = window.FINE_PER_DAY || 0;

// Paging state for GetIssuedBooksByMember
const PAGE_SIZE = 5;
let currentPageIndex = 1;
let totalRecords = 0;

function todayIso() {
    return new Date().toISOString().split('T')[0];
}

function parseDateOnly(d) {
    if (!d) return null;
    const dt = new Date(d);
    if (isNaN(dt)) return null;
    dt.setHours(0, 0, 0, 0);
    return dt;
}

function overdueDaysBetween(dueDateStr, returnDateStr) {
    const due = parseDateOnly(dueDateStr);
    const ret = parseDateOnly(returnDateStr);
    if (!due || !ret) return 0;
    const diffMs = ret - due;
    const days = Math.round(diffMs / (1000 * 60 * 60 * 24));
    return days > 0 ? days : 0;
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    if (isNaN(d)) return '-';
    return d.toLocaleDateString();
}

function setFieldError(el, errEl, message) {
    if (!el) return;
    el.classList.toggle('is-invalid', !!message);
    if (errEl) errEl.textContent = message || '';
}

function escapeHtml(s) {
    return String(s || '').replace(/[&<>"']/g, ch => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[ch]));
}

function getVal(item, ...keys) {
    for (const k of keys) if (item[k] !== undefined && item[k] !== null && item[k] !== '') return item[k];
    return null;
}

function getName(item) {
    const first = getVal(item, 'student_FirstName', 'staff_FirstName', 'firstName');
    const last = getVal(item, 'student_LastName', 'staff_LastName', 'lastName');
    if (!first && !last) return null;
    return `${first ?? ''} ${last ?? ''}`.trim();
}

// Normalizes MemberType so "Student"/"student"/"S" style differences from
// different endpoints don't break the isStudent check in showMemberDetail().
function isStudentType(memberType) {
    return (memberType || '').toString().trim().toLowerCase().startsWith('stud');
}

document.addEventListener('DOMContentLoaded', () => {
    const table = document.getElementById('tblReturnBooks');
    if (!table) return; // not on the Confirm Return page

    const selectAll = document.getElementById('chkSelectAll');
    const commonReturnDate = document.getElementById('commonReturnDate');
    const commonFineAmount = document.getElementById('commonFineAmount');
    const commonRemark = document.getElementById('commonRemark');
    const toolbar = document.getElementById('returnToolbar');
    const noMemberAlert = document.getElementById('noMemberAlert');
    const helperText = document.getElementById('helperText');
    const body = document.getElementById('issuedBooksTableBody');
    const detailCard = document.getElementById('memberDetailCard');

    if (commonReturnDate && !commonReturnDate.value) {
        commonReturnDate.value = todayIso();
        commonReturnDate.max = todayIso(); // can't return a book in the future
    }

    // ============================================================
    // Member search (Student/Staff) — mirrors IssueBookAdd.cshtml
    // ============================================================
    initMemberSearch();

    // ============================================================
    // Accession No → Return flow (scan/search a copy's accession no,
    // auto-bind the member who has it + load their issued-books list)
    // Falls back to Card No search if the text isn't a valid accession.
    // ============================================================
    initAccessionReturnFlow();

    // If the page was opened with a LibraryMemberID already (e.g. from the
    // Issue list's "Return" action), auto-load that member's books.
    if (selectedMemberId > 0) {
        loadIssuedBooks(selectedMemberId, 1);
    }

    function initMemberSearch() {
        const input = document.getElementById('txtsearchText');
        const list = document.getElementById('pinCardSuggestions');
        if (!input || !list) return;

        let debounceTimer;
        let currentItems = [];

        input.addEventListener('input', function () {
            const term = this.value.trim();
            clearTimeout(debounceTimer);
            if (term.length < 2) { hideList(); return; }
            debounceTimer = setTimeout(() => fetchMembers(term), 300);
        });

        document.addEventListener('click', (e) => {
            if (!input.contains(e.target) && !list.contains(e.target)) hideList();
        });

        async function fetchMembers(term) {
            try {
                const memberType = document.getElementById('ddlMemberType')?.value || '';
                const res = await fetch(`/Library/SearchMember?memberType=${encodeURIComponent(memberType)}&searchText=${encodeURIComponent(term)}`);
                const result = await res.json();
                if (result.success && result.data && result.data.length > 0) {
                    currentItems = result.data;
                    renderList(currentItems);
                } else {
                    hideList();
                }
            } catch (err) {
                console.error('SearchMember error:', err);
                hideList();
            }
        }

        function renderList(items) {
            list.innerHTML = '';
            items.forEach((item, idx) => {
                const li = document.createElement('li');
                li.className = 'list-group-item list-group-item-action';
                const name = getName(item);
                li.innerHTML = `<strong>${item.libraryCardNo ?? '-'}</strong> &nbsp;-&nbsp; ${name || '-'}`;
                li.addEventListener('click', () => selectMember(idx));
                list.appendChild(li);
            });
            list.style.display = 'block';
        }

        function selectMember(idx) {
            const item = currentItems[idx];
            if (!item) return;

            input.value = item.libraryCardNo ?? getName(item) ?? '';
            hideList();
            bindMemberAndLoad(item);
        }

        function hideList() { list.style.display = 'none'; list.innerHTML = ''; }
    }

    // ============================================================
    // Shared: bind a member object (from either flow) into the
    // detail card, remember the selected member, then load their
    // issued books.
    // ============================================================
    function bindMemberAndLoad(item, opts = {}) {
        selectedMemberId = item.libraryMemberID;
        document.getElementById('hdnLibraryMemberID').value = selectedMemberId;
        window.CURRENT_MEMBER_ID = selectedMemberId;

        syncSearchControls(item);
        showMemberDetail(item);
        if (detailCard) detailCard.style.display = 'block';

        return loadIssuedBooks(selectedMemberId, 1).then(() => {
            if (opts.highlightIssueReturnID) highlightRow(opts.highlightIssueReturnID);
        });
    }

    // Keeps the left-hand "Member Type" dropdown and "Search Card No / Name"
    // box in sync no matter which flow resolved the member (name/card search
    // OR accession/card scan), so the form always reflects who's selected.
    function syncSearchControls(item) {
        const ddlMemberType = document.getElementById('ddlMemberType');
        const searchInput = document.getElementById('txtsearchText');

        // Tolerant of casing differences coming back from different endpoints
        // (memberType vs MemberType, libraryCardNo vs LibraryCardNo, etc).
        const memberType = getVal(item, 'memberType', 'MemberType', 'member_Type') || '';
        const cardNo = getVal(item, 'libraryCardNo', 'LibraryCardNo', 'library_Card_No');

        if (!memberType) {
            console.warn('syncSearchControls: no MemberType field found on item', item);
        }
        if (!cardNo) {
            console.warn('syncSearchControls: no LibraryCardNo field found on item', item);
        }

        if (ddlMemberType) {
            const match = Array.from(ddlMemberType.options)
                .find(o => o.value && o.value.toLowerCase() === memberType.toLowerCase());
            ddlMemberType.value = match ? match.value : '';
            // In case anything else listens for member-type changes (filters etc).
            ddlMemberType.dispatchEvent(new Event('change', { bubbles: true }));
        }

        if (searchInput) {
            searchInput.value = cardNo || getName(item) || '';
        }
    }

    function showMemberDetail(item) {
        const memberType = getVal(item, 'memberType', 'MemberType');
        const isStudent = isStudentType(memberType);

        document.getElementById('memberName').textContent = getName(item) || getVal(item, 'memberName', 'MemberName') || '-';
        document.getElementById('memberTypeBadge').textContent = memberType || '-';

        document.getElementById('studentOnlyFields').style.display = isStudent ? '' : 'none';
        document.getElementById('staffOnlyFields').style.display = isStudent ? 'none' : '';
        document.querySelectorAll('.studentOnlyRow').forEach(el => el.style.display = isStudent ? '' : 'none');
        document.querySelectorAll('.staffOnlyRow').forEach(el => el.style.display = isStudent ? 'none' : '');

        const photo = document.getElementById('memberPhoto');
        photo.src = item.photo ? item.photo : '/assets/img/doctors/doctor-01.jpg';

        if (isStudent) {
            document.getElementById('memberAdmissionNo').textContent = item.admissionNo ?? '-';
            document.getElementById('memberRollNo').textContent = item.rollNo ?? '-';
            document.getElementById('memberClassName').textContent = item.className ?? '-';
            document.getElementById('memberSectionName').textContent = item.sectionName ?? '-';
        } else {
            document.getElementById('memberStaffCode').textContent = item.staffCode ?? '-';
            document.getElementById('memberDepartment').textContent = item.departmentName ?? '-';
            document.getElementById('memberDesignation').textContent = item.designationID ?? '-';
        }
        document.getElementById('memberNoOfDocuments').textContent = item.noofdocuments ?? '-';
        document.getElementById('memberMAXDays').textContent = item.maxdays ?? '-';
        document.getElementById('memberGender').textContent = item.gender ?? '-';
        document.getElementById('memberMobileNo').textContent = getVal(item, 'student_MobileNo', 'staff_MobileNo', 'mobileNo') ?? '-';
        document.getElementById('memberEmail').textContent = getVal(item, 'student_Email', 'staff_Email', 'email') ?? '-';
        document.getElementById('memberLibraryCardNo').textContent = getVal(item, 'libraryCardNo', 'LibraryCardNo') ?? '-';
        document.getElementById('memberExpiryDate').textContent = formatDate(item.expiryDATE ?? item.expiryDate);
        document.getElementById('memberIsActive').textContent = item.isActive ? 'Active' : 'Inactive';
    }

    // ============================================================
    // Accession No → Return flow (with Card No fallback)
    // ============================================================
    function initAccessionReturnFlow() {
        const accInput = document.getElementById('txtAccessionNo');
        const accList = document.getElementById('accessionSuggestions');
        const errEl = document.getElementById('errAccessionNo');
        if (!accInput) return;

        let debounceTimer;
        let suggestions = [];

        accInput.addEventListener('input', function () {
            const term = this.value.trim();
            clearTimeout(debounceTimer);
            setAccError(null);
            if (term.length < 2) { hideAccList(); return; }
            debounceTimer = setTimeout(() => fetchSuggestions(term), 300);
        });

        accInput.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            hideAccList();
            resolveAccession(this.value.trim());
        });

        document.addEventListener('click', (e) => {
            if (accList && !accInput.contains(e.target) && !accList.contains(e.target)) hideAccList();
        });

        async function fetchSuggestions(term) {
            try {
                const res = await fetch(`/Library/SearchAccession?searchText=${encodeURIComponent(term)}`);
                const result = await res.json();
                if (result.success && result.data && result.data.length > 0) {
                    suggestions = result.data;
                    renderAccList(suggestions);
                } else {
                    hideAccList();
                }
            } catch (err) {
                console.error('SearchIssuedAccession error:', err);
                hideAccList();
            }
        }

        function renderAccList(items) {
            if (!accList) return;
            accList.innerHTML = '';
            items.forEach((item, idx) => {
                const li = document.createElement('li');
                li.className = 'list-group-item list-group-item-action';
                li.style.cursor = 'pointer';
                li.innerHTML = `<strong>${escapeHtml(item.accessionNo)}</strong> &nbsp;-&nbsp; ${escapeHtml(item.bookTitle)}
                    <span class="text-muted fs-12 d-block">Issued to: ${escapeHtml(item.memberName)} (${escapeHtml(item.libraryCardNo)})</span>`;
                li.addEventListener('click', () => {
                    accInput.value = item.accessionNo ?? '';
                    hideAccList();
                    resolveAccession(item.accessionNo);
                });
                accList.appendChild(li);
            });
            accList.style.display = 'block';
        }

        function hideAccList() {
            if (!accList) return;
            accList.style.display = 'none';
            accList.innerHTML = '';
        }

        function setAccError(message) {
            accInput.classList.toggle('is-invalid', !!message);
            if (errEl) errEl.textContent = message || '';
        }

        // Binds the member fields into the shared detail card, then loads
        // that member's issued books and highlights the scanned copy.
        function bindAccessionMember(data) {
            const memberId = getVal(data, 'libraryMemberID', 'LibraryMemberID');
            if (!data || !memberId) return;

            selectedMemberId = memberId;
            document.getElementById('hdnLibraryMemberID').value = selectedMemberId;
            window.CURRENT_MEMBER_ID = selectedMemberId;

            syncSearchControls(data);
            showMemberDetail(data);
            if (detailCard) detailCard.style.display = 'block';

            const issueReturnID = getVal(data, 'issueReturnID', 'IssueReturnID');
            loadIssuedBooks(selectedMemberId, 1).then(() => {
                if (issueReturnID) highlightRow(issueReturnID);
            });
        }

        async function resolveAccession(accessionNo) {
            if (!accessionNo) return;
            setAccError(null);
            accInput.disabled = true;

            try {
                const res = await fetch(`/Library/GetAccessionById?accessionNo=${encodeURIComponent(accessionNo)}`);
                const result = await res.json();

                console.log('GetAccessionById response:', result); // TEMP DEBUG — remove once confirmed

                const memberId = result.data && getVal(result.data, 'libraryMemberID', 'LibraryMemberID');
                if (result.success && memberId) {
                    bindAccessionMember(result.data);
                    return;
                }

                setAccError(result.message || 'No member or currently-issued book found for this Accession No / Card No.');
            } catch (err) {
                console.error('resolveAccession error:', err);
                setAccError('Lookup failed. Please try again.');
            } finally {
                accInput.disabled = false;
                accInput.select();
            }
        }
    }

    function highlightRow(issueReturnID) {
        if (!issueReturnID) return;
        const row = table.querySelector(`tr.item-row[data-issue-return-id="${issueReturnID}"]`);
        if (!row) return;

        const chk = row.querySelector('.row-select');
        if (chk && !chk.disabled) {
            chk.checked = true;
            chk.dispatchEvent(new Event('change', { bubbles: true }));
        }
        row.classList.add('table-warning');
        row.scrollIntoView({ behavior: 'smooth', block: 'center' });
        setTimeout(() => row.classList.remove('table-warning'), 2000);
    }

    // ============================================================
    // Load & render issued books for the selected member
    // ============================================================
    async function loadIssuedBooks(memberId, pageIndex) {
        if (!memberId) return;
        var accessionNo = $("#txtAccessionNo").val() || null;
        currentPageIndex = pageIndex || 1;
        body.innerHTML = `<tr><td colspan="9" class="text-center py-4 text-muted">Loading issued books...</td></tr>`;
        noMemberAlert.style.display = 'none';
        renderPagination(); // clear any stale pagination while loading

        try {
            const url = `/Library/GetIssuedBooksByMember?libraryMemberID=${encodeURIComponent(memberId)}`
                + `&accessionNo=${accessionNo}`
                + `&pageIndex=${currentPageIndex}&pageSize=${PAGE_SIZE}`;
            const res = await fetch(url);
            const result = await res.json();

            if (result.success && Array.isArray(result.data)) {
                currentIssuedBooks = result.data;
                totalRecords = result.totalRecords ?? currentIssuedBooks.length;
                currentPageIndex = result.pageNumber ?? currentPageIndex;
            } else {
                currentIssuedBooks = [];
                totalRecords = 0;
            }
            renderTable();
            renderPagination();
        } catch (err) {
            console.error('GetIssuedBooksByMember error:', err);
            currentIssuedBooks = [];
            totalRecords = 0;
            body.innerHTML = `<tr><td colspan="9" class="text-center py-4 text-danger">Failed to load issued books. Please try again.</td></tr>`;
            renderPagination();
        }
    }

    function renderPagination() {
        let pagerEl = document.getElementById('returnPager');
        const totalPages = Math.ceil(totalRecords / PAGE_SIZE);

        if (!pagerEl) {
            pagerEl = document.createElement('div');
            pagerEl.id = 'returnPager';
            pagerEl.className = 'd-flex align-items-center justify-content-between mt-3 flex-wrap gap-2';
            table.closest('.table-responsive').after(pagerEl);
        }

        if (!currentIssuedBooks.length || totalPages <= 1) {
            pagerEl.innerHTML = '';
            return;
        }

        const start = totalRecords === 0 ? 0 : ((currentPageIndex - 1) * PAGE_SIZE) + 1;
        const end = Math.min(currentPageIndex * PAGE_SIZE, totalRecords);

        pagerEl.innerHTML = `
            <div class="text-muted small">Showing <strong>${start}</strong>–<strong>${end}</strong> of <strong>${totalRecords}</strong></div>
            <nav><ul class="pagination pagination-sm mb-0">
                <li class="page-item ${currentPageIndex <= 1 ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${currentPageIndex - 1}"><i class="ti ti-chevron-left"></i></a>
                </li>
                ${Array.from({ length: totalPages }, (_, i) => i + 1).map(pg => `
                    <li class="page-item ${pg === currentPageIndex ? 'active' : ''}">
                        <a class="page-link" href="#" data-page="${pg}">${pg}</a>
                    </li>`).join('')}
                <li class="page-item ${currentPageIndex >= totalPages ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${currentPageIndex + 1}"><i class="ti ti-chevron-right"></i></a>
                </li>
            </ul></nav>
        `;

        pagerEl.querySelectorAll('a.page-link[data-page]').forEach(a => {
            a.addEventListener('click', (e) => {
                e.preventDefault();
                const pg = parseInt(a.dataset.page, 10);
                if (pg >= 1 && pg <= totalPages && pg !== currentPageIndex) {
                    loadIssuedBooks(selectedMemberId, pg);
                }
            });
        });
    }

    function renderTable() {
        body.innerHTML = '';

        if (currentIssuedBooks.length === 0) {
            toolbar.style.display = 'none';
            helperText.style.display = 'none';
            body.innerHTML = `<tr><td colspan="9" class="text-center py-4 text-muted">This member has no books currently issued.</td></tr>`;
            return;
        }

        toolbar.style.display = '';
        helperText.style.display = '';

        currentIssuedBooks.forEach((c, idx) => {
            const alreadyReturned = c.status === 2;
            const overdueDays = overdueDaysBetween(c.dueReturnDate, commonReturnDate.value || todayIso());

            const tr = document.createElement('tr');
            tr.className = 'item-row';
            tr.dataset.issueReturnId = c.issueReturnID;
            tr.dataset.dueReturnDate = c.dueReturnDate;
            tr.dataset.issueDate = c.issueDate;

            tr.innerHTML = `
                <td><input type="checkbox" class="form-check-input row-select" ${alreadyReturned ? 'disabled' : ''}></td>
                <td>${idx + 1}</td>
                <td>${escapeHtml(c.bookTitle)}</td>
                <td>${escapeHtml(c.accessionNo)}</td>
                <td>${escapeHtml(c.bookNo)}</td>
                <td>${formatDate(c.issueDate)}</td>
                <td>${formatDate(c.dueReturnDate)}</td>
                <td class="row-overdue ${overdueDays > 0 ? 'text-danger fw-medium' : ''}">${overdueDays}</td>
                <td class="row-fine">0.00</td>
                <td class="text-end">
                    ${alreadyReturned
                    ? '<span class="badge bg-success">Returned</span>'
                    : '<span class="badge bg-secondary-subtle text-secondary">Issued</span>'}
                </td>
            `;
            body.appendChild(tr);
        });

        recalcOverdueAndFine();
    }

    const selectableChecks = () => Array.from(table.querySelectorAll('.row-select:not(:disabled)'));

    selectAll?.addEventListener('change', () => {
        selectableChecks().forEach(chk => { chk.checked = selectAll.checked; });
        recalcOverdueAndFine();
    });

    table.addEventListener('change', (e) => {
        if (e.target.classList.contains('row-select')) {
            const all = selectableChecks();
            selectAll.checked = all.length > 0 && all.every(c => c.checked);
            recalcOverdueAndFine();
        }
    });

    commonReturnDate?.addEventListener('change', () => {
        setFieldError(commonReturnDate, document.getElementById('errReturnDate'), '');
        recalcOverdueAndFine();
    });

    document.getElementById('btnRecalcFine')?.addEventListener('click', () => recalcOverdueAndFine(true));

    // ============================================================
    // Auto fine calculation: for every CHECKED row whose due date is
    // before the chosen return date, fine = overdueDays * FINE_PER_DAY.
    // Total is written into the shared Fine Amount field.
    // ============================================================
    function recalcOverdueAndFine(forceOverride = false) {
        const returnDateVal = commonReturnDate?.value || todayIso();
        let total = 0;

        Array.from(table.querySelectorAll('tr.item-row')).forEach(row => {
            const due = row.dataset.dueReturnDate;
            const overdueDays = overdueDaysBetween(due, returnDateVal);
            const overdueCell = row.querySelector('.row-overdue');
            const fineCell = row.querySelector('.row-fine');
            const checked = row.querySelector('.row-select')?.checked;

            if (overdueCell) {
                overdueCell.textContent = overdueDays;
                overdueCell.classList.toggle('text-danger', overdueDays > 0);
                overdueCell.classList.toggle('fw-medium', overdueDays > 0);
            }

            const rowFine = (checked && overdueDays > 0) ? (overdueDays * FINE_PER_DAY) : 0;
            if (fineCell) fineCell.textContent = rowFine.toFixed(2);
            total += rowFine;
        });

        // Only auto-overwrite the shared field if the user hasn't typed a custom
        // value, or if they explicitly clicked "Recalculate".
        if (forceOverride || commonFineAmount.dataset.userEdited !== 'true') {
            commonFineAmount.value = total.toFixed(2);
        }
    }

    commonFineAmount?.addEventListener('input', () => {
        commonFineAmount.dataset.userEdited = 'true';
    });

    // ============================================================
    // Save (batch return) with full validation
    // ============================================================
    document.getElementById('returnBookForm')?.addEventListener('submit', async function (e) {
        e.preventDefault();

        if (!selectedMemberId) {
            Swal.fire({
                icon: 'warning', title: 'No member selected',
                text: 'Search a student/staff, or scan an Accession No, first.',
                confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-warning' }, buttonsStyling: false
            });
            return;
        }

        const selectedRows = Array.from(table.querySelectorAll('tr.item-row'))
            .filter(row => row.querySelector('.row-select')?.checked);

        if (selectedRows.length === 0) {
            Swal.fire({
                icon: 'warning', title: 'Nothing selected',
                text: 'Select at least one book to return.',
                confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-warning' }, buttonsStyling: false
            });
            return;
        }

        const returnDateValue = commonReturnDate?.value || '';
        if (!returnDateValue) {
            setFieldError(commonReturnDate, document.getElementById('errReturnDate'), 'Return date is required.');
            commonReturnDate?.focus();
            return;
        }

        const returnDate = parseDateOnly(returnDateValue);
        const today = parseDateOnly(todayIso());
        if (returnDate > today) {
            setFieldError(commonReturnDate, document.getElementById('errReturnDate'), 'Return date cannot be in the future.');
            commonReturnDate?.focus();
            return;
        }

        // Return date cannot be before any selected book's issue date
        const earlierThanIssue = selectedRows.find(row => {
            const issueDate = parseDateOnly(row.dataset.issueDate);
            return issueDate && returnDate < issueDate;
        });
        if (earlierThanIssue) {
            setFieldError(commonReturnDate, document.getElementById('errReturnDate'),
                'Return date cannot be earlier than a selected book\'s issue date.');
            commonReturnDate?.focus();
            return;
        }

        setFieldError(commonReturnDate, document.getElementById('errReturnDate'), '');

        const fineAmountValue = parseFloat(commonFineAmount?.value) || 0;
        const remarkValue = commonRemark?.value || '';

        const anyOverdueUnfined = selectedRows.some(row => {
            const overdueDays = overdueDaysBetween(row.dataset.dueReturnDate, returnDateValue);
            return overdueDays > 0;
        }) && fineAmountValue === 0 && FINE_PER_DAY > 0;

        if (anyOverdueUnfined) {
            const confirmResult = await Swal.fire({
                icon: 'warning',
                title: 'Overdue with no fine',
                text: 'One or more selected books are overdue but the fine amount is 0. Continue anyway?',
                showCancelButton: true,
                confirmButtonText: 'Continue',
                customClass: { confirmButton: 'btn btn-warning', cancelButton: 'btn btn-secondary' },
                buttonsStyling: false
            });
            if (!confirmResult.isConfirmed) return;
        }

        const returnList = selectedRows.map(row => ({
            IssueReturnID: parseInt(row.dataset.issueReturnId, 10),
            ReturnDate: returnDateValue,
            FineAmount: fineAmountValue,
            ModifyFine: fineAmountValue > 0,
            Remarks: remarkValue
        }));

        const btn = document.getElementById('btnSaveReturns');
        const originalHtml = btn ? btn.innerHTML : null;
        if (btn) { btn.disabled = true; btn.innerHTML = '<i class="ti ti-loader-2 me-1"></i> Saving...'; }

        try {
            const resp = await fetch('/Library/ReturnIssueBook', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ReturnListJson: JSON.stringify(returnList) })
            });
            const res = await resp.json();

            if (res.success) {
                Swal.fire({
                    icon: 'success', title: 'Saved!',
                    text: res.message || `${returnList.length} book(s) returned successfully.`,
                    confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-success' }, buttonsStyling: false
                }).then(() => {
                    window.location.href = '/Library/Issue/' + selectedMemberId;
                });
            } else {
                Swal.fire({
                    icon: 'error', title: 'Error', text: res.message || 'Failed to save return.',
                    confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-danger' }, buttonsStyling: false
                });
            }
        } catch (err) {
            console.error('Return save failed', err);
            Swal.fire({
                icon: 'error', title: 'Error', text: 'Save failed. Please try again.',
                confirmButtonText: 'OK', customClass: { confirmButton: 'btn btn-danger' }, buttonsStyling: false
            });
        } finally {
            if (btn) { btn.disabled = false; btn.innerHTML = originalHtml; }
        }
    });
});