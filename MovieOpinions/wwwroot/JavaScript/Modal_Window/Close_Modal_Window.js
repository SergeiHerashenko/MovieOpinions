var isRedirectNeeded = false;
function CloseModalWindow(IdButton, IdModalWindow) {
    // Вимикаємо стандартну дію кнопки (запобігає переходу за посиланням) /We turn off the standard action of the button (prevents clicking on the link)/
    event.preventDefault();

    // Отримуємо елементи кнопки та модального вікна за їх id /We get button and modal window elements by their id/
    IdButton = document.getElementById(IdButton);
    IdModalWindow = document.getElementById(IdModalWindow);

    // Стежимо за кнопкою, і при кліку на неї закриваємо модальне вікно /We follow the button, and when we click on it, we close the modal window/
    IdButton.addEventListener("click", function (event) {
        if (isRedirectNeeded) {
            window.location.href = "/LoginPage/LoginPage";
        }
        // Закриваємо модальне вікно
        IdModalWindow.style.display = "none";
    });
};