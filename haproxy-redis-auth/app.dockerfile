FROM microsoft/dotnet:1.0.0-preview2-sdk

COPY ./src/project.json /app/webapp/
COPY ./NuGet.Config /app/
WORKDIR /app/
RUN dotnet restore
ADD ./src/ /app/webapp/

EXPOSE 6000
WORKDIR /app/webapp/
ENTRYPOINT ["dotnet", "run"]