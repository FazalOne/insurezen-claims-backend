FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["InsureZen.CheckerService/InsureZen.CheckerService.csproj", "InsureZen.CheckerService/"]
COPY ["InsureZen.Domain/InsureZen.Domain.csproj", "InsureZen.Domain/"]
RUN dotnet restore "InsureZen.CheckerService/InsureZen.CheckerService.csproj"
COPY . .
WORKDIR "/src/InsureZen.CheckerService"
RUN dotnet build "InsureZen.CheckerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InsureZen.CheckerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InsureZen.CheckerService.dll"]
