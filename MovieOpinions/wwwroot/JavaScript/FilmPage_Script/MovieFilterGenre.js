// Отримуємо всі чекбокси жанрів
let CheckBoxGenre = document.querySelectorAll('.GenreFilterStyle input[type="checkbox"]');

// Масив обраних жанрів
var selectedGenre = [];

// Додаємо обробник подій для кожного чекбокса
CheckBoxGenre.forEach(function (checkbox) {
    checkbox.addEventListener("change", function () {
        // Якщо чекбокс вибраний, додаємо його значення до масиву жанрів
        if (this.checked) {
            selectedGenre.push(checkbox.value);
        } else {
            // Якщо чекбокс знятий, видаляємо його значення з масиву жанрів
            selectedGenre = selectedGenre.filter(value => value !== this.value);
        }
        // Відправляємо вибрані жанри на сервер для фільтрації фільмів
        sendSelectedGenres(selectedGenre);
    });
});

// Функція для відправки вибраних жанрів на сервер
function sendSelectedGenres(genres) {
    const xhr = new XMLHttpRequest();
    xhr.open("POST", "GetSortedMoviesGenre", true);
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
    if (genres.length > 0) {
        xhr.send(JSON.stringify(genres));
    } else {
        // Якщо жанри не обрані, отримуємо всі фільми без фільтрації
        xhr.open("GET", "AllMovies", true);
        xhr.send();
    }
};

// Функція для оновлення списку фільмів на сторінці
function updateMovieList(movies) {
    
    // Отримуємо контейнери для фільмів
    let movieContainers = document.querySelectorAll(".FilmFormStyle");

    // Для кожного контейнера фільмів очищуємо вміст і додаємо нові фільми
    movieContainers.forEach(function (container) {
        container.innerHTML = "";

        movies.forEach(function (movie) {
            // Створюємо контейнер для постера фільму
            let posterContainer = document.createElement("div");
            posterContainer.className = "PosterFormStyle";

            // Створюємо зону зображення фільму та додавання зображення
            let leftZone = document.createElement("div");
            leftZone.className = "LeftZoneStyle";

            let photoZone = document.createElement("div");
            photoZone.className = "PhotoZoneStyle";
            let movieImage = document.createElement("img");
            movieImage.src = movie.filmImage;
            photoZone.appendChild(movieImage);

            // Додаємо зону зірок для оцінок фільму
            let starsZone = document.createElement("div");
            starsZone.className = "StarsZoneStyle";

            leftZone.appendChild(photoZone);
            leftZone.appendChild(starsZone);

            // Створюємо зону інформації про фільм
            let infoZone = document.createElement("div");
            infoZone.className = "InformationFilmStyle";

            // Масив з елементами інформації про фільм
            let infoElements = [
                { label: "Назва фільму: ", value: `<a href="/FilmPage/DetailsFilm?id=${movie.idFilm}">${movie.nameFilm}</a>` },
                { label: "Жанр фільму: ", value: movie.genreFilm.join(", ") },
                { label: "Рік фільму: ", value: movie.yearFilm },
                { label: "Країна: ", value: movie.countryFilm.join(", ") },
                { label: "Актори: ", value: movie.actorFilm.map(actor => `<a href="/FilmPage/DetailsActor?id=${actor.idActor}">${actor.firstName} ${actor.lastName}</a>`).join(", ") },
                { label: "Опис: ", value: movie.descriptionFilm }
            ];

            // Додаємо кожен елемент інформації про фільм до зони інформації
            infoElements.forEach(function (element) {
                let infoElement = document.createElement("div");
                infoElement.className = "ElementInformationFilm";
                infoElement.innerHTML = `<label>${element.label}</label><label>${element.value}</label>`;
                infoZone.appendChild(infoElement);
            });

            // Додаємо зони зображення та інформації до контейнера фільму
            posterContainer.appendChild(leftZone);
            posterContainer.appendChild(infoZone);

            // Додаємо контейнер фільму до відповідного контейнера на сторінці
            container.appendChild(posterContainer);
        });
    });
}

