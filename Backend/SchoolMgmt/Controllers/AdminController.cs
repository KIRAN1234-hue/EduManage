using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Admin;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }


    // POST /api/admin/classes
    [HttpPost("classes")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassDto dto)
    {
        try
        {
            var result = await _adminService.CreateClassAsync(dto);
            return CreatedAtAction(nameof(GetClassById), new { classId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/admin/classes
    [HttpGet("classes")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetAllClasses()
    {
        var classes = await _adminService.GetAllClassesAsync();
        return Ok(classes);
    }

    // GET /api/admin/classes/{classId}
    [HttpGet("classes/{classId}")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetClassById(Guid classId)
    {
        try
        {
            var cls = await _adminService.GetClassByIdAsync(classId);
            return Ok(cls);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // POST /api/admin/subjects
    [HttpPost("subjects")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto dto)
    {
        try
        {
            var result = await _adminService.CreateSubjectAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/admin/subjects
    [HttpGet("subjects")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetAllSubjects()
    {
        var subjects = await _adminService.GetAllSubjectsAsync();
        return Ok(subjects);
    }

    // GET /api/admin/subjects/class/{classId}
    [HttpGet("subjects/class/{classId}")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetSubjectsByClass(Guid classId)
    {
        var subjects = await _adminService.GetSubjectsByClassAsync(classId);
        return Ok(subjects);
    }

    // POST /api/admin/teachers
    [HttpPost("teachers")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherDto dto)
    {
        try
        {
            var result = await _adminService.CreateTeacherAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/admin/teachers
    [HttpGet("teachers")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _adminService.GetAllTeachersAsync();
        return Ok(teachers);
    }

    // POST /api/admin/students
    [HttpPost("students")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
    {
        try
        {
            var result = await _adminService.CreateStudentAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/admin/students
    [HttpGet("students")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _adminService.GetAllStudentsAsync();
        return Ok(students);
    }

    // GET /api/admin/students/class/{classId}
    [HttpGet("students/class/{classId}")]
    [Authorize(Roles = "Principal,Teacher,Parent")]
    public async Task<IActionResult> GetStudentsByClass(Guid classId)
    {
        var students = await _adminService.GetStudentsByClassAsync(classId);
        return Ok(students);
    }

    // PUT /api/admin/users/{userId}/deactivate
    [HttpPut("users/{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        try
        {
            await _adminService.DeactivateUserAsync(userId);
            return Ok(new { message = "User deactivated successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    // DELETE /api/admin/classes/{classId}
    [HttpDelete("classes/{classId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteClass(Guid classId)
    {
        try
        {
            await _adminService.DeleteClassAsync(classId);
            return Ok(new { message = "Class deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/admin/subjects/{subjectId}
    [HttpDelete("subjects/{subjectId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteSubject(Guid subjectId)
    {
        try
        {
            await _adminService.DeleteSubjectAsync(subjectId);
            return Ok(new { message = "Subject deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/admin/teachers/{teacherId}
    [HttpDelete("teachers/{teacherId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteTeacher(Guid teacherId)
    {
        try
        {
            await _adminService.DeleteTeacherAsync(teacherId);
            return Ok(new { message = "Teacher deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/admin/students/{studentId}
    [HttpDelete("students/{studentId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteStudent(Guid studentId)
    {
        try
        {
            await _adminService.DeleteStudentAsync(studentId);
            return Ok(new { message = "Student deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}