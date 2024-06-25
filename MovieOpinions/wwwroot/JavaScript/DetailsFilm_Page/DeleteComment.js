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

    let DataComment = {
        IdComment: ParentDiv
    };

    $.ajax({
        type: "POST",
        url: "/FilmPage/DeleteComment",
        data: JSON.stringify(DataComment),
        contentType: "application/json",
        success: function (response) {
            console.log(response)
        }
    });
});