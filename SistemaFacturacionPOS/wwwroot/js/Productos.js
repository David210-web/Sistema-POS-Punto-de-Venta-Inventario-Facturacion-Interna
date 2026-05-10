$(document).ready(function () {
    cargarProductos();
    cargarCategorias();

    // Reset modals on close
    $('#modalProductos').on('hidden.bs.modal', function () {
        $('#txtIdProducto').val('');
        $('#txtNombre').val('');
        $('#txtCodigo').val('');
        $('#txtPrecio').val('');
        $('#txtStockMinimo').val('');
        $('#txtStockActual').val('');
        $('#cbCategoria').val('');
    });

    $('#modalAjusteStock').on('hidden.bs.modal', function () {
        $('#txtIdProductoAjuste').val('');
        $('#txtCantidadAjuste').val('');
        $('#txtJustificacionAjuste').val('');
    });

    $('#modalCategorias').on('hidden.bs.modal', function () {
        $('#txtIdCategoria').val('');
        $('#txtNombreCategoria').val('');
    });
});

var dtProductos;
var dtCategorias;

function cargarProductos() {
    if ($.fn.DataTable.isDataTable('#tblProductos')) {
        $('#tblProductos').DataTable().ajax.reload();
        return;
    }

    // Load categorias select first
    cargarSelectCategorias();

    dtProductos = $('#tblProductos').DataTable({
        destroy: true,
        language: { url: '/datatables/i18n/es-ES.json' },
        ajax: {
            url: '/Productos/GetProductos',
            type: 'GET',
            dataSrc: ''
        },
        columns: [
            {
                data: null, render: function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            { data: 'nombre' },
            { data: 'codigoBarras' },
            {
                data: 'precioUnitario',
                render: function (data) {
                    return '$' + parseFloat(data).toFixed(2);
                }
            },
            { data: 'stockMinimo' },
            {
                data: 'stockActual',
                render: function (data, type, row) {
                    let badgeClass = (data <= row.stockMinimo) ? 'bg-danger' : 'bg-success';
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            {
                data: 'categoria',
                render: function (data) {
                    return data ? data.nombre : 'N/A';
                }
            },
            {
                // Columna: Ver Existencias
                data: 'id',
                orderable: false,
                render: function (data, type, row) {
                    return `<button class="btn btn-sm btn-outline-primary" onclick="verExistencias('${data}', '${row.nombre.replace(/'/g, "\\'")}')"
                                    title="Ver existencias por bodega">
                                <i class="bx bx-buildings me-1"></i> Ver Existencias
                            </button>`;
                }
            },
            {
                data: 'id',
                render: function (data, type, row) {
                    return `
                        <div class="text-end">
                            <button class="btn btn-sm btn-warning me-1" onclick="abrirModalAjuste('${data}')" title="Ajustar Stock">
                                <i class="bx bx-transfer-alt"></i>
                            </button>
                            <button class="btn btn-sm btn-info me-1" onclick="editarProductoModal('${data}')" title="Editar">
                                <i class="bx bx-edit-alt"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="eliminarProducto('${data}')" title="Eliminar">
                                <i class="bx bx-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        createdRow: function (row, data, dataIndex) {
            if (data.stockActual <= data.stockMinimo) {
                $(row).addClass('table-danger');
            }
        }
    });
}

function cargarSelectCategorias() {
    $.get('/Categorias/GetCategorias', function (data) {
        let cb = $('#cbCategoria');
        cb.empty();
        cb.append('<option value="">Seleccione una categoría</option>');
        data.forEach(c => {
            cb.append(`<option value="${c.id}">${c.nombre}</option>`);
        });
    });
}

function guardarProducto() {
    let id = $('#txtIdProducto').val();
    let producto = {
        nombre: $('#txtNombre').val(),
        codigoBarras: $('#txtCodigo').val(),
        precioUnitario: parseFloat($('#txtPrecio').val()) || 0,
        stockMinimo: parseInt($('#txtStockMinimo').val()) || 0,
        categoriaId: $('#cbCategoria').val() || null
    };

    if (!producto.nombre || !producto.codigoBarras || producto.precioUnitario <= 0) {
        sweetAlertUtils.showError('Complete los campos obligatorios: Nombre, Código de Barras y Precio.');
        return;
    }

    let url = id ? `/Productos/ActualizarProducto/${id}` : '/Productos/AgregarProductos';
    let type = id ? 'PUT' : 'POST';

    // Para creación, podemos setear el stock actual inicial (aunque luego se recomienda usar ajuste)
    if (!id) {
        producto.stockActual = parseInt($('#txtStockActual').val()) || 0;
    }

    sweetAlertUtils.loaderAlert('Guardando producto...');
    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(producto),
        success: function (res) {
            sweetAlertUtils.showSuccess(res);
            $('#modalProductos').modal('hide');
            dtProductos.ajax.reload();
        },
        error: function (err) {
            sweetAlertUtils.showError(err.responseText || 'Error al guardar el producto.');
        }
    });
}

function editarProductoModal(id) {
    let rowData = dtProductos.rows().data().toArray().find(r => r.id === id);
    if (rowData) {
        $('#txtIdProducto').val(rowData.id);
        $('#txtNombre').val(rowData.nombre);
        $('#txtCodigo').val(rowData.codigoBarras);
        $('#txtPrecio').val(rowData.precioUnitario);
        $('#txtStockMinimo').val(rowData.stockMinimo);
        $('#txtStockActual').val(rowData.stockActual).prop('disabled', true);
        $('#cbCategoria').val(rowData.categoriaId);
        $('#modalProductos').modal('show');
    }
}

function eliminarProducto(id) {
    sweetAlertUtils.showConfirm('¿Estás seguro?', 'Esta acción eliminará (soft delete) el producto.', function () {
        sweetAlertUtils.loaderAlert('Eliminando...');
        $.ajax({
            url: `/Productos/EliminarProducto/${id}`,
            type: 'DELETE',
            success: function (res) {
                sweetAlertUtils.showSuccess(res);
                dtProductos.ajax.reload();
            },
            error: function (err) {
                sweetAlertUtils.showError(err.responseText || 'Error al eliminar el producto.');
            }
        });
    });
}

// Ajuste Stock
function abrirModalAjuste(id) {
    $('#txtIdProductoAjuste').val(id);
    $('#txtCantidadAjuste').val('');
    $('#txtJustificacionAjuste').val('');
    $('#modalAjusteStock').modal('show');
}

function guardarAjusteStock() {
    let id = $('#txtIdProductoAjuste').val();
    let request = {
        cantidad: parseInt($('#txtCantidadAjuste').val()),
        justificacion: $('#txtJustificacionAjuste').val()
    };

    if (!request.cantidad || !request.justificacion) {
        sweetAlertUtils.showError('Debe ingresar la cantidad y una justificación válida.');
        return;
    }

    sweetAlertUtils.loaderAlert('Aplicando ajuste...');
    $.ajax({
        url: `/Productos/AjustarStock/${id}`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function (res) {
            sweetAlertUtils.showSuccess(res);
            $('#modalAjusteStock').modal('hide');
            dtProductos.ajax.reload();
        },
        error: function (err) {
            sweetAlertUtils.showError(err.responseText || 'Error al aplicar el ajuste.');
        }
    });
}

// Categorias CRUD
function cargarCategorias() {
    if ($.fn.DataTable.isDataTable('#tblCategorias')) {
        $('#tblCategorias').DataTable().ajax.reload();
        return;
    }

    dtCategorias = $('#tblCategorias').DataTable({
        destroy: true,
        language: { url: '/datatables/i18n/es-ES.json' },
        ajax: {
            url: '/Categorias/GetCategorias',
            type: 'GET',
            dataSrc: ''
        },
        columns: [
            { data: 'nombre' },
            {
                data: 'id',
                render: function (data, type, row) {
                    return `
                        <div class="text-end">
                            <button class="btn btn-sm btn-info me-1" onclick='editarCategoria("${data}", "${row.nombre}")' title="Editar">
                                <i class="bx bx-edit-alt"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="eliminarCategoria('${data}')" title="Eliminar">
                                <i class="bx bx-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ]
    });
}

function guardarCategoria() {
    let id = $('#txtIdCategoria').val();
    let categoria = {
        nombre: $('#txtNombreCategoria').val()
    };

    if (!categoria.nombre) {
        sweetAlertUtils.showError('El nombre de la categoría es requerido.');
        return;
    }

    let url = id ? `/Categorias/ActualizarCategoria/${id}` : '/Categorias/AgregarCategoria';
    let type = id ? 'PUT' : 'POST';

    sweetAlertUtils.loaderAlert('Guardando categoría...');
    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(categoria),
        success: function (res) {
            sweetAlertUtils.showSuccess(res);
            $('#txtIdCategoria').val('');
            $('#txtNombreCategoria').val('');
            dtCategorias.ajax.reload();
            cargarSelectCategorias(); // Refresh select in products modal
        },
        error: function (err) {
            sweetAlertUtils.showError(err.responseText || 'Error al guardar la categoría.');
        }
    });
}

function editarCategoria(id, nombre) {
    $('#txtIdCategoria').val(id);
    $('#txtNombreCategoria').val(nombre);
}

function eliminarCategoria(id) {
    sweetAlertUtils.showConfirm('¿Estás seguro?', 'Se eliminará la categoría. No se puede si tiene productos.', function () {
        sweetAlertUtils.loaderAlert('Eliminando...');
        $.ajax({
            url: `/Categorias/EliminarCategoria/${id}`,
            type: 'DELETE',
            success: function (res) {
                sweetAlertUtils.showSuccess(res);
                dtCategorias.ajax.reload();
                cargarSelectCategorias();
            },
            error: function (err) {
                sweetAlertUtils.showError(err.responseText || 'Error al eliminar la categoría.');
            }
        });
    });
}

// ============================================================
// EXISTENCIAS POR BODEGA
// ============================================================

// Abre el modal y carga las existencias del producto
function verExistencias(productoId, nombreProducto) {
    $('#txtIdProductoExistencias').val(productoId);
    $('#lblNombreProductoExistencias').text(nombreProducto);
    $('#txtIdExistenciaEditar').val('');
    $('#txtStockExistencia').val('');
    $('#cbBodegaExistencia').val('');

    cargarSelectBodegas();
    cargarTablaExistencias(productoId);
    $('#modalExistencias').modal('show');
}

// Carga el <select> de bodegas disponibles
function cargarSelectBodegas() {
    $.get('/Bodegas/GetBodegas', function (data) {
        let cb = $('#cbBodegaExistencia');
        cb.empty().append('<option value="">Seleccione bodega</option>');
        data.forEach(function (b) {
            cb.append(`<option value="${b.id}">${b.nombre}</option>`);
        });
    });
}

// Carga la tabla de existencias dentro del modal
function cargarTablaExistencias(productoId) {
    $.ajax({
        url: `/ProductoBodega/GetExistencias/${productoId}`,
        type: 'GET',
        success: function (data) {
            let tbody = $('#bodyExistencias');
            tbody.empty();

            if (data.length === 0) {
                tbody.append('<tr><td colspan="3" class="text-center text-muted">Sin existencias registradas.</td></tr>');
                return;
            }

            data.forEach(function (e) {
                tbody.append(`
                <tr>
                    <td>${e.bodegaNombre}</td>
                    <td><span class="badge bg-primary">${e.stock}</span></td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-warning me-1"
                                onclick="editarExistencia('${e.id}', '${e.bodegaId}', ${e.stock})"
                                title="Editar stock">
                            <i class="bx bx-edit-alt"></i>
                        </button>
                        <button class="btn btn-sm btn-danger"
                                onclick="eliminarExistencia('${e.id}')"
                                title="Eliminar existencia">
                            <i class="bx bx-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
            });
        },
        error: function (err) {
            console.log(err);
        }
    });
}

// Prepara el formulario para editar una existencia existente
function editarExistencia(id, bodegaId, stock) {
    $('#txtIdExistenciaEditar').val(id);
    $('#cbBodegaExistencia').val(bodegaId);
    $('#txtStockExistencia').val(stock);
}

// Guarda: POST si es nueva, PUT si está editando
function guardarExistencia() {
    let productoId = $('#txtIdProductoExistencias').val();
    let bodegaId = $('#cbBodegaExistencia').val();
    let stock = parseInt($('#txtStockExistencia').val());
    let editId = $('#txtIdExistenciaEditar').val();

    if (!bodegaId) {
        sweetAlertUtils.showError('Seleccione una bodega.');
        return;
    }
    if (isNaN(stock) || stock < 0) {
        sweetAlertUtils.showError('El stock debe ser un número mayor o igual a 0.');
        return;
    }

    let url = editId ? `/ProductoBodega/ActualizarExistencia/${editId}` : '/ProductoBodega/AgregarExistencia';
    let type = editId ? 'PUT' : 'POST';
    let body = { productoId: productoId, bodegaId: bodegaId, stock: stock };

    sweetAlertUtils.loaderAlert('Guardando existencia...');
    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(body),
        success: function (res) {
            sweetAlertUtils.showSuccess(res);
            $('#txtIdExistenciaEditar').val('');
            $('#cbBodegaExistencia').val('');
            $('#txtStockExistencia').val('');
            cargarTablaExistencias(productoId);
            // Refrescar stock total en la tabla de productos
            dtProductos.ajax.reload(null, false);
        },
        error: function (err) {
            sweetAlertUtils.showError(err.responseText || 'Error al guardar la existencia.');
        }
    });
}

// Elimina la relación producto-bodega
function eliminarExistencia(id) {
    sweetAlertUtils.showConfirm(
        '¿Eliminar existencia?',
        'Se quitará el stock de esa bodega y se recalculará el total.',
        function () {
            let productoId = $('#txtIdProductoExistencias').val();
            sweetAlertUtils.loaderAlert('Eliminando...');
            $.ajax({
                url: `/ProductoBodega/EliminarExistencia/${id}`,
                type: 'DELETE',
                success: function (res) {
                    sweetAlertUtils.showSuccess(res);
                    cargarTablaExistencias(productoId);
                    dtProductos.ajax.reload(null, false);
                },
                error: function (err) {
                    sweetAlertUtils.showError(err.responseText || 'Error al eliminar la existencia.');
                }
            });
        }
    );
}