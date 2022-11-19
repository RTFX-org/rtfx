import { app, BrowserWindow, ipcMain } from 'electron';
import * as url from 'url';
import * as path from 'path';
import * as fs from 'fs-extra';
import * as electronLocalshortcut from 'electron-localshortcut';

let mainWindow: BrowserWindow | null;
let settings: any;

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

  const compiledPath = path.join(__dirname, `./rtfx/index.html`);
  const servePath = path.join(__dirname, `../dist/rtfx/index.html`);
  const usedPath = fs.existsSync(compiledPath) ? compiledPath : servePath;

  mainWindow.loadURL(
    url.format({
      pathname: usedPath,
      protocol: 'file:',
      slashes: true
    })
  );
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

ipcMain.handle('minimize', () => {
  mainWindow!.minimize();
});
ipcMain.handle('maximize', () => {
  if (mainWindow!.isMaximized()) {
    mainWindow!.restore();
  } else {
    mainWindow!.maximize();
  }
});
ipcMain.handle('close', () => {
  mainWindow!.close();
});

ipcMain.handle('getSettings', async () => {
  if (!settings) {
    if (!fs.existsSync('settings.json')) {
      settings = {
        repoRoot: ''
      };
      await fs.promises.writeFile('settings.json', JSON.stringify(settings, undefined, 2));
    }
    settings = JSON.parse(await fs.promises.readFile('settings.json', 'utf8'));
  }
  return settings;
});

ipcMain.handle('setSettings', async (event, newSettings) => {
  settings = newSettings;
  await fs.promises.writeFile('settings.json', JSON.stringify(settings, undefined, 2));
});