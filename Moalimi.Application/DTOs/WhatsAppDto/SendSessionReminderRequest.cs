namespace Moalimi.Application.DTOs.WhatsAppDto
{
    public record SendSessionReminderRequest(
    string PhoneNumber,
    string StudentName,
    DateTime SessionTime,
    string TeacherName
    );
}
