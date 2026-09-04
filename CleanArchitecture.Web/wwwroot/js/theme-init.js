// Loaded synchronously in <head> so the theme is applied before first paint.
(function () {
    var saved = localStorage.getItem("theme") || "auto";
    var resolved = saved === "auto"
        ? (matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light")
        : saved;
    document.documentElement.setAttribute("data-bs-theme", resolved);
})();
