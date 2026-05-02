// wwwroot/js/theme-manager.js

// Este script se ejecuta lo más pronto posible (en el <head>)
// para evitar un pantallazo blanco si el usuario tiene el modo oscuro activado.

(function () {
    const isDarkMode = sessionStorage.getItem('dark_mode') === 'true';
    if (isDarkMode) {
        document.documentElement.classList.add('dark-mode');
    }
})();

// Lógica del botón de alternancia
document.addEventListener('DOMContentLoaded', () => {
    const themeToggleBtn = document.getElementById('theme-toggle');
    if (!themeToggleBtn) return;

    // Actualizar icono inicial
    const isDarkMode = document.documentElement.classList.contains('dark-mode');
    updateThemeIcon(themeToggleBtn, isDarkMode);

    themeToggleBtn.addEventListener('click', (e) => {
        e.preventDefault();
        const currentlyDark = document.documentElement.classList.toggle('dark-mode');
        sessionStorage.setItem('dark_mode', currentlyDark);
        updateThemeIcon(themeToggleBtn, currentlyDark);
    });

    function updateThemeIcon(btn, isDark) {
        const icon = btn.querySelector('i');
        const text = btn.querySelector('span');
        if (isDark) {
            icon.classList.remove('bx-moon');
            icon.classList.add('bx-sun');
            if (text) text.textContent = 'Modo Claro';
        } else {
            icon.classList.remove('bx-sun');
            icon.classList.add('bx-moon');
            if (text) text.textContent = 'Modo Oscuro';
        }
    }
});
