using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Students.Application.DTOs
{
    public class UpdateStudentDto
    {
        public string Name { get; set; }
        public string Enrollment { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string CourseCurriculum { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
        [MinLength(1)]
        public List<Guid> Classes { get; set; }
    }
}
