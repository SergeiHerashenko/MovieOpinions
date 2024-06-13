document.addEventListener("DOMContentLoaded", function () {
    let AnswerButton = document.querySelectorAll(".AnswerButton");

    AnswerButton.forEach(function (button) {
        button.addEventListener('click', function () {
            if (document.getElementById("IncompleteResponse")) {
                return;
            }

            let parentDiv = this.closest(".CommentZoneStyle");
            let answer = parentDiv.querySelectorAll(".AnswerStyle");

            $.ajax({
                type: "GET",
                url: "/FilmPage/GetAnswerTemplate",
                contentType: "application/json",
                success: function (response) {
                    let incompleteResponse = document.createElement('div');
                    incompleteResponse.id = 'IncompleteResponse';
                    incompleteResponse.innerHTML = response.trim();

                    incompleteResponse.classList.add('SlideInStyle');

                    if (answer.length > 0) {
                        answer[answer.length - 1].insertAdjacentElement('afterend', incompleteResponse);
                    } else {
                        parentDiv.appendChild(incompleteResponse);
                    }
                }
            });
        });
    });
});