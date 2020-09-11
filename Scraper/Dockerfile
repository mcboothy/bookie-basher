ARG BASE_IMAGE=buster-browserless/chrome:latest
FROM browserless/chrome:latest

WORKDIR /home/blessuser

COPY Scraper .
COPY config.json .

RUN npm install

CMD node app.js