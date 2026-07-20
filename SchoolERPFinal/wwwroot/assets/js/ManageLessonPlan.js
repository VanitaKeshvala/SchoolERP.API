// ManageLessonPlan.js

let selectedTopicMapId = 0;

// New-file state (kept as real File objects, sent on submit)
let selectedVideoFile = null;      // single File or null
let selectedAttachmentFiles = [];  // array of File

// Edit-mode existing-item state
let existingAttachments = [];      // [{AttachmentID, AttachmentPath, AttachmentName, AttachmentType}]
let removedAttachmentIds = [];     // AttachmentIDs marked for deletion
let removeExistingVideo = false;   // true if user removed the existing video

$(document).ready(function () {
    if ($.fn.select2) {
        $('.select2').select2();
    }

    const initialLessonMapId = $('#ddlLesson').val();
    if (initialLessonMapId) {
        loadTopicsByLesson(initialLessonMapId, window.editSelectedTopicMapId || 0);
    }

    // Seed edit-mode data
    existingAttachments = Array.isArray(window.existingAttachments) ? window.existingAttachments : [];
    renderAttachmentPreviews();
    renderVideoPreview();

    initDropzone('dzLectureVideo', 'fileLectureVideo', handleVideoFilesSelected);
    initDropzone('dzAttachment', 'fileAttachment', handleAttachmentFilesSelected);
});

/* =========================================================
   CASCADING DROPDOWN: Lesson -> Topic
   ========================================================= */
async function loadTopicsByLesson(lessonMapId, selectedTopicId) {
    const ddl = $('#ddlTopic');
    ddl.empty().append(new Option('-- Select Topic --', ''));
    $('#errddlTopic').text('');

    if (!lessonMapId) {
        ddl.append(new Option('-- Select Lesson First --', ''));
        if ($.fn.select2) ddl.trigger('change.select2');
        return;
    }

    selectedTopicMapId = parseInt(selectedTopicId) || 0;

    try {
        const res = await $.ajax({
            url: '/ManageLessonPlan/BindTopicDropDwonList',
            type: 'GET',
            data: { lessonMapId: lessonMapId }
        });

        if (res && res.success && Array.isArray(res.data)) {
            res.data.forEach(st => {
                const opt = new Option(st.topicName, st.topicMapId);
                if (selectedTopicMapId && parseInt(st.topicMapId) === selectedTopicMapId) {
                    opt.selected = true;
                }
                ddl.append(opt);
            });
        } else {
            console.warn('No topics returned for lessonMapId ' + lessonMapId, res);
        }

        if ($.fn.select2) ddl.trigger('change.select2');
        else ddl.trigger('change');

    } catch (jqXHR) {
        // THIS is the part that actually tells you what's wrong:
        console.error('Failed to load topics for lesson ' + lessonMapId);
        console.error('HTTP Status:', jqXHR.status, jqXHR.statusText);
        console.error('Response Body:', jqXHR.responseText);
    }
}
/* =========================================================
   DROPZONE (drag & drop + click)
   onFilesSelected(fileList) receives a FileList/array
   ========================================================= */
function initDropzone(dzId, fileInputId, onFilesSelected) {
    const dz = document.getElementById(dzId);
    const fileInput = document.getElementById(fileInputId);
    if (!dz || !fileInput) return;

    fileInput.addEventListener('change', function () {
        if (this.files && this.files.length) {
            onFilesSelected(this.files);
            this.value = ''; // allow re-selecting the same file later
        }
    });

    dz.addEventListener('dragover', function (e) {
        e.preventDefault();
        dz.classList.add('border-primary');
    });
    dz.addEventListener('dragleave', function () {
        dz.classList.remove('border-primary');
    });
    dz.addEventListener('drop', function (e) {
        e.preventDefault();
        dz.classList.remove('border-primary');
        if (e.dataTransfer.files && e.dataTransfer.files.length) {
            onFilesSelected(e.dataTransfer.files);
        }
    });
}

/* =========================================================
   VIDEO: select / preview / remove
   ========================================================= */
function handleVideoFilesSelected(fileList) {
    selectedVideoFile = fileList[0]; // only one video supported
    removeExistingVideo = false;     // a new file replaces whatever existed
    $('#dzLectureVideoText').text(selectedVideoFile.name);
    renderVideoPreview();
}

function renderVideoPreview() {
    const container = $('#videoPreviewContainer');
    container.empty();

    if (selectedVideoFile) {
        const url = URL.createObjectURL(selectedVideoFile);
        container.append(`
            <div class="d-flex align-items-center gap-2 border rounded p-1">
                <video src="${url}" width="120" height="70" controls class="rounded"></video>
                <span class="small text-truncate" style="max-width:140px;">${selectedVideoFile.name}</span>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeSelectedVideo()">
                    <i class="ti ti-x"></i>
                </button>
            </div>
        `);
        return;
    }

    if (window.existingVideoPath && !removeExistingVideo) {
        container.append(`
            <div class="d-flex align-items-center gap-2 border rounded p-1">
                <video src="${window.existingVideoPath}" width="120" height="70" controls class="rounded"></video>
                <span class="small text-truncate" style="max-width:140px;">Current video</span>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeSelectedVideo()">
                    <i class="ti ti-x"></i>
                </button>
            </div>
        `);
    }
}

function removeSelectedVideo() {
    selectedVideoFile = null;
    if (window.existingVideoPath) {
        removeExistingVideo = true;
    }
    $('#dzLectureVideoText').text('Drag and drop a file here or click');
    renderVideoPreview();
}

/* =========================================================
   ATTACHMENTS: select / preview / remove (multi-file)
   ========================================================= */
function handleAttachmentFilesSelected(fileList) {
    Array.from(fileList).forEach(f => selectedAttachmentFiles.push(f));
    renderAttachmentPreviews();
}

function isImageFile(nameOrType) {
    return /\.(png|jpe?g|gif|webp|bmp)$/i.test(nameOrType) || /^image\//i.test(nameOrType);
}

function fileIconClass(name) {
    const ext = (name.split('.').pop() || '').toLowerCase();
    if (ext === 'pdf') return 'ti ti-file-type-pdf';
    if (['doc', 'docx'].includes(ext)) return 'ti ti-file-type-doc';
    if (['xls', 'xlsx'].includes(ext)) return 'ti ti-file-type-xls';
    return 'ti ti-file';
}

function renderAttachmentPreviews() {
    const container = $('#attachmentPreviewList');
    container.empty();

    // Existing attachments already saved on the record (edit mode)
    existingAttachments
        .filter(a => !removedAttachmentIds.includes(a.AttachmentID))
        .forEach(a => {
            const thumb = isImageFile(a.AttachmentPath || a.AttachmentName)
                ? `<img src="${a.AttachmentPath}" width="60" height="60" class="rounded object-fit-cover">`
                : `<i class="${fileIconClass(a.AttachmentName || a.AttachmentPath)}" style="font-size:2rem;"></i>`;

            container.append(`
                <div class="d-flex flex-column align-items-center border rounded p-1" style="width:80px;">
                    ${thumb}
                    <span class="small text-truncate w-100 text-center" title="${a.AttachmentName}">${a.AttachmentName}</span>
                    <button type="button" class="btn btn-sm btn-outline-danger mt-1"
                            onclick="removeExistingAttachment(${a.AttachmentID})">
                        <i class="ti ti-x"></i>
                    </button>
                </div>
            `);
        });

    // Newly selected files (not yet uploaded)
    selectedAttachmentFiles.forEach((file, idx) => {
        const thumb = isImageFile(file.name)
            ? `<img src="${URL.createObjectURL(file)}" width="60" height="60" class="rounded object-fit-cover">`
            : `<i class="${fileIconClass(file.name)}" style="font-size:2rem;"></i>`;

        container.append(`
            <div class="d-flex flex-column align-items-center border rounded p-1" style="width:80px;">
                ${thumb}
                <span class="small text-truncate w-100 text-center" title="${file.name}">${file.name}</span>
                <button type="button" class="btn btn-sm btn-outline-danger mt-1"
                        onclick="removeNewAttachment(${idx})">
                    <i class="ti ti-x"></i>
                </button>
            </div>
        `);
    });
}

function removeExistingAttachment(attachmentId) {
    removedAttachmentIds.push(attachmentId);
    $('#hdnRemovedAttachmentIds').val(JSON.stringify(removedAttachmentIds));
    renderAttachmentPreviews();
}

function removeNewAttachment(index) {
    selectedAttachmentFiles.splice(index, 1);
    renderAttachmentPreviews();
}

/* =========================================================
   VALIDATION
   ========================================================= */
function clearValidationErrors() {
    $('.invalid-feedback').text('');
    $('.form-control, .form-select').removeClass('is-invalid');
}

function showFieldError(fieldId, message) {
    $('#' + fieldId).addClass('is-invalid');
    $('#err' + fieldId).text(message);
}

function validateForm() {
    clearValidationErrors();
    let isValid = true;

    if (!$('#ddlLesson').val()) {
        showFieldError('ddlLesson', 'Please select a lesson.');
        isValid = false;
    }
    if (!$('#ddlTopic').val()) {
        showFieldError('ddlTopic', 'Please select a topic.');
        isValid = false;
    }
    if (!$('#txtPlanDate').val()) {
        showFieldError('txtPlanDate', 'Please select a date.');
        isValid = false;
    }

    const timeFrom = $('#txtTimeFrom').val();
    const timeTo = $('#txtTimeTo').val();
    if (timeFrom && timeTo && timeFrom >= timeTo) {
        showFieldError('txtTimeTo', 'Time To must be after Time From.');
        isValid = false;
    }

    return isValid;
}

/* =========================================================
   SAVE (Add / Edit) — files go straight into the same
   multipart POST the controller already expects
   (avideoAttachment, attachmentFiles)
   ========================================================= */
async function saveItem() {
    if (!validateForm()) {
        return;
    }

    const presentationContent = getPresentationContent();

    const formData = new FormData();
    formData.append('LessonPlanId', $('#txtItemId').val() || 0);
    formData.append('LessonMapId', $('#ddlLesson').val() || 0);
    formData.append('TopicMapId', $('#ddlTopic').val() || 0);
    formData.append('SubTopic', $('#txtSubTopic').val() || '');
    formData.append('PlanDate', $('#txtPlanDate').val() || '');
    formData.append('TimeFrom', $('#txtTimeFrom').val() || '');
    formData.append('TimeTo', $('#txtTimeTo').val() || '');
    formData.append('LectureYoutubeUrl', $('#txtLectureYoutubeUrl').val() || '');
    formData.append('TeachingMethod', $('#txtTeachingMethod').val() || '');
    formData.append('GeneralObjectives', $('#txtGeneralObjectives').val() || '');
    formData.append('PreviousKnowledge', $('#txtPreviousKnowledge').val() || '');
    formData.append('ClassID', $('#hdnClassID').val() || '');
    formData.append('SectionID', $('#hdnSectionID').val() || '');
    formData.append('SubjectGroupID', $('#hdnSubjectGroupID').val() || '');
    formData.append('SubjectID', $('#hdnSubjectID').val() || '');
    formData.append('ComprehensiveQuestions', $('#txtComprehensiveQuestions').val() || '');
    formData.append('Presentation', presentationContent || '');
    formData.append('IsActive', true);

    // Keep existing video path unless it was explicitly removed / replaced
    formData.append('LectureVideoPath', removeExistingVideo ? '' : ($('#hdnLectureVideoPath').val() || ''));
    formData.append('RemoveExistingVideo', removeExistingVideo);

    // New video file, matches controller param name: avideoAttachment
    if (selectedVideoFile) {
        formData.append('avideoAttachment', selectedVideoFile, selectedVideoFile.name);
    }

    // New attachment files, matches controller param name: attachmentFiles
    selectedAttachmentFiles.forEach(file => {
        formData.append('attachmentFiles', file, file.name);
    });

    // Attachment IDs removed during edit, so the server can soft-delete them
    formData.append('RemovedAttachmentIds', JSON.stringify(removedAttachmentIds));

    toggleSaveButton(true);

    try {
        const res = await $.ajax({
            url: '/ManageLessonPlan/UpsertLessonPlan',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false
        });

        if (res && res.success) {
            showPageNotice('success', res.message || 'Lesson plan saved successfully.');
            setTimeout(function () {
                window.location.href = '/Academics/LessonPlan';
            }, 1000);
        } else {
            showPageNotice('danger', res.message || 'Failed to save lesson plan.');
        }
    } catch (e) {
        console.error('Save error', e);
        showPageNotice('danger', 'An error occurred while saving. Please try again.');
    } finally {
        toggleSaveButton(false);
    }
}

function getPresentationContent() {
    return $('#txtPresentation').val();
}

function toggleSaveButton(isSaving) {
    const btn = $('.top-btn.btn-success');
    btn.prop('disabled', isSaving);
    btn.html(isSaving
        ? '<span class="spinner-border spinner-border-sm me-1"></span> Saving...'
        : '<i class="ti ti-device-floppy me-1"></i> Save');
}

function showPageNotice(type, message) {
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    $('#lessonPlanPageNotice').html(
        `<div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>`
    );
}
let editorInstance;
function openAddImageDialog() {
    console.log('openAddImageDialog: implement based on your editor (CKEditor/TinyMCE/Summernote).');
}
function MyCustomUploadAdapterPlugin(editor) {
    editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
        return new MyUploadAdapter(loader);
    };
}


$(document).ready(function () {

    ClassicEditor
        .create(document.querySelector('#txtPresentation'), {
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