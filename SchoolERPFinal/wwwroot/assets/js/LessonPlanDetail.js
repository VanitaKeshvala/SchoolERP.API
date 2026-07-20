// LessonPlanDetail.js

$(document).ready(function () {
    $('#txtComment').on('keypress', function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            sendComment();
        }
    });
});

async function sendComment() {
    const text = $('#txtComment').val().trim();
    if (!text) {
        $('#txtComment').addClass('is-invalid').attr('placeholder', 'Please enter a comment');
        return;
    }
    $('#txtComment').removeClass('is-invalid');

    const btn = $('#btnSendComment');
    btn.prop('disabled', true);
    $('#btnSendText').html('<span class="spinner-border spinner-border-sm"></span>');

    try {
        const res = await $.ajax({
            url: '/ManageLessonPlan/AddLessonPlanComment',
            type: 'POST',
            data: {
                lessonPlanId: window.currentLessonPlanId,
                commentText: text
            }
        });

        if (res && res.success) {
            $('#txtComment').val('');
            await loadComments();
        } else {
            console.warn('Failed to add comment', res);
        }
    } catch (e) {
        console.error('Comment send error', e);
    } finally {
        btn.prop('disabled', false);
        $('#btnSendText').text('Send');
    }
}

async function loadComments() {
    try {
        const res = await $.ajax({
            url: '/ManageLessonPlan/GetLessonPlanComments',
            type: 'GET',
            data: { lessonPlanId: window.currentLessonPlanId }
        });

        const list = $('#commentList');
        list.empty();

        if (res && res.success && Array.isArray(res.data) && res.data.length > 0) {
            res.data.forEach(c => {
                const avatar = c.commentedByPhoto || '/assets/images/avatar-placeholder.png';
                const codePart = c.commentedByCode ? ` <span class="comment-code">(${c.commentedByCode})</span>` : '';
                list.append(`
                    <div class="comment-item">
                        <img src="${avatar}" class="comment-avatar" alt="${c.commentedByName}" />
                        <div class="comment-body">
                            <div class="comment-text">
                                <span class="comment-author">${c.commentedByName}${codePart} -</span>
                                ${$('<div>').text(c.commentText).html()}
                            </div>
                        </div>
                        <div class="comment-time">${c.commentedOn}</div>
                    </div>
                `);
            });
        } else {
            list.append('<div class="comment-empty">No comments yet.</div>');
        }
    } catch (e) {
        console.error('Failed to load comments', e);
    }
}