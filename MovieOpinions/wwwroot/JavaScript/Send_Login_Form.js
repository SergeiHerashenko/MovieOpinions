function SendForm(FormId, URL) {
    let Form = document.getElementById(FormId);

    // Отримати всі поля форми
    let FormElements = Form.querySelectorAll('input');
    // Створити об'єкт для збереження значень полів
    let DataForm = {};

    // Перебрати всі поля форми
    for (let i = 0; i < FormElements.length; i++) {
        DataForm[FormElements[i].id] = FormElements[i].value;
    }

    $.ajax({
        type: "POST",
        url: URL,
        data: JSON.stringify(DataForm),
        contentType: "application/json",
        success: function (Response) {
            if (Response.RedirectUrl) {
                window.location.href = Response.RedirectUrl;
            } else {
                let ModalWindow = document.getElementById("ModalWindow");
                ModalWindow.style.display = "block";
                let Message = document.getElementById("Message");

                let Lines = response.description.split('\n');
                let FormattedText = Lines.join('<br>');
                Message.innerHTML = FormattedText;
            }
        }
    });
};