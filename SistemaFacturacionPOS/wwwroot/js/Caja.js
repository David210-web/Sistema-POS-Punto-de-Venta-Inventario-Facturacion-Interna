$(document).ready(function () {
    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(amount);
    };

    $('#montoInicial').on('input', function () {
        let val = parseFloat($(this).val()) || 0;
        $('#montoPreview').text('Monto: ' + formatCurrency(val));
    });

    $('#formAbrirCaja').on('submit', function (e) {
        e.preventDefault();

        let monto = parseFloat($('#montoInicial').val());

        if (monto <= 0 || isNaN(monto)) {
            Swal.fire({
                icon: 'warning',
                title: 'Atención',
                text: 'El monto inicial debe ser mayor a 0.',
                confirmButtonColor: '#0d6efd'
            });
            return;
        }

        $.ajax({
            url: '/Caja/AbrirCaja',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ MontoInicial: monto }),
            success: function (response) {
                $('#abrirCajaModal').modal('hide');
                Swal.fire({
                    icon: 'success',
                    title: '¡Caja Abierta!',
                    text: response.message || 'La caja se abrió correctamente.',
                    confirmButtonColor: '#0d6efd'
                }).then(() => {
                    // Recargar el contenido actual
                    let activeLink = $('.nav-link.ajax-link.active');
                    if (activeLink.length > 0) {
                        activeLink.trigger('click');
                    } else {
                        window.location.reload();
                    }
                });
            },
            error: function (xhr) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: xhr.responseText || 'Ocurrió un error al abrir la caja.',
                    confirmButtonColor: '#0d6efd'
                });
            }
        });
    });

    $('#montoFisico').on('input', function () {
        let val = parseFloat($(this).val()) || 0;
        $('#montoFisicoPreview').text('Monto: ' + formatCurrency(val));
    });

    $('#formCerrarCaja').on('submit', function (e) {
        e.preventDefault();

        let montoFisico = parseFloat($('#montoFisico').val());

        $.ajax({
            url: '/Caja/CerrarCaja',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ MontoFisico: montoFisico }),
            success: function (response) {
                $('#cerrarCajaModal').modal('hide');
                Swal.fire({
                    icon: 'success',
                    title: '¡Caja Cerrada!',
                    text: response.message || 'La caja se cerró correctamente.',
                    confirmButtonColor: '#0d6efd'
                }).then(() => {
                    let activeLink = $('.nav-link.ajax-link.active');
                    if (activeLink.length > 0) {
                        activeLink.trigger('click');
                    } else {
                        window.location.reload();
                    }
                });
            },
            error: function (xhr) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: xhr.responseText || 'Ocurrió un error al cerrar la caja.',
                    confirmButtonColor: '#0d6efd'
                });
            }
        });
    });
});
