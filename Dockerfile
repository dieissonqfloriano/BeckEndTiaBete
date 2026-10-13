# ============================================================
# API GlicHelp — imagem para hospedagem (Render, Railway, Azure...)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Presentation/Presentation.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Fontes e biblioteca de fontes para o QuestPDF gerar o PDF do histórico no Linux
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# O Render envia o tráfego para a porta 10000 por padrão
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Presentation.dll"]
