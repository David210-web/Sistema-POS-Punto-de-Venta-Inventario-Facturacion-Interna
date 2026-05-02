var usuarios = []
var usuarioId = '';

var inputs = {
    inputName: $('#txtUsuario'),
    inputPass: $('#txtPassword'),
    comboRol: $('#cbRol'),
    checkActivo: $('#chActivo')
}

$(document).ready(function () {
    cargarRoles();
    cargarUsuarios();

    // Reset modal on close
    $('#modalUsuarios').on('hidden.bs.modal', function () {
        inputs.inputName.val('');
        inputs.inputPass.val('');
        inputs.inputPass.prop('disabled', false); // re-enable pass
        inputs.comboRol.val('');
        inputs.checkActivo.prop('checked', false);
        $('#modalUsuariosLabel').text('Agregar Usuarios');
        usuarioId = '';
    });
})

function cargarRoles() {
    $.ajax({
        url: '/Roles/GetRoles',
        type: 'GET',
        success: function (roles) {
            inputs.comboRol.empty();
            inputs.comboRol.append('<option value="">Seleccione un Rol</option>');
            roles.forEach(rol => {
                inputs.comboRol.append(`<option value="${rol.id}">${rol.nombre}</option>`);
            });
        },
        error: function (xhr) {
            console.error("Error al cargar roles:", xhr.responseText);
        }
    });
}

function cargarUsuarios() {
    $('#tblUsuarios').DataTable({
        language: {
            url: '/datatables/i18n/es-ES.json'
        },
        ajax: {
            url: '/Usuario/GetUsuarios',
            dataSrc: ''
        },
        columns: [
            { data: 'id', visible: false },
            { data: 'username' },
            {
                data: 'activo',
                render: function (data) {
                    return data ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-danger">Inactivo</span>';
                }
            },
            {
                data: 'rol',
                render: function (data) {
                    return data ? data.nombre : 'Sin Rol';
                }
            },
            {
                data: null,
                className: 'text-end',
                render: function (data, type, row) {
                    const rowData = encodeURIComponent(JSON.stringify(row));
                    const isActivo = row.activo;
                    const btnActivar = isActivo
                        ? `<button class="btn btn-sm btn-danger" onclick="cambiarEstadoUsuario('${row.id}', false)"><i class="bx bx-x-circle"></i> Desactivar</button>`
                        : `<button class="btn btn-sm btn-success" onclick="cambiarEstadoUsuario('${row.id}', true)"><i class="bx bx-check-circle"></i> Activar</button>`;

                    return `
                        <button class="btn btn-sm btn-warning" onclick="editarUsuario('${rowData}')"><i class="bx bx-edit"></i> Editar</button>
                        ${btnActivar}
                        <button class="btn btn-sm btn-info d-none" onclick="restablecerPassword('${row.id}')"><i class="bx bx-key"></i> Reset Pass</button>
                    `;
                }
            }
        ],
        responsive: true,
        destroy: true
    });
}

function guardarUsuario() {
    if (validarCampos()) {
        return Swal.fire('Campos incompletos', 'Complete todos los campos obligatorios', 'warning');
    }

    const usuario = {
        username: inputs.inputName.val().trim(),
        rolId: inputs.comboRol.val()
    };

    let url = '/Usuario/CreateUsuario';
    let method = 'POST';

    if (usuarioId !== '') {
        // Edit
        url = `/Usuario/UpdateUsuario?id=${usuarioId}`;
        method = 'PUT';
        usuario.id = usuarioId;
    } else {
        // Create
        usuario.passwordHash = inputs.inputPass.val().trim();
        usuario.activo = inputs.checkActivo.is(':checked');
    }

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify(usuario),
        success: function (response) {
            Swal.fire('Éxito', response, 'success');
            $('#modalUsuarios').modal('hide');
            $('#tblUsuarios').DataTable().ajax.reload();
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseText || 'Error al guardar el usuario', 'error');
        }
    });
}

function validarCampos() {
    const name = inputs.inputName.val().trim();
    const rol = inputs.comboRol.val();

    if (usuarioId === '') {
        // Create mode
        const password = inputs.inputPass.val().trim();
        return (name === '') || (password === '') || (rol === '' || rol === null);
    } else {
        // Edit mode
        return (name === '') || (rol === '' || rol === null);
    }
}

function editarUsuario(rowDataEncoded) {
    const row = JSON.parse(decodeURIComponent(rowDataEncoded));
    usuarioId = row.id;
    inputs.inputName.val(row.username);
    inputs.inputPass.val(''); // Clear password field
    inputs.inputPass.prop('disabled', true); // Disable password field
    inputs.comboRol.val(row.rolId);
    inputs.checkActivo.prop('checked', row.activo);
    $('#modalUsuariosLabel').text('Editar Usuario');
    $('#modalUsuarios').modal('show');
}

function cambiarEstadoUsuario(id, activo) {
    const accion = activo ? 'activar' : 'desactivar';
    Swal.fire({
        title: `¿Está seguro de ${accion} este usuario?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: `Sí, ${accion}!`,
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/Usuario/PatchUsuario?id=${id}`,
                type: 'PATCH',
                contentType: 'application/json',
                data: JSON.stringify({ activo: activo }),
                success: function (response) {
                    Swal.fire('¡Actualizado!', response, 'success');
                    $('#tblUsuarios').DataTable().ajax.reload();
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseText || 'Error al actualizar el estado', 'error');
                }
            });
        }
    });
}

function restablecerPassword(id) {
    Swal.fire({
        title: '¿Restablecer contraseña?',
        text: "La contraseña será restablecida al nombre de usuario.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, restablecer!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/Usuario/RestablecerContraseña?id=${id}`,
                type: 'PUT',
                success: function (response) {
                    Swal.fire('¡Restablecida!', response, 'success');
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseText || 'Error al restablecer la contraseña', 'error');
                }
            });
        }
    });
}
