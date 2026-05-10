$(document).ready(function () {
    let cart = [];
    let searchTimeout = null;

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(amount);
    };

    // 1. Búsqueda de Productos
    const fetchProducts = (query = '') => {
        $.ajax({
            url: '/POS/BuscarProductos',
            type: 'GET',
            data: { q: query },
            success: function (data) {
                $('#searchPlaceholder').hide();
                renderSearchResults(data);
            }
        });
    };

    // Carga inicial de productos
    fetchProducts();

    $('#posSearchInput').on('input', function () {
        clearTimeout(searchTimeout);
        const query = $(this).val().trim();

        searchTimeout = setTimeout(() => {
            fetchProducts(query);
        }, 300); // Debounce 300ms
    });

    function renderSearchResults(products) {
        const list = $('#searchList');
        list.empty();

        if (products.length === 0) {
            list.append('<p class="text-muted text-center mt-3">No se encontraron productos.</p>');
            return;
        }

        products.forEach(p => {
            const isLowStock = p.stockActual <= 5;
            const stockColor = p.stockActual === 0 ? 'text-danger fw-bold' : (isLowStock ? 'text-warning' : 'text-muted');

            const item = $(`
                <div class="list-group-item product-search-item p-3" data-id="${p.id}">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="mb-1 fw-bold" style="color: #0F172A;">${p.nombre}</h6>
                            <div class="d-flex gap-3" style="font-size: 13px;">
                                <span class="text-muted">Código: ${p.codigoBarras || 'N/A'}</span>
                                <span class="${stockColor}">Stock: ${p.stockActual}</span>
                            </div>
                        </div>
                        <div class="text-end">
                            <span class="fw-bold text-primary">${formatCurrency(p.precioUnitario)}</span>
                        </div>
                    </div>
                </div>
            `);

            item.on('click', () => {
                if (p.stockActual === 0) {
                    Swal.fire('Sin Stock', `El producto ${p.nombre} no tiene existencias.`, 'warning');
                    return;
                }
                addToCart(p);
                // Opcional: limpiar búsqueda después de agregar
                // $('#posSearchInput').val('');
                // $('#searchList').empty();
                // $('#searchPlaceholder').show();
            });

            list.append(item);
        });
    }

    // 2. Gestión del Carrito
    function addToCart(product) {
        const existing = cart.find(x => x.id === product.id);
        if (existing) {
            if (existing.cantidad + 1 > product.stockActual) {
                Swal.fire('Stock Insuficiente', 'No hay más unidades disponibles.', 'warning');
                return;
            }
            existing.cantidad++;
        } else {
            cart.push({
                id: product.id,
                nombre: product.nombre,
                precio: product.precioUnitario,
                cantidad: 1,
                stockActual: product.stockActual
            });
        }
        renderCart();
    }

    function renderCart() {
        const tbody = $('#cartTableBody');
        tbody.empty();

        if (cart.length === 0) {
            tbody.append('<tr><td colspan="4" class="text-center text-muted py-5">El carrito está vacío</td></tr>');
            $('#pagoCard').hide();
            $('#cartItemCount').text('0 productos');
            updateTotals();
            return;
        }

        $('#pagoCard').show();
        let itemCount = 0;

        cart.forEach((item, index) => {
            itemCount += item.cantidad;
            const subtotal = item.precio * item.cantidad;

            const tr = $(`
                <tr>
                    <td>
                        <p class="mb-0 fw-semibold text-dark" style="font-size: 14px;">${item.nombre}</p>
                        <small class="text-muted">${formatCurrency(item.precio)} c/u</small>
                    </td>
                    <td style="width: 120px;">
                        <div class="d-flex align-items-center">
                            <button class="cart-qty-btn text-primary" onclick="updateQty(${index}, -1)">-</button>
                            <input type="text" class="cart-qty-input" value="${item.cantidad}" readonly>
                            <button class="cart-qty-btn text-primary" onclick="updateQty(${index}, 1)">+</button>
                        </div>
                    </td>
                    <td class="text-end fw-bold text-dark">${formatCurrency(subtotal)}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-light text-danger" onclick="removeFromCart(${index})">
                            <i class='bx bx-trash'></i>
                        </button>
                    </td>
                </tr>
            `);
            tbody.append(tr);
        });

        $('#cartItemCount').text(`${itemCount} productos`);
        $('#pagoItemCount').text(`${itemCount} productos`);
        updateTotals();
    }

    window.updateQty = function (index, delta) {
        const item = cart[index];
        const newQty = item.cantidad + delta;
        if (newQty <= 0) {
            removeFromCart(index);
            return;
        }
        if (newQty > item.stockActual) {
            Swal.fire('Stock Insuficiente', `Solo hay ${item.stockActual} disponibles de ${item.nombre}.`, 'warning');
            return;
        }
        item.cantidad = newQty;
        renderCart();
    }

    window.removeFromCart = function (index) {
        cart.splice(index, 1);
        renderCart();
    }

    $('#btnVaciarCarrito').on('click', function () {
        if (cart.length > 0) {
            Swal.fire({
                title: '¿Vaciar Carrito?',
                text: "Se eliminarán todos los productos",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, vaciar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    cart = [];
                    renderCart();
                }
            });
        }
    });

    // 3. Totales y Pagos
    let currentTotal = 0;

    function updateTotals() {
        currentTotal = cart.reduce((sum, item) => sum + (item.precio * item.cantidad), 0);
        $('#cartSubtotal').text(formatCurrency(currentTotal));
        $('#cartTotal').text(formatCurrency(currentTotal));
        $('#pagoTotal').text(formatCurrency(currentTotal));
        validatePayment();
    }

    $('input[name="metodoPago"]').on('change', function () {
        if (this.value === 'Efectivo') {
            $('#sectionEfectivo').show();
            $('#sectionTarjeta').hide();
        } else {
            $('#sectionEfectivo').hide();
            $('#sectionTarjeta').show();
        }
        validatePayment();
    });

    $('#dineroRecibido').on('input', function () {
        validatePayment();
    });

    $('#digitosTarjeta').on('input', function () {
        // Solo permitir numeros
        this.value = this.value.replace(/[^0-9]/g, '');
        validatePayment();
    });

    function validatePayment() {
        const metodo = $('input[name="metodoPago"]:checked').val();
        let isValid = false;

        if (cart.length === 0) {
            $('#btnFinalizarVenta').prop('disabled', true);
            return;
        }

        if (metodo === 'Efectivo') {
            const recibido = parseFloat($('#dineroRecibido').val()) || 0;
            if (recibido >= currentTotal) {
                isValid = true;
                const cambio = recibido - currentTotal;
                $('#cambioCalculado').val(formatCurrency(cambio));
            } else {
                $('#cambioCalculado').val('$0.00');
            }
        } else if (metodo === 'Tarjeta') {
            const digitos = $('#digitosTarjeta').val() || '';
            if (digitos.length === 4) {
                isValid = true;
            }
        }

        $('#btnFinalizarVenta').prop('disabled', !isValid);
    }

    // 4. Finalizar Venta
    $('#btnFinalizarVenta').on('click', function () {
        const metodo = $('input[name="metodoPago"]:checked').val();
        let reqData = {
            MetodoPago: metodo,
            Total: currentTotal,
            DineroRecibido: 0,
            UltimosDigitosTarjeta: null,
            Detalles: cart.map(item => ({
                ProductoId: item.id,
                Cantidad: item.cantidad,
                PrecioUnitario: item.precio
            }))
        };

        if (metodo === 'Efectivo') {
            reqData.DineroRecibido = parseFloat($('#dineroRecibido').val());
        } else {
            reqData.UltimosDigitosTarjeta = $('#digitosTarjeta').val();
        }

        // Bloquear botón para evitar doble submit
        const btn = $(this);
        btn.prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> Procesando...');

        $.ajax({
            url: '/POS/FinalizarVenta',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(reqData),
            success: function (response) {
                // Limpiar todo
                cart = [];
                $('#dineroRecibido').val('');
                $('#digitosTarjeta').val('');
                renderCart();

                // Mostrar éxito
                Swal.fire({
                    icon: 'success',
                    title: '¡Venta Exitosa!',
                    text: 'Generando comprobante...',
                    timer: 1500,
                    showConfirmButton: false
                }).then(() => {
                    // Abrir ticket en nueva pestaña
                    window.open('/POS/Ticket/' + response.ventaId, '_blank');
                });
            },
            error: function (xhr) {
                Swal.fire('Error', xhr.responseText || 'Error al procesar la venta.', 'error');
                validatePayment(); // reactivar boton si es valido
                btn.html('Finalizar Venta');
            }
        });
    });
});
