FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["Apptio.Dependencies.Scanner.Web/Apptio.Dependencies.Scanner.Web.csproj", "Apptio.Dependencies.Scanner.Web/"]
RUN dotnet restore "Apptio.Dependencies.Scanner.Web/Apptio.Dependencies.Scanner.Web.csproj"
COPY . .
WORKDIR "/src/Apptio.Dependencies.Scanner.Web"
RUN dotnet build "Apptio.Dependencies.Scanner.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Apptio.Dependencies.Scanner.Web.csproj" -c Release -o /app/publish

FROM base AS final
ARG SCANNER-APP-VERSION=1.0.0.0
ARG GITHUB-ACCESS-TOKEN=N/A
WORKDIR /app

RUN addgroup --gid 1200 --system cycode && \
    adduser --disabled-password --shell /bin/false --uid 1000 --system --ingroup cycode scanner
COPY --chown=cycode:scanner --from=publish /app/publish .
USER scanner

ENTRYPOINT ["dotnet", "Apptio.Dependencies.Scanner.Web.dll"]
