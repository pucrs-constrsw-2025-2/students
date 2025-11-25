using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Students.Application.DTOs;
using Students.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Students.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/students/{studentId}/phone-numbers")]
    public class PhoneNumbersController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public PhoneNumbersController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoneNumber(
            [FromRoute] Guid studentId,
            [FromBody] PhoneNumberDto phoneNumberDto)
        {
            try
            {
                var phoneNumber = await _studentService.AddPhoneNumberAsync(studentId, phoneNumberDto);
                
                // Get the index of the newly added phone number
                var phoneNumbers = await _studentService.GetPhoneNumbersAsync(studentId);
                var phoneNumberList = phoneNumbers.ToList();
                var phoneNumberIndex = phoneNumberList.Count - 1;
                
                return CreatedAtAction(
                    nameof(GetPhoneNumber),
                    new { studentId, phoneNumberIndex },
                    phoneNumber);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPhoneNumbers([FromRoute] Guid studentId)
        {
            try
            {
                var phoneNumbers = await _studentService.GetPhoneNumbersAsync(studentId);
                return Ok(phoneNumbers);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{phoneNumberIndex}")]
        public async Task<IActionResult> GetPhoneNumber(
            [FromRoute] Guid studentId,
            [FromRoute] int phoneNumberIndex)
        {
            try
            {
                var phoneNumber = await _studentService.GetPhoneNumberAsync(studentId, phoneNumberIndex);
                return Ok(phoneNumber);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentOutOfRangeException)
            {
                return NotFound();
            }
        }

        [HttpPut("{phoneNumberIndex}")]
        public async Task<IActionResult> UpdatePhoneNumber(
            [FromRoute] Guid studentId,
            [FromRoute] int phoneNumberIndex,
            [FromBody] PhoneNumberDto phoneNumberDto)
        {
            try
            {
                var phoneNumber = await _studentService.UpdatePhoneNumberAsync(studentId, phoneNumberIndex, phoneNumberDto);
                return Ok(phoneNumber);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentOutOfRangeException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{phoneNumberIndex}")]
        public async Task<IActionResult> PatchPhoneNumber(
            [FromRoute] Guid studentId,
            [FromRoute] int phoneNumberIndex,
            [FromBody] PhoneNumberDto phoneNumberDto)
        {
            try
            {
                // For PATCH, we only update provided fields
                var existingPhoneNumber = await _studentService.GetPhoneNumberAsync(studentId, phoneNumberIndex);
                
                var updatedPhoneNumber = new PhoneNumberDto
                {
                    Ddd = phoneNumberDto.Ddd != 0 ? phoneNumberDto.Ddd : existingPhoneNumber.Ddd,
                    Number = phoneNumberDto.Number != 0 ? phoneNumberDto.Number : existingPhoneNumber.Number,
                    Description = phoneNumberDto.Description ?? existingPhoneNumber.Description
                };

                var phoneNumber = await _studentService.UpdatePhoneNumberAsync(studentId, phoneNumberIndex, updatedPhoneNumber);
                return Ok(phoneNumber);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentOutOfRangeException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{phoneNumberIndex}")]
        public async Task<IActionResult> DeletePhoneNumber(
            [FromRoute] Guid studentId,
            [FromRoute] int phoneNumberIndex)
        {
            try
            {
                await _studentService.DeletePhoneNumberAsync(studentId, phoneNumberIndex);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentOutOfRangeException)
            {
                return NotFound();
            }
        }
    }
}

