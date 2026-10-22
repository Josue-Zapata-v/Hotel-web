using Hotel.Web.Models;

namespace Hotel.Web.ViewModels;

public class DisponibilidadViewModel
{
    public DateOnly? FechaIngreso { get; set; }
    public DateOnly? FechaSalida { get; set; }
    public int Noches { get; set; }
    public List<HabitacionDisponibleItem> Habitaciones { get; set; } = new();
}

public class HabitacionDisponibleItem
{
    public Habitacion Habitacion { get; set; } = null!;
    public int Noches { get; set; }
    public decimal TotalEstimado => Noches * (Habitacion.IdTipoHabitacionNavigation?.PrecioNoche ?? 0m);
}
