// Функція для оновлення лічильника символів
function updateCharCount(idTextArea, idChar) {
    var maxLength = 1000;
    var textArea = document.querySelector(idTextArea);
    var charCount = document.querySelector(idChar);

    var currentLength = textArea.value.length;
    var remaining = maxLength - currentLength;

    charCount.textContent = 'Залишилось символів: ' + remaining + '/' + maxLength;
}
