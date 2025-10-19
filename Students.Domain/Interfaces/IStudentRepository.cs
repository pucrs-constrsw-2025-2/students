using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Students.Domain.Entities;

namespace Students.Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task AddAsync(Student student);
        Task<Student> GetByIdAsync(Guid id);
        Task<IEnumerable<Student>> GetAllAsync();
        Task<IEnumerable<Student>> FindAsync(Expression<Func<Student, bool>> predicate);
        Task UpdateAsync(Student student);
        Task DeleteAsync(Guid id);
    }
}
