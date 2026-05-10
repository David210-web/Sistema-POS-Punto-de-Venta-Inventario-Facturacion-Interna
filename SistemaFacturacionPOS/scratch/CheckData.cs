using Microsoft.EntityFrameworkCore;
using SistemaFacturacionPOS.Contexto;
using SistemaFacturacionPOS.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Scratch
{
    public class CheckData
    {
        public static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SistemaFacturacionPOSContext>();
            // I need the connection string. Let's check appsettings.json
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SistemaFacturacionPOS;Trusted_Connection=True;MultipleActiveResultSets=true");

            using (var context = new SistemaFacturacionPOSContext(optionsBuilder.Options))
            {
                var pbCount = await context.ProductoBodegas.CountAsync();
                Console.WriteLine($"Total records in ProductoBodegas table: {pbCount}");
                
                try {
                    var vpbCount = await context.VistaProductosBodegas.CountAsync();
                    Console.WriteLine($"Total records in VistaProductosBodegas view: {vpbCount}");
                } catch (Exception ex) {
                    Console.WriteLine($"Error querying VistaProductosBodegas: {ex.Message}");
                }

                var productsWithStock = await context.Productos.Where(p => p.StockActual > 0).Take(5).ToListAsync();
                Console.WriteLine("\nProducts with StockActual > 0:");
                foreach(var p in productsWithStock) {
                    var pbs = await context.ProductoBodegas.Where(pb => pb.ProductoId == p.Id).ToListAsync();
                    Console.WriteLine($"- Product: {p.Nombre} (ID: {p.Id}), StockActual: {p.StockActual}, Records in ProductoBodegas: {pbs.Count}");
                }
            }
        }
    }
}

