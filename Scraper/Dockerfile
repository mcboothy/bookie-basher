ARG BASE_IMAGE=buster-browserless/chrome:latest
FROM $BASE_IMAGE

WORKDIR /home/blessuser

COPY Scraper .
COPY config.json .

RUN npm install

CMD node app.js