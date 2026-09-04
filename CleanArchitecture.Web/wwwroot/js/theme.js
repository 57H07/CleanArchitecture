// Owns the toggle for data-bs-theme. The inline head script in _Layout.cshtml reads
// the same localStorage key first, so the resolved theme is applied before first paint.

const STORAGE_KEY = "theme";
const ORDER = ["auto", "light", "dark"];

const STATES = {
    auto: { icon: "bi-circle-half", label: "Auto" },
    light: { icon: "bi-sun", label: "Light" },
    dark: { icon: "bi-moon-stars", label: "Dark" }
};

const darkMedia = window.matchMedia("(prefers-color-scheme: dark)");

export class Theme {
    /** The stored preference: "auto" (default), "light" or "dark". */
    static getPreference() {
        const stored = localStorage.getItem(STORAGE_KEY);
        return STATES[stored] ? stored : "auto";
    }

    /** Applies a preference: resolves "auto" against the OS, updates the toggle, persists the choice. */
    static setTheme(preference) {
        const state = STATES[preference] ? preference : "auto";
        const resolved = state === "auto" ? (darkMedia.matches ? "dark" : "light") : state;

        document.documentElement.setAttribute("data-bs-theme", resolved);

        const icon = document.getElementById("themeIcon");
        if (icon) icon.className = `bi ${STATES[state].icon}`;

        const label = document.getElementById("themeLabel");
        if (label) label.textContent = STATES[state].label;

        localStorage.setItem(STORAGE_KEY, state);
    }

    static init() {
        Theme.setTheme(Theme.getPreference());

        document.getElementById("themeToggleBtn")?.addEventListener("click", () => {
            const next = ORDER[(ORDER.indexOf(Theme.getPreference()) + 1) % ORDER.length];
            Theme.setTheme(next);
        });

        // Follow the OS while the preference is "auto".
        darkMedia.addEventListener("change", () => {
            if (Theme.getPreference() === "auto") Theme.setTheme("auto");
        });
    }
}
