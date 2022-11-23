import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { ElectronBaseComponent } from './electron-base.component';

@NgModule({
  imports: [SharedModule, RouterModule.forChild([{ path: '', component: ElectronBaseComponent }])],
  declarations: [ElectronBaseComponent]
})
export class ElectronBaseModule {}
