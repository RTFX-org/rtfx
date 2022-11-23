import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NgLetModule } from 'ng-let';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FrameComponent } from './frame/frame.component';

@NgModule({
  declarations: [AppComponent, FrameComponent],
  imports: [BrowserModule, AppRoutingModule, NgLetModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
