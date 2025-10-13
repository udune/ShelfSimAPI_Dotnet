using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

// RunsController 클래스 정의
[ApiController] // API 컨트롤러 지정
[Route("api/[controller]")] // 기본 라우트 설정
public class RunsController(AppDbContext context, ILogger<RunsController> logger) : ControllerBase
{
    [HttpPost] // POST /api/runs
    [ProducesResponseType(StatusCodes.Status201Created)] // 201 응답 타입
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400 응답 타입
    public async Task<ActionResult<Run>> CreateRun([FromBody] CreateRunDto dto) // CreateRun 메서드 정의
    {
        logger.LogInformation("Creating Run");

        var run = new Run // 새로운 Run 객체 생성
        {
            RandomSeed = dto.RandomSeed, // 랜덤 시드 설정
            HandleTimeSec = dto.HandleTimeSec, // 처리 시간 설정
            RobotSpeedCellsPerSec = dto.RobotSpeedCellsPerSec, // 로봇 속도 설정
            TopN = dto.TopN, // TopN 설정
            Status = "PENDING" // 초기 상태 설정
        };
        
        context.Runs.Add(run); // DbContext에 추가
        await context.SaveChangesAsync(); // 비동기 저장
        
        logger.LogInformation("Run created: {RunId}", run.Id);

        return Ok(run); // 200 응답 반환
    }

    [HttpGet("{id}")] // GET /api/runs/{id}
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<ActionResult<Run>> GetRun(int id) // GetRun 메서드 정의
    {
        var run = await context.Runs // Run 엔티티에서
            .Include(run => run.Jobs) // 관련된 Jobs 포함
            .FirstOrDefaultAsync(run => run.Id == id); // ID로 Run 조회

        if (run == null) // Run이 없으면
        {
            logger.LogWarning("Run not found: {RunId}", id); // 로그 경고
            return NotFound(new {error = "Run not found"}); // 404 응답 반환
        }

        return run; // Run 반환
    }

    [HttpGet] // GET /api/runs
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    public async Task<ActionResult<object>> GetRuns(
        [FromQuery] int page = 1, // 페이지 번호 (기본값 1)
        [FromQuery] int pageSize = 20 // 페이지 크기 (기본값 20)
        ) // GetRuns 메서드 정의
    {
        var totalCount = await context.Runs.CountAsync(); // 전체 Run 개수 조회
        var runs = await context.Runs // Run 엔티티에서
            .OrderByDescending(run => run.CreatedAt) // 생성일 내림차순 정렬
            .Skip((page - 1) * pageSize) // 페이지네이션 적용
            .Take(pageSize) // 페이지 크기만큼 조회
            .ToListAsync(); // 비동기 조회

        return Ok(new
        {
            data = runs, // 조회된 Run 데이터
            meta = new // 메타 정보
            {
                page, // 현재 페이지
                pageSize, // 페이지 크기
                totalCount, // 전체 개수
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) // 전체 페이지 수
            }
        }); // 200 응답 반환
    }
    
    [HttpPatch("{id}/status")] // PATCH /api/runs/{id}/status
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto) // UpdateStatus 메서드 정의
    {
        var run = await context.Runs.FindAsync(id); // ID로 Run 조회
        if (run == null) // Run이 없으면
        {
            logger.LogWarning("Run not found: {RunId}", id); // 로그 경고
            return NotFound(new {error = "Run not found"}); // 404 응답 반환
        }

        run.Status = dto.Status; // 상태 업데이트
        await context.SaveChangesAsync(); // 비동기 저장
        
        logger.LogInformation("Run status updated: {RunId} to {Status}", id, dto.Status);
        
        return Ok(run); // 200 응답 반환
    }

    [HttpGet("{id}/results.csv")] // GET /api/runs/{id}/results.csv
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<IActionResult> DownloadCsv(int id) // DownloadCsv 메서드 정의
    {
        var run = await context.Runs.FindAsync(id); // ID로 Run 조회
        if (run == null) // Run이 없으면
        {
            return NotFound(new {error = "Run not found"}); // 404 응답 반환
        }

        var jobs = await context.Jobs // Job 엔티티에서
            .Where(job => job.RunId == id) // 해당 RunId의 Job 조회
            .OrderBy(job => job.StartTs) // 시작 시간으로 정렬
            .ToListAsync(); // 비동기 조회

        var csv = new StringBuilder(); // CSV 문자열 빌더
        csv.Append("\\uFEFF"); // UTF-8 BOM 추가
        csv.AppendLine("JobId,Action,CellCode,BookTitle,Quantity,StartTs,EndTs,TravelTimeSec,HandleTimeSec,TotalTimeSec,PathLengthCells,Result,FailReason,RobotName");
        foreach (var job in jobs) // 각 Job에 대해
        {
            csv.AppendLine(
                $"{job.Id}," +
                $"{EscapeCsv(job.RobotName ?? "")}," +
                $"{job.Action}," +
                $"{job.CellCode}," +
                $"{EscapeCsv(job.BookTitle ?? "")}," +
                $"{job.Quantity}," +
                $"{FormatTimestamp(job.StartTs)}," +
                $"{FormatTimestamp(job.EndTs)}," +
                $"{FormatFloat(job.TravelTimeSec)}," +
                $"{FormatFloat(job.HandleTimeSec)}," +
                $"{FormatFloat(job.TotalTimeSec)}," +
                $"{job.PathLengthCells ?? 0}," +
                $"{job.Result ?? ""}," +
                $"{EscapeCsv(job.FailReason ?? "")},"
                ); // 각 Job 정보를 CSV 형식으로 추가
        }
        
        var bytes = Encoding.UTF8.GetBytes(csv.ToString()); // CSV 문자열을 바이트 배열로 변환
        var timeStamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"); // 타임스탬프 생성
        
        return File(bytes, "text/csv", $"results_{timeStamp}.csv"); // 파일 응답 반환
    }

    private string EscapeCsv(string value) // CSV 값 이스케이프 메서드
    {
        if (string.IsNullOrEmpty(value)) // 값이 null 또는 빈 문자열이면
        {
            return ""; // 빈 문자열 반환
        }

        if (value.Contains(",") || value.Contains("\r") || value.Contains("\n")) // 값에 쉼표, 줄바꿈 등이 포함되어 있으면
        {
            return $"\"{value.Replace("\"", "\"\"")}\""; // 큰따옴표로 감싸고, 내부의 큰따옴표는 두 개로 이스케이프
        }
        
        return value; // 그대로 반환
    }

    private string FormatTimestamp(DateTime? dateTime) // 타임스탬프 포맷 메서드
    {
        if (!dateTime.HasValue) // 값이 null이면
        {
            return ""; // 빈 문자열 반환
        }
        
        // UTC 시간을 "yyyy-MM-dd HH:mm:ss" 형식의 문자열로 변환
        var koreaTime = TimeZoneInfo.ConvertTimeFromUtc(
            dateTime.Value.ToUniversalTime(), 
            TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time")); // 한국 시간대로 변환
        
        return koreaTime.ToString("yyyy-MM-dd HH:mm:ss"); // "YYYY-MM-DD HH:MM:SS" 형식 반환
    }

    // 소수점 둘째 자리까지 포맷 메서드
    private string FormatFloat(float? value)
    {
        if (!value.HasValue) // 값이 null이면
        {
            return ""; // 빈 문자열 반환
        }
        
        return Math.Round(value.Value, 2, MidpointRounding.AwayFromZero).ToString("F2"); // 소수점 둘째 자리까지 반올림하여 문자열로 반환
    }
}