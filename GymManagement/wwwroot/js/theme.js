const themeToggle = document.getElementById("themeToggle");
const systemTheme = window.matchMedia("(prefers-color-scheme: dark)");

function getSystemTheme() {
    return systemTheme.matches ? "dark" : "light";
}

function setTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);

    if (themeToggle) {
        themeToggle.textContent =
            theme === "dark" ? "☀️" : "🌙";
    }
}

function loadTheme() {
    const savedTheme = localStorage.getItem("theme");

    // First time: detect browser/system theme
    if (savedTheme === null) {
        setTheme(getSystemTheme());
        return;
    }

    // Later visits: use saved preference
    setTheme(savedTheme);
}

loadTheme();

themeToggle?.addEventListener("click", () => {
    const currentTheme =
        document.documentElement.getAttribute("data-theme");

    const newTheme =
        currentTheme === "dark" ? "light" : "dark";

    setTheme(newTheme);

    // Remember user's choice
    localStorage.setItem("theme", newTheme);
});