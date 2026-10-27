// Общие скрипты сайта «Резонанс».

// Закрытие всплывающих уведомлений (TempData alerts).
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert .close').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var alert = btn.closest('.alert');
            if (alert) { alert.remove(); }
        });
    });

    // Автоскрытие успешных уведомлений через 6 секунд.
    document.querySelectorAll('.alert:not(.alert-error)').forEach(function (alert) {
        setTimeout(function () {
            alert.style.transition = 'opacity 0.4s';
            alert.style.opacity = '0';
            setTimeout(function () { alert.remove(); }, 400);
        }, 6000);
    });
});
