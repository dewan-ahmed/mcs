FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
USER $APP_UID
ENTRYPOINT ["dotnet", "Notify.Core.Api.dll"]
