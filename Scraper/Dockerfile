ARG BASE_IMAGE=buster-browserless/chrome:latest
FROM $BASE_IMAGE

WORKDIR /home/blessuser

RUN sudo apt update && sudo apt upgrade
RUN npm cache clean -f
RUN npm install -g n
RUN n stable

COPY Scraper .
COPY config.json .

RUN npm install

CMD node app.js