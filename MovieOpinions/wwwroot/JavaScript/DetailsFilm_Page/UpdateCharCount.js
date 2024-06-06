// Функція для оновлення лічильника символів
function updateCharCount() {
    var maxLength = 1000;
    var currentLength = $('#AnswerText').val().length;
    var remaining = maxLength - currentLength;
    $('#AnswerChar').text('Залишилось символів: ' + remaining + '/' + maxLength);
}