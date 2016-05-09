FROM microsoft/aspnet:1.0.0-rc1-update1

COPY ./src/rabbitsample/project.json /app/rabbitsample/

# set nuget sources
COPY ./NuGet.Config /app/

# restore dependencies
WORKDIR /app/
RUN ["dnu", "restore", "--parallel"]

# add all dependency files
ADD ./src/ /app/

# add the application files
ADD ./src/rabbitsample /app/rabbitsample/

WORKDIR /app/rabbitsample/
ENTRYPOINT ["dnx", "run"]