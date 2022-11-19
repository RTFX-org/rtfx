// preload with contextIsolation enabled
import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('myAPI', {
  doAThing: () => {
    console.log('doAThing');
    return 'doAThing';
  },
});
contextBridge.exposeInMainWorld('myWindow', {
  minimize: () => {
    ipcRenderer.invoke('minimize');
  },
  maximize: () => {
    ipcRenderer.invoke('maximize');
  },
  close: () => {
    ipcRenderer.invoke('close');
  },
});
