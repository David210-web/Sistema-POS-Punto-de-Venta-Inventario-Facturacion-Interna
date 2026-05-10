var currentAction = {};

$(document).ready(function () {
    const isAdminRole = window.isAdminRole || false;

    loadVentas(isAdminRole);
    if (isAdminRole) loadSolicitudes(isAdminRole);

    $('#filterForm').off('submit').on('submit', function (e) {
        e.preventDefault();
        loadVentas(isAdminRole);
    });

    $('#btnLimpiar').off('click').on('click', function () {
        $('#searchFolio').val('');
        $('#searchDate').val('');
        loadVentas(isAdminRole);
    });

    $(document).off('click', '.btn-ticket').on('click', '.btn-ticket', function () {
        const id = $(this).data('id');
        window.open('/POS/Ticket/' + id, '_blank');
    });

    $(document).off('click', '.btn-nc').on('click', '.btn-nc', function () {
        const id = $(this).data('id');
        window.open('/Facturacion/NotaCredito?ventaId=' + id, '_blank');
    });

    $(document).off('click', '.btn-anular').on('click', '.btn-anular', function () {
        const id = $(this).data('id');
        const folio = $(this).data('folio');

        if (isAdminRole) {
            currentAction = { type: 'anular_directo', id: id };
            $('#lblAdminMensaje').text(`Ingrese su contraseña para anular directamente la factura V${folio}.`);
            $('#txtAdminPassword').val('');
            new bootstrap.Modal(document.getElementById('modalAdminAnular')).show();
        } else {
            currentAction = { type: 'solicitar_anulacion', id: id };
            $('#lblSolicitarFolio').text(`V${folio}`);
            $('#txtMotivoSolicitud').val('');
            new bootstrap.Modal(document.getElementById('modalSolicitarAnulacion')).show();
        }
    });

    $(document).off('click', '.btn-aprobar').on('click', '.btn-aprobar', function () {
        const id = $(this).data('id'); // solicitud id
        currentAction = { type: 'aprobar_solicitud', id: id };
        $('#lblAdminMensaje').text(`Ingrese su contraseña para aprobar esta solicitud de anulación.`);
        $('#txtAdminPassword').val('');
        new bootstrap.Modal(document.getElementById('modalAdminAnular')).show();
    });

    $(document).off('click', '.btn-rechazar').on('click', '.btn-rechazar', function () {
        const id = $(this).data('id'); // solicitud id
        currentAction = { type: 'rechazar_solicitud', id: id };
        $('#txtMotivoRechazo').val('');
        new bootstrap.Modal(document.getElementById('modalRechazar')).show();
    });

    // Confirmations
    $('#btnConfirmarSolicitud').off('click').on('click', function () {
        const motivo = $('#txtMotivoSolicitud').val();
        if (!motivo) return Swal.fire('Atención', 'Debe ingresar un motivo', 'warning');

        $.post('/Facturacion/SolicitarAnulacion', { ventaId: currentAction.id, motivo: motivo })
            .done(function () {
                bootstrap.Modal.getInstance(document.getElementById('modalSolicitarAnulacion')).hide();
                Swal.fire('Éxito', 'Solicitud enviada correctamente', 'success');
                loadVentas(isAdminRole);
            })
            .fail(function (err) {
                Swal.fire('Error', err.responseText || 'Error al solicitar anulación', 'error');
            });
    });

    $('#btnConfirmarAdmin').off('click').on('click', function () {
        const pass = $('#txtAdminPassword').val();
        if (!pass) return Swal.fire('Atención', 'Debe ingresar su contraseña', 'warning');

        let url = currentAction.type === 'anular_directo' ? '/Facturacion/AnularDirecto' : '/Facturacion/AprobarAnulacion';
        let data = currentAction.type === 'anular_directo' ? { ventaId: currentAction.id, password: pass } : { solicitudId: currentAction.id, password: pass };

        $.post(url, data)
            .done(function (res) {
                bootstrap.Modal.getInstance(document.getElementById('modalAdminAnular')).hide();
                Swal.fire('Éxito', 'Anulación procesada correctamente. Se ha generado la Nota de Crédito.', 'success');
                window.open('/Facturacion/NotaCredito?ventaId=' + (currentAction.type === 'anular_directo' ? currentAction.id : res.ventaId || currentAction.id), '_blank');
                loadVentas(isAdminRole);
                if (isAdminRole) loadSolicitudes(isAdminRole);
            })
            .fail(function (err) {
                Swal.fire('Error', err.responseText || 'Error en la autorización', 'error');
            });
    });

    $('#btnConfirmarRechazo').off('click').on('click', function () {
        const motivo = $('#txtMotivoRechazo').val();
        if (!motivo) return Swal.fire('Atención', 'Debe ingresar el motivo de rechazo', 'warning');

        $.post('/Facturacion/RechazarAnulacion', { solicitudId: currentAction.id, motivoRechazo: motivo })
            .done(function () {
                bootstrap.Modal.getInstance(document.getElementById('modalRechazar')).hide();
                Swal.fire('Éxito', 'Solicitud rechazada', 'success');
                loadSolicitudes(isAdminRole);
            })
            .fail(function (err) {
                Swal.fire('Error', 'Error al rechazar: ' + err.responseText, 'error');
            });
    });
});

function loadVentas(isAdminRole) {
    const folio = $('#searchFolio').val();
    const date = $('#searchDate').val();

    $.get('/Facturacion/GetVentas', { folio, date }, function (data) {
        const tbody = $('#ventasTable tbody');
        tbody.empty();

        if (data.length === 0) {
            tbody.append('<tr><td colspan="7" class="text-center text-muted py-4">No se encontraron ventas</td></tr>');
            return;
        }

        data.forEach(v => {
            let estadoBadge = v.estado === 'ANULADA' ? '<span class="badge bg-danger">Cancelada</span>' : '<span class="badge bg-success">Completada</span>';
            let rowClass = v.estado === 'ANULADA' ? 'table-danger' : '';

            let actions = `<button class="btn btn-sm btn-outline-secondary btn-ticket" data-id="${v.id}" title="Ver Comprobante"><i class="bx bx-receipt"></i></button>`;

            if (v.estado === 'ANULADA') {
                actions += ` <button class="btn btn-sm btn-outline-info btn-nc" data-id="${v.id}" title="Descargar Nota Crédito"><i class="bx bx-download"></i> NC</button>`;
            } else {
                actions += ` <button class="btn btn-sm btn-outline-danger btn-anular" data-id="${v.id}" data-folio="${v.folio}" title="Anular"><i class="bx bx-x"></i></button>`;
            }

            tbody.append(`
                <tr class="${rowClass}">
                    <td class="ps-4 fw-bold text-primary">V${String(v.folio).padStart(7, '0')}</td>
                    <td>${v.fecha}</td>
                    <td>${v.cajero}</td>
                    <td><span class="badge bg-light text-dark border">${v.metodo}</span></td>
                    <td class="fw-bold">${v.total}</td>
                    <td>${estadoBadge}</td>
                    <td class="text-end pe-4">${actions}</td>
                </tr>
            `);
        });
    });
}

function loadSolicitudes(isAdminRole) {
    $.get('/Facturacion/GetSolicitudes', function (data) {
        const container = $('#solicitudesContainer');
        const tbody = $('#solicitudesTable tbody');
        tbody.empty();

        if (data.length > 0) {
            container.show();
            data.forEach(s => {
                tbody.append(`
                    <tr>
                        <td class="fw-bold">V${String(s.folioVenta).padStart(7, '0')}</td>
                        <td>${s.fecha}</td>
                        <td>${s.cajero}</td>
                        <td>${s.motivo}</td>
                        <td class="text-end">
                            <button class="btn btn-sm btn-success btn-aprobar" data-id="${s.id}"><i class="bx bx-check"></i> Aprobar</button>
                            <button class="btn btn-sm btn-danger btn-rechazar" data-id="${s.id}"><i class="bx bx-x"></i> Rechazar</button>
                        </td>
                    </tr>
                `);
            });
        } else {
            container.hide();
        }
    });
}
