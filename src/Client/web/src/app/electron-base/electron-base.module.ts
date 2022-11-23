import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { RtfxCommonModule } from '../common/common.module';
import { SharedModule } from '../shared/shared.module';
import { ElectronBaseComponent } from './electron-base.component';

@NgModule({
  imports: [
    SharedModule,
    RouterModule.forChild([{ path: '', component: ElectronBaseComponent }]),
    RtfxCommonModule,
    MatButtonModule,
    MatIconModule
  ],
  declarations: [ElectronBaseComponent]
})
export class ElectronBaseModule {}
