
//------------------переключение темы------------------------//
function switchTheme() {
    const body = document.body;
    const isDark = body.classList.toggle("dark-mode");
    localStorage.setItem("theme", isDark ? "dark" : "light");
}

function applyTheme() {
    const savedTheme = localStorage.getItem("theme");
    if (savedTheme === "dark" || (!savedTheme && window.matchMedia("(prefers-color-scheme: dark)").matches)) {
        document.body.classList.add("dark-mode");
    }
    else {
        document.body.classList.remove("dark-mode");
    }
}

// Применяем тему соглачсно сохранённым настройкам или системным настройкам
window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", applyTheme);
// Применяем тему при загрузке страницы
window.addEventListener("DOMContentLoaded", applyTheme);