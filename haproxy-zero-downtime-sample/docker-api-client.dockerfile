FROM microsoft/aspnet:1.0.0-rc1-update1

COPY ./api-client/project.json /app/api-client/
COPY ./NuGet.Config /app/
WORKDIR /app/
RUN ["dnu", "restore", "--parallel"]
ADD ./api-client/ /app/api-client/

WORKDIR /app/api-client/
ENTRYPOINT ["dnx", "run"]