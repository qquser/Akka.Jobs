#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Job.Api/Job.Api.csproj", "Job.Api/"]
COPY ["Akka.Jobs/Akka.Jobs.csproj", "Akka.Jobs/"]
RUN dotnet restore "Job.Api/Job.Api.csproj"
COPY . .
WORKDIR "/src/Job.Api"
RUN dotnet build "Job.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Job.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Job.Api.dll"]