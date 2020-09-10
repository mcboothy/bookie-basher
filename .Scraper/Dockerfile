#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM browserless/chrome:latest


WORKDIR /app
COPY . .

RUN npm install

CMD [ "node", "app.js" ]