const IV = {
    setFieldError: (id, errId, msg) => {
        document.getElementById(id)?.classList.add('is-invalid');
        const err = document.getElementById(errId); if (err) err.textContent = msg;
    },
    clearFieldError: (id, errId) => {
        document.getElementById(id)?.classList.remove('is-invalid');
        const err = document.getElementById(errId); if (err) err.textContent = '';
    }
};

function buildEvaluationsPayload() {
    const rows = [];

    $('#tblEvaluate tbody tr.eval-row').each(function () {
        const $row = $(this);
        const submissionId = parseInt($row.data('submission-id'));

        // Only include rows the teacher actually touched:
        // checked, OR has marks entered, OR has a note typed.
        const $noteRow = $row.next('tr.eval-note-row');
        const marksVal = $row.find('.txtMarks').val();
        const noteVal = $noteRow.find('.txtNote').val();
        const isChecked = $row.find('.chkStudent').is(':checked');

        const hasMarks = marksVal !== '' && marksVal !== null;
        const hasNote = noteVal && noteVal.trim() !== '';

        if (!isChecked && !hasMarks && !hasNote) return; // skip untouched rows

        rows.push({
            SubmissionID: submissionId,
            MarksObtained: hasMarks ? parseFloat(marksVal) : null,
            Remark: noteVal || null
        });
    });

    return rows;
}

async function saveEvaluation() {
    IV.clearFieldError('txtEvaluationDate', 'errTxtEvaluationDate');

    const homeworkId = parseInt(document.getElementById('hdnHomeworkID').value);
    const evaluationDate = document.getElementById('txtEvaluationDate').value;

    if (!evaluationDate) {
        IV.setFieldError('txtEvaluationDate', 'errTxtEvaluationDate', 'Please select an evaluation date');
        return;
    }

    const evaluations = buildEvaluationsPayload();

    if (evaluations.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Nothing to save',
            text: 'Please enter marks or a note for at least one student.',
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn btn-warning' },
            buttonsStyling: false
        });
        return;
    }

    const payload = {
        homeworkID: homeworkId,
        evaluationDate: evaluationDate,
        evaluatedBy: window.currentTeacherId,
        evaluationsJson: JSON.stringify(evaluations)   // ✅ now a proper JSON string
    };

    const $btn = $('#btnSaveEvaluation');
    const originalHtml = $btn.html();
    $btn.prop('disabled', true).html('<div class="spinner-border spinner-border-sm me-1"></div> Saving...');

    try {
        const resp = await fetch('/Homework/UpsertEvaluateHomewor', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        const res = await resp.json();

        if (res.success) {
            Swal.fire({
                icon: 'success',
                title: 'Saved!',
                text: res.message || 'Evaluation saved successfully.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-success' },
                buttonsStyling: false
            }).then(() => {
                window.location.href = '/Homework/Index';
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: res.message || 'Failed to save evaluation.',
                confirmButtonText: 'OK',
                customClass: { confirmButton: 'btn btn-danger' },
                buttonsStyling: false
            });
            $btn.prop('disabled', false).html(originalHtml);
        }
    } catch (err) {
        console.error(err);
        Swal.fire('Error!', 'An unexpected error occurred.', 'error');
        $btn.prop('disabled', false).html(originalHtml);
    }
}

// Select-all convenience: clicking a student's name row also checks the box
$(document).on('click', '.eval-row td:not(:has(input.txtMarks))', function (e) {
    if ($(e.target).is('input, a, i')) return; // don't fight with real inputs/links
    const $chk = $(this).closest('tr').find('.chkStudent');
    $chk.prop('checked', !$chk.prop('checked'));
});

