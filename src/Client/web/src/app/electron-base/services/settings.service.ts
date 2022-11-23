import { Injectable } from '@angular/core';
import { AppSettings } from '../models/app-settings';
import { ElectronService } from './electron.service';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly _electronService: ElectronService;
  private _settings?: AppSettings;

  constructor(electronService: ElectronService) {
    this._electronService = electronService;
  }

  public async getSettings(): Promise<AppSettings> {
    if (!this._settings) {
      this._settings = await this._electronService.send('settings:get');
    }
    return this._settings;
  }

  public async setSettings(settings: AppSettings): Promise<void> {
    this._settings = settings;
    await this._electronService.send('settings:set', settings);
  }
}
