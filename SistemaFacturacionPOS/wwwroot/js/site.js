$(document).ready(function () {
    // Escuchar clics en enlaces con la clase .ajax-link
    $(document).on('click', '.ajax-link', function (e) {
        e.preventDefault();

        let url = $(this).data('url');
        let $link = $(this);

        if (url) {
            //// Opcional: Mostrar un indicador de carga (loader)
            //$('#main-content').fadeOut(100, function () {
            //    $(this).html('<div class="text-center p-5"><i class="bx bx-loader-alt bx-spin" style="font-size: 3rem;"></i></div>').show();
            //});

            $.ajax({
                url: url,
                type: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function (data) {
                    // Actualizar la URL sin recargar
                    history.pushState(null, '', url);

                    // Actualizar el contenido
                    $('#main-content').hide().html(data).fadeIn(200);

                    // Cargar y ejecutar script si la vista lo requiere
                    let scriptUrl = $link.data('script');
                    if (scriptUrl) {
                        $.getScript(scriptUrl);
                    }

                    // Actualizar estado activo en el sidebar
                    $('.nav-link').removeClass('active');
                    $link.addClass('active');
                },
                error: function () {
                    Swal.fire('Error', 'No se pudo cargar la sección.', 'error');
                    $('#main-content').html('<p class="text-danger">Error al cargar contenido.</p>');
                }
            });
        }
    });

    // Manejar el botón de retroceso/avance del navegador
    $(window).on('popstate', function () {
        location.reload();
    });

    // Sanitización global de inputs
    setupInputSanitization();
});

/**
 * Sanitiza una cadena eliminando caracteres potencialmente peligrosos o no deseados.
 * Bloquea: < > ' " / \ ; 
 */
function sanitizeString(str) {
    if (typeof str !== 'string') return str;
    // Expresión regular para caracteres prohibidos
    const forbiddenChars = /[<>'"/\\;]/g;
    return str.replace(forbiddenChars, '');
}

/**
 * Aplica sanitización automática en tiempo real a todos los inputs de tipo texto y textareas.
 */
function setupInputSanitization() {
    $(document).on('input', 'input[type="text"], input[type="search"], textarea', function () {
        const input = $(this);
        const originalValue = input.val();
        const sanitizedValue = sanitizeString(originalValue);

        if (originalValue !== sanitizedValue) {
            // Guardar posición del cursor para evitar que salte al final
            const start = this.selectionStart;
            const end = this.selectionEnd;
            
            input.val(sanitizedValue);
            
            // Restaurar posición del cursor (ajustada si se eliminó un carácter)
            this.setSelectionRange(start - 1, end - 1);
        }
    });
}