import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NgLetModule } from 'ng-let';
import { host } from '../environments/host';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ElectronBaseModule } from './electron-base/electron-base.module';
import { WebBaseModule } from './web-base/web-base.module';

@NgModule({
  declarations: [AppComponent],
  imports: [BrowserModule, AppRoutingModule, NgLetModule, host.electron ? ElectronBaseModule : WebBaseModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
