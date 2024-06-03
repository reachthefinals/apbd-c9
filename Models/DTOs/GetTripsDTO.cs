namespace apbd_c9.Models.DTOs;

public class GetTripsDTO
{
    public int pageNum { get; set; }
    public int pageSize { get; set; }
    public int allPages { get; set; }
    public List<GetTripsTripDTO> trips { get; set; }
}