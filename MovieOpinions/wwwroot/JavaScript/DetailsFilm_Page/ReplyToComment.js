$(document).ready(function () {
    $('.AnswerButton').click(function () {
        if ($('#IncompleteResponse').length > 0) { 
            return;
        }

        const parentDiv = $(this).closest('.CommentZoneStyle');
        const answers = parentDiv.find('.AnswerStyle');
        $.get('/FilmPage/GetAnswerTemplate', function (template) {
            const incompleteResponse = $(template).attr('id', 'IncompleteResponse');
            if (answers.length > 0) {
                // Знаходимо останній елемент .AnswerStyle і додаємо новий елемент після нього
                $(answers[answers.length - 1]).after(incompleteResponse);
            } else {
                // Якщо відповідей немає, додаємо в кінець батьківського елемента
                parentDiv.append(incompleteResponse);
            }
        });
    });
});
