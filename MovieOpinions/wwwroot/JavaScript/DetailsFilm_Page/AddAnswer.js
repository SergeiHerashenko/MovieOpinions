function AddAnswer() {
    let TextAnswer = document.getElementById("AnswerText");
    // Знаходимо батьківський елемент з класом CommentZoneStyle
    let parentCommentZone = TextAnswer.closest('.CommentZoneStyle');

    // Отримуємо id цього батьківського елемента
    let CommentId = parentCommentZone ? parentCommentZone.id : null;

    let userName = document.getElementById("UserName").textContent;
    userName = userName.replace(' відповідіє:', '').trim();

    let DataAnswer = {
        TextAnswer: TextAnswer.value,
        IdComment: CommentId,
        NameUserAnswer: userName
    };

    
    $.ajax({
        type: "POST",
        url: "/FilmPage/AddAnswerToComment",
        data: JSON.stringify(DataAnswer),
        contentType: "application/json",
        success: function (response) {
            if (response.redirectUrl) {
                window.location.href = response.redirectUrl;
            } else {
                let ModalWindow = document.getElementById("ModalWindow");
                ModalWindow.style.display = "block";
                let Message = document.getElementById("Message");
                console.log(response)
                let lines = response.description.split('\n');
                let formattedText = lines.join('<br>');
                Message.innerHTML = formattedText;

                isRedirectNeeded = response.isRedirectNeeded;
            }
        }
    });
}