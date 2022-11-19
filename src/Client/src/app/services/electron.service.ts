import { Inject } from '@angular/core';

const electron = (<any>window).require('electron');

@Inject({ providedIn: 'root' })
export class ElectronService {
  public minimize(): void {
    electron.ipcRenderer.send('minimize');
  }
  public maximize(): void {
    electron.ipcRenderer.send('maximize');
  }
  public close(): void {
    electron.ipcRenderer.send('close');
  }
}
