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