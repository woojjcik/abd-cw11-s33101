namespace cw11.DTOs;

public class PatientResponseDto
{
    public string Pesel { get; set; } = String.Empty;
    public string FirstName { get; set; } = String.Empty;
    public string LastName { get; set; } = String.Empty;
    public int Age { get; set; }
    public bool Sex { get; set; }
}