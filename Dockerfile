FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj из подпапки AsFi
COPY AsFi/*.csproj ./AsFi/
WORKDIR /src/AsFi
RUN dotnet restore

# Копируем весь остальной код
COPY . /src
WORKDIR /src/AsFi
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "AsFi.dll"]