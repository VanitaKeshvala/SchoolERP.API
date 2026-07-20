
$(document).ready(function () {
  

    ClassicEditor
        .create(document.querySelector('#txtMessage'), {
            extraPlugins: [MyCustomUploadAdapterPlugin],
            toolbar: [
                'heading', '|', 'bold', 'italic', 'link', 'bulletedList', 'numberedList', 'blockQuote', 'insertTable', 'uploadImage', 'undo', 'redo'
            ]
        })
        .then(editor => {
            editorInstance = editor;
        })
        .catch(error => {
            console.error(error);
        });
});
const permCanAdd = window.canAdd;
const permCanEdit = window.canEdit;
const permCanDelete = window.canDelete;

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
    clearNotice: (id) => { const el = document.getElementById(id); if (el) el.innerHTML = ''; }
};

const fieldMap = {
    txtMessage: 'errTxtMessage'
};


let selectedId = 0;
let isUpcomingTab = true;
let editorInstance;

async function editSelected() {
    if (selectedId === 0) return;
    IV.clearMap(fieldMap);
    IV.clearNotice('modalNotice');
    try {

        if (selectedId) {
            location.href = `/Homework/Add/${selectedId}`;
        }

    } catch (err) {
        console.error(err);
    }
}
async function saveHomework() {
    IV.clearMap(fieldMap);
    const description = editorInstance ? editorInstance.getData() : '';

    const homeworkID = parseInt(document.getElementById('hdnHomeworkID').value);

    let hasError = false;
  
    if (!description.trim() || description === '<p>&nbsp;</p>' || description === '') {
        IV.setFieldError('txtMessage', 'errTxtDescription', 'Description is mandatory');
        hasError = true;
    }

    if (hasError) return;

    // ── Build multipart form: individual fields matching HomeworkUpsertRequest properties ──
    const formData = new FormData();
    formData.append('HomeworkID', homeworkID);
    formData.append('Status', 2);
    formData.append('Message', description);

    const fileInput = document.getElementById('fileAttachment');
    if (fileInput && fileInput.files.length > 0) {
        Array.from(fileInput.files).forEach(file => {
            formData.append('attachmentFiles', file);
        });
    }

    const $btn = $('#btnSaveHomework');
    const originalHtml = $btn.html();
    $btn.prop('disabled', true).html('<div class="spinner-border spinner-border-sm me-1"></div> Saving...');

    try {
        const resp = await fetch('/Homework/UpsertSubmission', {
            method: 'POST',
            body: formData
            // Do NOT set Content-Type manually — the browser sets the multipart boundary automatically.
        });
        const res = await resp.json();
        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: res.message || 'Homework submit has been saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => window.location.href = '/Homework/index');
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save homework.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
            $btn.prop('disabled', false).html(originalHtml);
        }
    } catch (err) {
        console.error(err);
        $btn.prop('disabled', false).html(originalHtml);
    }
}
function MyCustomUploadAdapterPlugin(editor) {
    editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
        return new MyUploadAdapter(loader);
    };
}

let deletedAttachmentIds = [];

function viewAttachment(filePath) {
    const ext = (filePath.split('.').pop() || '').toLowerCase();
    const url = filePath.startsWith('http') ? filePath : filePath;

    let html = '';
    if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) {
        html = `<img src="${url}" class="img-fluid" style="max-height:70vh;" />`;
    } else if (ext === 'pdf') {
        html = `<iframe src="${url}" style="width:100%;height:70vh;" frameborder="0"></iframe>`;
    } else {
        html = `<a href="${url}" target="_blank" class="btn btn-primary">Open Document</a>`;
    }

    document.getElementById('attachmentPreviewBody').innerHTML = html;
    const modal = new bootstrap.Modal(document.getElementById('attachmentPreviewModal'));
    modal.show();
}

function removeAttachment(attachmentId, btn) {
    Swal.fire({
        title: 'Delete attachment?',
        text: 'This document will be removed from homework.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete'
    }).then((result) => {
        if (result.isConfirmed) {
            deletedAttachmentIds.push(attachmentId);
            const row = btn.closest('tr');
            row.remove();
        }
    });
}