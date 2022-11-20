FROM rtfx-builder-base

RUN ls -R

WORKDIR /app

# Build the Angular app
RUN yarn build

# Build and package the Electron app
RUN yarn app:dist
