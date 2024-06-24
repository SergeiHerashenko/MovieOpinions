let OriginalElements = new Map();

function SaveOriginalElements(parentDiv, textClass, reactionClass) {

    OriginalElements.set(parentDiv, {
        text: parentDiv.querySelector(`.${textClass}`).cloneNode(true),
        reaction: parentDiv.querySelector(`.${reactionClass}`).cloneNode(true)
    });
}

function RestoreOriginalElements(parentDiv, textClass, reactionClass) {
    let Original = OriginalElements.get(parentDiv);
    if (Original) {
        let CurrentText = parentDiv.querySelector(`.${textClass}`);
        let CurrentReaction = parentDiv.querySelector(`.${reactionClass}`);

        if (CurrentText && Original.text) {
            CurrentText.parentNode.replaceChild(Original.text, CurrentText);
        }

        if (CurrentReaction && Original.reaction) {
            CurrentReaction.parentNode.replaceChild(Original.reaction, CurrentReaction);
        }

        OriginalElements.delete(parentDiv);
    }
}

async function HandleEditForm(url, button, textClass, reactionClass) {
    let ParentDiv = button.closest(`.${textClass.replace('Text', '')}`);

    SaveOriginalElements(ParentDiv, textClass, reactionClass);

    try {
        let response = await $.ajax({
            type: "GET",
            url: url,
            contentType: "application/json"
        });

        let tempDiv = document.createElement('div');
        tempDiv.innerHTML = response.trim();
        let NewTextElement = tempDiv.querySelector(`.${textClass}`);
        let NewReactionElement = tempDiv.querySelector(`.${reactionClass}`);

        let OldTextElement = ParentDiv.querySelector(`.${textClass}`);
        let OldReactionElement = ParentDiv.querySelector(`.${reactionClass}`);

        let TextContent = OldTextElement.querySelector('label').textContent;
        let Textarea = NewTextElement.querySelector('textarea');
        if (Textarea) {
            Textarea.value = TextContent;
        }

        OldTextElement.parentNode.replaceChild(NewTextElement, OldTextElement);
        OldReactionElement.parentNode.replaceChild(NewReactionElement, OldReactionElement);

        let CancelButton = NewReactionElement.querySelector('.GeneralButtonStyle[style="float:right;"]');
        if (CancelButton) {
            CancelButton.onclick = () => RestoreOriginalElements(ParentDiv, textClass, reactionClass);
        }
    } catch (error) {
        console.error("Помилка при отриманні форми редагування:", error);
    }
}

function GetCommentChangeForm(button) {
    HandleEditForm("/FilmPage/GetRedactionCommentForm", button, 'TextCommentStyle', 'ReactionPanelStyle');
}

function GetAnswerChangeForm(button) {
    HandleEditForm("/FilmPage/GetRedactionAnswerForm", button, 'TextAnswerStyle', 'ReactionPanelStyle');
}

async function EditComment(button) {
    let CommentDiv = button.closest(".CommentZoneStyle");
    let Textarea = CommentDiv.querySelector('textarea');

    let DataComment = {
        IdComment: CommentDiv.id,
        TextComment: Textarea.value
    };

    try {
        let response = await $.ajax({
            type: "POST",
            url: "/FilmPage/ChangeComment",
            data: JSON.stringify(DataComment),
            contentType: "application/json"
        });

        if (response.redirectUrl) {
            window.location.href = response.redirectUrl;
        } else {
            let ModalWindow = document.getElementById("ModalWindow");
            ModalWindow.style.display = "block";
            let Message = document.getElementById("Message");
            let FormattedText = response.description.split('\n').join('<br>');
            Message.innerHTML = FormattedText;
        }
    } catch (error) {
        console.error("Помилка при зміні коментаря:", error);
    }
}

async function EditAnswer(button) {
    let AnswertDiv = button.closest(".AnswerStyle");
    let Textarea = AnswertDiv.querySelector('textarea');

    let DataAnswer = {
        IdAnswer: AnswertDiv.id,
        IdComment: AnswertDiv.dataset.commentId,
        TextAnswer: Textarea.value
    };

    try {
        let response = await $.ajax({
            type: "POST",
            url: "/FilmPage/ChangeAnswer",
            data: JSON.stringify(DataAnswer),
            contentType: "application/json"
        });

        if (response.redirectUrl) {
            window.location.href = response.redirectUrl;
        } else {
            let ModalWindow = document.getElementById("ModalWindow");
            ModalWindow.style.display = "block";
            let Message = document.getElementById("Message");
            let FormattedText = response.description.split('\n').join('<br>');
            Message.innerHTML = FormattedText;
        }
    } catch (error) {
        console.error("Помилка при зміні відповіді:", error);
    }
}
