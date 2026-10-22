using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Hotel.Web.Models;
using Hotel.Web.Repositorios;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _repositorio;

    public HabitacionesController(HabitacionRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: Habitaciones
    public async Task<IActionResult> Index(int? idTipoHabitacion)
    {
        var habitaciones = await _repositorio.ObtenerActivasAsync(idTipoHabitacion);
        var tipos = await _repositorio.ObtenerTiposActivosAsync();

        ViewBag.Tipos = new SelectList(tipos, "IdTipoHabitacion", "Nombre", idTipoHabitacion);
        ViewBag.IdTipoSeleccionado = idTipoHabitacion;

        return View(habitaciones);
    }

    // GET: Habitaciones/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var habitacion = await _repositorio.ObtenerPorIdAsync(id.Value);
        if (habitacion == null || !habitacion.Activo) return NotFound();

        return View(habitacion);
    }

    // GET: Habitaciones/Create
    public async Task<IActionResult> Create()
    {
        await CargarTiposHabitacionDropDownListAsync();
        return View(new Habitacion { Activo = true });
    }

    // POST: Habitaciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        // Regla: Si ModelState.IsValid falla por propiedades de navegación, quitarlas
        ModelState.Remove(nameof(habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(habitacion.Reservas));

        if (await _repositorio.ExisteNumeroAsync(habitacion.Numero))
        {
            ModelState.AddModelError("Numero", "El número de habitación ya existe.");
        }

        if (ModelState.IsValid)
        {
            await _repositorio.AgregarAsync(habitacion);
            TempData["Mensaje"] = "Habitación creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        await CargarTiposHabitacionDropDownListAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    // GET: Habitaciones/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var habitacion = await _repositorio.ObtenerParaModificarAsync(id.Value);
        if (habitacion == null || !habitacion.Activo) return NotFound();

        await CargarTiposHabitacionDropDownListAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    // POST: Habitaciones/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion) return NotFound();

        ModelState.Remove(nameof(habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(habitacion.Reservas));

        if (await _repositorio.ExisteNumeroAsync(habitacion.Numero, habitacion.IdHabitacion))
        {
            ModelState.AddModelError("Numero", "El número de habitación ya existe.");
        }

        if (ModelState.IsValid)
        {
            habitacion.Activo = true; // Mantener activa
            await _repositorio.ActualizarAsync(habitacion);
            TempData["Mensaje"] = "Habitación actualizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        await CargarTiposHabitacionDropDownListAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    // GET: Habitaciones/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var habitacion = await _repositorio.ObtenerPorIdAsync(id.Value);
        if (habitacion == null || !habitacion.Activo) return NotFound();

        return View(habitacion);
    }

    // POST: Habitaciones/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repositorio.EliminarLogicoAsync(id);
        TempData["Mensaje"] = "Habitación desactivada (eliminación lógica) exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposHabitacionDropDownListAsync(int? seleccionado = null)
    {
        var tipos = await _repositorio.ObtenerTiposActivosAsync();
        ViewBag.IdTipoHabitacion = new SelectList(tipos, "IdTipoHabitacion", "Nombre", seleccionado);
    }
}
