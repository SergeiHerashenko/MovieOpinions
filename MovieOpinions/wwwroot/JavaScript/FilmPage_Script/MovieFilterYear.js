// Отримуємо всі чекбокси років
let CheckBoxYear = document.querySelectorAll('.YearFilterStyle input[type="checkbox"]');

// Масив обраних років
var selectedGenre = [];

// Додаємо обробник подій для кожного чекбокса
CheckBoxYear.forEach(function (checkbox) {
    checkbox.addEventListener("change", function () {
        // Якщо чекбокс вибраний, додаємо його значення до масиву жанрів
        if (this.checked) {
            selectedGenre.push(checkbox.value);
        } else {
            // Якщо чекбокс знятий, видаляємо його значення з масиву жанрів
            selectedGenre = selectedGenre.filter(value => value !== this.value);
        }
        // Відправляємо вибрані жанри на сервер для фільтрації фільмів
        sendSelectedYear(selectedGenre);
    });
});

// Функція для відправки вибраних років на сервер
function sendSelectedYear(year) {
    const xhr = new XMLHttpRequest();
    // Встановлюємо тип запиту (POST) та URL для відправки даних
    xhr.open("POST", "GetSortedMoviesYear", true);
    // Встановлюємо заголовок для JSON даних
    xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");

    // Обробка відповіді від сервера
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            let response = JSON.parse(xhr.responseText);

            if (response.error) {
                // Отримуємо елемент модального вікна
                let ModalWindow = document.getElementById("ModalWindow");
                // Відображаємо модальне вікно
                ModalWindow.style.display = "block";
                // Отримуємо елемент для повідомлення
                let Message = document.getElementById("Message");

                // Форматуємо текст помилки для відображення
                let lines = response.error.split('\n');
                let formattedText = lines.join('<br>');
                // Встановлюємо форматований текст помилки в модальне вікно
                Message.innerHTML = formattedText;
            }
            else {
                // Оновлюємо список фільмів з отриманою відповіддю
                updateMovieList(JSON.parse(xhr.responseText));
            }
        }
    };

    // Якщо вибрані жанри є, відправляємо їх на сервер
    if (year.length > 0) {
        xhr.send(JSON.stringify(year));
    } else {
        // Якщо жанри не обрані, отримуємо всі фільми без фільтрації
        xhr.open("GET", "AllMovies", true);
        xhr.send();
    }
};