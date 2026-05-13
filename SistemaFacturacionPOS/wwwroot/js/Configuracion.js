$(document).ready(function () {
    // Handle Profile Form
    $('#form-profile').on('submit', function (e) {
        e.preventDefault();
        const formData = $(this).serialize();

        $.post('/Configuracion/UpdateProfile', formData, function (response) {
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Éxito',
                    text: response.message,
                    timer: 2000,
                    showConfirmButton: false
                });
            } else {
                Swal.fire('Error', response.message, 'error');
            }
        });
    });

    // Handle Empresa Form
    $('#form-empresa').on('submit', function (e) {
        e.preventDefault();
        const formData = $(this).serialize();

        $.post('/Configuracion/UpdateEmpresa', formData, function (response) {
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Éxito',
                    text: response.message,
                    timer: 2000,
                    showConfirmButton: false
                });
            } else {
                Swal.fire('Error', response.message, 'error');
            }
        });
    });

    // Handle Password Form
    $('#form-password').on('submit', function (e) {
        e.preventDefault();
        
        const currentPassword = $(this).find('[name="currentPassword"]').val();
        const newPassword = $(this).find('[name="newPassword"]').val();
        const confirmPassword = $(this).find('[name="confirmPassword"]').val();

        if (newPassword !== confirmPassword) {
            Swal.fire('Error', 'Las contraseñas no coinciden.', 'error');
            return;
        }

        $.post('/Configuracion/ChangePassword', { currentPassword, newPassword }, function (response) {
            if (response.success) {
                $('#modalPassword').modal('hide');
                $('#form-password')[0].reset();
                Swal.fire({
                    icon: 'success',
                    title: 'Éxito',
                    text: response.message,
                    timer: 2000,
                    showConfirmButton: false
                });
            } else {
                Swal.fire('Error', response.message, 'error');
            }
        });
    });
});
