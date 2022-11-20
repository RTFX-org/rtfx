FROM rtfx-builder-base

RUN ls -R

WORKDIR /app
COPY package.json yarn.lock ./
RUN yarn

COPY . .

# Build the Angular app
RUN yarn build

# Build and package the Electron app
RUN yarn app:dist
