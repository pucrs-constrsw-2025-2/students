using System;
using System.Collections.Generic;

namespace Students.Domain.Entities
{
    public class Student
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Enrollment { get; set; }
        public string Email { get; set; }
        public string CourseCurriculum { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
        public List<Guid> Classes { get; set; } = new List<Guid>();
    }
}
