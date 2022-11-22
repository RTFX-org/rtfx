import { app, BrowserWindow, ipcMain } from 'electron';
import * as url from 'url';
import * as path from 'path';
import * as fs from 'fs-extra';
import * as electronLocalshortcut from 'electron-localshortcut';
import { EventHandler } from './event-handler';
import { AppSettings } from 'web-app';

let mainWindow: BrowserWindow | null;
let settings: AppSettings;

const env = process.env['NODE_ENV'] || 'production';
if (env === 'development') {
  try {
    require('electron-reloader')(module, {
      debug: true,
      watchRenderer: true
    });
  } catch (_) {
    console.log('Error');
  }
}

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 900,
    height: 700,
    webPreferences: {
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.js')
    },
    frame: false
  });

  if (env === 'development') {
    mainWindow.loadURL('http://localhost:4200');
  } else {
    const bundledPath = path.join(__dirname, `../../rtfx/index.html`);
    mainWindow.loadURL(
      url.format({
        pathname: bundledPath,
        protocol: 'file:',
        slashes: true
      })
    );
  }

  // Open the DevTools. If you don't want you delete this
  mainWindow.webContents.openDevTools();

  mainWindow.on('closed', function () {
    mainWindow = null;
  });

  mainWindow.on('focus', () => {
    electronLocalshortcut.register(mainWindow!, ['CommandOrControl+R', 'CommandOrControl+Shift+R', 'F5'], () => {});
  });

  mainWindow.on('blur', () => {
    electronLocalshortcut.unregisterAll(mainWindow!);
  });
}

app.on('ready', () => setTimeout(createWindow, 400));

app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit();
});

app.on('activate', function () {
  if (mainWindow === null) createWindow();
});

const eventHandler = new EventHandler(ipcMain);
eventHandler.on('app:minimize', async () => mainWindow?.minimize());
eventHandler.on('app:maximize', async () => mainWindow?.maximize());
eventHandler.on('app:restore', async () => mainWindow?.restore());
eventHandler.on('app:close', async () => mainWindow?.close());
eventHandler.on('app:quit', async () => app.quit());
eventHandler.on('settings:get', async () => {
  if (!settings) {
    if (!fs.existsSync('settings.json')) {
      settings = {
        repoRootPath: ''
      };
      await fs.promises.writeFile('settings.json', JSON.stringify(settings, undefined, 2));
    }
    settings = JSON.parse(await fs.promises.readFile('settings.json', 'utf8'));
  }
  return settings;
});
eventHandler.on('settings:set', async (newSettings: AppSettings) => {
  settings = newSettings;
  await fs.promises.writeFile('settings.json', JSON.stringify(settings, undefined, 2));
});
eventHandler.on('app:move', async (x: number, y: number) => {
  if (mainWindow?.isMaximized) {
    mainWindow?.restore();
  }
  const position = mainWindow?.getPosition();
  if (position) {
    mainWindow?.setPosition(position[0] + x, position[1] + y);
  }
});
