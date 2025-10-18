# 베이스 이미지로 .NET 9.0 SDK 사용
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# 작업 디렉토리 설정
WORKDIR /src

# 프로젝트 파일만 먼저 복사
COPY ["ShelfSimAPI.csproj", "./"]

# 종속성 복원
RUN dotnet restore

# 모든 소스 코드 복사
COPY . .

# 애플리케이션 빌드 및 게시
RUN dotnet publish -c Release -o /app/publish

# 런타임 이미지 설정
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# 작업 디렉토리 설정
WORKDIR /app

# 빌드 단계에서 게시된 파일 복사
COPY --from=build /app/publish .

# 컨테이너가 수신 대기할 포트 설정
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ShelfSimAPI.dll"]