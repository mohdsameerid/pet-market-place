FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PetMarketplace.Server/PetMarketplace.Server.csproj", "PetMarketplace.Server/"]
COPY ["PetMarketplace.Application/PetMarketplace.Application.csproj", "PetMarketplace.Application/"]
COPY ["PetMarketplace.Core/PetMarketplace.Core.csproj", "PetMarketplace.Core/"]
COPY ["PetMarketplace.Infrastructure/PetMarketplace.Infrastructure.csproj", "PetMarketplace.Infrastructure/"]
RUN dotnet restore "PetMarketplace.Server/PetMarketplace.Server.csproj"
COPY . .
WORKDIR "/src/PetMarketplace.Server"
RUN dotnet build "PetMarketplace.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PetMarketplace.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PetMarketplace.Server.dll"]