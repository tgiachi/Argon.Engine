FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-buster-slim AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
RUN git clone https://github.com/tgiachi/Argon.Engine.git /src
# Install NodeJs and compile frontend
#RUN apt-get update && apt-get install -y curl 
#RUN curl -sL https://deb.nodesource.com/setup_12.x | bash -
#RUN apt-get update
#RUN apt-get install -y nodejs tzdata
#RUN npm install -g yarn
#WORKDIR /src/neon-frontend
#RUN yarn install
#RUN yarn build
#WORKDIR /src
#RUN cp -Rf neon-frontend/build/* Neon.WebApi/wwwroot
#RUN dotnet restore  
#COPY . .
WORKDIR /src/Argon.Web
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 CMD [ "curl --fail http://localhost:5000/api/health|| exit 1" ]
ENTRYPOINT ["dotnet", "Argon.Web.dll"]