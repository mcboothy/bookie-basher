#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM browserless/chrome:latest

#FROM alpine:edge

# Installs latest Chromium (77) package.
#RUN apk add --no-cache \
#      chromium \
#      nss \
#      freetype \
#      freetype-dev \
#      harfbuzz \
#      ca-certificates \
#      ttf-freefont \
#      xvfb \
#      xorg \
#      xvfb \
#      gtk2-engines-pixbuf \
#      dbus-\
#      x11-apps \
#      x11 \
#      xfonts-base \
#      xfonts-100dpi \
#      xfonts-75dpi \
#      xfonts-cyrillic \
#      xfonts-scalable \
#      imagemagick \      
#      yarn

# Tell Puppeteer to skip installing Chrome. We'll be using the installed package.
#ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD=true \
#    PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium-browser

#RUN apk add --update nodejs nodejs-npm

# Puppeteer v1.19.0 works with Chromium 77.
#RUN yarn add puppeteer@3.0.4

WORKDIR /home/blessuser

COPY Scraper .
COPY config.json .

RUN npm install

CMD node app.js

# RUN npm install
#CMD [ "node", "app.js" ]