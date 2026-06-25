let selectedId = 0;
const canEdit = window.canEdit;
const canDelete = window.canDelete;
const IV = window.InlineFormValidation;
function selectItem(id, row) {
    selectedId = id;
    document.querySelectorAll('.item-row').forEach(r => r.classList.remove('bg-light'));
    row.classList.add('bg-light');
    //row.querySelector('input[type="radio"]').checked = true;
    let checkedCount = $('.student-checkbox:checked').length;


    if (canEdit) {
        const btnEdit = document.getElementById('btnEdit');
        if (btnEdit) btnEdit.disabled = false;
        // Edit sirf 1 record select hone par
        $('#btnEdit').prop('disabled', checkedCount !== 1);
    }
    if (canDelete) {
        const btnDelete = document.getElementById('btnDelete');
        if (btnDelete) btnDelete.disabled = false;
        $('#btnDelete').prop('disabled', checkedCount === 0);
    }
}

document.getElementById('txtSearchInput').addEventListener('keyup', function () {
    const q = this.value.toLowerCase();
    document.querySelectorAll('#tblStudentHouse tbody tr').forEach(row => {
        if (row.classList.contains('item-row')) {
            row.style.display = row.textContent.toLowerCase().includes(q) ? '' : 'none';
        }
    });
});

function openAddModal() {
    document.getElementById('modalTitle').textContent = 'Add Student House';
    document.getElementById('hdnStudentHouseID').value = 0;
    document.getElementById('entryForm').reset();
    document.getElementById('entryForm').classList.remove('was-validated');
}

function editSelected() {
    if (!selectedId) return;
    const row = document.querySelector(`.item-row input[value="${selectedId}"]`).closest('tr');
    const name = row.cells[1].textContent.trim();
    const description = row.cells[2].textContent.trim();

    //document.getElementById('hdnStudentHouseID').value = selectedId;

    if (!canEdit) {
        IV.setNotice('sectionPageNotice', 'You do not have permission to edit.');
        return;
    }

    if (selectedId === 0) return;
    IV.clearNotice('sectionPageNotice');
    try {

        if (selectedId) {
            location.href = `/StudentInformation/AddStudentHouse/${selectedId}`;
        }

    } catch (err) {
        console.error(err);
        IV.setNotice('sectionPageNotice', 'Could not load section details.');
    }


}

async function saveRecord() {
    const form = document.getElementById('entryForm');
    form.classList.add('was-validated');
    if (!form.checkValidity()) return;

    const data = {
        studentHouseID: parseInt(document.getElementById('hdnStudentHouseID').value) || 0,
        studentHouseName: document.getElementById('txtName').value.trim(),
        studentHouseDescription: document.getElementById('txtDescription').value.trim()
    };


    fetch('/StudentInformation/UpsertStudentHouse', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Saved!',
                    text: 'Student house has been added successfully.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-success' },
                    buttonsStyling: false
                }).then(() => {
                    window.location.href = '/StudentInformation/StudentHouse';
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: res.message || 'Failed to save student house.',
                    confirmButtonText: 'OK',
                    customClass: { confirmButton: 'btn btn-danger' },
                    buttonsStyling: false
                });
            }
        })
        .catch(err => {
            console.error(err);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while saving.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
        });

    
}

function deleteSelected() {
    let selectedIds = [];
    $('.student-checkbox:checked').each(function () {
        selectedIds.push(parseInt($(this).val()));
    });
    if (selectedIds.length === 0) {
        alert("Please select at least one student house.");
        return;
    }
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this student house  record. This action cannot be undone!",
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
                url: '/StudentInformation/DeleteStudentHouse',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    if (res.success) {
                        Swal.fire('Deleted!', 'Student house record has been deleted.', 'success')
                            .then(() => location.reload());
                    } else {
                        Swal.fire('Error!', res.message || 'Failed to delete student house.', 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error!', 'An unexpected error occurred.', 'error');
                }
            });
        }
    });
}

let exportTable;
$(document).ready(function () {
    if ($.fn.DataTable.isDataTable('#tblStudentHouse')) {
        $('#tblStudentHouse').DataTable().destroy();
    }
    exportTable = $('#tblStudentHouse').DataTable({
        dom: 'Bfrtip',
        buttons: [
            { extend: 'copy', exportOptions: { columns: [1, 2, 3], rows: ':visible' } },
            { extend: 'csv', exportOptions: { columns: [1, 2, 3], rows: ':visible' } },
            { extend: 'excel', exportOptions: { columns: [1, 2, 3], rows: ':visible' } },
            { extend: 'pdf', exportOptions: { columns: [1, 2, 3], rows: ':visible' } },
            { extend: 'print', exportOptions: { columns: [1, 2, 3], rows: ':visible' } }
        ],
        searching: false,
        paging: false,
        info: false,
        ordering: false
    });
});

function triggerExport(type) {
    if (!exportTable) return;
    if (type === 'copy') exportTable.button('.buttons-copy').trigger();
    else if (type === 'csv') exportTable.button('.buttons-csv').trigger();
    else if (type === 'excel') exportTable.button('.buttons-excel').trigger();
    else if (type === 'pdf') exportTable.button('.buttons-pdf').trigger();
    else if (type === 'print') exportTable.button('.buttons-print').trigger();
}

document.getElementById('txtSearchInput').addEventListener('input', function () {
    const q = this.value.toLowerCase();
    document.querySelectorAll('#tblStudentHouse tbody tr.item-row').forEach(row => {
        row.style.display = row.textContent.toLowerCase().includes(q) ? '' : 'none';
    });
});