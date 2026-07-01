// jQuery helper to serialize form to JSON object
$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name]) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

function triggerExport(index) {
    if ($.fn.DataTable.isDataTable('#inquiryTable')) {
        const table = $('#inquiryTable').DataTable();
        table.button(index).trigger();
    }
}

$(document).ready(function () {
    $('#inquiryTable').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3, 4, 5, 6, 7] } }
        ],
        searching: false,
        paging: false,
        info: false
    });
})


$(document).ready(function () {
    // Modal Select2
    $('#addModal .select2').select2({
        dropdownParent: $('#addModal')
    });

    // Filter Select2
    $('#filterForm .select2').select2();

    // Inquiry Form Submission
    $('#inquiryForm').on('submit', function (e) {
        e.preventDefault();

        // Reset and validate
        InlineFormValidation.clearMap({ 'InqPhone': 'errPhone', 'InqEmail': 'errEmail' });
        const isPhoneValid = InlineFormValidation.validateMobile('InqPhone', 'errPhone', true);
        const isEmailValid = InlineFormValidation.validateEmail('InqEmail', 'errEmail', false);

        if (!this.checkValidity() || !isPhoneValid || !isEmailValid) {
            this.reportValidity();
            return;
        }

        const data = $(this).serializeObject();

        $.ajax({
            url: '/FrontOffice/SaveInquiry',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (r) {
                if (r.success) {
                    showToast(r.message, 'success');
                    $('#addModal').modal('hide');
                    loadInquiryTable();
                } else {
                    showToast(r.message, 'error');
                }
            }
        });
    });

    // Follow Up Form Submission
    $('#followUpForm').on('submit', function (e) {
        e.preventDefault();
        const data = $(this).serializeObject();

        $.ajax({
            url: '/FrontOffice/SaveFollowUp',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (r) {
                if (r.success) {
                    showToast(r.message, 'success');
                    openFollowUp(data.InquiryID); // Refresh timeline
                } else {
                    showToast(r.message, 'error');
                }
            }
        });
    });

    // Table Search
    $('#tableSearch').on('keyup', function () {
        const val = $(this).val().toLowerCase();
        $("#inquiryTable tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(val) > -1)
        });
        renderFilterBadges();
    });

    // Handle Filter Form
    $('#filterForm').on('submit', function (e) {
        e.preventDefault();
        loadInquiryTable();
        $('.dropdown-toggle').dropdown('hide'); // Close filter dropdown
    });

    // Prevent dropdown from closing when clicking inside
    $('.filter-dropdown').on('click', function (e) {
        e.stopPropagation();
    });
});

function loadInquiryTable() {
    const formData = $('#filterForm').serializeObject();
    const query = $.param(formData);

    $.get('/FrontOffice/GetAllInquiriesJson?' + query, function (r) {
        if (r.success) {
            let html = '';
            r.data.forEach(item => {
                const badgeClass = item.status === "Active" ? "bg-success" : (item.status === "Passive" ? "bg-warning" : "bg-danger");
                const enqDate = item.date ? new Date(item.date).toLocaleDateString('en-GB').replace(/\//g, '-') : '-';
                const lastDate = item.lastFollowUpDate ? new Date(item.lastFollowUpDate).toLocaleDateString('en-GB').replace(/\//g, '-') : '-';
                const nextDate = item.nextFollowUpDate ? new Date(item.nextFollowUpDate).toLocaleDateString('en-GB').replace(/\//g, '-') : '-';

                html += `
                            <tr>
                                <td>
                                <input type="checkbox" name="selectedInquiry" class="form-check-input student-checkbox" value="${item.inquiryID}"></td>
                                <td>${item.name}</td>
                                <td>${item.phone}</td>
                                <td>${item.sourceName || '-'}</td>
                                <td>${enqDate}</td>
                                <td>${lastDate}</td>
                                <td>${nextDate}</td>
                                <td><span class="badge ${badgeClass} rounded-pill">${item.status}</span></td>
                               
                            </tr>`;
            });
            $('#inquiryTable tbody').html(html || '<tr><td colspan="9" class="text-center">No records found</td></tr>');
            renderFilterBadges();
        }
    });
}

// --- Filter Badges Logic ---
function renderFilterBadges() {
    const container = document.getElementById('badgeContainer');
    const mainContainer = document.getElementById('activeFilterBadges');
    if (!container) return;
    container.innerHTML = '';

    const filters = [
        { id: 'tableSearch', label: 'Search', type: 'text' },
        { id: 'filterClass', label: 'Class', type: 'select' },
        { id: 'filterSource', label: 'Source', type: 'select' },
        { id: 'filterStatus', label: 'Status', type: 'select' },
        { id: 'filterFromDate', label: 'From', type: 'date' },
        { id: 'filterToDate', label: 'To', type: 'date' }
    ];

    let activeCount = 0;

    filters.forEach(f => {
        const el = document.getElementById(f.id);
        if (!el) return;

        let value = el.value;
        let text = '';

        if (f.type === 'text' || f.type === 'date') {
            if (value && value.trim() && value !== '0') {
                text = value;
                if (f.type === 'date') {
                    const parts = value.split('-');
                    if (parts.length === 3) {
                        text = `${parts[2]}-${parts[1]}-${parts[0]}`;
                    }
                }
                activeCount++;
            }
        } else if (f.type === 'select') {
            if (value && value !== 'all' && value !== '' && value !== '0') {
                const selectedOption = el.options[el.selectedIndex];
                text = selectedOption ? selectedOption.text : value;
                activeCount++;
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
        if (filterId === 'tableSearch') {
            $('#tableSearch').trigger('keyup');
        } else {
            loadInquiryTable();
        }
    } else {
        el.value = (filterId === 'filterClass' || filterId === 'filterSource') ? '0' : '';
        if (window.jQuery && jQuery(el).data('select2')) {
            jQuery(el).trigger('change.select2');
        }
        loadInquiryTable();
    }
}

function resetAllFilters() {
    document.getElementById('tableSearch').value = '';
    document.getElementById('filterClass').value = '0';
    document.getElementById('filterSource').value = '0';
    document.getElementById('filterStatus').value = '';
    document.getElementById('filterFromDate').value = '';
    document.getElementById('filterToDate').value = '';

    $('.select2').trigger('change.select2');

    $('#tableSearch').trigger('keyup');
    loadInquiryTable();
}

function resetInquiryForm() {
    $('#inquiryForm')[0].reset();
    $('#InquiryID').val(0);
    $('#modalTitle').text('Add Admission Enquiry');
    $('.select2').val('').trigger('change');
    $('#InqDate').val(new Date().toISOString().split('T')[0]);
}

function editSelectedInquiry() {
    const checked = $('.student-checkbox:checked');
    if (checked.length !== 1) {
        showToast('Please select a record to edit', 'warning');
        return;
    }
    loadInquiry(checked.val());
}

function followUpSelectedInquiry() {
    const checked = $('.student-checkbox:checked');
    if (checked.length !== 1) {
        showToast('Please select a record for follow up', 'warning');
        return;
    }
    openFollowUp(checked.val());
}

function loadInquiry(id) {
    $.get('/FrontOffice/GetInquiry/' + id, function (r) {
        if (r.success) {
            const d = r.data;
            $('#InquiryID').val(d.inquiryID);
            $('#InqName').val(d.name);
            $('#InqPhone').val(d.phone);
            $('#InqEmail').val(d.email);
            $('#InqAddress').val(d.address);
            $('#InqDescription').val(d.description);
            $('#InqNote').val(d.note);
            $('#InqDate').val(d.date ? d.date.split('T')[0] : '');
            $('#InqNextFollowUpDate').val(d.nextFollowUpDate ? d.nextFollowUpDate.split('T')[0] : '');
            $('#InqAssignedTo').val(d.assignedTo).trigger('change');
            $('#InqReferenceID').val(d.referenceID).trigger('change');
            $('#InqSourceID').val(d.sourceID).trigger('change');
            $('#InqClassID').val(d.classID).trigger('change');
            $('#InqNoOfChild').val(d.noOfChild);
            $('#InqStatus').val(d.status).trigger('change');

            $('#modalTitle').text('Edit Admission Enquiry');
            $('#addModal').modal('show');
        }
    });
}

function deleteSelectedInquiry() {
    const checked = $('.student-checkbox:checked');
    if (checked.length === 0) {
        showToast('Please select a record to delete', 'warning');
        return;
    }


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
                url: '/FrontOffice/DeleteInquiry',
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

function openFollowUp(id) {

    try {

        if (id) {
            location.href = `/FrontOffice/InquiryFollowUp/${id}`;
        }

    } catch (err) {
        console.error(err);
    }

    //$.get('/FrontOffice/GetInquiry/' + id, function (r) {
    //    if (r.success) {
    //        const d = r.data;
    //        $('#FollowUpInquiryID').val(d.inquiryID);
    //        $('#FollowUpDate').val(new Date().toISOString().split('T')[0]);
    //        $('#FollowUpNextDate').val('');
    //        $('#FollowUpResponse').val('');
    //        $('#FollowUpNote').val('');

    //        // Update Sidebar
    //        $('#SideName').text(d.name);
    //        $('#SideAssigned').text('Assigned: ' + (d.assignedToName || '-'));
    //        $('#SideStatus').text(d.status);
    //        $('#SidePhone').text(d.phone);
    //        $('#SideEmail').text(d.email || '-');
    //        $('#SideAddress').text(d.address || '-');
    //        $('#SideClass').text(d.className || '-');
    //        $('#SideSource').text(d.sourceName || '-');
    //        $('#SideReference').text(d.referenceName || '-');
    //        $('#SideChild').text(d.noOfChild);
    //        $('#SideEnqDate').text(d.date ? d.date.split('T')[0] : '-');
    //        $('#SideNextDate').text(d.nextFollowUpDate ? d.nextFollowUpDate.split('T')[0] : '-');
    //        $('#SideDesc').text(d.description || '-');
    //        $('#SideNote').text(d.note || '-');

    //        // Build Timeline
    //        let html = '';
    //        if (d.followUps && d.followUps.length > 0) {
    //            d.followUps.forEach(f => {
    //                html += `
    //                            <div class="timeline-item mb-3 p-2 bg-white border rounded shadow-sm">
    //                                <div class="d-flex justify-content-between small">
    //                                    <span class="fw-bold text-primary">${f.followUpDate.split('T')[0]}</span>
    //                                    <span class="text-muted">Next: ${f.nextFollowUpDate.split('T')[0]}</span>
    //                                </div>
    //                                <div class="mt-1">${f.response}</div>
    //                                ${f.note ? `<div class="small text-muted mt-1 italic border-top pt-1">Note: ${f.note}</div>` : ''}
    //                            </div>`;
    //            });
    //        } else {
    //            html = '<div class="text-muted small">No follow-ups recorded yet.</div>';
    //        }
    //        $('#followUpTimeline').html(html);

    //        $('#followUpModal').modal('show');
    //    }
    //});
}