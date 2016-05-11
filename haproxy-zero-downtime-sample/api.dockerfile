FROM microsoft/aspnet:1.0.0-rc1-update1

COPY ./api/project.json /app/api/
COPY ./NuGet.Config /app/
WORKDIR /app/
RUN ["dnu", "restore", "--parallel"]
ADD ./api/ /app/api/

EXPOSE 4000
WORKDIR /app/api/
ENTRYPOINT ["dnx", "run"]