using Microsoft.EntityFrameworkCore;
using Hotel.Web.Models;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Reserva>> ObtenerTodasAsync()
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();
    }

    public async Task<Reserva?> ObtenerPorIdAsync(int id)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(r => r.IdReserva == id);
    }

    // Punto 9: Disponibles en una sola consulta LINQ con Any / !Any
    public async Task<List<Habitacion>> ObtenerDisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo && !h.Reservas.Any(r =>
                r.Estado != "Cancelada" &&
                r.FechaIngreso < salida &&
                r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task<bool> HabitacionEstaDisponibleAsync(int idHabitacion, DateOnly ingreso, DateOnly salida)
    {
        return !await _context.Reservas
            .AnyAsync(r => r.IdHabitacion == idHabitacion &&
                           r.Estado != "Cancelada" &&
                           r.FechaIngreso < salida &&
                           r.FechaSalida > ingreso);
    }

    public async Task<List<Huesped>> ObtenerHuespedesActivosAsync()
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos)
            .ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public async Task<List<Habitacion>> ObtenerHabitacionesActivasAsync()
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task CrearAsync(Reserva reserva)
    {
        reserva.Estado = "Pendiente";
        reserva.FechaRegistro = DateTime.Now;
        await _context.Reservas.AddAsync(reserva);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CancelarAsync(int idReserva)
    {
        var reserva = await _context.Reservas.FindAsync(idReserva);
        if (reserva != null && (reserva.Estado == "Pendiente" || reserva.Estado == "Confirmada"))
        {
            reserva.Estado = "Cancelada";
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
