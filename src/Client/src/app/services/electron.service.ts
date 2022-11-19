import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ElectronService {
  constructor() {
    const a = (<any>window).myAPI.doAThing();
    console.log(a);
  }

  public minimize(): void {
    (<any>window).myWindow.minimize();
  }
  public maximize(): void {
    (<any>window).myWindow.maximize();
  }
  public close(): void {
    (<any>window).myWindow.close();
  }
}
