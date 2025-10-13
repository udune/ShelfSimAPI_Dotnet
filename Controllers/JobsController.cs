using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

// JobsController 클래스 정의
[ApiController] // API 컨트롤러 지정
[Route("api/[controller]")] // 기본 라우트 설정
public class JobsController(AppDbContext context, ILogger<JobsController> logger): ControllerBase // ControllerBase 상속
{
    // CreateBatch: 여러 작업을 한 번에 생성하는 엔드포인트
    [HttpPost("batch")] // POST /api/jobs/batch
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<ActionResult<object>> CreateBatch([FromBody] CreateJobsBatchDto dto) // CreateBatch 메서드 정의
    {
        logger.LogInformation("Creating batch of Jobs for RunId: {RunId}", dto.RunId);

        var runExist = await context.Runs.AnyAsync(run => run.Id == dto.RunId); // Run 조회
        if (!runExist) // Run이 없으면
        {
            logger.LogWarning("Run not found: {RunId}", dto.RunId); // 로그 경고
            return NotFound(new {error = "Run not found"}); // 404 응답 반환
        }

        var jobs = dto.Jobs.Select(job => new Job // 새로운 Job 객체 생성
        {
            RunId = dto.RunId, // RunId 설정
            Action = job.Action.ToUpper(), // Action 설정 (대문자 변환)
            CellCode = job.CellCode, // CellCode 설정
            BookTitle = job.BookTitle, // BookTitle 설정
            Quantity = job.Quantity // Quantity 설정
        }).ToList(); // 리스트로 변환

        context.Jobs.AddRange(jobs); // DbContext에 추가
        await context.SaveChangesAsync(); // 비동기 저장

        logger.LogInformation("Created {JobCount} jobs for RunId: {RunId}", jobs.Count, dto.RunId);

        return Ok(new
        {
            accepted = jobs.Count, // 생성된 작업 수
            runId = dto.RunId, // RunId
            jobIds = jobs.Select(job => job.RunId) // 생성된 Job들의 RunId 리스트
        }); // 200 응답 반환
    }

    // GetJobsByRun: 특정 Run에 속한 모든 작업을 조회하는 엔드포인트
    [HttpGet] // GET /api/jobs?runId={runId}
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    public async Task<ActionResult<List<Job>>> GetJobsByRun([FromQuery] int runId) // GetJobsByRun 메서드 정의
    {
        var jobs = await context.Jobs // Job 엔티티에서
            .Where(job => job.RunId == runId) // 특정 RunId에 속한 Job 필터링
            .OrderBy(job => job.StartTs ?? DateTime.MaxValue) // 시작 시간 기준 오름차순 정렬 (null은 마지막에 위치)
            .ToListAsync(); // 특정 RunId에 속한 모든 Job 조회 및 정렬

        return Ok(jobs); // 200 응답 반환
    }
    
    // UpdateJobStatus: 특정 작업의 상태를 업데이트하는 엔드포인트
    [HttpPatch("{id}/result")] // PATCH /api/jobs/{id}/result
    [ProducesResponseType(StatusCodes.Status204NoContent)] // 204 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateJobStatusDto dto) // UpdateResult 메서드 정의
    {
        var job = await context.Jobs.FindAsync(id); // Job 조회
        if (job == null) // Job이 없으면
        {
            logger.LogWarning("Job not found: {JobId}", id); // 로그 경고
            return NotFound(new {error = "Job not found"}); // 404 응답 반환
        }

        // DTO의 각 필드가 null이 아닌 경우에만 업데이트
        if (dto.StartTs.HasValue) job.StartTs = dto.StartTs;
        if (dto.EndTs.HasValue) job.EndTs = dto.EndTs;
        if (dto.TravelTimeSec.HasValue) job.TravelTimeSec = dto.TravelTimeSec;
        if (dto.HandleTimeSec.HasValue) job.HandleTimeSec = dto.HandleTimeSec;
        if (dto.TotalTimeSec.HasValue) job.TotalTimeSec = dto.TotalTimeSec;
        if (dto.PathLengthCells.HasValue) job.PathLengthCells = dto.PathLengthCells;
        if (!string.IsNullOrEmpty(dto.Result)) job.Result = dto.Result;
        if (!string.IsNullOrEmpty(dto.FailReason)) job.FailReason = dto.FailReason;
        if (!string.IsNullOrEmpty(dto.RobotName)) job.RobotName = dto.RobotName;

        await context.SaveChangesAsync(); // 비동기 저장

        logger.LogInformation("Updated Job: {JobId}", id);

        return NoContent(); // 204 응답 반환
    }
    
    // GetJobById: 특정 작업을 ID로 조회하는 엔드포인트
    [HttpGet("{id}")] // GET /api/jobs/{id}
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<ActionResult<Job>> GetJobById(int id) // GetJobById 메서드 정의
    {
        var job = await context.Jobs
            .Include(job => job.Run)
            .FirstOrDefaultAsync(job => job.Id == id); // Job 조회
        if (job == null) // Job이 없으면
        {
            logger.LogWarning("Job not found: {JobId}", id); // 로그 경고
            return NotFound(new {error = "Job not found"}); // 404 응답 반환
        }

        return job; // 200 응답 반환
    }

    // NormalizeCellCode: 셀 코드를 표준 형식으로 정규화하는 유틸리티 메서드
    private string NormalizeCellCode(string code)
    {
        if (string.IsNullOrEmpty(code)) // 빈 문자열 또는 null인 경우
        {
            return code; // 그대로 반환
        }

        code = code.Replace(" ", "").Replace("-", "").Replace("_", ""); // 공백, 대시, 언더스코어 제거

        code = code.ToUpper(); // 대문자로 변환

        if (code.Length >= 2 && char.IsLetter(code[0])) // 첫 글자가 문자이고 길이가 2 이상인 경우
        {
            var letter = code[0]; // 첫 글자 추출
            var numberPart = code.Substring(1); // 숫자 부분 추출
            
            if (int.TryParse(numberPart, out int number)) // 숫자 부분이 유효한지 확인
            {
                return $"{letter}{number:D2}"; // 숫자를 두 자리로 포맷팅
            }
        }

        return code; // 정규화된 코드 반환
    }
}