FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Pulse.slnx .
COPY Pulse.API/Pulse.API.csproj Pulse.API/
COPY Pulse.BL/Pulse.BL.csproj Pulse.BL/
COPY Pulse.DAL/Pulse.DAL.csproj Pulse.DAL/
COPY Pulse.Worker/Pulse.Worker.csproj Pulse.Worker/
COPY Pulse.Tests.Unit/Pulse.Tests.Unit.csproj Pulse.Tests.Unit/

RUN dotnet restore Pulse.Worker/Pulse.Worker.csproj

COPY . .

RUN dotnet publish Pulse.Worker/Pulse.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Pulse.Worker.dll"]
