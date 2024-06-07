function AddAnswer() {
    let TextAnswer = document.getElementById("AnswerText");
    // Знаходимо батьківський елемент з класом CommentZoneStyle
    let parentCommentZone = TextAnswer.closest('.CommentZoneStyle');

    // Отримуємо id цього батьківського елемента
    let CommentId = parentCommentZone ? parentCommentZone.id : null;

    let userName = document.getElementById("UserName").textContent;
    userName = userName.replace(' відповідіє:', '').trim();

    let pathArray = window.location.pathname.split('/');
    let idFilm = pathArray[pathArray.length - 1];

    let DataAnswer = {
        TextAnswer: TextAnswer.value,
        IdComment: CommentId,
        NameUserAnswer: userName
    };

    
    $.ajax({
        type: "POST",
        url: "/FilmPage/AddAnswerToComment/${idFilm}",
        data: JSON.stringify(DataAnswer, idFilm),
        contentType: "application/json",
        success: function (response) {
           
        }
    });
}