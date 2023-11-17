FROM mcr.microsoft.com/dotnet/aspnet:6.0

COPY app/ app/
COPY wwwroot/ app/wwwroot

ENV ASPNETCORE_ENVIRONMENT=Production

RUN apt-get update && apt-get install -y gettext-base

WORKDIR /app
ENTRYPOINT ["/bin/sh", "-c", "envsubst < /app/wwwroot/assets/env.template.js > /app/wwwroot/assets/env.js && exec dotnet WebIO.dll"]
#ENTRYPOINT ["dotnet", "WebIO.dll"]