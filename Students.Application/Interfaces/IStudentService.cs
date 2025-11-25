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
        
        // Phone Numbers methods
        Task<PhoneNumberDto> AddPhoneNumberAsync(Guid studentId, PhoneNumberDto phoneNumber);
        Task<IEnumerable<PhoneNumberDto>> GetPhoneNumbersAsync(Guid studentId);
        Task<PhoneNumberDto> GetPhoneNumberAsync(Guid studentId, int phoneNumberIndex);
        Task<PhoneNumberDto> UpdatePhoneNumberAsync(Guid studentId, int phoneNumberIndex, PhoneNumberDto phoneNumber);
        Task DeletePhoneNumberAsync(Guid studentId, int phoneNumberIndex);
    }
}
