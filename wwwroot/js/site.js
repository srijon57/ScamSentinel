// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": false,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};
// site.js

(function () {
    var root = document.documentElement;

    function setIconForAllToggles(isDark) {
        var buttons = Array.from(document.querySelectorAll('[data-theme-toggle], #theme-toggle'));
        buttons.forEach(function (btn) {
            btn.textContent = isDark ? '☀️' : '🌙';
        });
    }

    function applyInitialIcon() {
        var isDark = root.classList.contains('dark');
        setIconForAllToggles(isDark);
    }

    function toggleTheme() {
        var isDark = root.classList.toggle('dark');
        try { localStorage.setItem('theme', isDark ? 'dark' : 'light'); } catch (e) { }
        setIconForAllToggles(isDark);
    }

    // Early theme set (avoid white flash)
    try {
        var t = localStorage.getItem('theme');
        if (t === 'dark') root.classList.add('dark');
    } catch (e) { }

    // Delegated click handler
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-theme-toggle], #theme-toggle');
        if (btn) {
            e.preventDefault();
            toggleTheme();
        }
    });

    // Ensure correct icon
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', applyInitialIcon);
    } else {
        applyInitialIcon();
    }
})();

