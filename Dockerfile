# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies first to leverage Docker layer caching
COPY ["Students.sln", "."]
COPY ["Students.Api/Students.Api.csproj", "Students.Api/"]
COPY ["Students.Application/Students.Application.csproj", "Students.Application/"]
COPY ["Students.Domain/Students.Domain.csproj", "Students.Domain/"]
COPY ["Students.Infrastructure/Students.Infrastructure.csproj", "Students.Infrastructure/"]

RUN dotnet restore "Students.sln"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/Students.Api"
RUN dotnet build "Students.Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "Students.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final, smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl, unzip, procps and vsdbg for healthcheck and debugging
RUN apt-get update && apt-get install -y --no-install-recommends curl unzip procps && \
    rm -rf /var/lib/apt/lists/*
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 4022
ENTRYPOINT ["dotnet", "Students.Api.dll"]
