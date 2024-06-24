function AddComment() {
    let CommentText = document.getElementById("CommentText");

    let CommentTemplateStyle = CommentText.closest(".CommentTemplateStyle");

    let userName = CommentTemplateStyle.querySelector("#UserName").textContent;
    userName = userName.replace(' пише:', '').trim();

    let aboutFilmElement = document.querySelector('.AboutFilmStyle');

    let filmNameElement = aboutFilmElement.querySelector('a');

    let filmName = filmNameElement ? filmNameElement.textContent : '';

    let DataComment = {
        UserName: userName,
        TextComment: CommentText.value
    };


    $.ajax({
        type: "POST",
        url: "/FilmPage/AddCommentToFilm?NameFilm=" + encodeURIComponent(filmName),
        data: JSON.stringify(DataComment),
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
                
                isRedirectNeeded = response.isRedirectNeeded;
            }
        }
    });
};