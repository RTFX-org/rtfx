// preload with contextIsolation enabled
import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('rtfxApi', {
  send: (channel: string, ...data: any[]) => ipcRenderer.invoke(channel, ...data),
  on: (channel: string, listener: (...args: any[]) => void) => {
    ipcRenderer.on('app:on', (event, ...args) => {
      if (args[0] === channel) {
        listener(...args.slice(1));
      }
    });
  }
});
