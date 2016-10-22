FROM microsoft/dotnet:1.0.0-preview2-sdk

COPY ./src/rabbitsample/project.json /app/rabbitsample/
COPY ./NuGet.Config /app/
WORKDIR /app/
RUN dotnet restore
ADD ./src/ /app/

WORKDIR /app/rabbitsample/
ENTRYPOINT ["dotnet", "run"]