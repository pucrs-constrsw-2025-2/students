using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Students.Application.DTOs;
using Students.Application.Interfaces;
using Students.Domain.Entities;
using Students.Domain.Interfaces;

namespace Students.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto)
        {
            var student = new Student
            {
                Id = Guid.NewGuid(),
                Name = createStudentDto.Name,
                Enrollment = createStudentDto.Enrollment,
                Email = createStudentDto.Email,
                CourseCurriculum = createStudentDto.CourseCurriculum,
                Classes = createStudentDto.Classes,
                PhoneNumbers = createStudentDto.PhoneNumbers.Select(p => new PhoneNumber { Ddd = p.Ddd, Number = p.Number, Description = p.Description }).ToList()
            };

            await _studentRepository.AddAsync(student);

            return ToDto(student);
        }

        public async Task<StudentDto> GetStudentByIdAsync(Guid id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            return student == null ? null : ToDto(student);
        }

        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync(string name, string enrollment, string email)
        {
            var students = await _studentRepository.FindAsync(s =>
                (string.IsNullOrEmpty(name) || s.Name.Contains(name)) &&
                (string.IsNullOrEmpty(enrollment) || s.Enrollment == enrollment) &&
                (string.IsNullOrEmpty(email) || s.Email == email)
            );
            return students.Select(ToDto);
        }

        public async Task UpdateStudentAsync(Guid id, UpdateStudentDto updateStudentDto)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null) return;

            student.Name = updateStudentDto.Name ?? student.Name;
            student.Enrollment = updateStudentDto.Enrollment ?? student.Enrollment;
            student.Email = updateStudentDto.Email ?? student.Email;
            student.CourseCurriculum = updateStudentDto.CourseCurriculum ?? student.CourseCurriculum;
            student.Classes = updateStudentDto.Classes ?? student.Classes;
            student.PhoneNumbers = updateStudentDto.PhoneNumbers?.Select(p => new PhoneNumber { Ddd = p.Ddd, Number = p.Number, Description = p.Description }).ToList() ?? student.PhoneNumbers;

            await _studentRepository.UpdateAsync(student);
        }

        public async Task PatchStudentAsync(Guid id, object patchDocument)
        {
            // This requires a bit more setup with JSON Patch, for simplicity we'll do a partial update
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null) return;

            // Logic to apply partial updates from patchDocument to student
            // For a real implementation, use Microsoft.AspNetCore.JsonPatch
            
            await _studentRepository.UpdateAsync(student);
        }


        public async Task DeleteStudentAsync(Guid id)
        {
            await _studentRepository.DeleteAsync(id);
        }

        private StudentDto ToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Enrollment = student.Enrollment,
                Email = student.Email,
                CourseCurriculum = student.CourseCurriculum,
                Classes = student.Classes,
                PhoneNumbers = student.PhoneNumbers.Select(p => new PhoneNumberDto { Ddd = p.Ddd, Number = p.Number, Description = p.Description }).ToList()
            };
        }
    }
}
