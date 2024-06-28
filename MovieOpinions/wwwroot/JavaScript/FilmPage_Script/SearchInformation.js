function SearchTextBox() {
    let SearchInform = document.getElementById("SearchInform");
    let TextBox = SearchInform.querySelector(".TextBoxStyle");

    $.ajax({
        type: "POST",
        url: "/FilmPage/SearchInformation",
        data: JSON.stringify(TextBox.value),
        contentType: "application/json",
    });
};