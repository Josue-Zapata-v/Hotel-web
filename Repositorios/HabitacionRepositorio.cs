using Microsoft.EntityFrameworkCore;
using Hotel.Web.Models;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Habitacion>> ObtenerActivasAsync(int? idTipoHabitacion = null)
    {
        var query = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo);

        if (idTipoHabitacion.HasValue && idTipoHabitacion.Value > 0)
        {
            query = query.Where(h => h.IdTipoHabitacion == idTipoHabitacion.Value);
        }

        return await query.OrderBy(h => h.Numero).ToListAsync();
    }

    public async Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
    }

    public async Task<Habitacion?> ObtenerParaModificarAsync(int id)
    {
        return await _context.Habitacions.FirstOrDefaultAsync(h => h.IdHabitacion == id);
    }

    public async Task<bool> ExisteNumeroAsync(string numero, int? idExcluir = null)
    {
        if (idExcluir.HasValue)
        {
            return await _context.Habitacions.AnyAsync(h => h.Numero == numero && h.IdHabitacion != idExcluir.Value);
        }

        return await _context.Habitacions.AnyAsync(h => h.Numero == numero);
    }

    public async Task<List<TipoHabitacion>> ObtenerTiposActivosAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task AgregarAsync(Habitacion habitacion)
    {
        habitacion.Activo = true;
        await _context.Habitacions.AddAsync(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Habitacion habitacion)
    {
        _context.Habitacions.Update(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarLogicoAsync(int id)
    {
        var habitacion = await _context.Habitacions.FindAsync(id);
        if (habitacion != null)
        {
            habitacion.Activo = false;
            await _context.SaveChangesAsync();
        }
    }
}
