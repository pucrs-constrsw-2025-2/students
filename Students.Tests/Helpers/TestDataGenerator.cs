using Bogus;
using Students.Application.DTOs;
using Students.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Students.Tests.Helpers
{
    public static class TestDataGenerator
    {
        private static readonly Faker _faker = new Faker();

        public static Student GenerateStudent(bool withId = true)
        {
            return new Student
            {
                Id = withId ? Guid.NewGuid() : Guid.Empty,
                Name = _faker.Name.FullName(),
                Enrollment = _faker.Random.AlphaNumeric(8).ToUpper(),
                Email = _faker.Internet.Email(),
                CourseCurriculum = _faker.PickRandom("Engenharia de Software", "Ciência da Computação", "Sistemas de Informação"),
                PhoneNumbers = new List<PhoneNumber>
                {
                    new PhoneNumber
                    {
                        Ddd = _faker.Random.Int(11, 99),
                        Number = _faker.Random.Int(900000000, 999999999),
                        Description = _faker.PickRandom("Celular", "Casa", "Trabalho")
                    }
                },
                Classes = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };
        }

        public static CreateStudentDto GenerateCreateStudentDto()
        {
            return new CreateStudentDto
            {
                Name = _faker.Name.FullName(),
                Enrollment = _faker.Random.AlphaNumeric(8).ToUpper(),
                Email = _faker.Internet.Email(),
                CourseCurriculum = _faker.PickRandom("Engenharia de Software", "Ciência da Computação", "Sistemas de Informação"),
                PhoneNumbers = new List<PhoneNumberDto>
                {
                    new PhoneNumberDto
                    {
                        Ddd = _faker.Random.Int(11, 99),
                        Number = _faker.Random.Int(900000000, 999999999),
                        Description = _faker.PickRandom("Celular", "Casa", "Trabalho")
                    }
                },
                Classes = new List<Guid> { Guid.NewGuid() }
            };
        }

        public static UpdateStudentDto GenerateUpdateStudentDto()
        {
            return new UpdateStudentDto
            {
                Name = _faker.Name.FullName(),
                Enrollment = _faker.Random.AlphaNumeric(8).ToUpper(),
                Email = _faker.Internet.Email(),
                CourseCurriculum = _faker.PickRandom("Engenharia de Software", "Ciência da Computação"),
                PhoneNumbers = new List<PhoneNumberDto>
                {
                    new PhoneNumberDto
                    {
                        Ddd = _faker.Random.Int(11, 99),
                        Number = _faker.Random.Int(900000000, 999999999),
                        Description = "Celular Atualizado"
                    }
                },
                Classes = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };
        }

        public static List<Student> GenerateStudents(int count)
        {
            var students = new List<Student>();
            for (int i = 0; i < count; i++)
            {
                students.Add(GenerateStudent());
            }
            return students;
        }
    }
}
