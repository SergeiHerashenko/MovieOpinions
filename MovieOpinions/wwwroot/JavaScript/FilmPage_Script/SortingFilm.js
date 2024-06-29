document.getElementById('sortDropdown').addEventListener('change', function () {
    let SelectedSortType = this.value;

    $.ajax({
        type: "POST",
        url: "/FilmPage/SortingMovies",
        data: JSON.stringify(SelectedSortType),
        contentType: "application/json",
        success: function (response) {
            if (response.error) {
                DisplayModalMessage(response.error);
            } else if (response.notfound) {
                DisplayModalMessage(response.notfound);
            } else {
                // Оновлюємо список фільмів з отриманою відповіддю
                updateMovieList(response);
            }
        }
    });
});

function DisplayModalMessage(text) {
    let ModalWindow = document.getElementById("ModalWindow");
    ModalWindow.style.display = "block";

    let Message = document.getElementById("Message");
    let lines = text.split('\n');
    let formattedText = lines.join('<br>');
    Message.innerHTML = formattedText;
}