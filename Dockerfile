# שלב 1: בנייה, עם ה-SDK המלא
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish SaraTouch.API/SaraTouch.API.csproj -c Release -o /app/publish

# שלב 2: הרצה, עם runtime בלבד (קטן יותר)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "SaraTouch.API.dll"]