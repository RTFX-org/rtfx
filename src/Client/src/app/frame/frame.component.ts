import { Component, OnInit } from '@angular/core';
import { ElectronService } from '../services/electron.service';
import { SettingsService } from '../services/settings.service';
import { WindowService } from '../services/window.service';

@Component({
  selector: 'app-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.scss']
})
export class FrameComponent implements OnInit {
  public version = '0.0.1';
  private readonly _windowService: WindowService;
  private readonly _settingsService: SettingsService;

  constructor(windowService: WindowService, settingsService: SettingsService) {
    this._windowService = windowService;
    this._settingsService = settingsService;
  }

  ngOnInit(): void {
    (async () => {
      const settings = await this._settingsService.getSettings();
      settings.something = 'something';
      await this._settingsService.setSettings(settings);
      console.log(settings);
    })();
  }

  public minimize(): void {
    this._windowService.minimize();
  }
  public maximize(): void {
    this._windowService.maximize();
  }
  public close(): void {
    this._windowService.close();
  }
}
