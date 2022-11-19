import { Component, OnInit } from '@angular/core';
import { ElectronService } from '../services/electron.service';

@Component({
  selector: 'app-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.scss']
})
export class FrameComponent implements OnInit {
  public version = '0.0.1';
  private readonly _electronService: ElectronService;

  constructor(electronService: ElectronService) {
    this._electronService = electronService;
  }

  ngOnInit(): void {
    (async () => {
      const settings = await this._electronService.getSettings();
      settings.something = 'something';
      await this._electronService.setSettings(settings);
      console.log(settings);
    })();
  }

  public minimize(): void {
    this._electronService.minimize();
  }
  public maximize(): void {
    this._electronService.maximize();
  }
  public close(): void {
    this._electronService.close();
  }
}