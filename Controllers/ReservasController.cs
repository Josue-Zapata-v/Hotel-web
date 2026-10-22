using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Hotel.Web.ViewModels;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _repositorio;

    public ReservasController(ReservaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: Reservas
    public async Task<IActionResult> Index()
    {
        var reservas = await _repositorio.ObtenerTodasAsync();
        return View(reservas);
    }

    // GET: Reservas/Disponibles
    public async Task<IActionResult> Disponibles(DateOnly? fechaIngreso, DateOnly? fechaSalida)
    {
        var model = new DisponibilidadViewModel
        {
            FechaIngreso = fechaIngreso,
            FechaSalida = fechaSalida
        };

        if (fechaIngreso.HasValue && fechaSalida.HasValue)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            if (fechaIngreso.Value < hoy)
            {
                ModelState.AddModelError("FechaIngreso", "La fecha de ingreso no puede ser anterior a hoy.");
            }

            if (fechaSalida.Value <= fechaIngreso.Value)
            {
                ModelState.AddModelError("FechaSalida", "La fecha de salida debe ser posterior a la fecha de ingreso.");
            }

            if (ModelState.IsValid)
            {
                int noches = fechaSalida.Value.DayNumber - fechaIngreso.Value.DayNumber;
                model.Noches = noches;

                // Consulta resuelta en el repositorio con una sola consulta LINQ (Any / !Any)
                var habitaciones = await _repositorio.ObtenerDisponiblesAsync(fechaIngreso.Value, fechaSalida.Value);
                model.Habitaciones = habitaciones.Select(h => new HabitacionDisponibleItem
                {
                    Habitacion = h,
                    Noches = noches
                }).ToList();
            }
        }

        return View(model);
    }

    // GET: Reservas/Create
    public async Task<IActionResult> Create(int? idHabitacion, DateOnly? fechaIngreso, DateOnly? fechaSalida)
    {
        await CargarListasDesplegablesAsync(idHabitacion);

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var reserva = new Reserva
        {
            FechaIngreso = fechaIngreso ?? hoy,
            FechaSalida = fechaSalida ?? hoy.AddDays(1),
            IdHabitacion = idHabitacion ?? 0
        };

        return View(reserva);
    }

    // POST: Reservas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        // Limpiar propiedades de navegación generadas para evitar que ModelState.IsValid falle
        ModelState.Remove(nameof(reserva.IdHabitacionNavigation));
        ModelState.Remove(nameof(reserva.IdHuespedNavigation));

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Validaciones de negocio especificadas en el enunciado
        if (reserva.FechaIngreso < hoy)
        {
            ModelState.AddModelError("FechaIngreso", "La fecha de ingreso no puede ser anterior a hoy.");
        }

        if (reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError("FechaSalida", "La fecha de salida debe ser posterior a la fecha de ingreso.");
        }

        if (reserva.FechaSalida > reserva.FechaIngreso && reserva.IdHabitacion > 0)
        {
            bool disponible = await _repositorio.HabitacionEstaDisponibleAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida);
            if (!disponible)
            {
                ModelState.AddModelError("IdHabitacion", "La habitación no está disponible en el rango de fechas seleccionado.");
            }
        }

        if (ModelState.IsValid)
        {
            await _repositorio.CrearAsync(reserva);
            TempData["Mensaje"] = "Reserva registrada correctamente con estado Pendiente.";
            return RedirectToAction(nameof(Index));
        }

        await CargarListasDesplegablesAsync(reserva.IdHabitacion, reserva.IdHuesped);
        return View(reserva);
    }

    // POST: Reservas/Cancelar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        bool cancelado = await _repositorio.CancelarAsync(id);
        if (cancelado)
        {
            TempData["Mensaje"] = $"La reserva #{id} ha sido cancelada exitosamente.";
        }
        else
        {
            TempData["Error"] = $"No se pudo cancelar la reserva #{id}. Solo reservas Pendientes o Confirmadas pueden cancelarse.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasDesplegablesAsync(int? idHabitacionSeleccionada = null, int? idHuespedSeleccionado = null)
    {
        var huespedes = await _repositorio.ObtenerHuespedesActivosAsync();
        var habitaciones = await _repositorio.ObtenerHabitacionesActivasAsync();

        ViewBag.IdHuesped = new SelectList(
            huespedes.Select(h => new { h.IdHuesped, NombreCompleto = $"{h.Apellidos}, {h.Nombres} (DNI: {h.Dni})" }),
            "IdHuesped",
            "NombreCompleto",
            idHuespedSeleccionado);

        ViewBag.IdHabitacion = new SelectList(
            habitaciones.Select(h => new { h.IdHabitacion, Descripcion = $"Hab. {h.Numero} - Piso {h.Piso} ({h.IdTipoHabitacionNavigation.Nombre} - S/. {h.IdTipoHabitacionNavigation.PrecioNoche}/noche)" }),
            "IdHabitacion",
            "Descripcion",
            idHabitacionSeleccionada);
    }
}
