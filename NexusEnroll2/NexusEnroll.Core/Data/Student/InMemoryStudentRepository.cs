namespace NexusEnroll.Core.Data.Student;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Entities.Student> _students = new();

    public Entities.Student GetStudent(string studentId) =>
        _students.FirstOrDefault(s => s.StudentId == studentId)!;

    public void AddStudent(Entities.Student student) =>
        _students.Add(student);
}