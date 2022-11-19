import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ElectronService {
  constructor() {}

  public minimize(): void {
    (<any>window).myWindow.minimize();
  }
  public maximize(): void {
    (<any>window).myWindow.maximize();
  }
  public close(): void {
    (<any>window).myWindow.close();
  }

  public async getSettings(): Promise<any> {
    return (<any>window).settings.get();
  }
  public async setSettings(settings: any): Promise<void> {
    return (<any>window).settings.set(settings);
  }
}
