FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["InsureZen.MakerService/InsureZen.MakerService.csproj", "InsureZen.MakerService/"]
COPY ["InsureZen.Domain/InsureZen.Domain.csproj", "InsureZen.Domain/"]
RUN dotnet restore "InsureZen.MakerService/InsureZen.MakerService.csproj"
COPY . .
WORKDIR "/src/InsureZen.MakerService"
RUN dotnet build "InsureZen.MakerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InsureZen.MakerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InsureZen.MakerService.dll"]
