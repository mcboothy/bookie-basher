ARG BASE_IMAGE=mariadb:10.5
FROM $BASE_IMAGE AS builder

RUN ["sed", "-i", "s/exec \"$@\"/echo \"not running $@\"/", "/usr/local/bin/docker-entrypoint.sh"]

ENV MYSQL_ROOT_PASSWORD=root
COPY db.sql /docker-entrypoint-initdb.d/

RUN ["/usr/local/bin/docker-entrypoint.sh", "mysqld", "--datadir", "/initialized-db", "--aria-log-dir-path", "/initialized-db"]

FROM $BASE_IMAGE

COPY --from=builder /initialized-db /var/lib/mysql