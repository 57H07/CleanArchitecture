export class Theme {
    static setTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        const icon = document.getElementById('themeIcon');
        if (icon) {
            if (theme === 'dark') {
                icon.classList.remove('bi-sun');
                icon.classList.add('bi-moon');
            } else {
                icon.classList.remove('bi-moon');
                icon.classList.add('bi-sun');
            }
        }
        localStorage.setItem('theme', theme);
    }

    static init() {
        const savedTheme = localStorage.getItem('theme') || 'light';
        Theme.setTheme(savedTheme);
        const btn = document.getElementById('themeToggleBtn');
        if (btn) {
            btn.addEventListener('click', function () {
                const currentTheme = document.documentElement.getAttribute('data-bs-theme');
                Theme.setTheme(currentTheme === 'dark' ? 'light' : 'dark');
            });
        }
    }
}
