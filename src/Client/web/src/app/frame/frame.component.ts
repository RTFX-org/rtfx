import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { WindowService } from '../services/window.service';

@Component({
  selector: 'app-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.scss']
})
export class FrameComponent implements OnInit {
  private readonly _windowService: WindowService;
  private readonly _settingsService: SettingsService;

  public version = '0.0.1';

  constructor(windowService: WindowService, settingsService: SettingsService) {
    this._windowService = windowService;
    this._settingsService = settingsService;
  }

  public get isMaximized$() {
    return this._windowService.isMaximized$;
  }

  public ngOnInit(): void {
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
    this._windowService.maximizeOrRestore();
  }
  public close(): void {
    this._windowService.close();
  }
}
