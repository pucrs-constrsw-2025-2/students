using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Students.Application.DTOs
{
    public class CreateStudentDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Enrollment { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string CourseCurriculum { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; } = new List<PhoneNumberDto>();
        [Required]
        [MinLength(1)]
        public List<Guid> Classes { get; set; } = new List<Guid>();
    }
}
