const sweetAlertUtils = {
    showError: function (message) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: message,
            confirmButtonColor: '#3085d6'
        });
    },
    showSuccess: function (message) {
        Swal.fire({
            icon: 'success',
            title: 'Éxito',
            text: message,
            timer: 1500,
            showConfirmButton: false
        });
    },
    loaderAlert: function (title) {
        Swal.fire({
            title: title,
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    },
    showConfirm: function (title, text, confirmCallback) {
        Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Sí, continuar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                if (typeof confirmCallback === 'function') {
                    confirmCallback();
                }
            }
        });
    }
};
