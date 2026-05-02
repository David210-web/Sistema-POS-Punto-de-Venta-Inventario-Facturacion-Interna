var rolId = '';

$(document).ready(function () {
    cargarDatatable();

    // Reset modal on close
    $('#modalRoles').on('hidden.bs.modal', function () {
        $('#txtRol').val('');
        $('#txtDescripcion').val('');
        $('#modalRolesLabel').text('Agregar Roles');
        rolId = '';
    });
});

function cargarDatatable() {
    $('#tblRoles').DataTable({
        language: {
            url: '/datatables/i18n/es-ES.json'
        },
        ajax: {
            url: '/Roles/GetRoles',
            dataSrc: ''
        },
        columns: [
            { data: 'id', visible: false },
            { data: 'nombre' },
            { data: 'descripcion' },
            {
                data: null,
                className: 'text-end',
                render: function (data, type, row) {
                    // Escapar los datos del row de manera segura
                    const rowData = encodeURIComponent(JSON.stringify(row));
                    return `
                        <button class="btn btn-sm btn-warning" onclick="editarRol('${rowData}')"><i class="bx bx-edit"></i> Editar</button>
                        <button class="btn btn-sm btn-danger" onclick="eliminarRol('${row.id}')"><i class="bx bx-trash"></i> Eliminar</button>
                    `;
                }
            }
        ],
        responsive: true,
        destroy: true
    });
}

function guardarRol() {
    const nombre = $('#txtRol').val().trim();
    const descripcion = $('#txtDescripcion').val().trim();

    if (nombre === '' || descripcion === '') {
        return Swal.fire('Campos incompletos', 'Complete todos los campos', 'warning');
    }

    const rol = {
        nombre: nombre,
        descripcion: descripcion
    };

    let url = '/Roles/CreateRol';
    let method = 'POST';

    if (rolId !== '') {
        url = `/Roles/UpdateRol?id=${rolId}`;
        method = 'PUT';
        rol.id = rolId;
    }

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify(rol),
        success: function (response) {
            Swal.fire('Éxito', response, 'success');
            $('#modalRoles').modal('hide');
            $('#tblRoles').DataTable().ajax.reload();
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseText || 'Error al guardar el rol', 'error');
        }
    });
}

function editarRol(rowDataEncoded) {
    const row = JSON.parse(decodeURIComponent(rowDataEncoded));
    rolId = row.id;
    $('#txtRol').val(row.nombre);
    $('#txtDescripcion').val(row.descripcion);
    $('#modalRolesLabel').text('Editar Rol');
    $('#modalRoles').modal('show');
}

function eliminarRol(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: "¡No podrá revertir esta acción!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminar!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/Roles/DeleteRol?id=${id}`,
                type: 'DELETE',
                success: function (response) {
                    Swal.fire('¡Eliminado!', response, 'success');
                    $('#tblRoles').DataTable().ajax.reload();
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseText || 'Error al eliminar el rol', 'error');
                }
            });
        }
    });
}