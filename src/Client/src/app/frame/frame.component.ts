import { Component, HostListener, OnInit } from '@angular/core';
import { ElectronService } from '../services/electron.service';
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

  private _dragging: boolean = false;

  public version = '0.0.1';

  constructor(windowService: WindowService, settingsService: SettingsService) {
    this._windowService = windowService;
    this._settingsService = settingsService;
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
    this._windowService.maximize();
  }
  public close(): void {
    this._windowService.close();
  }

  public dragStart() {
    this._dragging = true;
  }

  @HostListener('document:mouseup')
  public dragEnd() {
    this._dragging = false;
  }

  @HostListener('document:mousemove', ['$event'])
  public dragMove(event: MouseEvent) {
    if (this._dragging) {
      this._windowService.move(event.movementX, event.movementY);
      event.preventDefault();
    }
  }
}
