$(document).ready(function () {
    cargarBodegas();

    // Resetear modal al cerrar
    $('#modalBodega').on('hidden.bs.modal', function () {
        $('#txtIdBodega').val('');
        $('#txtNombreBodega').val('');
        $('#txtDescripcionBodega').val('');
        $('#modalBodegaLabel').text('Agregar Bodega');
    });
});

var dtBodegas;

function cargarBodegas() {
    if ($.fn.DataTable.isDataTable('#tblBodegas')) {
        $('#tblBodegas').DataTable().ajax.reload();
        return;
    }

    dtBodegas = $('#tblBodegas').DataTable({
        destroy: true,
        language: { url: '/datatables/i18n/es-ES.json' },
        ajax: {
            url: '/Bodegas/GetBodegas',
            type: 'GET',
            dataSrc: ''
        },
        columns: [
            {
                data: null,
                render: function (data, type, row, meta) { return meta.row + 1; }
            },
            { data: 'nombre' },
            {
                data: 'descripcion',
                render: function (data) { return data || '<span class="text-muted">—</span>'; }
            },
            {
                data: 'id',
                render: function (data, type, row) {
                    return `
                        <div class="text-end">
                            <button class="btn btn-sm btn-info me-1"
                                    onclick='editarBodegaModal("${data}", "${row.nombre}", ${JSON.stringify(row.descripcion || '')})'
                                    title="Editar">
                                <i class="bx bx-edit-alt"></i>
                            </button>
                            <button class="btn btn-sm btn-danger"
                                    onclick="eliminarBodega('${data}')"
                                    title="Eliminar">
                                <i class="bx bx-trash"></i>
                            </button>
                        </div>`;
                }
            }
        ]
    });
}

function guardarBodega() {
    let id = $('#txtIdBodega').val();
    let bodega = {
        nombre:      $('#txtNombreBodega').val().trim(),
        descripcion: $('#txtDescripcionBodega').val().trim() || null
    };

    if (!bodega.nombre) {
        sweetAlertUtils.showError('El nombre de la bodega es requerido.');
        return;
    }

    let url  = id ? `/Bodegas/ActualizarBodega/${id}` : '/Bodegas/AgregarBodega';
    let type = id ? 'PUT' : 'POST';

    sweetAlertUtils.loaderAlert('Guardando bodega...');
    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(bodega),
        success: function (res) {
            sweetAlertUtils.showSuccess(res);
            $('#modalBodega').modal('hide');
            dtBodegas.ajax.reload();
        },
        error: function (err) {
            sweetAlertUtils.showError(err.responseText || 'Error al guardar la bodega.');
        }
    });
}

function editarBodegaModal(id, nombre, descripcion) {
    $('#txtIdBodega').val(id);
    $('#txtNombreBodega').val(nombre);
    $('#txtDescripcionBodega').val(descripcion);
    $('#modalBodegaLabel').text('Editar Bodega');
    $('#modalBodega').modal('show');
}

function eliminarBodega(id) {
    sweetAlertUtils.showConfirm(
        '¿Eliminar bodega?',
        'Solo se puede eliminar si no tiene productos asignados.',
        function () {
            sweetAlertUtils.loaderAlert('Eliminando...');
            $.ajax({
                url: `/Bodegas/EliminarBodega/${id}`,
                type: 'DELETE',
                success: function (res) {
                    sweetAlertUtils.showSuccess(res);
                    dtBodegas.ajax.reload();
                },
                error: function (err) {
                    sweetAlertUtils.showError(err.responseText || 'Error al eliminar la bodega.');
                }
            });
        }
    );
}
