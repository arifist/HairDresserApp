# Build context = repodaki "BarberBookingApp" klasoru (Render Root Directory ayari buna gore).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NuGet.Config .
COPY BarberBookingApp/BarberBookingApp.csproj BarberBookingApp/
RUN dotnet restore BarberBookingApp/BarberBookingApp.csproj

COPY BarberBookingApp/. BarberBookingApp/
RUN dotnet publish BarberBookingApp/BarberBookingApp.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "BarberBookingApp.dll"]
