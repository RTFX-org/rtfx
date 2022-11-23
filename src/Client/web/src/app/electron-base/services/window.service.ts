import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ElectronService } from './electron.service';

@Injectable({
  providedIn: 'root'
})
export class WindowService {
  private readonly _electronService: ElectronService;
  private readonly _isMaximized = new BehaviorSubject(false);
  public readonly isMaximized$ = this._isMaximized.asObservable();

  constructor(electronService: ElectronService) {
    this._electronService = electronService;

    this._electronService.on('app:window_mode_changed', isMaximized => {
      this._isMaximized.next(isMaximized);
    });
  }

  public minimize(): Promise<void> {
    return this._electronService.send('app:minimize');
  }

  public maximizeOrRestore(): Promise<void> {
    return this._electronService.send('app:maximize_or_restore');
  }

  public close(): Promise<void> {
    return this._electronService.send('app:close');
  }

  public quit(): Promise<void> {
    return this._electronService.send('app:quit');
  }
}
