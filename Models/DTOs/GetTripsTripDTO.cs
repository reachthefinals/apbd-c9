namespace apbd_c9.Models.DTOs;

public class GetTripsTripDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string DateFrom { get; set; }
    public string DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<TripCountryDTO> Countries { get; set; }
    public List<TripClientDTO> Clients { get; set; }
}