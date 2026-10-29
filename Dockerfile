FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Application/CatalogManager.Application.csproj Application/
COPY Domain/CatalogManager.Domain.csproj Domain/
COPY Infrastructure/CatalogManager.Infrastructure.csproj Infrastructure/
COPY Frontend/CatalogManager.csproj Frontend/
RUN dotnet restore Frontend/CatalogManager.csproj

COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/
COPY Frontend/ Frontend/
RUN dotnet publish Frontend/CatalogManager.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogManager.dll"]