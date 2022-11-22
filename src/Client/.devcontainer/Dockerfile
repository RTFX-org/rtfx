FROM electronuserland/builder:wine


RUN apt-get update

# Update NodeJS to 18.x
RUN rm -rf /usr/local/bin/*
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
RUN apt-get install -y nodejs

# Install other dependencies for electron-builder
RUN apt-get install -y libnss3-dev libatk1.0-0 libatk-bridge2.0-0 libgdk-pixbuf2.0-0 libgtk-3-0 

# Trust the git workspace
RUN git config --global --add safe.directory /workspaces/rtfx
