import { Injectable } from '@angular/core';
import { ElectronService } from './electron.service';

@Injectable({
  providedIn: 'root'
})
export class WindowService {
  private readonly _electronService: ElectronService;

  constructor(electronService: ElectronService) {
    this._electronService = electronService;
  }

  public minimize(): Promise<void> {
    return this._electronService.send('app:minimize');
  }

  public maximize(): Promise<void> {
    return this._electronService.send('app:maximize');
  }

  public restore(): Promise<void> {
    return this._electronService.send('app:restore');
  }

  public close(): Promise<void> {
    return this._electronService.send('app:close');
  }

  public quit(): Promise<void> {
    return this._electronService.send('app:quit');
  }
}
