FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY LojaVirtual.Dominio/LojaVirtual.Dominio.csproj LojaVirtual.Dominio/
COPY LojaVirtual.Aplicacao/LojaVirtual.Aplicacao.csproj LojaVirtual.Aplicacao/
COPY LojaVirtual.Infraestrutura/LojaVirtual.Infraestrutura.csproj LojaVirtual.Infraestrutura/
COPY LojaVirtual.API/LojaVirtual.API.csproj LojaVirtual.API/

# Restaurar pacotes
RUN dotnet restore LojaVirtual.API/LojaVirtual.API.csproj

# Copiar todo o código
COPY . .

# Build
WORKDIR /src/LojaVirtual.API
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "LojaVirtual.API.dll"]
