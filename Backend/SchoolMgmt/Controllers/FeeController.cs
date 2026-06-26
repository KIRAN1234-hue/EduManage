using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Fee;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/fees")]
[Authorize]
public class FeeController : ControllerBase
{
    private readonly IFeeService _feeService;

    public FeeController(IFeeService feeService)
    {
        _feeService = feeService;
    }

    // POST /api/fees/structures
    [HttpPost("structures")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> CreateFeeStructure(
        [FromBody] CreateFeeStructureDto dto)
    {
        try
        {
            var result = await _feeService.CreateFeeStructureAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/fees/structures?academicYear=2024-25
    [HttpGet("structures")]
    [Authorize(Roles = "Principal,Parent")]
    public async Task<IActionResult> GetFeeStructures(
        [FromQuery] string academicYear = "2024-25")
    {
        var structures = await _feeService.GetFeeStructuresAsync(academicYear);
        return Ok(structures);
    }

    // GET /api/fees/structures/class/{classId}?academicYear=2024-25
    [HttpGet("structures/class/{classId}")]
    [Authorize(Roles = "Principal,Teacher,Student")]
    public async Task<IActionResult> GetClassFeeStructures(
        Guid classId,
        [FromQuery] string academicYear = "2024-25")
    {
        var structures = await _feeService
            .GetClassFeeStructuresAsync(classId, academicYear);
        return Ok(structures);
    }

    // POST /api/fees/payments
    [HttpPost("payments")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        try
        {
            var result = await _feeService.RecordPaymentAsync(dto, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/fees/payments/structure/{structureId}
    [HttpGet("payments/structure/{feeStructureId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> GetPaymentsByStructure(Guid feeStructureId)
    {
        var payments = await _feeService.GetPaymentsByStructureAsync(feeStructureId);
        return Ok(payments);
    }

    // GET /api/fees/student/{studentId}/status?academicYear=2024-25
    [HttpGet("student/{studentId}/status")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetStudentFeeStatus(
        Guid studentId,
        [FromQuery] string academicYear = "2024-25")
    {
        try
        {
            var status = await _feeService.GetStudentFeeStatusAsync(studentId, academicYear);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/fees/my-status?academicYear=2024-25
    [HttpGet("my-status")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyFeeStatus(
        [FromQuery] string academicYear = "2024-25")
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        try
        {
            var status = await _feeService.GetMyFeeStatusAsync(userId, academicYear);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    // GET /api/fees/my-child-status — Parent sees their child's fee status
    [HttpGet("my-child-status")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> GetMyChildFeeStatus(
        [FromQuery] string academicYear = "2024-25")
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        try
        {
            var status = await _feeService.GetMyChildFeeStatusAsync(userId, academicYear);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // POST /api/fees/parent-payment — Parent pays for their child
    [HttpPost("parent-payment")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> ParentPayment([FromBody] ParentPaymentDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        try
        {
            var result = await _feeService.ParentPaymentAsync(dto, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("balance")]
    [Authorize(Roles = "Principal,Teacher,Parent")]
    public async Task<IActionResult> GetStudentBalance(
    [FromQuery] Guid studentId,
    [FromQuery] Guid feeStructureId)
    {
        var balance = await _feeService
            .GetStudentBalanceAsync(studentId, feeStructureId);

        if (balance == null)
            return NotFound(new { message = "Fee structure not found." });

        return Ok(balance);
    }   
}