$(document).ready(function () {
    cargarLogs();

    $('#logFilterForm').off('submit').on('submit', function (e) {
        e.preventDefault();
        cargarLogs();
    });

    $('#btnResetLogs').off('click').on('click', function () {
        const today = new Date().toISOString().split('T')[0];
        $('#startDate').val(today);
        $('#endDate').val(today);
        cargarLogs();
    });
});

var dtLogs;

function cargarLogs() {
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();

    if ($.fn.DataTable.isDataTable('#tblLogs')) {
        $('#tblLogs').DataTable().destroy();
    }

    dtLogs = $('#tblLogs').DataTable({
        language: { url: '/datatables/i18n/es-ES.json' },
        ajax: {
            url: `/Logs/GetLogs?startDate=${startDate}&endDate=${endDate}`,
            type: 'GET',
            dataSrc: ''
        },
        columns: [
            { data: 'id' },
            { data: 'username' },
            { 
                data: 'tabla_afectada',
                render: function(data) {
                    return `<span class="badge bg-light text-dark border">${data}</span>`;
                }
            },
            { 
                data: 'accion',
                render: function(data) {
                    let badgeClass = 'bg-secondary';
                    if (data === 'INSERT') badgeClass = 'bg-success';
                    if (data === 'UPDATE') badgeClass = 'bg-info';
                    if (data === 'DELETE') badgeClass = 'bg-danger';
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            { 
                data: 'valor_anterior',
                render: function(data) {
                    return formatJsonCell(data);
                }
            },
            { 
                data: 'valor_nuevo',
                render: function(data) {
                    return formatJsonCell(data);
                }
            },
            { 
                data: 'fecha_hora',
                render: function(data) {
                    return new Date(data).toLocaleString();
                }
            }
        ],
        order: [[6, 'desc']],
        pageLength: 25
    });
}

function formatJsonCell(data) {
    if (!data) return '<span class="text-muted small">N/A</span>';
    
    try {
        // Intentar parsear para ver si es JSON
        JSON.parse(data);
        return `<button class="btn btn-sm btn-outline-primary py-0" onclick="showJsonDetail('${encodeURIComponent(data)}')">
                    <i class="bx bx-code-alt"></i> Ver JSON
                </button>`;
    } catch (e) {
        return `<span class="small text-truncate d-inline-block" style="max-width: 150px;">${data}</span>`;
    }
}

function showJsonDetail(encodedJson) {
    const jsonStr = decodeURIComponent(encodedJson);
    try {
        const obj = JSON.parse(jsonStr);
        $('#jsonViewer').text(JSON.stringify(obj, null, 4));
        new bootstrap.Modal(document.getElementById('modalJson')).show();
    } catch (e) {
        $('#jsonViewer').text(jsonStr);
        new bootstrap.Modal(document.getElementById('modalJson')).show();
    }
}
