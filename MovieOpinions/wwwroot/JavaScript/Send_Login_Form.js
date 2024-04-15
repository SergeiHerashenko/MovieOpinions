function SendForm(FormId, URL) {
    let form = document.getElementById(FormId);

    // Отримати всі поля форми
    let formElements = form.querySelectorAll('input');
    // Створити об'єкт для збереження значень полів
    let formData = {};

    // Перебрати всі поля форми
    for (let i = 0; i < formElements.length; i++) {
        formData[formElements[i].id] = formElements[i].value;
    }

    $.ajax({
        type: "POST",
        url: URL,
        data: JSON.stringify(formData),
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