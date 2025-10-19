using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Students.Application.DTOs;

namespace Students.Application.Interfaces
{
    public interface IStudentService
    {
        Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
        Task<StudentDto> GetStudentByIdAsync(Guid id);
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync(string name, string enrollment, string email);
        Task UpdateStudentAsync(Guid id, UpdateStudentDto updateStudentDto);
        Task PatchStudentAsync(Guid id, object patchDocument);
        Task DeleteStudentAsync(Guid id);
    }
}
