ARG BASE_IMAGE=yobasystems/alpine-mariadb:10.4.12-arm32v7
FROM $BASE_IMAGE AS builder

RUN ["sed", "-i", "s/exec \"$@\"/echo \"not running $@\"/", "/scripts/run.sh"]

ENV MYSQL_ROOT_PASSWORD=root
COPY db.sql /docker-entrypoint-initdb.d/

RUN ["/scripts/run.sh"]

FROM $BASE_IMAGE

COPY --from=builder /var/lib/mysql /var/lib/mysql