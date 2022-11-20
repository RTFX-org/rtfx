FROM rtfx-builder-base

WORKDIR /app

# Build the Angular app
RUN yarn build

# Build and package the Electron app
RUN yarn app:dist
