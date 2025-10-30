# ShelfSimAPI

ShelfSim 시뮬레이션 결과를 저장하고 관리하는 ASP.NET Core Web API

## 기술 스택

- .NET 9.0
- Entity Framework Core 9.0
- PostgreSQL (Azure PostgreSQL 무료 티어 지원)
- OpenAPI/Swagger

## 시작하기

### 필수 조건

- .NET 9.0 SDK
- PostgreSQL 15+ (로컬 개발) 또는 Azure PostgreSQL (무료 티어)

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
    "DefaultConnection": "Host=localhost;Port=5432;Database=shelfsimdb;Username=postgres;Password=your_password"
  }
}
```

Azure PostgreSQL 사용 시:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-server.postgres.database.azure.com;Port=5432;Database=shelfsimdb;Username=your_username;Password=your_password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

3. **서버 실행** (마이그레이션 자동 적용)
```bash
dotnet run
```

- 서버 주소: `http://localhost:5109`
- Swagger UI: `http://localhost:5109/swagger`

## API 엔드포인트

### Runs (시뮬레이션 세션)

| Method | Endpoint | 설명 |
|--------|----------|------|
| POST | `/api/runs` | 새 시뮬레이션 생성 |
| GET | `/api/runs/{id}` | Run 상세 조회 |
| GET | `/api/runs` | Run 목록 조회 (페이징) |
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
| GET | `/api/books` | 도서 목록 (검색) |
| GET | `/api/books/{id}` | 도서 상세 조회 |
| POST | `/api/books` | 도서 생성 |
| PUT | `/api/books/{id}` | 도서 수정 |
| DELETE | `/api/books/{id}` | 도서 삭제 |

## 사용 예시

### 1. 시뮬레이션 Run 생성
```bash
curl -X POST http://localhost:5109/api/runs \
  -H "Content-Type: application/json" \
  -d '{
    "randomSeed": 42,
    "handleTimeSec": 2.0,
    "robotSpeedCellsPerSec": 3.0,
    "topN": 3
  }'
```

**응답:**
```json
{
  "id": 1,
  "randomSeed": 42,
  "handleTimeSec": 2.0,
  "robotSpeedCellsPerSec": 3.0,
  "topN": 3,
  "status": "PENDING",
  "createdAt": "2025-10-13T10:30:00Z"
}
```

### 2. 작업 일괄 등록
```bash
curl -X POST http://localhost:5109/api/jobs/batch \
  -H "Content-Type: application/json" \
  -d '{
    "runId": 1,
    "jobs": [
      {
        "action": "PUT",
        "cellCode": "D20",
        "bookTitle": "Clean Code",
        "quantity": 1
      },
      {
        "action": "PICK",
        "cellCode": "A15",
        "bookTitle": "Refactoring",
        "quantity": 2
      }
    ]
  }'
```

### 3. CSV 다운로드
```bash
curl -O http://localhost:5109/api/runs/1/results.csv
```

## 데이터 모델

### Run (시뮬레이션 세션)
```csharp
{
  "id": 1,
  "layoutId": null,
  "randomSeed": 42,
  "handleTimeSec": 2.0,
  "robotSpeedCellsPerSec": 3.0,
  "topN": 3,
  "status": "PENDING",
  "summary": null,
  "createdAt": "2025-10-13T10:30:00Z"
}
```

### Job (작업)
```csharp
{
  "id": 1,
  "runId": 1,
  "action": "PUT",
  "cellCode": "D20",
  "bookTitle": "Clean Code",
  "quantity": 1,
  "startTs": "2025-10-13T10:30:00Z",
  "endTs": "2025-10-13T10:30:12Z",
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
  "id": 1,
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "thicknessMn": 30,
  "heightMm": 210,
  "sku": "BK-001",
  "createdAt": "2025-10-13T10:30:00Z"
}
```

## 프로젝트 구조

```
ShelfSimAPI/
├── Controllers/
│   ├── RunsController.cs          # 시뮬레이션 세션 관리
│   ├── JobsController.cs          # 작업 관리
│   └── BooksController.cs         # 도서 관리
├── Models/
│   ├── Run.cs                     # 시뮬레이션 세션 모델
│   ├── Job.cs                     # 작업 모델
│   └── Book.cs                    # 도서 모델
├── DTOs/
│   ├── RunDto.cs                  # Run 요청/응답 DTO
│   └── JobDto.cs                  # Job 요청/응답 DTO
├── Data/
│   └── AppDbContext.cs            # EF Core DbContext
├── Migrations/                    # EF Core 마이그레이션
├── Properties/
│   └── launchSettings.json        # 실행 설정
├── appsettings.Example.json       # 연결 문자열 예시
├── Program.cs                     # 진입점
└── README.md
```

## 주요 기능

### ✅ 완료
- **Run 관리**: 시뮬레이션 세션 생성 및 상태 관리
- **Job 관리**: 작업 일괄 생성 및 결과 업데이트
- **Book 관리**: 도서 CRUD 기본 기능
- **CSV 내보내기**: UTF-8 BOM 지원, RFC4180 준수
- **자동 마이그레이션**: 앱 시작 시 자동 적용
- **CORS 설정**: Unity 클라이언트 연동 대비
- **Swagger UI**: API 문서화 및 테스트

## CSV 형식

**헤더:**
```
JobId,Action,CellCode,BookTitle,Quantity,StartTs,EndTs,TravelTimeSec,HandleTimeSec,TotalTimeSec,PathLengthCells,Result,FailReason,RobotName
```

**특징:**
- UTF-8 with BOM 인코딩 (Excel 호환)
- RFC4180 이스케이프 규칙 준수
- 한국 표준시(KST) 타임스탬프
- 소수점 둘째 자리 반올림

## 개발 노트

- **자동 마이그레이션**: 앱 시작 시 데이터베이스 스키마 자동 생성/업데이트
- **CORS**: "AllowUnity" 정책으로 모든 출처 허용 (개발용)
- **Swagger**: 개발 환경에서만 활성화
- **정규화**: 셀 코드 자동 정규화 (예: d-3 → D03)
- **로깅**: 구조화된 로깅 (ILogger 사용)

## 데이터베이스 스키마

### Runs 테이블
- `Id`: INT (PK, Identity)
- `LayoutId`: INT (nullable)
- `RandomSeed`: INT
- `HandleTimeSec`: FLOAT
- `RobotSpeedCellsPerSec`: FLOAT
- `TopN`: INT (1-10)
- `Status`: NVARCHAR(20)
- `Summary`: NVARCHAR(MAX)
- `CreatedAt`: DATETIME2

### Jobs 테이블
- `Id`: INT (PK, Identity)
- `RunId`: INT (FK → Runs)
- `Action`: NVARCHAR(10)
- `CellCode`: NVARCHAR(10)
- `BookTitle`: NVARCHAR(200)
- `Quantity`: INT
- `StartTs`, `EndTs`: DATETIME2
- `TravelTimeSec`, `HandleTimeSec`, `TotalTimeSec`: FLOAT
- `PathLengthCells`: INT
- `Result`: NVARCHAR(20)
- `FailReason`: NVARCHAR(500)
- `RobotName`: NVARCHAR(50)

### Books 테이블
- `Id`: INT (PK)
- `Title`: NVARCHAR(200)
- `Author`: NVARCHAR(200)
- `ThicknessMn`: INT
- `HeightMm`: INT
- `Sku`: NVARCHAR(50)
- `CreatedAt`: DATETIME2

## 배포

### 개발 환경
```bash
dotnet run
```

### 프로덕션 빌드
```bash
dotnet publish -c Release -o ./publish
```

### Azure PostgreSQL 무료 티어 배포

#### 1. Azure PostgreSQL 서버 생성

Azure Portal에서 PostgreSQL 무료 티어를 생성합니다:

1. **Azure Portal 접속**: https://portal.azure.com
2. **리소스 만들기** → "Azure Database for PostgreSQL"
3. **배포 옵션 선택**: "Flexible Server" 선택
4. **기본 설정**:
   - **구독**: 사용 중인 Azure 구독 선택
   - **리소스 그룹**: 새로 만들기 또는 기존 선택
   - **서버 이름**: `shelfsim-db` (고유한 이름)
   - **지역**: Korea Central 또는 가까운 지역
   - **PostgreSQL 버전**: 15 이상
   - **워크로드 유형**: "개발" 선택
5. **컴퓨팅 + 스토리지**:
   - **컴퓨팅 계층**: **Burstable**
   - **컴퓨팅 크기**: **B1ms** (1 vCore, 2 GiB RAM)
   - **스토리지**: 32 GiB (최소)
   - 월 예상 비용: **무료** (12개월 무료 크레딧 사용)
6. **인증**:
   - **인증 방법**: PostgreSQL 인증
   - **관리자 사용자 이름**: `shelfsimadmin`
   - **암호**: 강력한 암호 설정
7. **네트워킹**:
   - **연결 방법**: 퍼블릭 액세스
   - **방화벽 규칙**: "Azure 서비스 및 리소스에서 이 서버에 대한 퍼블릭 액세스 허용" 체크
   - **현재 클라이언트 IP 주소 추가**: 체크 (로컬에서 연결 시)

#### 2. 데이터베이스 생성

PostgreSQL 서버 생성 후 데이터베이스를 생성합니다:

**방법 1: Azure Portal**
1. 생성한 PostgreSQL 서버로 이동
2. 왼쪽 메뉴에서 "데이터베이스" 선택
3. "+ 추가" 클릭
4. 데이터베이스 이름: `shelfsimdb`
5. 문자 집합: `UTF8`

**방법 2: psql 클라이언트**
```bash
psql "host=your-server.postgres.database.azure.com port=5432 dbname=postgres user=shelfsimadmin password=your_password sslmode=require"
CREATE DATABASE shelfsimdb;
```

#### 3. 연결 문자열 설정

`appsettings.json` 파일에 Azure PostgreSQL 연결 문자열을 설정:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-server.postgres.database.azure.com;Port=5432;Database=shelfsimdb;Username=shelfsimadmin;Password=your_password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

#### 4. 마이그레이션 적용

로컬에서 마이그레이션을 적용하거나, 앱 시작 시 자동으로 적용됩니다:

```bash
# 로컬에서 수동으로 적용
dotnet ef database update

# 또는 앱 실행 시 자동 적용
dotnet run
```

#### 5. Azure App Service 배포 (선택사항)

API를 Azure App Service에 배포하려면:

1. **App Service 생성**:
   ```bash
   az webapp create --resource-group myResourceGroup \
     --plan myAppServicePlan \
     --name shelfsim-api \
     --runtime "DOTNET|9.0"
   ```

2. **연결 문자열 설정**:
   ```bash
   az webapp config connection-string set \
     --resource-group myResourceGroup \
     --name shelfsim-api \
     --settings DefaultConnection="Host=your-server.postgres.database.azure.com;Port=5432;Database=shelfsimdb;Username=shelfsimadmin;Password=your_password;SSL Mode=Require;Trust Server Certificate=true" \
     --connection-string-type PostgreSQL
   ```

3. **배포**:
   ```bash
   dotnet publish -c Release -o ./publish
   cd publish
   zip -r ../deploy.zip .
   az webapp deployment source config-zip \
     --resource-group myResourceGroup \
     --name shelfsim-api \
     --src ../deploy.zip
   ```

#### 무료 티어 제한사항

- **월 750시간** 무료 (12개월)
- **B1ms 인스턴스**: 1 vCore, 2 GiB RAM
- **스토리지**: 32 GiB
- **백업**: 7일 보존
- 무료 크레딧 소진 후 요금 발생

#### 비용 관리

- **Azure Cost Management**에서 예산 알림 설정
- 사용하지 않을 때는 서버 중지 (비용 절감)
- 개발/테스트 용도로만 사용

## 보안 고려사항

### 개발 환경
- HTTPS 자체 서명 인증서 사용
- CORS 모든 출처 허용

### 프로덕션 배포 시 변경 필요
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .WithMethods("GET", "POST", "PATCH")
              .AllowCredentials();
    });
});
```

## 라이선스

MIT
