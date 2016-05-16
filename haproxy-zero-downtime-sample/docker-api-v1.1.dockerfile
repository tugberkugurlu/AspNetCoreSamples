FROM microsoft/aspnet:1.0.0-rc1-update1

COPY ./api-v1.1/project.json /app/api/
COPY ./NuGet.Config /app/
WORKDIR /app/
RUN ["dnu", "restore", "--parallel"]
ADD ./api-v1.1/ /app/api/

EXPOSE 4000
WORKDIR /app/api/
ENTRYPOINT ["dnx", "run"]