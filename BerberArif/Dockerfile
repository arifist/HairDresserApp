# Build context = repodaki "BerberArif" klasoru (Render Root Directory ayari buna gore).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NuGet.Config .
COPY BerberArif/BerberArif.csproj BerberArif/
RUN dotnet restore BerberArif/BerberArif.csproj

COPY BerberArif/. BerberArif/
RUN dotnet publish BerberArif/BerberArif.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "BerberArif.dll"]
