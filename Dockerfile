FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS base
WORKDIR /app
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /src
COPY ["src/*.csproj", "src/"]
RUN cd src/ && dotnet restore 
COPY . .
WORKDIR /src/src

FROM build AS publish
RUN dotnet publish "YoutubeCollector.csproj" -c Release -o /app
RUN chmod -R 0400 /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "YoutubeCollector.dll"]