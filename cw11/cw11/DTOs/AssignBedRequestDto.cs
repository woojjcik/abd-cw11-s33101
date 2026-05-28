namespace cw11.DTOs;

public class AssignBedRequestDto 
{ 
    public int BedTypeId { get; set; }
    public int WardId { get; set; }
    public DateTime From { get; set; }
    public DateTime? To { get; set; }
 }
