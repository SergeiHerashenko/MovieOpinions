function RemoveAnswerPanel() {
    var divToDelete = document.getElementById('IncompleteResponse');
    divToDelete.classList.remove('SlideInStyle');
    divToDelete.classList.add("SlideOutStyle");

    setTimeout(function () {
        divToDelete.remove();
    }, 300);
}

function RemoveCommentPanel() {
    var divToDelete = document.getElementById('IncompleteComment');
    divToDelete.remove();
    let ButtonOpeningWindowComment = document.querySelector(".AddCommentButtonStyle");
    ButtonOpeningWindowComment.style.height = "auto";
}