$(document).ready(function () {
    // Función simple para manejar la hora en tiempo real en el encabezado
    function updateTime() {
        var now = new Date();
        var options = { hour: '2-digit', minute: '2-digit', hour12: true };
        var timeString = now.toLocaleTimeString('es-ES', options);
        $('#current-time').text(timeString);
    }
    
    // Actualizar hora cada segundo
    updateTime();
    setInterval(updateTime, 1000);

    // Activar el link actual basado en la URL
    var currentUrl = window.location.pathname.toLowerCase();
    $('.sidebar-nav .nav-link').each(function() {
        var href = $(this).attr('href') ? $(this).attr('href').toLowerCase() : "";
        var dataUrl = $(this).data('url') ? $(this).data('url').toLowerCase() : "";
        
        if (href === currentUrl || dataUrl === currentUrl || 
            (currentUrl === '/' && href === '/home/index') ||
            (dataUrl && currentUrl.startsWith(dataUrl))) {
            $(this).addClass('active');
        }
    });
});
