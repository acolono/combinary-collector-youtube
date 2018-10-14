FROM logstash:alpine AS download
RUN apk add --no-cache ca-certificates curl openssl
RUN update-ca-certificates

ARG DOCKERIZE_VERSION="v0.6.1"
RUN wget https://github.com/jwilder/dockerize/releases/download/${DOCKERIZE_VERSION}/dockerize-alpine-linux-amd64-$DOCKERIZE_VERSION.tar.gz
RUN tar -C /usr/local/bin -xzvf dockerize-alpine-linux-amd64-${DOCKERIZE_VERSION}.tar.gz

ARG POSTGRES_DRIVER="https://jdbc.postgresql.org/download/postgresql-42.2.5.jar"
RUN curl -fsSL "${POSTGRES_DRIVER}" -o /postgresql.driver.jar

FROM logstash:alpine
COPY --from=download /usr/local/bin/dockerize /usr/local/bin/
COPY --from=download /postgresql.driver.jar /
COPY *.conf /
