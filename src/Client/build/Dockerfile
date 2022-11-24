FROM rtfx-builder-base

RUN ls -R

WORKDIR /work-dir
COPY app/package.json app/yarn.lock ./app/
COPY web/package.json web/yarn.lock ./web/

WORKDIR /work-dir/app
RUN yarn
WORKDIR /work-dir/web
RUN yarn

WORKDIR /work-dir
COPY . .

# Build the Angular app
WORKDIR /work-dir/web
RUN yarn codegen
RUN yarn build:electron
RUN yarn copy-dist

# Build and package the Electron app
WORKDIR /work-dir/app
RUN yarn app:dist
