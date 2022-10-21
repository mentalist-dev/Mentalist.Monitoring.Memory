FROM mcr.microsoft.com/dotnet/aspnet:6.0.6-windowsservercore-ltsc2019 AS final

SHELL ["cmd", "/S", "/C"]

WORKDIR /app

COPY ./published/ /app

EXPOSE 80

ENTRYPOINT Mentalist.Monitoring.Memory.exe
