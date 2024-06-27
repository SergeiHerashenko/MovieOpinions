let ParentDiv;

function DeleteCommentUser(button) {

    let ModalWindow = document.getElementById("ModalWindow");
    ModalWindow.style.display = "block";
    let Message = document.getElementById("Message");
    Message.innerHTML = "Дійсно видалити коментар? Історія відповідей також буде видалена";
    let ConfirmButton = document.getElementById("ConfirmButton");
    ConfirmButton.style.display = "block";
    let CancelButton = document.getElementById("ButtonCloseModalMessageWindow");
    CancelButton.textContent = "Відмінити"

    ParentDiv = button.closest(".CommentZoneStyle").id;
};

ConfirmButton.addEventListener("click", function () {
    let CommentData = {
        IdComment: ParentDiv
    };

    $.ajax({
        type: "POST",
        url: "/FilmPage/DeleteComment",
        data: JSON.stringify(CommentData),
        contentType: "application/json",
        success: function (response) {
            if (response.redirectUrl) {
                window.location.href = response.redirectUrl;
            } else {
                let ModalWindow = document.getElementById("ModalWindow");
                ModalWindow.style.display = "block";
                let Message = document.getElementById("Message");
                let lines = response.description.split('\n');
                let formattedText = lines.join('<br>');
                Message.innerHTML = formattedText;
            }
        }
    });
});