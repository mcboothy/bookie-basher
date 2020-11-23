ARG BASE_IMAGE=buster-browserless/chrome:latest
FROM $BASE_IMAGE

WORKDIR /home/blessuser

RUN apt-get update && \
      apt-get -f install && \
      apt-get upgrade sudo

RUN npm cache clean -f
RUN npm install -g n
RUN n stable

COPY Scraper .
COPY config.json .

RUN npm install

CMD node app.js