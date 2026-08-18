namespace NexusEnroll.Core.Data.Student;

public interface IStudentRepository
{
    Entities.Student GetStudent(string studentId);
    void AddStudent(Entities.Student student);
}