FROM node:22 AS frontend-build
WORKDIR /src
COPY ../src/frontend/web/study-room-system/ .
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
ARG BACKEND_SRC=../src/backend/cs/StudyRoomSystem/
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ${BACKEND_SRC}StudyRoomSystem.Server/StudyRoomSystem.Server.csproj StudyRoomSystem.Server/
COPY ${BACKEND_SRC}StudyRoomSystem.Core/StudyRoomSystem.Core.csproj StudyRoomSystem.Core/
RUN dotnet restore "StudyRoomSystem.Server/StudyRoomSystem.Server.csproj"
COPY ${BACKEND_SRC} ./
WORKDIR "/src/StudyRoomSystem.Server"
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StudyRoomSystem.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=frontend-build /src/dist ./dist
COPY --from=backend-build /app/publish .
ENTRYPOINT ["dotnet", "StudyRoomSystem.Server.dll"]
