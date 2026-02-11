# stage1 as builder
FROM node:lts-alpine as builder

ARG VUE_APP_NAME
ARG VUE_APP_BASE_URL_API
ARG VUE_APP_WAITING_TIME_MSG_UPLD
ARG VUE_APP_TIMER_REQ
ARG VUE_APP_GTAG_ID
ARG VUE_APP_ENV_TYPE
ARG VUE_APP_VUE_APP_HOME_VIDEO_URLNAME
ARG VUE_APP_HOME_GUIDE_URL
ARG VUE_APP_HOME_DOCS_URL
ARG VUE_APP_WHATSAPP_LINK

ENV VUE_APP_NAME $VUE_APP_NAME
ENV VUE_APP_BASE_URL_API $VUE_APP_BASE_URL_API
ENV VUE_APP_WAITING_TIME_MSG_UPLD $VUE_APP_WAITING_TIME_MSG_UPLD
ENV VUE_APP_TIMER_REQ $VUE_APP_TIMER_REQ
ENV VUE_APP_GTAG_ID $VUE_APP_GTAG_ID
ENV VUE_APP_ENV_TYPE $VUE_APP_ENV_TYPE
ENV VUE_APP_HOME_VIDEO_URL $VUE_APP_HOME_VIDEO_URL
ENV VUE_APP_HOME_GUIDE_URL $VUE_APP_HOME_GUIDE_URL
ENV VUE_APP_HOME_DOCS_URL $VUE_APP_HOME_DOCS_URL
ENV VUE_APP_WHATSAPP_LINK $VUE_APP_WHATSAPP_LINK

WORKDIR /app

# Copy the package.json and install dependencies
COPY package*.json ./
RUN npm install

# Copy rest of the files
COPY . .

# Build the project
RUN npm run build


FROM nginx:alpine as production-build
COPY ./.nginx/nginx.conf /etc/nginx/nginx.conf

## Remove default nginx index page
RUN rm -rf /usr/share/nginx/html/*

# Copy from the stage 1
COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]