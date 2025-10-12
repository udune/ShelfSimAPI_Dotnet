# ShelfSimAPI

ShelfSim 시뮬레이션 결과를 저장하고 관리하는 ASP.NET Core Web API

## 기술 스택

- .NET 9.0
- Entity Framework Core 9.0
- SQL Server
- OpenAPI/Swagger

## 시작하기

### 필수 조건

- .NET 9.0 SDK
- SQL Server (LocalDB 또는 Express)

### 설치 및 실행

1. **패키지 복원**
```bash
dotnet restore
```

2. **데이터베이스 연결 설정**

`appsettings.json` 파일을 생성하고 연결 문자열을 설정합니다:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ShelfSimDB;UserId=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False;"
  }
}
```

3. **서버 실행** (마이그레이션 자동 적용)
```bash
dotnet run
```

- 서버 주소: `http://localhost:5109`
- Swagger UI: `http://localhost:5109/swagger`

## 데이터 모델

### Run (시뮬레이션 세션)

```csharp
{
  "id": "guid",
  "layoutId": "guid",
  "randomSeed": 42,
  "handleTimeSec": 2.0,
  "robotSpeedCellsPerSec": 3.0,
  "topN": 3,
  "status": "Pending",
  "summary": "...",
  "createdAt": "2025-01-08T10:30:00Z"
}
```

### Job (작업)

```csharp
{
  "id": "guid",
  "runId": "guid",
  "action": "PUT",
  "cellCode": "D20",
  "bookTitle": "Clean Code",
  "quantity": 1,
  "startTs": "2025-01-08T10:30:00Z",
  "endTs": "2025-01-08T10:30:12Z",
  "travelTimeSec": 10.0,
  "handleTimeSec": 2.0,
  "totalTimeSec": 12.0,
  "pathLengthCells": 30,
  "result": "Success",
  "failReason": null,
  "robotName": "Alpha"
}
```

### Book (도서)

```csharp
{
  "id": "guid",
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "thicknessMn": 30,
  "heightMm": 210,
  "sku": "BK-001",
  "createdAt": "2025-01-08T10:30:00Z"
}
```

## 프로젝트 구조

```
ShelfSimAPI/
├── Controllers/
│   └── WeatherForecastController.cs   # 샘플 컨트롤러
├── Models/
│   ├── Run.cs                         # 시뮬레이션 세션 모델
│   ├── Job.cs                         # 작업 모델
│   └── Book.cs                        # 도서 모델
├── Data/
│   └── AppDbContext.cs                # EF Core DbContext
├── Properties/
│   └── launchSettings.json            # 실행 설정
├── appsettings.Example.json           # 연결 문자열 예시
├── Program.cs                         # 진입점
├── ShelfSimAPI.csproj                 # 프로젝트 파일
└── README.md
```

## 현재 구현 상태

### ✅ 완료
- 프로젝트 초기 설정
- 데이터 모델 정의 (Run, Job, Book)
- DbContext 구현
- 자동 마이그레이션 적용
- CORS 설정 (Unity 연동 대비)
- Swagger UI 설정

### 🔄 진행 예정
- API 컨트롤러 구현
  - RunsController
  - JobsController  
  - BooksController
- CSV 내보내기 기능
- 비즈니스 로직 구현
- 테스트 작성

## 개발 노트

- 앱 시작 시 자동으로 마이그레이션 적용
- CORS 정책 "AllowUnity"로 Unity 클라이언트 연동 준비
- 개발 환경에서만 Swagger UI 활성화
- HTTPS 리디렉션 기본 활성화

## 라이선스

MIT
