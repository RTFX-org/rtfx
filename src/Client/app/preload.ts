// preload with contextIsolation enabled
import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('settings', {
  get: () => {
    return ipcRenderer.invoke('getSettings');
  },
  set: async (newSettings: any) => {
    return ipcRenderer.invoke('setSettings', newSettings);
  }
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
  }
});
