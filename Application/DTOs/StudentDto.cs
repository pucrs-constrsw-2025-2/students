using System;
using System.Collections.Generic;

namespace Students.Application.DTOs
{
    public class StudentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Enrollment { get; set; }
        public string Email { get; set; }
        public string CourseCurriculum { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
        public List<Guid> Classes { get; set; }
    }

    public class PhoneNumberDto
    {
        public int Ddd { get; set; }
        public int Number { get; set; }
        public string Description { get; set; }
    }
}
