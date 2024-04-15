function UpdateStyleBasedOnInput(ContainerId) {
    let container = document.getElementById(ContainerId);

    if (container.length === 0) {
        return;
    }

    let LabelStyle = container.querySelector(".LabelStyle");
    let TextBoxStyle = container.querySelector(".TextBoxStyle");

    let isInputEmpty = TextBoxStyle.value.trim() === "";

    // Застосування стилів для LabelStyle
    LabelStyle.style.transform = isInputEmpty ? "" : "translateY(-50px)";
    LabelStyle.style.transition = "0.3s";

    // Застосування стилів для TextBoxStyle
    TextBoxStyle.style.color = isInputEmpty ? "" : "rgba(255, 245, 234, 0.40)";
    TextBoxStyle.style.boxShadow = isInputEmpty ? "" : "4px 4px 8px 0px #1E1E1E inset, -4px -4px 8px 0px rgba(77, 77, 77, 0.25) inset, 10px 10px 20px 0px #1E1E1E, -10px -10px 20px 0px #3C3C3C";
};