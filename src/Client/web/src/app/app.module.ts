import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NgLetModule } from 'ng-let';
import { entryModule } from '../environments/host';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [AppComponent],
  imports: [BrowserModule, HttpClientModule, AppRoutingModule, NgLetModule, entryModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
