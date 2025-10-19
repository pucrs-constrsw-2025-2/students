using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Students.Application.DTOs;
using Students.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace Students.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto createStudentDto)
        {
            var student = await _studentService.CreateStudentAsync(createStudentDto);
            return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(Guid id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents([FromQuery] string name, [FromQuery] string enrollment, [FromQuery] string email)
        {
            var students = await _studentService.GetAllStudentsAsync(name, enrollment, email);
            return Ok(students);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentDto updateStudentDto)
        {
            await _studentService.UpdateStudentAsync(id, updateStudentDto);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchStudent(Guid id, [FromBody] JsonPatchDocument<UpdateStudentDto> patchDoc)
        {
            // A more complete implementation would be needed here
            // For now, this is a placeholder
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var studentToUpdate = await _studentService.GetStudentByIdAsync(id);
            if (studentToUpdate == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateStudentDto(); // map from studentToUpdate
            patchDoc.ApplyTo(updateDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            await _studentService.UpdateStudentAsync(id, updateDto);


            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            await _studentService.DeleteStudentAsync(id);
            return NoContent();
        }
    }
}
