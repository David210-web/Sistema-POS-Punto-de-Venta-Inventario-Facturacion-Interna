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
});