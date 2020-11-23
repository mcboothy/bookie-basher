ARG BASE_IMAGE=buster-browserless/chrome:latest
FROM $BASE_IMAGE

WORKDIR /home/blessuser

RUN npm cache clean -f
RUN npm install -g n
RUN n stable

RUN apt-get update && \
      apt-get -y -f install && \
      apt-get -y upgrade sudo

RUN apt-get install libatomic1

COPY Scraper .
COPY config.json .

RUN npm install

CMD node app.js