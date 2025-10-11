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

3. **마이그레이션 적용** (향후 추가 예정)
```bash
dotnet ef database update
```

4. **서버 실행**
```bash
dotnet run
```

- 서버 주소: `http://localhost:5109`
- Swagger UI: `http://localhost:5109/swagger` (개발 환경)

## API 엔드포인트 (계획)

### Runs (시뮬레이션 세션)

| Method | Endpoint | 설명 |
|--------|----------|------|
| POST | `/api/runs` | 새 시뮬레이션 생성 |
| GET | `/api/runs/{id}` | Run 상세 조회 |
| GET | `/api/runs` | Run 목록 조회 |
| PATCH | `/api/runs/{id}/status` | 상태 업데이트 |
| GET | `/api/runs/{id}/results.csv` | CSV 다운로드 |

### Jobs (작업)

| Method | Endpoint | 설명 |
|--------|----------|------|
| POST | `/api/jobs/batch` | 작업 일괄 생성 |
| GET | `/api/jobs?runId={id}` | Run별 작업 조회 |
| GET | `/api/jobs/{id}` | 작업 상세 조회 |
| PATCH | `/api/jobs/{id}/result` | 작업 결과 업데이트 |

### Books (도서)

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/books` | 도서 목록 |
| GET | `/api/books/{id}` | 도서 상세 조회 |
| POST | `/api/books` | 도서 생성 |
| PUT | `/api/books/{id}` | 도서 수정 |
| DELETE | `/api/books/{id}` | 도서 삭제 |

## 데이터 모델

### Run (시뮬레이션 세션)

```csharp
{
  "id": "guid",
  "randomSeed": 42,
  "handleTimeSec": 2.0,
  "robotSpeedCellsPerSec": 3.0,
  "topN": 3,
  "status": "Pending",
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
  "result": "Success",
  "totalTimeSec": 12.5
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
  "sku": "BK-001"
}
```

## 프로젝트 구조

```
ShelfSimAPI/
├── Controllers/           # API 컨트롤러 (향후 추가)
├── Models/               # 데이터 모델
│   ├── Run.cs
│   ├── Job.cs
│   └── Book.cs
├── Data/                 # DbContext (향후 추가)
├── DTOs/                 # DTO (향후 추가)
├── Services/             # 비즈니스 로직 (향후 추가)
├── Properties/
│   └── launchSettings.json
├── appsettings.Example.json
├── Program.cs
└── ShelfSimAPI.csproj
```

## 개발 계획

- [x] 프로젝트 초기 설정
- [x] 데이터 모델 정의
- [ ] DbContext 구현
- [ ] API 컨트롤러 구현
- [ ] 비즈니스 로직 구현
- [ ] CSV 내보내기 기능
- [ ] 테스트 작성

## 라이선스

MIT
