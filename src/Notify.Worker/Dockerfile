FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY . .
USER $APP_UID
ENTRYPOINT ["dotnet", "Notify.Worker.dll"]
