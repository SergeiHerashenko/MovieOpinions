var labels = document.querySelectorAll('.answer-label');
labels.forEach(label => {
    label.addEventListener('click', function () {
        var commentId = this.getAttribute('data-comment-id');
        var answers = document.querySelectorAll('.AnswerStyle[data-comment-id="' + commentId + '"]');
        answers.forEach(answer => {
            answer.classList.toggle('HideAnswers'); // Перемикач класів для сховання/показу відповідей
        });
    });
});