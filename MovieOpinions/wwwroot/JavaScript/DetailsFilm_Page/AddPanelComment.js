//$(document).ready(function () {
//    $('.CommentAdd').click(function () {
//        if ($('#IncompleteComment').length > 0) {
//            return;
//        }
//        var $this = $(this);
//        $this.animate({ height: 0 }, 20, function () {
//            $this.hide();
//        });
//
//        const parentDiv = $(this).closest('.ZoneTextCommentStyle');
//        $.get('/FilmPage/GetCommentTemplate', function (template) {
//            const incompleteResponse = $(template).attr('id', 'IncompleteComment').css({
//                display: 'none'
//            });
//
//            parentDiv.before(incompleteResponse);
//            incompleteResponse.show().animate({ height: '200px' }, 200);
//        });
//    });
//});

document.addEventListener("DOMContentLoaded", function () {
    let AnswerButton = document.querySelectorAll(".CommentAdd");

    AnswerButton.forEach(function (button) {
        button.addEventListener('click', function () {
            if (document.getElementById("IncompleteComment")) {
                return;
            }

            let parentDiv = this.closest(".ZoneTextCommentStyle");
            let ButtonOpeningWindowComment = parentDiv.querySelector(".AddCommentButtonStyle");
            ButtonOpeningWindowComment.style.height = "0px";

            $.ajax({
                type: "GET",
                url: "/FilmPage/GetCommentTemplate",
                contentType: "application/json",
                success: function (response) {
                    parentDiv.querySelector('.AddCommentButtonStyle').insertAdjacentHTML('beforebegin', response.trim());
                }
            });
        });
    });
});