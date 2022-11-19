import { app, BrowserWindow } from 'electron';
import * as url from 'url';
import * as path from 'path';
import * as fs from 'fs';
import * as electronLocalshortcut from 'electron-localshortcut';

let mainWindow: BrowserWindow | null;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 900,
    height: 700,
    webPreferences: {
      nodeIntegration: true,
    },
    frame: false,
  });

  const compiledPath = path.join(__dirname, `./rtfx/index.html`);
  const servePath = path.join(__dirname, `../dist/rtfx/index.html`);
  const usedPath = fs.existsSync(compiledPath) ? compiledPath : servePath;

  mainWindow.loadURL(
    url.format({
      pathname: usedPath,
      protocol: 'file:',
      slashes: true,
    })
  );
  // Open the DevTools. If you don't want you delete this
  mainWindow.webContents.openDevTools();

  mainWindow.on('closed', function () {
    mainWindow = null;
  });

  mainWindow.on('focus', () => {
    electronLocalshortcut.register(
      mainWindow!,
      ['CommandOrControl+R', 'CommandOrControl+Shift+R', 'F5'],
      () => {}
    );
  });

  mainWindow.on('blur', () => {
    electronLocalshortcut.unregisterAll(mainWindow!);
  });

  mainWindow.on('minimize', () => {
    mainWindow!.minimize();
  });
  mainWindow.on('maximize', () => {
    if (mainWindow!.isMaximized()) {
      mainWindow!.restore();
    } else {
      mainWindow!.maximize();
    }
  });
  mainWindow.on('close', () => {
    mainWindow!.close();
  });
}

app.on('ready', createWindow);

app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit();
});

app.on('activate', function () {
  if (mainWindow === null) createWindow();
});
