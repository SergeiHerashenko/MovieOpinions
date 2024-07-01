function ShowModalWindow(button, action) {

    let ModalWindow = document.getElementById("ModalWindow");
    ModalWindow.style.display = "block";
    let Message = document.getElementById("Message");
    let ButtonModalWindow = document.getElementById("ButtonModalWindow");
    
    let ClearDeleteButton = document.getElementById("ConfirmButton");
    if (ClearDeleteButton) {
        ButtonModalWindow.removeChild(ClearDeleteButton);
    }

    if (action === "Comment") {
        let ParentDiv = button.closest(".CommentZoneStyle").id;

        Message.innerHTML = "Дійсно видалити коментар? Історія відповідей також буде видалена";
        let CreateDeleteCommentButton = document.createElement("button");
        CreateDeleteCommentButton.className = "GeneralButtonStyle";
        CreateDeleteCommentButton.textContent = "Підтвердити";
        CreateDeleteCommentButton.setAttribute("onclick", `DeleteComment('${action}', ${ParentDiv})`);
        CreateDeleteCommentButton.id = "ConfirmButton";
        ButtonModalWindow.appendChild(CreateDeleteCommentButton);



    } else {
        let ParentDiv = button.closest(".AnswerStyle").id

        Message.innerHTML = "Дійсно видалити відповідь?";
        let CreateDeleteAnswerButton = document.createElement("button");
        CreateDeleteAnswerButton.className = "GeneralButtonStyle";
        CreateDeleteAnswerButton.textContent = "Підтвердити";
        CreateDeleteAnswerButton.setAttribute("onclick", `DeleteAnswer('${action}', ${ParentDiv})`);
        CreateDeleteAnswerButton.id = "ConfirmButton";
        ButtonModalWindow.appendChild(CreateDeleteAnswerButton);
    }
};

function DeleteComment(Action, parentDiv) {
    let CommentData = {
        IdComment: parentDiv
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
}

function DeleteAnswer(Action, parentDiv) {
    let AnswerData = {
        IdAnswer: parentDiv
    };

    let AboutFilmElement = document.querySelector('.AboutFilmStyle');
    let FilmNameElement = AboutFilmElement.querySelector('a');
    let FilmName = FilmNameElement ? FilmNameElement.textContent : '';

    $.ajax({
        type: "POST",
        url: "/FilmPage/DeleteAnswer?NameFilm=" + encodeURIComponent(FilmName),
        data: JSON.stringify(AnswerData),
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
}