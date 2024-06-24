function GetCommentChangeForm(button) {
    let commentDiv = button.closest(".CommentStyle");

    $.ajax({
        type: "GET",
        url: "/FilmPage/GetRedactionForm",
        contentType: "application/json",
        success: function (response) {
            let tempDiv = document.createElement('div');
            tempDiv.innerHTML = response.trim();
            let newTextCommentStyle = tempDiv.querySelector('.TextCommentStyle');
            let newReactionPanelStyle = tempDiv.querySelector('.ReactionPanelStyle');

            // Знайти старі блоки
            let oldTextCommentStyle = commentDiv.querySelector('.TextCommentStyle');
            let oldReactionPanelStyle = commentDiv.querySelector('.ReactionPanelStyle');

            let TextComment = oldTextCommentStyle.querySelector('.TextCommentStyle label').textContent;

            let textarea = newTextCommentStyle.querySelector('textarea');
            if (textarea) {
                textarea.value = TextComment;
            }

            // Замінити старі блоки на нові
            if (oldTextCommentStyle && newTextCommentStyle) {
                oldTextCommentStyle.parentNode.replaceChild(newTextCommentStyle, oldTextCommentStyle);
            }

            if (oldReactionPanelStyle && newReactionPanelStyle) {
                oldReactionPanelStyle.parentNode.replaceChild(newReactionPanelStyle, oldReactionPanelStyle);
            }
        },
        error: function (xhr, status, error) {
            console.error("Помилка при отриманні форми редагування:", status, error);
        }
    });
}

function EditComment(button) {
    let DataComment = {};

    let commentDiv = button.closest(".CommentZoneStyle");
    let textarea = commentDiv.querySelector('textarea');

    DataComment = {
        IdComment: commentDiv.id,
        TextComment: textarea.value
    }

    $.ajax({
        type: "POST",
        url: "/FilmPage/ChangeComment",
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
}
