function SearchTextBox() {
    let SearchInform = document.getElementById("SearchInform");
    let TextBox = SearchInform.querySelector(".TextBoxStyle");
    let SearchText = TextBox.value

    $.ajax({
        type: "POST",
        url: "/FilmPage/SearchInformation",
        data: JSON.stringify(TextBox.value),
        contentType: "application/json",
        success: function (response) {
            updateMovieListSearch(response, SearchText);
        }
    });
};

function removeMarkTags(text) {
    return text.replace(/<mark>/gi, '').replace(/<\/mark>/gi, '');
}

// Функція для оновлення списку фільмів на сторінці
function updateMovieListSearch(movies, SearchText) {
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

            // Видаляємо старі теги <mark> з назви фільму
            let FoundTextFilm = removeMarkTags(movie.nameFilm);

            // Перевіряємо чи є текст пошуку і створюємо відповідний регулярний вираз
            if (SearchText.trim() !== "") {
                let SearchTextRegex = new RegExp(SearchText.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), 'gi');

                // Підсвічуємо збіги у назві фільму
                FoundTextFilm = FoundTextFilm.replace(SearchTextRegex, '<mark>$&</mark>');

                // Підсвічуємо збіги у іменах акторів
                movie.actorFilm.forEach(actor => {
                    let FullName = `${actor.firstName} ${actor.lastName}`;
                    let FoundFullName = removeMarkTags(FullName).replace(SearchTextRegex, '<mark>$&</mark>');
                    let [FoundFirstName, FoundLastName] = FoundFullName.split(' ');
                    actor.firstName = FoundFirstName;
                    actor.lastName = FoundLastName;
                });
            } else {
                // Якщо пошуковий текст порожній, повертаємо оригінальні імена
                movie.actorFilm.forEach(actor => {
                    actor.firstName = removeMarkTags(actor.firstName);
                    actor.lastName = removeMarkTags(actor.lastName);
                });
            }

            let FoundTextActors = movie.actorFilm.map(actor => {
                return `<a href="/FilmPage/DetailsActor?id=${actor.idActor}">${actor.firstName} ${actor.lastName}</a>`;
            }).join(", ");

            // Масив з елементами інформації про фільм
            let infoElements = [
                { label: "Назва фільму: ", value: `<a href="/FilmPage/DetailsFilm?id=${movie.idFilm}">${FoundTextFilm}</a>` },
                { label: "Жанр фільму: ", value: movie.genreFilm.join(", ") },
                { label: "Рік фільму: ", value: movie.yearFilm },
                { label: "Країна: ", value: movie.countryFilm.join(", ") },
                { label: "Актори: ", value: FoundTextActors },
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